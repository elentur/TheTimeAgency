using UnityEngine;
using System.Collections;
using Assets.TheTimeAgency.Scripts;

public class PingState : ICrimeSceneState {

    private readonly CrimeScene crimeScene;

    public PingState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    public void UpdateState()
    {

    }

    public void OnGUIState()
    {

    }
}
