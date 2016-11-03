using UnityEngine;
using System.Collections;

namespace Assets.TheTimeAgency.Scripts
{
    public interface ICrimeSceneState { 

        // Update is called once per frame
        void UpdateState();

        void OnGUIState();
    }
}
