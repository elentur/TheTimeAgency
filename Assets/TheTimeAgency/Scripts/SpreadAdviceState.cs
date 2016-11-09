using System;
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

    void ICrimeSceneState.UpdateState()
    {

        //crimeScene.markerList.Sort((IComparer) new ClockwiseVector3Comparer());

        var xArray = new float[crimeScene.markerList.Count];
        var zArray = new float[crimeScene.markerList.Count];

        for (int i = 0; i < crimeScene.markerList.Count; i++)
        {
            xArray[i] = ((GameObject)crimeScene.markerList[i]).transform.localPosition.x;
            zArray[i] = ((GameObject)crimeScene.markerList[i]).transform.localPosition.z;
        }

        var maxX = xArray.Max();
        var minX = xArray.Min();
        var maxZ = zArray.Max();
        var minZ = zArray.Min();

        float steps = 10.0f;

        ArrayList points = new ArrayList();

        for (float z = minZ; z < maxZ; z += steps)
        {
            for (float x = minX; x < maxX; x += steps)
            {

                var p = new Vector3(x, ((GameObject)crimeScene.markerList[0]).transform.localPosition.y, z);

                foreach (Triangle2D triagle in crimeScene.triangleList)
                {
                    if (triagle.PointInTriangle(p))
                    {
                        GameObject cube = setACube(x + "/" + z);

                        cube.transform.localPosition = p;

                        cube.transform.localScale = new Vector3(steps - steps/steps, steps - steps/steps,
                            steps - steps/steps);

                        points.Add(cube.transform.localPosition);
                    }
                }
            }
        }


        Debug.Log("------------Points Order---------------");
        Debug.Log(((GameObject)crimeScene.markerList[0]).transform.localPosition);
        Debug.Log(((GameObject)crimeScene.markerList[1]).transform.localPosition);
        Debug.Log(((GameObject)crimeScene.markerList[2]).transform.localPosition);
        Debug.Log(((GameObject)crimeScene.markerList[3]).transform.localPosition);
        Debug.Log("------------founded x line / z line---------------");
        Debug.Log(minX);
        Debug.Log(maxX);
        Debug.Log(minZ);
        Debug.Log(maxZ);
        Debug.Log("Arrays: " + points.Count);
        Debug.Log("---------------------------");

        ToPingState();
    }

    void ICrimeSceneState.OnGUIState()
    {

    }

    public class ClockwiseVector3Comparer : IComparer
    {
        public int Compare(object obj1, object obj2)
        {
            Vector3 v1 = ((GameObject)obj1).transform.localPosition;
            Vector3 v2 = ((GameObject)obj2).transform.localPosition;

            return Mathf.Atan2(v1.x, v1.z).CompareTo(Mathf.Atan2(v2.x, v2.z));
        }
    }

    private void ToPingState()
    {
        crimeScene.currentState = crimeScene.pingState;
    }

    private GameObject setACube(string name)
    {
        // copy of the maker
        GameObject myCube = GameObject.Instantiate<GameObject>(crimeScene.m_cube);

        myCube.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        myCube.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

        // Place the marker at the center of the screen at the found floor height.
        myCube.SetActive(true);

        return myCube;
    }
}
