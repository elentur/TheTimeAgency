using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.Collections.Generic;
using UnityEngine.UI;

public class Ping : MonoBehaviour
{
    private bool pingUp = false;
    private bool pingDown = false;
    private List<GameObject> items = new List<GameObject>();
    private ScreenOverlay overlay;
    private int screenShotCount = 0;
    private Assets.TheTimeAgency.Scripts.PingState pingState; 

    void Start()
    {
        GameObject crimeSceneObject = GameObject.Find("CrimeSceneObject");
        if(crimeSceneObject != null){
            pingState= crimeSceneObject.GetComponent<CrimeScene>().pingState;
        }

    }
    void Update()
    {
        if (pingUp && overlay != null)
        {
            overlay.intensity += 2.0f;
            if (overlay.intensity >= 5)
            {
                pingUp = false;
                pingDown = true;
                showItems();
            }

        }
        if (pingDown)
        {
            overlay.intensity -= 0.4f;
            if (overlay.intensity <= 0)
            {
                pingDown = false;
                overlay.intensity = 0.0f;
            }

        }
    }

    void LateUpdate()
    {
        if (items.Count > 0)
        {
            //GameObject itemList = GameObject.Find("ItemList");
            InitMenu init = GameObject.Find("MenuManager").GetComponent<InitMenu>();
            foreach (GameObject item in items)
            {
             
                GameObject cam = item.transform.Find("Camera").gameObject;
                Camera camera = cam.GetComponent<Camera>();
                RenderTexture rt = new RenderTexture(256, 256, 24);
                camera.targetTexture = rt;
                camera.aspect = 1.0f;
                Texture2D screenShot = new Texture2D(256, 256, TextureFormat.RGB24, false);
                camera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(camera.pixelRect, 0, 0);
                screenShot.Apply();
                camera.targetTexture = null;
                camera.enabled = false;
                RenderTexture.active = null; 
                Destroy(rt);
                init.addItemButton(item.name, Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0, 0)));
               /* for (int i = 0; i < itemList.transform.childCount; i++)
                {
                    GameObject button = itemList.transform.GetChild(i).gameObject;
                    GameObject image = button.transform.Find("Image").gameObject;
                    ItemContainer cont = image.GetComponent<ItemContainer>();
                    if (cont.item.name == item.name)
                    {
                        button.SetActive(true);
                        image.GetComponent<Image>().sprite = Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0, 0));
                    }
                }*/

            }

            items.Clear();
        }
        if (screenShotCount > 0) screenShotCount--;
    }


    public void startPing()
    {
        overlay = Camera.main.GetComponent<ScreenOverlay>();
        pingUp = true;


    }

    private void showItems()
    {

        Game game = Game.getInstance();
        List<Vector3> foundPositions = pingState.ping();
        foreach (Vector3 position in foundPositions)
        {
            foreach (GameObject item in game.getItemGameObjects())
            {
                if (!item.activeSelf)
                {
                    item.transform.position = position;
                    item.SetActive(true);
                    items.Add(item);
                    break;
                }
            }
        }
      /*  foreach (GameObject item in game.getItemGameObjects())
        {
            // Renderer r = (Renderer)item.GetComponentInChildren<Renderer>();
            // if (r.enabled) return;
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(item.transform.position);
 
            if(item.activeSelf)continue;
            bool onScreen = screenPoint.z > 0 && screenPoint.z < Camera.main.GetComponent<Camera>().farClipPlane && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            if (onScreen)
            {
                item.SetActive(true);
                items.Add(item);
            }
        }*/
    }

}
