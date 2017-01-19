using UnityEngine;
using System.Collections;

public class ShowPanels : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void showEvidenceList()
    {
        GetComponent<Animation>().Play();
       // Debug.Log(test);
    }
}
