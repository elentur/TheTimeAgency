using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.TheTimeAgency.Scripts;
using Object = UnityEngine.Object;

public class MarkCrimeSceneState : ICrimeSceneState
{

    private readonly CrimeScene _crimeScene;

    /// <summary>
    /// If <c>true</c>, floor finding is in progress.
    /// </summary>
    private bool m_setMarker = false, m_resetMarkers = false, m_defaultMarkers= false;

    private char _pointName = 'A';

    private const float DISTANCE = 2.0f;

    public GameObject MakersBox;

    public List<Vector3> Vertices = new List<Vector3>();

    private Vector3[] defaultVertces = new[]
    {
        /*new Vector3(1f, -1.300f, 2f),
        new Vector3(2f, -1.300f, -1f),
        new Vector3(-1f, -1.300f, -2f),
        new Vector3(-3f, -1.300f, 3f),*/

        new Vector3(0.8f, -0.3f, 3.9f),
        new Vector3(1.7f, -0.3f, 0.29f),
        new Vector3(-1.6f, -0.3f, -0.7f),
        new Vector3(-2.9f, -0.3f, 1.6f)
    };

    public MarkCrimeSceneState(CrimeScene crimeScenePattern)
    {
        _crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
        //  throw new System.NotImplementedException();

        MakersBox = new GameObject("makersBox");
        MakersBox.transform.SetParent(_crimeScene.m_marker.transform.parent.gameObject.transform, false);
    }

    void ICrimeSceneState.UpdateState()
    {

        if (m_resetMarkers) ResetMarkers();

        Vector3 p1 = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE;

        _crimeScene.m_marker.transform.position = new Vector3(p1.x, 1.0f + _crimeScene.m_floorPoint.y, p1.z);

        _crimeScene.m_marker.SetActive(true);

        if (m_defaultMarkers) SetDefaultMarker();

        if (m_setMarker) SetMarker(_crimeScene.m_marker.transform.position);
    }

    private void SetMarker(Vector3 position)
    {

        m_setMarker = false;

        bool toClose = vec3ToClose(position);

        bool isInside = false;

        if (toClose)
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

        AddMarker(position);

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

    private void AddMarker(Vector3 position)
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

        if (Vertices.Count >= _crimeScene.m_numberMarkers)
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("Congratulations!!!!! All makers set!"));
            m_setMarker = false;
            _crimeScene.m_marker.SetActive(false);
            SetRandomAdvices();

            ToPingState();
            return;
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("{0} / {1}  makers set!", Vertices.Count, _crimeScene.m_numberMarkers));
        }


        GUI.color = Color.white;

        if (!m_setMarker)
        {
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Set Marker</size>")) m_setMarker = true;
            
        }

        if (!m_resetMarkers)
        {
            if (GUI.Button(new Rect(Screen.width - 240, 120, 220, 80), "<size=30>Reset Marker</size>")) m_resetMarkers = true;
        }

        if (!m_defaultMarkers)
        {
            if (GUI.Button(new Rect(Screen.width - 240, 220, 220, 80), "<size=30>Default Marker</size>")) m_defaultMarkers = true;
        }
    }

    private void SetRandomAdvices() {

        var maxX = Math.Max(Vertices[0].x, Math.Max(Vertices[1].x, Math.Max(Vertices[2].x, Vertices[3].x)));
        var minX = Math.Min(Vertices[0].x, Math.Min(Vertices[1].x, Math.Min(Vertices[2].x, Vertices[3].x)));

        var maxZ = Math.Max(Vertices[0].z, Math.Max(Vertices[1].z, Math.Max(Vertices[2].z, Vertices[3].z)));
        var minZ = Math.Min(Vertices[0].z, Math.Min(Vertices[1].z, Math.Min(Vertices[2].z, Vertices[3].z)));

        while (_crimeScene.m_defaultAdvices.Count() < _crimeScene.m_numberAdvices) {

            
            Vector3 average = new Vector3(
                UnityEngine.Random.Range(minX, maxX),
                 _crimeScene.m_floorPoint.y,
                UnityEngine.Random.Range(minZ, maxZ));

            if (_crimeScene.triangleList[0].PointInTriangle(average) || _crimeScene.triangleList[1].PointInTriangle(average))
            {
                _crimeScene.m_defaultAdvices.Add(average);
            }
        }
    }

    private void ToPingState()
    {
        _crimeScene.currentState = _crimeScene.pingState;
        _crimeScene.currentState.StartState();
    }

    private bool vec3ToClose(Vector3 target)
    {
        bool toClose = false;

        foreach (Triangle2D triangle in _crimeScene.triangleList)
        {
            foreach (Vector3 vertice in triangle.GetVertices())
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
        }

        return toClose;
    }
}
