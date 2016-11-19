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
    private const float SENSITIVITY = 0.1f;

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


    private bool _fertig = false;

    private bool _setuped = false;

    private List<Vector3> m_pointList;

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

        foreach (Vector3 point in crimeScene.m_pointCloud.m_points)
        {

            if (point.x <= maxX && point.x >= minX && point.z <= maxZ && point.z >= minZ)
            {
                points.Add(point);

                GameObject myMarker = Object.Instantiate(crimeScene.m_marker);

                /*
                 * Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation. 
                 * http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
                */
                myMarker.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

                myMarker.transform.position = point;

                //myMarker.transform.localScale = new Vector3(1, 1, 1);

                myMarker.GetComponent<Renderer>().material.color = new Color(point.x, point.y, point.z, 1);

                myMarker.SetActive(true);

            }
        }

        m_points = points.Distinct(new Comparer()).ToArray();

        Debug.Log(string.Format("m_points count {0}", m_points.Length));

        _setuped = true;
    }

    public void UpdateState()
    {

        if (_fertig) return;

        //_fertig = true;

        if (crimeScene.m_cubeList.Count <= 0) return;

        if (!_setuped) SetUp();

        /*for (var i = 0; i < crimeScene.m_cubeList.Count; i++)
        {
            GameObject cube = crimeScene.m_cubeList[i];
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(cube.transform.position);

            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            if (onScreen)
            {
                var it = FindClosestPoint(Camera.main, new Vector2(cube.transform.position.x, cube.transform.position.z), 10);

                if (it <= -1) continue;

                var target = m_points[it];
                target.x = cube.transform.position.x;
                target.z = cube.transform.position.z;
                cube.transform.position = target;
                crimeScene.m_cubeList.Remove(cube);
            }
        }*/
    }

    public void OnGUIState()
    {

    }

    /// @endcond
    /// <summary>
    /// Finds the closest point from a point cloud to a position on screen.
    /// 
    /// This function is slow, as it looks at every single point in the point
    /// cloud. Avoid calling this more than once a frame.
    /// </summary>
    /// <returns>The index of the closest point, or -1 if not found.</returns>
    /// <param name="cam">The current camera.</param>
    /// <param name="pos">Position on screen (in pixels).</param>
    /// <param name="maxDist">The maximum pixel distance to allow.</param>
    public int FindClosestPoint(Camera cam, Vector2 pos, int maxDist)
    {
        int bestIndex = -1;
        float bestDistSqr = 0;

        for (int it = 0; it < m_points.Length; ++it)
        {

            Vector3 point = m_points[it];
            if (point.Equals(Vector3.zero)) continue;
            Vector3 screenPos3 = cam.WorldToScreenPoint(point);
            Vector2 screenPos = new Vector2(screenPos3.x, screenPos3.z);
            float distSqr = Vector3.SqrMagnitude(screenPos - pos);

            if (distSqr > maxDist * maxDist)
            {
                continue;
            }

            if (bestIndex == -1 || distSqr < bestDistSqr)
            {
                bestIndex = it;
                bestDistSqr = distSqr;
            }
        }

        return bestIndex;
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
