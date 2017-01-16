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

        public bool m_ping = false, m_show = false, m_reset = false;

        private readonly List<GameObject> _advicesList;

        private readonly Camera _cam;

        private GameObject _pingBox;

        public PingState(CrimeScene crimeScenePattern)
        {
            _crimeScene = crimeScenePattern;
            _advicesList = new List<GameObject>();
            _cam = Camera.main;
        }

        public void StartState()
        {
            _pingBox = new GameObject("pingBox");
            _pingBox.transform.SetParent(_crimeScene.m_AdvicePlaceHolder.transform.parent.gameObject.transform, false);
            SetRandomAdvices();

            _crimeScene.m_pingCanvas.SetActive(true);
        }

        private void SetRandomAdvices()
        {

            List<Vector3> Vertices = new List<Vector3>();

            Vertices = _crimeScene.triangleList[0].GetVertices().ToList()
                .Concat(_crimeScene.triangleList[1].GetVertices().ToList()).ToList();

            var maxX = Math.Max(Vertices[0].x, Math.Max(Vertices[1].x, Math.Max(Vertices[2].x, Vertices[3].x)));
            var minX = Math.Min(Vertices[0].x, Math.Min(Vertices[1].x, Math.Min(Vertices[2].x, Vertices[3].x)));

            var maxZ = Math.Max(Vertices[0].z, Math.Max(Vertices[1].z, Math.Max(Vertices[2].z, Vertices[3].z)));
            var minZ = Math.Min(Vertices[0].z, Math.Min(Vertices[1].z, Math.Min(Vertices[2].z, Vertices[3].z)));

            Color color = Color.red;

            int counter = 0;

            while (_advicesList.Count() < _crimeScene.m_numberAdvices)
            {
                // TODO warum läuft diese Schleife ins leere ohne counter???????
                if (counter > 10000) break;

                counter++;

                Vector3 average = new Vector3(
                    UnityEngine.Random.Range(minX, maxX),
                     _crimeScene.m_floorPoint.y,
                    UnityEngine.Random.Range(minZ, maxZ));

                if (!vec3ToClose(average) && (_crimeScene.triangleList[0].PointInTriangle(average) || _crimeScene.triangleList[1].PointInTriangle(average)))
                {
                    GameObject advice = AddCube("advice_" + average.x + "/" + average.y + "/" + average.z, average, color, new Vector3(10, 10, 10));
                    advice.SetActive(false);
                    _advicesList.Add(advice);
                }
            }
        }

        private bool vec3ToClose(Vector3 target)
        {
            bool toClose = false;

            foreach (GameObject advices in _advicesList)
            {
                float distSqr = Vector3.SqrMagnitude(
                    new Vector2(advices.transform.position.x, advices.transform.position.z) - new Vector2(target.x, target.z)
                );

                if (distSqr < _crimeScene.m_distanceAdvices * _crimeScene.m_distanceAdvices)
                {
                    toClose = true;
                    break;
                }
            }

            return toClose;
        }

        private bool InfiniteCameraCanSeePoint(Vector3 point)
        {
            Vector3 viewportPoint = _cam.WorldToViewportPoint(point);
            return (viewportPoint.z > 0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint) && viewportPoint.z < _cam.farClipPlane);
        }

        public void UpdateState()
        {

            if (m_show)
            {
                m_show = false;
                ShowAllInactiveCubes(); 
            }
            else if (m_reset)
            {

                m_reset = false;
                _advicesList.Clear();
                foreach (Transform child in _pingBox.transform)
                {
                    Object.Destroy(child.gameObject);
                }

                SetRandomAdvices();

                
            }
            else if (m_ping)
            {
                m_ping = false;

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
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

                    var counter = 1;
                    var sum = Vector3.zero;
                    var y = float.MinValue;

                    while (pIter.MoveNext())
                    {
                        if (pIter.Current != Vector3.zero)
                        {
                            var point = pIter.Current;

                            // get the higthest y onley one time
                            if (Math.Abs(y - float.MinValue) < 0.01) y = point.y;
                            // do we are in the same higth
                            if (Math.Abs(y - point.y) <= SENSITIVITY)
                            {
                                sum += point;
                                counter++;
                            }

                        }
                    }

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
                if(point != Vector3.zero)
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

            if (_advicesList.Count(n => n.activeSelf) >= _crimeScene.m_numberAdvices)
            {
                ToDefaultState();
            }

            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), string.Format("<size=30> {0} / {1} Aadvices added!</size>", _advicesList.Count(n => n.activeSelf), _advicesList.Count()));
        }

        private GameObject AddCube(string name, Vector3 position, Color color, Vector3 scale)
        {
            // copy of the maker
            GameObject cubeCopy = Object.Instantiate<GameObject>(_crimeScene.m_advice);

            cubeCopy.name = name;

            //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
            //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
            cubeCopy.transform.SetParent(_pingBox.transform, false);
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

        private void ToDefaultState()
        {
            _crimeScene.m_pingCanvas.SetActive(false);
            _crimeScene.currentState = _crimeScene.defaultState;
            _crimeScene.currentState.StartState();
        }
    }
}
