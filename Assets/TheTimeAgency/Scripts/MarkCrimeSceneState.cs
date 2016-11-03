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
    private bool m_findingFloor = false;

    public MarkCrimeSceneState(CrimeScene crimeScenePattern )
    {
        crimeScene = crimeScenePattern;
    }

    void ICrimeSceneState.UpdateState()
    {
        if (!m_findingFloor)
        {
            return;
        }

        // If the point cloud floor has found a new floor, place the marker at the found y position.
        if (crimeScene.m_pointCloudFloor.m_floorFound && crimeScene.m_pointCloud.m_floorFound)
        {
            Vector3 target = getFloorCoordinate();

            bool toClose = vec3ToClose(target);

            bool isInside = crimeScene.markerList.Count > 2 && isPointInside(target);

            if (toClose)
            {
                Debug.Log("The distance to all other makers has to be " + crimeScene.m_distanceMarkers);
                m_findingFloor = false;
                return;
            }

            if (isInside)
            {
                Debug.Log("The marker can't be within the crime scine area!");
                m_findingFloor = false;
                return;
            }

            // copy of the maker
            GameObject myMarker = GameObject.Instantiate<GameObject>(crimeScene.m_marker);

            /*
             * Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation. 
             * http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
            */
            myMarker.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

            myMarker.transform.position = target;
            // Place the marker at the center of the screen at the found floor height.
            myMarker.SetActive(true);
            AndroidHelper.ShowAndroidToastMessage(string.Format("Floor found. Unity world height = {0}", crimeScene.m_pointCloudFloor.transform.position.y.ToString()));

            crimeScene.markerList.Add(myMarker);

        }
    }

    void ICrimeSceneState.OnGUIState()
    {
        if (crimeScene.markerList.Count >= crimeScene.m_numberMarkers)
        {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), "<size=30>Congratulations!!!!! All makers set!</size>");
            m_findingFloor = false;
            ToSpreadAdviceState();
            return;
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), "<size=30>" + crimeScene.markerList.Count + "/" + crimeScene.m_numberMarkers + " makers set!</size>");
        }


        GUI.color = Color.white;

        if (!m_findingFloor)
        {
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Find Floor</size>"))
            {
                if (crimeScene.m_pointCloud == null)
                {
                    Debug.LogError("TangoPointCloud required to find floor.");
                    return;
                }

                m_findingFloor = true;
                //m_marker.SetActive(false);
                crimeScene.m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
                crimeScene.m_pointCloud.FindFloor();
            }
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), "<size=30>Searching for floor position. Make sure the floor is visible.</size>");
        }
    }

    private void ToSpreadAdviceState()
    {
        crimeScene.currentState = crimeScene.spreadAdviceState;
    }

    private Vector3 getFloorCoordinate()
    {
        Vector3 target;
        RaycastHit hitInfo;

        m_findingFloor = false;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f)), out hitInfo))
        {
            // Limit distance of the marker position from the camera to the camera's far clip plane. This makes sure that the marker
            // is visible on screen when the floor is found.
            Vector3 cameraBase = new Vector3(Camera.main.transform.position.x, hitInfo.point.y, Camera.main.transform.position.z);
            target = cameraBase + Vector3.ClampMagnitude(hitInfo.point - cameraBase, Camera.main.farClipPlane * 0.9f);
        }
        else
        {
            // If no raycast hit, place marker in the camera's forward direction.
            Vector3 dir = new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z);
            target = dir.normalized * (Camera.main.farClipPlane * 0.9f);
            target.y = crimeScene.m_pointCloudFloor.transform.position.y;
        }

        return target;
    }

    private bool isPointInside(Vector3 target)
    {
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

        return target.x >= minX && target.x <= maxX && target.z >= minZ && target.z <= maxZ;
    }

    private bool vec3ToClose(Vector3 target)
    {
        bool toClose = false;

        foreach (GameObject marker in crimeScene.markerList)
        {
            var distance = Vector2.Distance(
                new Vector2(marker.transform.position.x, marker.transform.position.z),
                new Vector2(target.x, target.z)
            );

            if (distance < crimeScene.m_distanceMarkers)
            {
                toClose = true;
                break;
            }
        }

        return toClose;
    }

    private void setPoints()
    {


        for (char i = 'A'; i < 'E'; i++)
        {
            // copy of the maker
            GameObject myMarker = GameObject.Instantiate<GameObject>(crimeScene.m_marker);

            myMarker.name = i.ToString();

            //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
            //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
            myMarker.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

            // Place the marker at the center of the screen at the found floor height.
            myMarker.SetActive(true);

            crimeScene.markerList.Add(myMarker);
        }

        ((GameObject)crimeScene.markerList[2]).transform.position = new Vector3(1f, -1.300f, 2f);
        ((GameObject)crimeScene.markerList[0]).transform.position = new Vector3(-1.5f, -1.300f, -1f);
        ((GameObject)crimeScene.markerList[3]).transform.position = new Vector3(-3f, -1.300f, 1f);
        ((GameObject)crimeScene.markerList[1]).transform.position = new Vector3(3f, -1.300f, 1f);

    }

}
