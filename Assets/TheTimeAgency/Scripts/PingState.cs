using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Tango;
using Object = UnityEngine.Object;

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


    private bool _setuped = false;

    private List<Vector3> m_pointList;



    private bool m_ping = false;

    public PingState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
    }

    bool InfiniteCameraCanSeePoint( Vector3 point)
    {    
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(point);
        return (viewportPoint.z >0 && (new Rect(0, 0, 1, 1)).Contains(viewportPoint ) && viewportPoint.z <Camera.main.farClipPlane);
    }
    public void SetUp()
    {
        Vector2 a = new Vector2(0.0f, 0.0f);
        Vector2 b = new Vector2(5.0f, 0.0f);
        Vector2 c = new Vector2(0.0f, 5.0f);
        Vector2 d = new Vector2(5.0f, 5.0f);
        Debug.Log("Intersection: " + intersection(d,a,d,c));
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

    private bool intersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        //TODO Verbinde punkt 4 mit jedem der 3 anderen und prüfe die enstandene Gerade 
        //mit allen geraden des dreicks, nim die ersten beiden treffer
        float A1 = p1.y - p2.y;
        float B1 = p2.x - p1.x;
        float C1 = A1 * p2.x + B1 * p2.y;

        // Get A,B,C of second line - points : ps2 to pe2
        float A2 = q1.y - q2.y;
        float B2 = q2.x - q1.x;
        float C2 = A2 * q2.x + B2 * q2.y;
      

        // Get delta and check if the lines are parallel
        float delta = A1 * B2 - A2 * B1;
        if (delta == 0)
            return false;

        // now return the Vector2 intersection point
        Vector2 intersect =  new Vector2(
            (B2 * C1 - B1 * C2) / delta,
            (A1 * C2 - A2 * C1) / delta
        );

        Debug.Log("intersectionPoint: " + intersect);

        if (intersect == p1 || intersect == p2 || intersect == q1 || intersect == q2) return false;

        if (Vector2.Distance(p1, intersect) + Vector2.Distance(p2, intersect) == Vector2.Distance(p1, p2) ||
            Vector2.Distance(q2, intersect) + Vector2.Distance(q1, intersect) == Vector2.Distance(q2, q1))
            return true; // C is on the line.
        return false;


    }

    public void UpdateState()
    {

       
        if (m_ping)
        {
            if (crimeScene.m_cubeList.Count <= 0) return;
            m_ping = false;

            SetUp();


            Debug.Log(string.Format("crimeScene {0}", crimeScene.m_cubeList.Count));
            Debug.Log(string.Format("m_points {0}", m_points.Length));
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing.
            stopwatch.Start();
            for (var i = crimeScene.m_cubeList.Count -1; i >= 0; i--)
            {
                GameObject cube = crimeScene.m_cubeList[i];
                cube.SetActive(true);

                float y = cube.transform.position.y;

        
                //foreach (Vector3 p in m_points)
                foreach(Vector3 p in m_points)
                {
         
 
                    Collider c = cube.GetComponent<Collider>();

                    if (c.bounds.Contains(new Vector3(p.x, cube.transform.position.y, p.z)))
                    {
                        if (p.y > y)
                        {
                            y = p.y;
                            break;
                        }
                    }
                }

                var target = cube.transform.position;
                float cubeY = target.y;
                target.y = y;
                cube.transform.position = target;

                // do we found a higher y than we can remove the cube from list
                if (y > cubeY)
                {
                    crimeScene.m_cubeList.RemoveAt(i);
                }
                

            }
            stopwatch.Stop();

            // Write result.
            Debug.Log(string.Format("Time elapsed for Loop: {0}", stopwatch.Elapsed));
            //crimeScene.m_cubeList.Clear();
        }
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
}
