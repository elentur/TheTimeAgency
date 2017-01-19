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

        public bool Ping = false;

        private readonly List<GameObject> _advicesList;

        private readonly Camera _cam;

        private GameObject _pingBox;

        private List<Vector3> randmPositions;
        private bool[,] checker;
        private float maxX;
        private float minX;
        private float maxZ;
        private float minZ;

        public PingState(CrimeScene crimeScenePattern)
        {
            _crimeScene = crimeScenePattern;
            _cam = Camera.main;
        }

        public void StartState()
        {
            List<Vector3> Vertices = new List<Vector3>();

            Vertices = _crimeScene.triangleList[0].GetVertices().ToList()
                .Concat(_crimeScene.triangleList[1].GetVertices().ToList()).ToList();

             maxX = Math.Max(Vertices[0].x, Math.Max(Vertices[1].x, Math.Max(Vertices[2].x, Vertices[3].x)));
             minX = Math.Min(Vertices[0].x, Math.Min(Vertices[1].x, Math.Min(Vertices[2].x, Vertices[3].x)));

             maxZ = Math.Max(Vertices[0].z, Math.Max(Vertices[1].z, Math.Max(Vertices[2].z, Vertices[3].z)));
             minZ = Math.Min(Vertices[0].z, Math.Min(Vertices[1].z, Math.Min(Vertices[2].z, Vertices[3].z)));

            randmPositions = new List<Vector3>();
            _pingBox = new GameObject("pingBox");
            _pingBox.transform.SetParent(_crimeScene.m_AdvicePlaceHolder.transform.parent.gameObject.transform, false);
            SetRandomAdvices();
            GameObject barrierInterface = GameObject.Find("MainInterface");
            if (barrierInterface != null)
            {
                _crimeScene.m_markerCanvas.GetComponent<PanelManager>().OpenPanel(barrierInterface.GetComponent<Animator>());
            }
            
        }

        private void SetRandomAdvices()
        {
           
            Game game = Game.getInstance();
            int numItems = game.getItems().Count();
            int minSize = (int)Math.Sqrt(numItems)+1;
            float minDistance = 0.15f;
            int w = (int)(Math.Abs(maxX - minX)/minDistance);
            if (w < minSize) w = minSize;
            int h = (int)(Math.Abs(maxZ - minZ) / minDistance);
            if (h < minSize) h = minSize;
            checker = new bool[w,h];
            for (var i=0; i < numItems; i++)
            {
                Vector3 rndPos = GetRandomPostion();
                if (rndPos == Vector3.zero) Debug.Log("Fehler");
                randmPositions.Add(rndPos);
            }
        }

        private Vector3 GetRandomPostion()
        {
            List<int> wRand = Enumerable.Range(0, checker.GetLength(0)).ToList();
            List<int> hRand = Enumerable.Range(0, checker.GetLength(1)).ToList();
            var rnd = new System.Random();
            wRand = wRand.OrderBy(item => rnd.Next()).ToList();
            hRand = hRand.OrderBy(item => rnd.Next()).ToList();

            Vector3 randomVector = Vector3.zero;
            for (int x = 0;x < wRand.Count; x++)
            {
                for (int y = 0; y < hRand.Count; y++)
                {
                    int w =wRand[x];
                    int h = hRand[y];
                    if (!checker[w, h])
                    {
                        checker[w,h] = true;

                        float wAdd = Math.Abs(maxX - minX) / checker.GetLength(0);
                        float hAdd = Math.Abs(maxZ - minZ) / checker.GetLength(1);
                        randomVector = new Vector3(
                   UnityEngine.Random.Range(minX+ wAdd*w,minX+ wAdd *(w+1)),
                    _crimeScene.m_floorPoint.y,
                   UnityEngine.Random.Range(minZ + hAdd *h, minZ + hAdd * (h + 1)));
                        

                        if ( (_crimeScene.triangleList[0].PointInTriangle(randomVector) || _crimeScene.triangleList[1].PointInTriangle(randomVector)))
                        {
                            Debug.Log(randomVector);
                            return randomVector;
                        }

                    }

                }
            }


            return randomVector;
        }

        private bool vec3ToClose(Vector3 target)
        {
            bool toClose = false;

            foreach (Vector3 position in randmPositions)
            {
                float distSqr = Vector3.SqrMagnitude(
                    new Vector2(position.x,position.z) - new Vector2(target.x, target.z)
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

             
        }

        public List<Vector3> ping()
        { 

                List<Vector3> pointList = GenerateCrimeScenePointList();

                KDTree<Vector3> pTree = CreateVector2KDTree(pointList);

                return SetAdvices(pTree, pointList);
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

        private List<Vector3> SetAdvices(KDTree<Vector3> pTree, List<Vector3> pointList)
        {

            List<Vector3> foundPositions = new List<Vector3>();

            foreach (Vector3 position in randmPositions.Reverse<Vector3>())
            {
                if (InfiniteCameraCanSeePoint(position))
                {
                    var pIter = pTree.NearestNeighbors(new double[] { position.x,position.z }, pointList.Count, DISTANCE);

                    var counter = 1;
                    var sum = Vector3.zero;
                    var y = float.MinValue;

                    while (pIter.MoveNext())
                    {
                        var point = pIter.Current;
                        if (point != Vector3.zero)
                        {
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
                    randmPositions.Remove(position);
                    foundPositions.Add(sum / counter);
                }
            }

            return foundPositions;
        }

        private KDTree<Vector3> CreateVector2KDTree(List<Vector3> pointList)
        {
            var kdTree = new KDTree<Vector3>(2);
            foreach (var point in pointList)
            {
                if (point != Vector3.zero)
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
                    // TODO delte placeholder if we find a advice from scene!
                }
            }
        }

        public void OnGUIState()
        {   

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
            _crimeScene.currentState = _crimeScene.defaultState;
            _crimeScene.currentState.StartState();
        }
    }
}
