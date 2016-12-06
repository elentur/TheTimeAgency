﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Assets.TheTimeAgency.Scripts.KDTree;
using Assets.TheTimeAgency.Scripts.Trees;
using Object = UnityEngine.Object;
using Random = System.Random;

public class PingState : ICrimeSceneState
{

    private readonly CrimeScene crimeScene;

    /// <summary>
    /// The interval in meters between buckets of points. For example, a high sensitivity of 0.01 will group 
    /// points into buckets every 1cm.
    /// </summary>
    private const float SENSITIVITY = 0.001f;

    /// <summary>
    /// The minimum number of points near a world position y to determine that it is not simply noise points.
    /// </summary>
    private const int NOISE_THRESHOLD = 50;

    /// <summary>
    /// The minimum number of points near a world position y to determine that it is a reasonable floor.
    /// </summary>
    private const int RECOGNITION_THRESHOLD = 100;

    private bool m_ping,  m_show;

    private readonly List<GameObject> _advicesList,_founded, _notFounded;

    private float _maxX, _minX, _maxZ, _minZ;

    private readonly SortedDictionary<float, List<Vector3>> _pointDic;

    private KNN _targetKnn;

    private KDTree<Vector3> pTree;

    public PingState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
        _advicesList = new List<GameObject>();
        _founded = new List<GameObject>();
        _notFounded = new List<GameObject>();
        _pointDic = new SortedDictionary<float, List<Vector3>>();

        _targetKnn = new KNN();

        pTree = new KDTree<Vector3>(3);
    }

    public void StartState()
    {
        SetMaxMin();
    }

    private void SetMaxMin()
    {
        var xArray = new float[crimeScene.markerList.Count];
        var zArray = new float[crimeScene.markerList.Count];

        for (var i = 0; i < crimeScene.markerList.Count; i++)
        {
            xArray[i] = crimeScene.markerList[i].transform.position.x;
            zArray[i] = crimeScene.markerList[i].transform.position.z;
        }

        _maxX = xArray.Max();
        _minX = xArray.Min();
        _maxZ = zArray.Max();
        _minZ = zArray.Min();
    }

    private static bool InfiniteCameraCanSeePoint( Vector3 point)
    {    
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return (viewportPoint.z >0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint ) && viewportPoint.z <Camera.main.farClipPlane);
    }

    private List<GameObject> PlaceholderInCameraView()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        List<GameObject> placeHolders = new List<GameObject>();

        foreach (GameObject placeholder in crimeScene.m_AdvicePlaceHolderList)
        {

            var script = placeholder.GetComponent<AdvicePlaceHolder>();
            // placeholder is already adapted and in the Dictionary
            if (script.Adapted) continue;

            var point = placeholder.transform.position;

            if (InfiniteCameraCanSeePoint(point))
            {
                placeHolders.Add(placeholder);  
            }
        }

        stopwatch.Stop();

        // Write result.
        Debug.Log(string.Format("Time elapsed for PlaceholderInCameraView: {0}", stopwatch.Elapsed));

        return placeHolders;
    }

    private Vector3[] PointCloudPointsICameraView()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        List<Vector3> points = new List<Vector3>();
       
        foreach (Vector3 point in crimeScene.m_pointCloud.m_points)
        {
            if (point.x <= _maxX && point.x >= _minX && point.z <= _maxZ && point.z >= _minZ)
            {
                if (InfiniteCameraCanSeePoint(point))
                {
                    if (crimeScene.triangleList[0].PointInTriangle(point) || crimeScene.triangleList[1].PointInTriangle(point))
                    {

                        /*for (float i = _minZ; i < _maxZ; i += 0.5f)
                        {
                            for (float j = _minX; j < _maxX; j += 0.5f)
                            {

                                Debug.Log(string.Format("{0}/{1}",i,j));

                            }
                        }*/


                        //pTree.AddPoint(new double[] { x, y }, new EllipseWrapper(x, y));
                        pTree.AddPoint(new double[] { point.x, point.y, point.z }, point);

                        points.Add(point);
                        // Group similar points into buckets based on sensitivity. 
                        float roundedY = Mathf.Round(point.y / SENSITIVITY) * SENSITIVITY;

                        if (!_pointDic.ContainsKey(roundedY))
                        {
                            _pointDic.Add(roundedY, new List<Vector3>());
                        }

                        _pointDic[roundedY].Add(point);

                    }
                }
            }
        }
       
        stopwatch.Stop();

        // Write result.
        Debug.Log(string.Format("Time elapsed for PointCloudPointsICameraView: {0}", stopwatch.Elapsed));

        var mPoints = points.ToArray();

        var pIter = pTree.NearestNeighbors(new double[] { mPoints[0].x, mPoints[0].y, mPoints[0].z }, 5, 0.9f);

        while (pIter.MoveNext())
        {
            // Get the ellipse.
            var point = pIter.Current;

            Debug.Log(string.Format("p: {0}", point));

            GameObject advice = AddCube("advice_" + point.x + "/" + point.y + "/" + point.z);

            advice.transform.position = point;

            advice.SetActive(true);

            _advicesList.Add(advice);

        }


       /* _targetKnn.build(mPoints, Enumerable.Range(0, points.Count).ToArray());

        var targetIndices = _targetKnn.knearest(mPoints[0], 50);

        for (var i = 0; i < targetIndices.Length; i++)
        {
            Debug.Log(string.Format("targetIndices {0}: {1}",i, targetIndices[i]));
            Debug.Log(string.Format("mPoints {0}: {1}", i, mPoints[i]));



            GameObject advice = AddCube("advice_" + mPoints[i].x + "/" + mPoints[i].y + "/" + mPoints[i].z);

            advice.transform.position = mPoints[i];

            advice.SetActive(true);

            _advicesList.Add(advice);


        }*/

        return mPoints;
    }

    private void AdaptAdvicePlaceHolders(List<GameObject> placeholderList, Vector3[] pointCloudPointList )
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        foreach (var placeholder in placeholderList)
        {

            var script = placeholder.GetComponent<AdvicePlaceHolder>();

            float y = placeholder.transform.position.y;

            Collider c = placeholder.GetComponent<Collider>();

            placeholder.SetActive(true);

            foreach (Vector3 p in pointCloudPointList)
            {
                if (c.bounds.Contains(new Vector3(p.x, placeholder.transform.position.y, p.z)))
                {
                    script.Adapted = true;
                    if (y >= p.y) continue;
                    y = p.y;
                    break;
                }
            }

            if (script.Adapted)
            {

                var target = placeholder.transform.position;

                target.y = y;
                placeholder.transform.position = target;
                script.Heigth = y;

                if (!_founded.Contains(placeholder)) _founded.Add(placeholder);
            }

        }

        stopwatch.Stop();

        // Write result.
        Debug.Log(string.Format("Time elapsed for AdaptAdvicePlaceHolders: {0}", stopwatch.Elapsed));
    }

    public void SetAdvices()
    {

        // TODO eventuell in SplitList auslagern
        List<GameObject> filteredList = _founded.Where(x => !x.GetComponent<AdvicePlaceHolder>().Checked).ToList();

        int max = crimeScene.m_AdvicePlaceHolderList.Count / crimeScene.m_numberAdvices;

        List<List<GameObject>> splitted = SplitList(filteredList, max);

        Random rnd = new Random();

        foreach (List<GameObject> list in splitted)
        {

            foreach (var plchlder in list)
            {
                plchlder.GetComponent<AdvicePlaceHolder>().Checked = true;
            }

            int r = rnd.Next(list.Count);

            GameObject placeholder = list[r];

            GameObject advice = AddCube("advice_" + placeholder.name);

            advice.transform.position = placeholder.transform.position;

            advice.SetActive(true);

            _advicesList.Add(advice);
        }
    }

    public void UpdateState()
    {
        if (m_ping)
        {
            if (crimeScene.m_AdvicePlaceHolderList.Count <= 0) return;

            m_ping = false;

            Vector3[] pointCloudPointList = PointCloudPointsICameraView();

            Debug.Log(string.Format("dic: {0}",_pointDic.Count));

            foreach (var pointList in _pointDic.Values)
            {

                if (pointList.Count > 10)
                {

                    Vector3 sum = Vector3.zero;

                    foreach (var point in pointList)
                    {
                        sum += point;
                    }

                    Vector3 average = sum/pointList.Count;

                    bool outOfReach = true;

                    foreach (var advice in _advicesList)
                    {
                        float dist = Vector3.Distance(advice.transform.position, average);

                        if (dist < 0.5f)
                        {
                            outOfReach = false;
                        }
                    }

                    if(outOfReach)
                    {
                   
                        GameObject advice = AddCube("advice_" + average.x + "/" + average.y + "/" + average.z);

                        advice.transform.position = average;

                        advice.SetActive(true);

                        _advicesList.Add(advice);
                    }


                }
            }

            //List<GameObject> placeHolderList = PlaceholderInCameraView();

            //AdaptAdvicePlaceHolders(placeHolderList, pointCloudPointList);

            //SetAdvices();
        }

        if (m_show)
        {
            ShowAllInactiveCubes();
            m_show = false;
        }

    }

    private void ShowAllInactiveCubes()
    {

        // disable all placeholders
        foreach (var placeholder in _notFounded)
        {
            placeholder.SetActive(false);
        }

        // set all unvisted placeholder active
        foreach (var placeholder in crimeScene.m_AdvicePlaceHolderList)
        {

            if (!placeholder.GetComponent<AdvicePlaceHolder>().Checked)
            {
                // das funktioniert leider nicht so!!!!! :( er findet leider den placeholder nicht
                GameObject notFound = _notFounded.Find(x => x.transform.position == placeholder.transform.position);

                GameObject placeholder_ =  (notFound) ? notFound : AddCube("placeholder_" + placeholder.name);

                placeholder_.transform.position = placeholder.transform.position;

                placeholder_.GetComponent<Renderer>().material.color = Color.green;

                placeholder_.SetActive(true);

                if (!_notFounded.Contains(placeholder_)) _notFounded.Add(placeholder_);

            }
        }
    }

    private static List<List<GameObject>> SplitList(List<GameObject> locations, int nSize = 30)
    {
        var list = new List<List<GameObject>>();

        for (int i = 0; i < locations.Count; i += nSize)
        {
            if (nSize <= locations.Count - i)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }
        }

        return list;
    }

    private List<GameObject> GetGameobjectsInRadius(GameObject target, List<GameObject> list, float distance)
    {

        List<GameObject> neighbours = new List<GameObject>();

        foreach (GameObject cube in list)
        {

            //Debug.Log(string.Format("distance: {0}", Vector3.Distance(target.transform.position, cube.transform.position)));

            if (Vector3.Distance(target.transform.position, cube.transform.position) < distance)
                neighbours.Add(cube);
        }

        return neighbours;
    }

    public void OnGUIState()
    {
        GUI.color = Color.white;
    
        if (!m_ping)
        {
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Ping</size>")) m_ping = true;
        }
      
        m_show = GUI.Toggle(new Rect(Screen.width - 220, Screen.height - 100, 200, 80), m_show, "<size=30>Show</size>");

        GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), string.Format("<size=30> {0} / {1} Aadvices added!</size>", _advicesList.Count, crimeScene.m_numberAdvices));
    }

    public class Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 vecL, Vector3 vecR)
        {
            return Math.Abs(vecL.x - vecR.x) < SENSITIVITY && Math.Abs(vecL.y - vecR.y) < SENSITIVITY && Math.Abs(vecL.z - vecR.z) < SENSITIVITY;
        }

        public int GetHashCode(Vector3 vector)
        {
            int x = Mathf.RoundToInt(vector.x);
            int y = Mathf.RoundToInt(vector.y);
            int z = Mathf.RoundToInt(vector.z);
            return x * 1000 + z + y * 1000000;
        }
    }

    private GameObject AddCube(string name)
    {
        // copy of the maker
        GameObject myCube = Object.Instantiate<GameObject>(crimeScene.m_advice);

        myCube.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        myCube.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);
        // Place the marker at the center of the screen at the found floor height.

        // adding a Colider for ping state
        BoxCollider bc = (BoxCollider)myCube.gameObject.AddComponent(typeof(BoxCollider));
        bc.center = Vector3.zero;

        myCube.SetActive(false);

        return myCube;
    }
}
