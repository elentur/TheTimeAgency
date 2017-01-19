using UnityEngine;
using System.Collections;

public class ClosePanel : MonoBehaviour {

	public void Close(GameObject obj)
    {
        obj.SetActive(false);
    }
}
