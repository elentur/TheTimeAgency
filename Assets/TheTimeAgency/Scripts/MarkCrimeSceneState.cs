using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.TheTimeAgency.Scripts;
using Tango;

public class MarkCrimeSceneState : ICrimeSceneState
{

    private readonly CrimeScene _crimeScene;

    /// <summary>
    /// If <c>true</c>, floor finding is in progress.
    /// </summary>
    private bool m_setMarker = false, m_resetMarkers = false, m_defaultMarkers= false;

    private char pointName = 'A';

    private const float DISTANCE = 2.0f;

    public GameObject makersBox;

    public MarkCrimeSceneState(CrimeScene crimeScenePattern)
    {
        _crimeScene = crimeScenePattern;
    }

    public void StartState()
    {
        //  throw new System.NotImplementedException();

        makersBox = new GameObject("makersBox");
        makersBox.transform.SetParent(_crimeScene.m_marker.transform.parent.gameObject.transform, false);
    }

    void ICrimeSceneState.UpdateState()
    {

        if (m_defaultMarkers)
        {

            m_defaultMarkers = false;
        }


        if (m_resetMarkers)
        {
            _crimeScene.triangleList.Clear();
            _crimeScene.markerList.Clear();
    
            foreach (Transform child in makersBox.transform)
            {
                Object.Destroy(child.gameObject);
            }

            m_resetMarkers = false;
        }

        Vector3 p1 = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE;

        _crimeScene.m_marker.transform.position = new Vector3(p1.x, 1.0f + _crimeScene.m_floorPoint.y, p1.z);

        _crimeScene.m_marker.SetActive(true);



        if (!m_setMarker) return;

        // copy of the maker
        GameObject myMarker = Object.Instantiate(_crimeScene.m_marker);

        myMarker.name = pointName.ToString();

        pointName++;

        /*
         * Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation. 
         * http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        */
        myMarker.transform.SetParent(makersBox.transform, false);

        Vector3 p = Camera.main.transform.position + Camera.main.transform.forward * DISTANCE;

        // Place the marker at the center of the screen at the found floor height.

        Vector3 position = myMarker.transform.position;

        bool toClose = vec3ToClose(position);

        bool isInside = false;

        if (toClose)
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("The distance to all other makers has to be {0}", _crimeScene.m_distanceMarkers));
            m_setMarker = false;
            Object.Destroy(myMarker);
            return;
        }

        foreach (Triangle2D triangle in _crimeScene.triangleList)
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

        Debug.Log(string.Format("Marker set on {0}", myMarker.transform.position));

        myMarker.SetActive(true);
        AndroidHelper.ShowAndroidToastMessage(string.Format("Floor found. Unity world height = {0}",
            _crimeScene.m_pointCloudFloor.transform.position.y.ToString()));

        _crimeScene.markerList.Add(myMarker);

        if (_crimeScene.markerList.Count == 3)
        {

            Triangle2D tri = new Triangle2D(
                _crimeScene.markerList[0].transform.position,
                _crimeScene.markerList[1].transform.position,
                _crimeScene.markerList[2].transform.position
            );

            _crimeScene.triangleList.Add(tri);
        }

        if (_crimeScene.markerList.Count > 3)
        {
            Triangle2D tri = _crimeScene.triangleList[0];
            _crimeScene.triangleList.Add(tri.AdjacentTriangle(_crimeScene.markerList[3].transform.position));
        }

    }

    void ICrimeSceneState.OnGUIState()
    {

        if (_crimeScene.markerList.Count >= _crimeScene.m_numberMarkers)
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("Congratulations!!!!! All makers set!"));
            m_setMarker = false;
            ToPingState();
            return;
        }
        else
        {
            AndroidHelper.ShowAndroidToastMessage(string.Format("{0} / {1}  makers set!", _crimeScene.markerList.Count, _crimeScene.m_numberMarkers));
        }


        GUI.color = Color.white;

        if (!m_setMarker)
        {
            if (GUI.Button(new Rect(Screen.width - 220, 20, 200, 80), "<size=30>Set Marker</size>")) m_setMarker = true;
            
        }

        if (!m_resetMarkers)
        {
            if (GUI.Button(new Rect(Screen.width - 240, 120, 220, 80), "<size=30>Reset Marker</size>")) m_resetMarkers = true;
        }

        if (!m_defaultMarkers)
        {
            if (GUI.Button(new Rect(Screen.width - 240, 220, 220, 80), "<size=30>Default Marker</size>")) m_defaultMarkers = true;
        }
    }

    private void ToPingState()
    {
        _crimeScene.currentState = _crimeScene.pingState;
        _crimeScene.currentState.StartState();
    }

    private bool vec3ToClose(Vector3 target)
    {
        bool toClose = false;

        foreach (GameObject marker in _crimeScene.markerList)
        {
            float distSqr = Vector3.SqrMagnitude(
                new Vector2(marker.transform.position.x, marker.transform.position.z) - new Vector2(target.x, target.z)
                );

            if (distSqr < _crimeScene.m_distanceMarkers * _crimeScene.m_distanceMarkers)
            {
                toClose = true;
                break;
            }
        }

        return toClose;
    }
}
