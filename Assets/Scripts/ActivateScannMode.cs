using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActivateScannMode : MonoBehaviour {

    public GameObject blue;
    public GameObject green;
    public GameObject yellow;
    public GameObject cam;

    public void setModeOnOff(GameObject active)
    {

        Camera c = cam.GetComponent<Camera>();
        if (blue == active) c.cullingMask = (1 << LayerMask.NameToLayer("Fingerprint"));
        if (green == active) c.cullingMask = (1 << LayerMask.NameToLayer("Biological"));
        if (yellow == active) c.cullingMask = (1 << LayerMask.NameToLayer("Chemical"));

        active.SetActive(!active.activeSelf);
        cam.SetActive(active.activeSelf);

        if (blue != active)
        {
            blue.SetActive(false);
        }
        if (green != active) green.SetActive(false);
        if (yellow != active) yellow.SetActive(false);
    }

    public void toggleOf(Toggle t)
    {
        t.isOn = false;
    }
}
