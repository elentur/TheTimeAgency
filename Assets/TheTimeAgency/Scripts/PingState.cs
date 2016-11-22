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
    private const float SENSITIVITY = 0.01f;

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
        throw new NotImplementedException();
    }

    public void SetUp()
    {
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

        Debug.Log(crimeScene.triangleList[0].ToString());
        Debug.Log(crimeScene.triangleList[1].ToString());

        foreach (Vector3 point in crimeScene.m_pointCloud.m_points)
        {
            if (point.x <= maxX && point.x >= minX && point.z <= maxZ && point.z >= minZ)
            {
                if (crimeScene.triangleList[0].PointInTriangle(point) || crimeScene.triangleList[1].PointInTriangle(point))
                {
                    points.Add(point);    
                }
            }
        }

        //m_points = points.Distinct(new Comparer()).ToArray();

        m_points = points.ToArray();
    }

    public void UpdateState()
    {

        if (crimeScene.m_cubeList.Count <= 0) return;

        if (m_ping)
        {
            m_ping = false;

            SetUp();


            Debug.Log(string.Format("crimeScene {0}", crimeScene.m_cubeList.Count));
            Debug.Log(string.Format("m_points {0}", m_points.Length));

            for (var i = crimeScene.m_cubeList.Count -1; i >= 0; i--)
            {
                GameObject cube = crimeScene.m_cubeList[i];
                cube.SetActive(true);

                float y = cube.transform.position.y;

                foreach (Vector3 p in m_points)
                {    
                    Collider c = cube.GetComponent<Collider>();

                    if (c.bounds.Contains(new Vector3(p.x, cube.transform.position.y, p.z)))
                    {
                        if (p.y > y)
                        {
                            y = p.y;
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
