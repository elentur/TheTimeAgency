using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Tango;
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

    /// <summary>
    /// The points of the point cloud, in world space.
    /// 
    /// Note that not every member of this array will be filled out. See
    /// m_pointsCount.
    /// </summary>
    [HideInInspector]
    public Vector3[] m_points;

    private bool m_ping;

    private bool m_show;

    private readonly List<GameObject> _advicesList;

    private readonly List<GameObject> _founded;

    public PingState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
        _advicesList = new List<GameObject>();
        _founded = new List<GameObject>();
    }

    public void StartState()
    {
    }

    bool InfiniteCameraCanSeePoint( Vector3 point)
    {    
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return (viewportPoint.z >0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint ) && viewportPoint.z <Camera.main.farClipPlane);
    }

    private void FilterPointCloudPoints()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        var xArray = new float[crimeScene.markerList.Count];
        var zArray = new float[crimeScene.markerList.Count];

        for (var i = 0; i < crimeScene.markerList.Count; i++)
        {
            xArray[i] = crimeScene.markerList[i].transform.position.x;
            zArray[i] = crimeScene.markerList[i].transform.position.z;
        }

        var maxX = xArray.Max();
        var minX = xArray.Min();
        var maxZ = zArray.Max();
        var minZ = zArray.Min();

        List<Vector3> points = new List<Vector3>();
       
        foreach (Vector3 point in crimeScene.m_pointCloud.m_points)
        {
            if (point.x <= maxX && point.x >= minX && point.z <= maxZ && point.z >= minZ)
            {
                if (InfiniteCameraCanSeePoint(point))
                {
                    if (crimeScene.triangleList[0].PointInTriangle(point) || crimeScene.triangleList[1].PointInTriangle(point))
                    {
                        points.Add(point);
                    }
                }
            }
        }

        m_points = points.Distinct(new Comparer()).OrderByDescending(o=>o.y).ToArray();

        stopwatch.Stop();

        // Write result.
        Debug.Log(string.Format("Time elapsed for SetUp: {0}", stopwatch.Elapsed));
    }

    private void AdaptAdvicePlaceHolders()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        for (var i = crimeScene.m_AdvicePlaceHolderList.Count - 1; i >= 0; i--)
        {
            GameObject cube = crimeScene.m_AdvicePlaceHolderList[i];

            var script = cube.GetComponent<AdvicePlaceHolder>();

            if (script.Adapted) continue;

            float y = cube.transform.position.y;

            Collider c = cube.GetComponent<Collider>();
            cube.SetActive(true);

            foreach (Vector3 p in m_points)
            {
                if (c.bounds.Contains(new Vector3(p.x, cube.transform.position.y, p.z)))
                {
                    script.Adapted = true;
                    if (!(p.y > y)) continue;

                    y = p.y;

                    script.Heigth = y;
                    break;
                }
            }

            if (!script.Adapted) continue;

            var target = cube.transform.position;
            target.y = y;
            cube.transform.position = target;

            if (!_founded.Contains(cube))  _founded.Add(cube);
            
        }

        stopwatch.Stop();

        // Write result.
        Debug.Log(string.Format("Time elapsed for Loop: {0}", stopwatch.Elapsed));
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

            FilterPointCloudPoints();

            AdaptAdvicePlaceHolders();

            SetAdvices();
        }

        if (m_show)
        {
            ShowAllInactiveCubes();
            m_show = false;
        }

    }

    private void ShowAllInactiveCubes()
    {
        foreach (var placeholder in crimeScene.m_AdvicePlaceHolderList)
        {

            if (!placeholder.GetComponent<AdvicePlaceHolder>().Checked)
            {
                GameObject advice = AddCube("placeholder_" + placeholder.name);

                advice.transform.position = placeholder.transform.position;

                advice.GetComponent<Renderer>().material.color = Color.green;
               

                advice.SetActive(true);
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
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Ping</size>"))
            {
                if (crimeScene.m_pointCloud == null)
                {
                    Debug.LogError("TangoPointCloud required to find floor.");
                    return;
                }

                m_ping = true;
            }
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50),
                "<size=30>Searching for floor position. Make sure the floor is visible.</size>");
        }

        if (!m_show)
        {
            if (GUI.Button(new Rect(Screen.width - 220, Screen.height - 100, 200, 80), "<size=30>Show</size>"))
            {
                if (crimeScene.m_pointCloud == null)
                {
                    Debug.LogError("TangoPointCloud required to find floor.");
                    return;
                }

                m_show = true;
            }
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50),
                "<size=30>Searching for floor position. Make sure the floor is visible.</size>");
        }

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
        GameObject myCube = Object.Instantiate<GameObject>(crimeScene.m_cube);

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
