using UnityEngine;
using System.Collections;
using Assets.TheTimeAgency.Scripts;

public class DefaultState : ICrimeSceneState
{



    public DefaultState(CrimeScene crimeScenePattern)
    {
      
    }

    public void StartState()
    {
       //Debug.Log("DefaultState - Start"); 
    }

    public void UpdateState()
    {
        //Debug.Log("DefaultState - Update");
    }

    public void OnGUIState()
    {
        //Debug.Log("DefaultState - OnGUI");
    }
}
