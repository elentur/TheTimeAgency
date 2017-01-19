using UnityEngine;
using System.Collections;
using Assets.TheTimeAgency.Scripts;

public class DefaultState : ICrimeSceneState
{

    private readonly CrimeScene crimeScene;

    public DefaultState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
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
