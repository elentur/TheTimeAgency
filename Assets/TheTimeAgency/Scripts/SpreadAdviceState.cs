using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Object = UnityEngine.Object;

public class SpreadAdviceState : ICrimeSceneState
{
    private readonly CrimeScene crimeScene;

    public SpreadAdviceState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
        //throw new NotImplementedException();
    }

    void ICrimeSceneState.UpdateState()
    {

        var xArray = new float[crimeScene.markerList.Count];
        var zArray = new float[crimeScene.markerList.Count];

        for (int i = 0; i < crimeScene.markerList.Count; i++)
        {
            xArray[i] = crimeScene.markerList[i].transform.position.x;
            zArray[i] = crimeScene.markerList[i].transform.position.z;
        }

        var maxX = xArray.Max();
        var minX = xArray.Min();
        var maxZ = zArray.Max();
        var minZ = zArray.Min();

        float steps = 0.1f;

        for (float z = minZ; z < maxZ; z += steps)
        {
            for (float x = minX; x < maxX; x += steps)
            {
                var p = new Vector3(x, crimeScene.markerList[0].transform.position.y, z);

                foreach (Triangle2D triagle in crimeScene.triangleList)
                {
                    if (triagle.PointInTriangle(p))
                    {
                        GameObject cube = SetACube(x + "/" + z);

                        cube.transform.position = p;

                        Vector3 sclale = cube.transform.localScale;

                        cube.transform.localScale = new Vector3(sclale.x - steps/steps, sclale.y - steps/steps, sclale.z - steps/steps);

                        crimeScene.m_AdvicePlaceHolderList.Add(cube);
                    }
                }
            }
        }

        ToPingState();
    }

    void ICrimeSceneState.OnGUIState()
    {

    }

    private void ToPingState()
    {
        crimeScene.currentState = crimeScene.pingState;
        crimeScene.currentState.StartState();
    }

    private GameObject SetACube(string name)
    {
        // copy of the maker
        GameObject myCube = Object.Instantiate<GameObject>(crimeScene.m_AdvicePlaceHolder);

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
