using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Assets.TheTimeAgency.Scripts.KDTree;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.TheTimeAgency.Scripts
{
    public class PingState : ICrimeSceneState
    {
        private readonly CrimeScene _crimeScene;

        /// <summary>
        /// The interval in meters between buckets of points. For example, a high sensitivity of 0.01 will group 
        /// points into buckets every 1cm.
        /// </summary>
        private const float SENSITIVITY = 0.002f;

        private const float DISTANCE = 0.5f;

        private bool m_ping, m_show;

        private readonly List<GameObject> _advicesList;

        private float _maxX, _minX, _maxZ, _minZ;

        private double Area;

        private readonly SortedDictionary<float, List<V3>> _pointDic;

        public PingState(CrimeScene crimeScenePattern)
        {
            _crimeScene = crimeScenePattern;
            _advicesList = new List<GameObject>();
            _pointDic = new SortedDictionary<float, List<V3>>();
        }

        public void StartState()
        {
            SetMaxMin();
            Area = _crimeScene.triangleList[0].Sureface() + _crimeScene.triangleList[1].Sureface();
        }

        private void SetMaxMin()
        {
            var xArray = new float[_crimeScene.markerList.Count];
            var zArray = new float[_crimeScene.markerList.Count];

            for (var i = 0; i < _crimeScene.markerList.Count; i++)
            {
                xArray[i] = _crimeScene.markerList[i].transform.position.x;
                zArray[i] = _crimeScene.markerList[i].transform.position.z;
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

            foreach (Vector3 point in _crimeScene.m_pointCloud.m_points)
            {
                if (point.x <= _maxX && point.x >= _minX && point.z <= _maxZ && point.z >= _minZ)
                {
                    if (InfiniteCameraCanSeePoint(point))
                    {
                        if (_crimeScene.triangleList[0].PointInTriangle(point) || _crimeScene.triangleList[1].PointInTriangle(point))
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
            Debug.Log(string.Format("Time elapsed for PointCloudPointsICameraView: {0}", stopwatch.Elapsed)); ;
        }

        public void UpdateState()
        {

            if (m_ping)
            {
                if (_crimeScene.m_AdvicePlaceHolderList.Count <= 0) return;

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

                // Begin timing.
                stopwatch.Start();

                m_ping = false;

                PointCloudPointsInCameraView();

                Debug.Log(string.Format("dic: {0}", _pointDic.Count));
   
                foreach (var y in _pointDic.Keys)
                {
                    var pointList = _pointDic[y];
                    var copyList = new List<V3>();

                    foreach (var point in pointList)
                    {
                        if (!point.Examined)
                        {
                            copyList.Add(point);
                        }
                    }

                    Debug.Log("My Y: " + y);

                    while (copyList.Count > 50)
                    {

                        Debug.Log("copyList length: " + copyList.Count);

                        KDTree<V3> pTree = CreateVector2KDTree(copyList);

                        var pIter = pTree.NearestNeighbors(new double[] { copyList[0].x, copyList[0].z }, copyList.Count, DISTANCE);

                        int counter = 0;
                        int numOnLine = 0;

                        List<V3> temp = new List<V3>();

                        Vector3 average = CalcAverageOfArea(pIter, ref counter, ref numOnLine, copyList, temp);

                        if (counter < 100 || numOnLine * 1.0f / counter * 100.0f >= 80) continue;

                        bool outOfReach = !InReachToOutherAdvices(average, DISTANCE);

                        if (!outOfReach) continue;

                        SetPointsExamined(pointList, temp);

                        Color color = Random.ColorHSV();

                        GameObject advice = AddCube("advice_" + average.x + "/" + average.y + "/" + average.z, average, color, new Vector3(10, 10, 10));

                        _advicesList.Add(advice);
                   }

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

        private void SetPointsExamined(List<V3> pointList, List<V3> temp)
        {
            foreach (var v3 in temp)
            {
                var index = pointList.IndexOf(v3);
                if (index > -1)
                {
                    var point = pointList[index];
                    point.Examined = true;
                    pointList[index] = point;
                }
            }
        }

        private Vector3 CalcAverageOfArea(NearestNeighbour<V3> pIter, ref int counter, ref int numOnLine, List<V3> list, List<V3> temp)
        {
            Vector3 sum = Vector3.zero;
            Vector3 a = Vector3.zero;
            Vector3 b = Vector3.zero;

            while (pIter.MoveNext())
            {
                var v3 = pIter.Current;

                temp.Add(v3);
                list.Remove(v3);

                var point = v3.Vec3;

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
            }
           
            return sum / counter;
        }

        private KDTree<V3> CreateVector2KDTree(List<V3> pointList)
        {
            var kdTree = new KDTree<V3>(2);

            foreach (var point in pointList)
            {
                if (!point.Examined)
                {
                    kdTree.AddPoint(new double[] {point.x, point.z}, point);
                }
            }
            return kdTree;
        }

        private bool InReachToOutherAdvices(Vector3 point, float minDist)
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
            float zero = (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z);
            return Math.Abs(zero) < t;
        }

        private void ShowAllInactiveCubes()
        {

            Color color = Color.red;

            Color colorUsed = Color.yellow;


            foreach (var pointList in _pointDic.Values)
            {

                foreach (var point in pointList)
                {
                    if (!point.Examined)
                    {

                        GameObject advice = AddCube("placeholder" + point.x + "/" + point.y + "/" + point.z, point.Vec3,
                            color, new Vector3(1, 1, 1));
                    }

                    else
                    {
                        GameObject advice = AddCube("placeholder" + point.x + "/" + point.y + "/" + point.z, point.Vec3,
                            colorUsed, new Vector3(1, 1, 1));
                    }
                }
            }
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

            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), string.Format("<size=30> {0} / {1} Aadvices added!</size>", _advicesList.Count, _crimeScene.m_numberAdvices));
        }

        private GameObject AddCube(string name, Vector3 position, Color color, Vector3 scale)
        {
            // copy of the maker
            GameObject cubeCopy = Object.Instantiate<GameObject>(_crimeScene.m_advice);

            cubeCopy.name = name;

            //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
            //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
            cubeCopy.transform.SetParent(_crimeScene.m_marker.transform.parent.gameObject.transform, false);
            // Place the marker at the center of the screen at the found floor height.

            // adding a Colider for ping state
            BoxCollider bc = (BoxCollider)cubeCopy.gameObject.AddComponent(typeof(BoxCollider));
            bc.center = Vector3.zero;

            cubeCopy.transform.position = position;

            cubeCopy.transform.localScale = scale;

            cubeCopy.GetComponent<Renderer>().material.color = color;

            cubeCopy.SetActive(true);

           // Debug.Log(string.Format("Cube {0} set on {1}", cubeCopy.name, cubeCopy.transform.position));

            return cubeCopy;
        }
    }
}
