using System;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Assets.TheTimeAgency.Scripts.KDTree;
using Assets.TheTimeAgency.Scripts.Trees;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class PingState : ICrimeSceneState
{

    private readonly CrimeScene crimeScene;

    /// <summary>
    /// The interval in meters between buckets of points. For example, a high sensitivity of 0.01 will group 
    /// points into buckets every 1cm.
    /// </summary>
    private const float SENSITIVITY = 0.002f;

    private bool m_ping, m_show;

    private readonly List<GameObject> _advicesList;

    private float _maxX, _minX, _maxZ, _minZ;

    private readonly SortedDictionary<float, List<V3>> _pointDic;

    public PingState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
        _advicesList = new List<GameObject>();
        _pointDic = new SortedDictionary<float, List<V3>>();
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

    private static bool InfiniteCameraCanSeePoint(Vector3 point)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return (viewportPoint.z > 0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint) && viewportPoint.z < Camera.main.farClipPlane);
    }

    private void PointCloudPointsInCameraView()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        foreach (Vector3 point in crimeScene.m_pointCloud.m_points)
        {
            if (point.x <= _maxX && point.x >= _minX && point.z <= _maxZ && point.z >= _minZ)
            {
                if (InfiniteCameraCanSeePoint(point))
                {
                    if (crimeScene.triangleList[0].PointInTriangle(point) || crimeScene.triangleList[1].PointInTriangle(point))
                    {
                        // Group similar points into buckets based on sensitivity. 
                        float roundedY = Mathf.Round(point.y / SENSITIVITY) * SENSITIVITY;

                        if (!_pointDic.ContainsKey(roundedY))
                        {
                            _pointDic.Add(roundedY, new List<V3>());
                        }

                        var v = new V3(point);                   
                        if (!_pointDic[roundedY].Contains(v))
                        {
                            _pointDic[roundedY].Add(v);
                        }

                    }
                }
            }
        }

        stopwatch.Stop();

        // Write result.
        Debug.Log(string.Format("Time elapsed for PointCloudPointsICameraView: {0}", stopwatch.Elapsed));;
    }

    

    public void UpdateState()
    {

        if (m_ping)
        {
            if (crimeScene.m_AdvicePlaceHolderList.Count <= 0) return;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing.
            stopwatch.Start();

            m_ping = false;

            PointCloudPointsInCameraView();

            Debug.Log(string.Format("dic: {0}", _pointDic.Count));

            foreach (var key in _pointDic.Keys)
            {

                var pointListCopy = _pointDic[key].ToList();

                Debug.Log(string.Format("Time elapsed for pointListCopy: {0}", stopwatch.Elapsed));

                //Debug.Log(string.Format("Wir sind in der Y-Koordinate: {0} mit {1} points", key, pointListCopy.Count));

                /*while (pointListCopy.Count > 10)
                {*/
                KDTree<V3> pTree = CreateVector2KDTree(pointListCopy);

                Debug.Log(string.Format("Time elapsed for KDTree: {0}", stopwatch.Elapsed));

                int pos = Random.Range(0, pointListCopy.Count - 1);
                var pIter = pTree.NearestNeighbors(new double[] {pointListCopy[pos].x, pointListCopy[pos].z}, pointListCopy.Count, 0.15f);

                Debug.Log(string.Format("Time elapsed for NearestNeighbors: {0}", stopwatch.Elapsed));

                int counter = 0;
                int numOnLine = 0;
                bool _checked = new Boolean();
                _checked = false;

                Vector3 average = CalcAverageOfArea(pIter, ref counter, ref numOnLine, pointListCopy, ref _checked);

                Debug.Log(string.Format("Time elapsed for CalcAverageOfArea: {0}", stopwatch.Elapsed));

                //Debug.Log("Points im Radius: " + counter);
                //Debug.Log(string.Format("Davon in einer Linie {0} von {1} / {2}%", numOnLine, counter, numOnLine*1.0f/counter*100.0f));

                if (counter < 10 || numOnLine*1.0f/counter*100.0f >= 80) continue;

                bool outOfReach = !InReachToOutherAdvices(average);

                Debug.Log(string.Format("Time elapsed for InReachToOutherAdvices: {0}", stopwatch.Elapsed));

                //Debug.Log(string.Format("Stelle gefunden bei: {0} und hat genügend Abstand: {1}", average, outOfReach));

                if (!outOfReach) continue;

                _checked = true;

                Color color = Random.ColorHSV();

                GameObject advice = AddCube("advice_" + average.x + "/" + average.y + "/" + average.z, average, color);

                _advicesList.Add(advice);
               /* }*/
            }

            stopwatch.Stop();

            // Write result.
            Debug.Log(string.Format("Time elapsed for AdaptAdvicePlaceHolders: {0}", stopwatch.Elapsed));
        }

        if (m_show)
        {
            ShowAllInactiveCubes();
            m_show = false;
        }

    }

    private Vector3 CalcAverageOfArea(NearestNeighbour<V3> pIter, ref int counter, ref int numOnLine, List<V3> pointList, ref bool _checked )
    {
        Vector3 sum = Vector3.zero;
        Vector3 a = Vector3.zero;
        Vector3 b = Vector3.zero;

        while (pIter.MoveNext())
        {

            var v3 = pIter.Current;
            v3.Checked = _checked;
            var point = v3.vec3;

            sum += point;

            counter++;

            if (a == Vector3.zero) a = point;
            else if (b == Vector3.zero)
            {
                b = point;
                numOnLine++;
            }
            else if (PointInLine2D(a, b, point))
            {
                numOnLine++;
            }

            pointList.Remove(new V3(point));
        }

        return sum/counter;
    }

    private KDTree<V3> CreateVector2KDTree(List<V3> pointList)
    {

        KDTree<V3> KDTree = new KDTree<V3>(2);

        foreach (var point in pointList)
        {

            //Debug.Log(string.Format("This point is checked", point.Checked));

            /*if (!point.Checked)
            {*/
                //pTree.AddPoint(new double[] { x, y }, new EllipseWrapper(x, y));
                KDTree.AddPoint(new double[] {point.x, point.z}, point);
            //}
        }

        return KDTree;
    }

    private bool InReachToOutherAdvices(Vector3 point, float minDist = 0.5f)
    {
        bool inReach = false;
        foreach (var oldAdvice in _advicesList)
        {
            float dist = Vector3.Distance(oldAdvice.transform.position, point);
            if (dist <= minDist)
            {
                inReach = true;
            }
        }

        return inReach;
    }

    private static bool PointInLine2D(Vector3 p, Vector3 a, Vector3 b, float t = 0.001f)
    {
        float zero = (b.x - a.x)*(p.z - a.z) - (p.x - a.x)*(b.z - a.z);
        return Math.Abs(zero) < t;
    }

    private void ShowAllInactiveCubes()
    {

    }

    public void OnGUIState()
    {
        GUI.color = Color.white;

        if (!m_ping)
        {
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Ping</size>")) m_ping = true;
        }

        if (!m_show)
        {
            if (GUI.Button(new Rect(Screen.width - 220, Screen.height - 100, 200, 80), "<size=30>Show</size>")) m_show = true;
        }

        GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), string.Format("<size=30> {0} / {1} Aadvices added!</size>", _advicesList.Count, crimeScene.m_numberAdvices));
    }

    private GameObject AddCube(string name, Vector3 position, Color color)
    {
        // copy of the maker
        GameObject cubeCopy = Object.Instantiate<GameObject>(crimeScene.m_advice);

        cubeCopy.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        cubeCopy.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);
        // Place the marker at the center of the screen at the found floor height.

        // adding a Colider for ping state
        BoxCollider bc = (BoxCollider)cubeCopy.gameObject.AddComponent(typeof(BoxCollider));
        bc.center = Vector3.zero;

        cubeCopy.transform.position = position;

        cubeCopy.GetComponent<Renderer>().material.color = color;

        cubeCopy.SetActive(true);

        Debug.Log(string.Format("Cube {0} set on {1}", cubeCopy.name, cubeCopy.transform.position));

        return cubeCopy;
    }
}
