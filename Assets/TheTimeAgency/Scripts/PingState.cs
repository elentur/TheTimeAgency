using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly SortedDictionary<float, List<Vector3>> _pointDic;

        private Camera cam;

        private readonly List<Camera> camList;

        private List<KeyValuePair<string, TimeSpan>> debugTime;

        public PingState(CrimeScene crimeScenePattern)
        {
            _crimeScene = crimeScenePattern;
            _advicesList = new List<GameObject>();
            _pointDic = new SortedDictionary<float, List<Vector3>>();
            cam = Camera.main;
            debugTime = new List<KeyValuePair<string, TimeSpan>>();
            camList = new List<Camera>();
        }

        public void StartState()
        {
            
        }

        private bool InfiniteCameraCanSeePoint(Vector3 point)
        {

            foreach (var myCam in camList)
            {
               Vector3 myYiewportPoint = myCam.WorldToViewportPoint(point);
               if((myYiewportPoint.z > 0 && (new Rect(0, 0, 1, 1)).Contains(myYiewportPoint) && myYiewportPoint.z < myCam.farClipPlane))
               {
                    return false;
               }
            }

            Vector3 viewportPoint = cam.WorldToViewportPoint(point);
            return (viewportPoint.z > 0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint) && viewportPoint.z < cam.farClipPlane);
        }

        private void PointCloudPointsInCameraView()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing.
            stopwatch.Start();

            foreach (Vector3 point in _crimeScene.m_pointCloud.m_points)
            {
                if (point.y <= 1.0f + _crimeScene.m_floorPoint.y)
                {
                    if (_crimeScene.triangleList[0].PointInTriangle(point) || _crimeScene.triangleList[1].PointInTriangle(point))
                    {

                        if (InfiniteCameraCanSeePoint(point))
                        {
                       
                            // Group similar points into buckets based on sensitivity. 
                            float roundedY = Mathf.Round(point.y / SENSITIVITY) * SENSITIVITY;

                            if (!_pointDic.ContainsKey(roundedY))
                            {
                                _pointDic.Add(roundedY, new List<Vector3>());
                            }

                            if (!_pointDic[roundedY].Contains(point))
                            {
                                _pointDic[roundedY].Add(point);
                            }
                        }
                    }
                }
            }

            if (!camList.Contains(cam))
            {
                var cameraGameObject = new GameObject(cam.name);
                var copyCam = cameraGameObject.AddComponent<Camera>();
                copyCam.enabled = false;
                copyCam.CopyFrom(cam);

                camList.Add(copyCam);
            }

            Debug.Log(string.Format("point dic : {0}", _pointDic.Count));

            stopwatch.Stop();

            // Write result.
            debugTime.Add(new KeyValuePair<string, TimeSpan>("PointCloudPointsICameraView", stopwatch.Elapsed));
            //Debug.Log(string.Format("Time elapsed for PointCloudPointsICameraView: {0}", stopwatch.Elapsed)); ;
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
   
                foreach (var dic in _pointDic.Reverse())
                {

                    var y = dic.Key;

                    Debug.Log("my y: " + y);

                    var pointList = _pointDic[y];

                    int oldCount = Int32.MaxValue;

                    while (pointList.Count > 0 && pointList.Count != oldCount)
                    {
                        oldCount = pointList.Count;

                        KDTree<Vector3> pTree = CreateVector2KDTree(pointList);

                        var pIter = pTree.NearestNeighbors(new double[] {pointList[0].x, pointList[0].z},
                            pointList.Count, DISTANCE);

                        int counter = 0;
                        int numOnLine = 0;

                        Vector3 average = CalcAverageOfArea(pIter, ref counter, ref numOnLine, ref pointList);

                        if (counter < 100 || numOnLine*1.0f/counter*100.0f >= 80) continue;

                        bool outOfReach = !InReachToOutherAdvices(average, DISTANCE);

                        if (!outOfReach) continue;

                        Color color = Random.ColorHSV();

                        GameObject advice = AddCube("advice_" + average.x + "/" + average.y + "/" + average.z, average,
                            color, new Vector3(10, 10, 10));

                        _advicesList.Add(advice);
                    }
                }

                _pointDic.Clear();

                stopwatch.Stop();

                // Write result.
                debugTime.Add(new KeyValuePair<string, TimeSpan>("AdaptAdvicePlaceHolders", stopwatch.Elapsed));
                //Debug.Log(string.Format("Time elapsed for AdaptAdvicePlaceHolders: {0}", stopwatch.Elapsed));

               // ShowDebugTime();
            }

            if (m_show)
            {
                ShowAllInactiveCubes();
                m_show = false;
            }
        }

        private void ShowDebugTime()
        {

            foreach (var time in debugTime)
            {
                Debug.Log(string.Format("Time elapsed for {0}: {1}",time.Key, time.Value));
            }

        }

        private Vector3 CalcAverageOfArea(NearestNeighbour<Vector3> pIter, ref int counter, ref int numOnLine, ref List<Vector3> pointList)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing.
            stopwatch.Start();

            Vector3 sum = Vector3.zero;
            Vector3 a = Vector3.zero;
            Vector3 b = Vector3.zero;

            while (pIter.MoveNext())
            {
                var point = pIter.Current;

                pointList.Remove(point);

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

            stopwatch.Stop();

            // Write result.
            debugTime.Add(new KeyValuePair<string, TimeSpan>("CalcAverageOfArea", stopwatch.Elapsed));
            //Debug.Log(string.Format("Time elapsed for CalcAverageOfArea: {0}", stopwatch.Elapsed));

            return sum / counter;
        }

        private KDTree<Vector3> CreateVector2KDTree(List<Vector3> pointList)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing.
            stopwatch.Start();

            var kdTree = new KDTree<Vector3>(2);

            foreach (var point in pointList)
            {
                kdTree.AddPoint(new double[] {point.x, point.z}, point);
            }

            stopwatch.Stop();

            // Write result.
            debugTime.Add(new KeyValuePair<string, TimeSpan>("CreateVector2KDTree", stopwatch.Elapsed));
            // Debug.Log(string.Format("Time elapsed for CreateVector2KDTree: {0}", stopwatch.Elapsed));

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
