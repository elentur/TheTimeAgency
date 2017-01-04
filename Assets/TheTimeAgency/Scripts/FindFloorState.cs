using UnityEngine;
using System.Collections;
using Assets.TheTimeAgency.Scripts;
using Tango;

public class FindFloorState : ICrimeSceneState
{
    private readonly CrimeScene crimeScene;

    /// <summary>
    /// If <c>true</c>, floor finding is in progress.
    /// </summary>
    private bool m_findingFloor = false;

    public FindFloorState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
        //throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        if (!m_findingFloor) return;

        // If the point cloud floor has found a new floor, place the marker at the found y position.
        if (crimeScene.m_pointCloudFloor.m_floorFound && crimeScene.m_pointCloud.m_floorFound)
        {
            crimeScene.m_floorPoint = getFloorCoordinate();
            crimeScene.m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
            crimeScene.m_pointCloud.FindFloor();
            ToMarkCrimeSceneState();
            return;
        }
    }

    public void OnGUIState()
    {
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
                crimeScene.m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
                crimeScene.m_pointCloud.FindFloor();
            }
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50),
                "<size=30>Searching for floor position. Make sure the floor is visible.</size>");
            /*crimeScene.m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
            crimeScene.m_pointCloud.FindFloor();*/
        }
    }

    private Vector3 getFloorCoordinate()
    {
        Vector3 target;
        RaycastHit hitInfo;

        m_findingFloor = false;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f)),
            out hitInfo))
        {
            // Limit distance of the marker position from the camera to the camera's far clip plane. This makes sure that the marker
            // is visible on screen when the floor is found.
            Vector3 cameraBase = new Vector3(Camera.main.transform.position.x, hitInfo.point.y,
                Camera.main.transform.position.z);
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

    private void ToMarkCrimeSceneState()
    {
        crimeScene.currentState = crimeScene.markCrimeSceneState;
        crimeScene.currentState.StartState();
    }
}
