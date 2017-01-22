﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.TheTimeAgency.Scripts;
using Object = UnityEngine.Object;
using System.Collections;

public class MarkCrimeSceneState : ICrimeSceneState
{

    private readonly CrimeScene _crimeScene;

    /// <summary>
    /// If <c>true</c>, floor finding is in progress.
    /// </summary>
    public bool m_setMarker = false, m_resetMarkers = false, m_defaultMarkers= false;

    private char _pointName = 'A';

    private const float DISTANCE = 2.0f;

    private GameObject MakersBox = new GameObject("makersBox");

    public List<Vector3> Vertices = new List<Vector3>();

    private Vector3[] defaultVertces = new[]
    {
        new Vector3(0.8f, -0.3f, 3.9f),
        new Vector3(1.7f, -0.3f, 0.29f),
        new Vector3(-1.6f, -0.3f, -0.7f),
        new Vector3(-2.9f, -0.3f, 1.6f)
    };
    private List<GameObject> markers = new List<GameObject>();

    public MarkCrimeSceneState(CrimeScene crimeScenePattern)
    {
        _crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
        //  throw new System.NotImplementedException();

        //MakersBox = new GameObject("makersBox");
        //MakersBox.transform.SetParent(_crimeScene.m_marker.transform.parent.gameObject.transform, false);

        GameObject barrierInterface = GameObject.Find("SetBarrierInterface");
        if (barrierInterface != null)
        {
            _crimeScene.m_markerCanvas.GetComponent<PanelManager>().OpenPanel(barrierInterface.GetComponent<Animator>());
        }
    }

    void ICrimeSceneState.UpdateState()
    {
        

        Vector3 p1 = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE;


        _crimeScene.m_marker.transform.position = new Vector3(p1.x, 1.0f + _crimeScene.m_floorPoint.y, p1.z);

        _crimeScene.m_marker.SetActive(true);

        if (m_defaultMarkers) SetDefaultMarker();

        if (m_setMarker) SetMarker(_crimeScene.m_marker.transform.position);
    }

    private void SetMarker(Vector3 position)
    {

        m_setMarker = false;

        bool tooClose = Vec3ToClose(position);

        bool isInside = false;

        if (tooClose)
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("The distance to all other makers has to be {0}", _crimeScene.m_distanceMarkers));
            return;
        }

        foreach (Triangle2D triangle in _crimeScene.triangleList)
        {
            if (triangle.PointInTriangle(position))
            {
                isInside = true;
            }
        }

        if (isInside)
        {
            AndroidHelper.ShowAndroidToastMessage("The marker can't be within the crime scine area!");
            return;
        }

       GameObject myMarker = AddMarker(position);


        /*  if (Vertices.Count > 0)
          {

              _crimeScene.StartCoroutine(createMesh(myMarker, markers[markers.Count - 1]));

          }


          if (Vertices.Count >= 4)
          {
              _crimeScene.StartCoroutine(createMesh(markers[0], myMarker));
          }*/
        markers.Add(myMarker);
        Vertices.Add(position);

        if (!_crimeScene.triangleList.Any() && Vertices.Count > 2)
        {
            Triangle2D tri = new Triangle2D(
                Vertices[0],
                Vertices[1],
                Vertices[2]
            );

            _crimeScene.triangleList.Add(tri);
        }

        if (Vertices.Count > 3)
        {
            Triangle2D tri = _crimeScene.triangleList[_crimeScene.triangleList.Count - 1];
            _crimeScene.triangleList.Add(tri.AdjacentTriangle(Vertices[3]));
        }
    }

    private GameObject AddMarker(Vector3 position)
    {
        // copy of the maker
        GameObject myMarker = Object.Instantiate(_crimeScene.m_marker);
        
        myMarker.name = _pointName.ToString();

        _pointName++;

        /*
         * Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation. 
         * http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        */
        myMarker.transform.SetParent(MakersBox.transform, false);

        myMarker.transform.position = position;
       
        myMarker.SetActive(true);
        myMarker.GetComponent<Animator>().enabled = true;
      
        return myMarker;
    }
    void createMesh(GameObject instance, GameObject lastInstance)
    {

      //  yield return new WaitForSeconds(2.4f);
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        Transform tra = lastInstance.transform;
        float height = tra.position.y + tra.localScale.y + 0.04f;
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        float scale = Vector3.Distance(instance.transform.position, lastInstance.transform.position);
        plane.transform.localScale = new Vector3(scale, 0.08f, 0.0f);
        Vector3 p = (instance.transform.position + lastInstance.transform.position) * 0.5f;
        p.y = height;
        plane.transform.position = p;
        instance.transform.LookAt(lastInstance.transform);
        plane.transform.rotation = instance.transform.rotation;
        plane.transform.Rotate(0, 90, 0);

        Material newMat = Resources.Load("Banner", typeof(Material)) as Material;
        Renderer renderer = plane.GetComponent<Renderer>();
        renderer.material = newMat;
        renderer.material.SetTextureScale("_MainTex", new Vector2(scale / 0.1f / 8.8f, 1));
    }
    private void ResetMarkers()
    {
        _crimeScene.triangleList.Clear();
        Vertices.Clear();

        foreach (Transform child in MakersBox.transform)
        {
            Object.Destroy(child.gameObject);
        }

        m_resetMarkers = false;
    }

    private void SetDefaultMarker()
    {
        foreach (var vertice in defaultVertces)
        {
            SetMarker(vertice);
        }
        m_defaultMarkers = false;
    }

    void ICrimeSceneState.OnGUIState()
    {

        if (Vertices.Count >= _crimeScene.m_numberMarkers )
        {

            _crimeScene.StartCoroutine(creatBarriers());

            m_setMarker = false;
            _crimeScene.m_marker.SetActive(false);

            ToPingState();

            return;
        }
       
        AndroidHelper.ShowAndroidToastMessage(string.Format("{0} / {1}  makers set!", Vertices.Count, _crimeScene.m_numberMarkers));
    }

    private IEnumerator creatBarriers()
    {
        yield return new WaitForSeconds(2.4f);

        List<Vector3> orderdVecs = new List<Vector3>();
        foreach (Vector3 v in _crimeScene.triangleList[0].GetVertices())
        {
            if (_crimeScene.triangleList[1].GetVertices().Contains(v)) orderdVecs.Add(v);
            else orderdVecs.Insert(0, v);
        }
        foreach (Vector3 v in _crimeScene.triangleList[1].GetVertices())
        {
            if (!orderdVecs.Contains(v))
            {
                orderdVecs.Insert(2, v);
                break;
            }

        }


        for (int i = 0; i < orderdVecs.Count; i++)
        {
            
            GameObject obj1 = null;
            GameObject obj2 = null;
            if (i < 3)
            {
                foreach (GameObject g in markers)
                {
                    if (g.transform.position == orderdVecs[i]) obj1 = g;
                    if (g.transform.position == orderdVecs[i + 1]) obj2 = g;
                }


                createMesh(obj1, obj2);

            }
            else
            {
                foreach (GameObject g in markers)
                {
                    if (g.transform.position == orderdVecs[i]) obj1 = g;
                    if (g.transform.position == orderdVecs[0]) obj2 = g;
                }
                createMesh(obj1, obj2);
            }
        }

    }

    private void ToPingState()
    {
       // _crimeScene.m_markerCanvas.SetActive(false);
        _crimeScene.currentState = _crimeScene.pingState;
        _crimeScene.currentState.StartState();
    }

    private bool Vec3ToClose(Vector3 target)
    {
        bool toClose = false;

 
            foreach (Vector3 vertice in Vertices)
            {
                float distSqr = Vector3.SqrMagnitude(
                    new Vector2(vertice.x, vertice.z) - new Vector2(target.x, target.z)
                );

                if (distSqr < _crimeScene.m_distanceMarkers*_crimeScene.m_distanceMarkers)
                {
                    toClose = true;
                    break;
                }
            }
        

        return toClose;
    }
}
