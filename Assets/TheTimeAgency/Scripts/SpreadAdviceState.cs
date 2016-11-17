﻿using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.TheTimeAgency.Scripts;

public class SpreadAdviceState : ICrimeSceneState
{
    private readonly CrimeScene crimeScene;

    public SpreadAdviceState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
        throw new NotImplementedException();
    }

    void ICrimeSceneState.UpdateState()
    {

        //crimeScene.markerList.Sort((IComparer) new ClockwiseVector3Comparer());

        var xArray = new float[crimeScene.markerList.Count];
        var zArray = new float[crimeScene.markerList.Count];

        for (int i = 0; i < crimeScene.markerList.Count; i++)
        {
            xArray[i] = ((GameObject)crimeScene.markerList[i]).transform.position.x;
            zArray[i] = ((GameObject)crimeScene.markerList[i]).transform.position.z;
        }

        var maxX = xArray.Max();
        var minX = xArray.Min();
        var maxZ = zArray.Max();
        var minZ = zArray.Min();

        Debug.Log(string.Format("X: {0} - {1}", minX, maxX));
        Debug.Log(string.Format("Z: {0} - {1}", minZ, maxZ));

        float steps = 10.0f;

        for (float z = minZ; z < maxZ; z += steps)
        {
            for (float x = minX; x < maxX; x += steps)
            {
                var p = new Vector3(x, ((GameObject)crimeScene.markerList[0]).transform.position.y, z);

                foreach (Triangle2D triagle in crimeScene.triangleList)
                {
                    if (triagle.PointInTriangle(p))
                    {
                        GameObject cube = SetACube(x + "/" + z);

                        cube.transform.position = p;

                        cube.transform.localScale = new Vector3(steps - steps/steps, steps - steps/steps, steps - steps/steps);

                        crimeScene.m_pointList.Add(cube.transform.position);
                        crimeScene.m_cubeList.Add(cube);
                    }
                }
            }
        }

        ToPingState();
    }

    void ICrimeSceneState.OnGUIState()
    {

    }

    public class ClockwiseVector3Comparer : IComparer
    {
        public int Compare(object obj1, object obj2)
        {
            Vector3 v1 = ((GameObject)obj1).transform.position;
            Vector3 v2 = ((GameObject)obj2).transform.position;

            return Mathf.Atan2(v1.x, v1.z).CompareTo(Mathf.Atan2(v2.x, v2.z));
        }
    }

    private void ToPingState()
    {
        crimeScene.currentState = crimeScene.pingState;
    }

    private GameObject SetACube(string name)
    {
        // copy of the maker
        GameObject myCube = GameObject.Instantiate<GameObject>(crimeScene.m_cube);

        myCube.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        myCube.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

        myCube.GetComponent<Renderer>().material.color = new Color(1.0f,1.0f,1.0f,0.1f);

        // Place the marker at the center of the screen at the found floor height.
        myCube.SetActive(true);

        return myCube;
    }
}
