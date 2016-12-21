using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Tango;

public class MarkCrimeSceneState : ICrimeSceneState
{

    private readonly CrimeScene crimeScene;

    /// <summary>
    /// If <c>true</c>, floor finding is in progress.
    /// </summary>
    private bool m_setMarker = false;

    private char pointName = 'A';

    private const float DISTANCE = 2.0f; 

    public MarkCrimeSceneState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    private Vector3[] defaultMarker = new[]
    {
        new Vector3(0.8f, -1.300f, 3.9f),
        new Vector3(1.7f, -1.300f, 0.29f),
        new Vector3(-1.6f, -1.300f, -0.7f),
        new Vector3(-2.9f, -1.300f, 1.6f),
    };

    public void StartState()
    {
      //  throw new System.NotImplementedException();
    }

    void ICrimeSceneState.UpdateState()
    {

       Vector3 p1 = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE;

        crimeScene.m_marker.transform.position = new Vector3(p1.x, 1.0f + crimeScene.m_floorPoint.y,p1.z);

        crimeScene.m_marker.SetActive(true);

        if (!m_setMarker) return;

            // copy of the maker
            GameObject myMarker = Object.Instantiate(crimeScene.m_marker);

            myMarker.name = pointName.ToString();

            pointName++;

            /*
             * Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation. 
             * http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
            */
            myMarker.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);


            Vector3 p = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE;

        // Place the marker at the center of the screen at the found floor height.

        Vector3 position = myMarker.transform.position;

            bool toClose = vec3ToClose(position);

            bool isInside = false;

            if (toClose)
            {
                AndroidHelper.ShowAndroidToastMessage(string.Format("The distance to all other makers has to be {0}", crimeScene.m_distanceMarkers));
                m_setMarker = false;
                Object.Destroy (myMarker);
                return;
            }

            foreach (Triangle2D triangle in crimeScene.triangleList)
            {
                if (triangle.PointInTriangle(position))
                {
                    isInside = true;
                }
            }

            if (isInside)
            {
                AndroidHelper.ShowAndroidToastMessage("The marker can't be within the crime scine area!");
                m_setMarker = false;
                Object.Destroy(myMarker);
                return;
            }

            myMarker.SetActive(true);
            AndroidHelper.ShowAndroidToastMessage(string.Format("Floor found. Unity world height = {0}",
                crimeScene.m_pointCloudFloor.transform.position.y.ToString()));

            crimeScene.markerList.Add(myMarker);

            if (crimeScene.markerList.Count == 3)
            {

                Triangle2D tri = new Triangle2D(
                    crimeScene.markerList[0].transform.position,
                    crimeScene.markerList[1].transform.position,
                    crimeScene.markerList[2].transform.position
                );

                crimeScene.triangleList.Add(tri);
            }

            if (crimeScene.markerList.Count > 3)
            {
                Triangle2D tri = crimeScene.triangleList[0];
                crimeScene.triangleList.Add(tri.AdjacentTriangle(crimeScene.markerList[3].transform.position));
            }
        
    }

    void ICrimeSceneState.OnGUIState()
    {

        if (crimeScene.markerList.Count >= crimeScene.m_numberMarkers)
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("Congratulations!!!!! All makers set!"));
            m_setMarker = false;
            // reset of the camera to leave out of the findFloor modus
            crimeScene.m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
            crimeScene.m_marker.SetActive(false);
            ToSpreadAdviceState();
            return;
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("{0} / {1}  makers set!", crimeScene.markerList.Count, crimeScene.m_numberMarkers));
        }


        GUI.color = Color.white;

        if (!m_setMarker)
        {
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Set Marker</size>"))
            {
               m_setMarker = true;
            }
        }
    }

    private void ToSpreadAdviceState()
    {
        crimeScene.currentState = crimeScene.spreadAdviceState;
        crimeScene.currentState.StartState();
    }

    private bool vec3ToClose(Vector3 target)
    {
        bool toClose = false;

        foreach (GameObject marker in crimeScene.markerList)
        {
            float distSqr = Vector3.SqrMagnitude(
                new Vector2(marker.transform.position.x, marker.transform.position.z) - new Vector2(target.x, target.z)
                );

            if (distSqr < crimeScene.m_distanceMarkers * crimeScene.m_distanceMarkers)
            {
                toClose = true;
                break;
            }
        }

        return toClose;
    }
}
