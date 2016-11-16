using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Tango;

public class PingState : ICrimeSceneState {

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

    private List<Vector3> m_pointList;

    private bool _fertig = false;

    public PingState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    public void UpdateState()
    {
        if (crimeScene.m_cubeList.Count <= 0) return;

        m_points = crimeScene.m_pointCloud.m_points.Distinct(new Comparer()).ToArray();

        Debug.Log(string.Format("m_points count {0}", m_points.Length));

        RaycastHit hitInfo;

        for (var i = 0; i < crimeScene.m_cubeList.Count; i++)
        {
            GameObject cube = crimeScene.m_cubeList[i];
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(cube.transform.position);


            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
          
            if (onScreen)
            { 
                int it = FindClosestPoint(Camera.main, new Vector2(cube.transform.position.x, cube.transform.position.z), 10);
                Vector3 target = m_points[it];
                target.x = cube.transform.position.x;
                target.z = cube.transform.position.z;
                cube.transform.position = target;
                crimeScene.m_cubeList.Remove(cube);
            }
        }
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
            //Debug.Log(string.Format("screenPos3 x {0}, y {1}, z {2}", screenPos3.x, screenPos3.y, screenPos3.z));
            Vector2 screenPos = new Vector2(screenPos3.x, screenPos3.y);

            float distSqr = Vector2.SqrMagnitude(screenPos - pos);
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
