using System;
using System.Collections.Generic;
using System.Linq;
using Assets.TheTimeAgency.Scripts.KDTree;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.TheTimeAgency.Scripts
{
    public class PingState : ICrimeSceneState
    {
        private readonly CrimeScene _crimeScene;

        /// <summary>
        /// The interval in meters between buckets of points. For example, a high sensitivity of 0.01 will group 
        /// points into buckets every 1cm.
        /// </summary>
        private const float SENSITIVITY = 0.02f;

        private const float DISTANCE = 0.1f;

        private bool m_ping = false, m_show = false, m_reset = false;

        private readonly List<GameObject> _advicesList;

        private readonly SortedDictionary<float, List<Vector3>> _pointDic;

        private Camera cam;

        private GameObject pingBox;



        public PingState(CrimeScene crimeScenePattern)
        {
            _crimeScene = crimeScenePattern;
            _advicesList = new List<GameObject>();
            _pointDic = new SortedDictionary<float, List<Vector3>>();
            cam = Camera.main;
        }

        public void StartState()
        {
            pingBox = new GameObject("pingBox");
            pingBox.transform.SetParent(_crimeScene.m_AdvicePlaceHolder.transform.parent.gameObject.transform, false);

            SetDefaultAdvices();
        }

        private void SetDefaultAdvices()
        {
            Color color = Color.red;

            foreach (var vec in _crimeScene.m_defaultAdvices)
            {
                GameObject advice = AddCube("advice_" + vec.x + "/" + vec.y + "/" + vec.z, vec, color, new Vector3(10, 10, 10));
                _advicesList.Add(advice);
            }
        }

        private bool InfiniteCameraCanSeePoint(Vector3 point)
        {
            Vector3 viewportPoint = cam.WorldToViewportPoint(point);
            return (viewportPoint.z > 0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint) && viewportPoint.z < cam.farClipPlane);
        }

        public void UpdateState()
        {

            if (m_show)
            {
                ShowAllInactiveCubes();
                m_show = false;
            }

            if (m_reset)
            {
                _pointDic.Clear();
                _advicesList.Clear();
                foreach (Transform child in pingBox.transform)
                {
                    Object.Destroy(child.gameObject);
                }

                m_reset = false;
            }

            if (m_ping)
            {
                m_ping = false;

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

                // Begin timing.
                stopwatch.Start();

                List<Vector3> pointList = GenerateCrimeScenePointList();

                KDTree<Vector3> pTree = CreateVector2KDTree(pointList);

                SetAdvices(pTree,pointList);

                stopwatch.Stop();
                Debug.Log(string.Format("Time: {0}", stopwatch.Elapsed));
            }
        }

        private List<Vector3> GenerateCrimeScenePointList()
        {
            List<Vector3> pointList = new List<Vector3>();

            foreach (var point in _crimeScene.m_pointCloud.m_points)
            {
                if ((_crimeScene.triangleList[0].PointInTriangle(point) || _crimeScene.triangleList[1].PointInTriangle(point)) && InfiniteCameraCanSeePoint(point))
                {
                    pointList.Add(point);
                }
            }

            return pointList;
        }

        private void SetAdvices(KDTree<Vector3> pTree, List<Vector3> pointList)
        {
            foreach (var advice in _advicesList)
            {


                if (!advice.activeSelf && InfiniteCameraCanSeePoint(advice.transform.position))
                {
                    var pIter = pTree.NearestNeighbors(new double[] { advice.transform.position.x, advice.transform.position.z }, pointList.Count, DISTANCE);

                    var counter = 0;
                    var sum = Vector3.zero;
                    var y = float.MinValue;

                    while (pIter.MoveNext())
                    {
                        if (pIter.Current != Vector3.zero)
                        {
                            var point = pIter.Current;
                            if (Math.Abs(y - float.MinValue) < 0.01) y = point.y;

                            if (Math.Abs(y - point.y) <= SENSITIVITY)
                            {
                                sum += point;
                                counter++;
                            }

                        }
                    }

                    AndroidHelper.ShowAndroidToastMessage(string.Format("Neue Koordinate {0}", pIter.Current), AndroidHelper.ToastLength.SHORT);

                    advice.transform.position = sum / counter;
                    advice.SetActive(true);

                }
            }
        }

        private KDTree<Vector3> CreateVector2KDTree(List<Vector3> pointList)
        {
            var kdTree = new KDTree<Vector3>(2); 
            foreach (var point in pointList)
            {
                kdTree.AddPoint(new double[] { point.x, point.z }, point);
            }
            return kdTree;
        }

        private void ShowAllInactiveCubes()
        {

            Color color = Color.gray;

            foreach (var advice in _advicesList)
            {
                if (!advice.activeSelf)
                {
                    var vec = advice.transform.position;
                    GameObject placeholder = AddCube("advice_" + vec.x + "/" + vec.y + "/" + vec.z, vec, color, new Vector3(10, 10, 10));
                    placeholder.SetActive(true);
                }
            }
        }

        public void OnGUIState()
        {
            GUI.color = Color.white;

            if (!m_reset)
            {
                if (GUI.Button(new Rect(Screen.width - 220, 100, 200, 80), "<size=30>Reset</size>")) m_reset = true;
            }

            if (!m_ping)
            {
                if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Ping</size>")) m_ping = true;
            }

            if (!m_show)
            {
                if (GUI.Button(new Rect(Screen.width - 220, Screen.height - 100, 200, 80), "<size=30>Show</size>")) m_show = true;
            }
 
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), string.Format("<size=30> {0} / {1} Aadvices added!</size>", _advicesList.Count(n => n.activeSelf), _crimeScene.m_numberAdvices));
        }

        private GameObject AddCube(string name, Vector3 position, Color color, Vector3 scale)
        {
            // copy of the maker
            GameObject cubeCopy = Object.Instantiate<GameObject>(_crimeScene.m_advice);

            cubeCopy.name = name;

            //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
            //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
            cubeCopy.transform.SetParent(pingBox.transform, false);
            // Place the marker at the center of the screen at the found floor height.

            // adding a Colider for ping state
            BoxCollider bc = (BoxCollider)cubeCopy.gameObject.AddComponent(typeof(BoxCollider));
            bc.center = Vector3.zero;

            cubeCopy.transform.position = position;

            cubeCopy.transform.localScale = scale;

            cubeCopy.GetComponent<Renderer>().material.color = color;

            cubeCopy.SetActive(false);

            // Debug.Log(string.Format("Cube {0} set on {1}", cubeCopy.name, cubeCopy.transform.position));

            return cubeCopy;
        }
    }
}
