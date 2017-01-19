using UnityEngine;
using System.Collections;

public class ActivateScannMode : MonoBehaviour {

    public GameObject blue;
    public GameObject green;
    public GameObject yellow;
    public GameObject camera;

    public void setModeOnOff(GameObject active)
    {
        Camera c = camera.GetComponent<Camera>();
        if (blue == active) c.cullingMask = (1 << LayerMask.NameToLayer("Fingerprint"));
        if (green == active) c.cullingMask = (1 << LayerMask.NameToLayer("Biological"));
        if (yellow == active) c.cullingMask = (1 << LayerMask.NameToLayer("Chemical"));

        active.SetActive(!active.activeSelf);
        camera.SetActive(active.activeSelf);
       
        if (blue!= active) blue.SetActive(false);
        if (green != active) green.SetActive(false);
        if (yellow != active) yellow.SetActive(false);
    }
}
