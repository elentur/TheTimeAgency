using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectItem : MonoBehaviour {
    private GameObject evidences;
	// Use this for initialization
	void Start ()
    {
        evidences = GameObject.Find("RectEvidences");
    }

    public void showEvidenceList()
    {
        Debug.Log(evidences);
    }
	
	// Update is called once per frame
	void Update () {
	
	}


}
