using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class ClickScript : MonoBehaviour {
    Camera cam;
    GameObject itemList; 

    void Start()
    {
        cam = Camera.main.transform.Find("Camera").GetComponent<Camera>();
        itemList = GameObject.Find("ItemList");
    }
    void Update()
    {
     
    }
    void OnMouseDown()
    {
        if (!Input.GetMouseButton(0))
            return;

        RaycastHit[] hit = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (hit.Length <=0)
        {
            Debug.Log("Return1");
            return;
        }
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider.gameObject.CompareTag("Fingerprint") && cam.cullingMask == (1 << LayerMask.NameToLayer("Fingerprint"))||
                hit[i].collider.gameObject.CompareTag("Biological") && cam.cullingMask == (1 << LayerMask.NameToLayer("Biological"))||
                   hit[i].collider.gameObject.CompareTag("Chemical") && cam.cullingMask == (1 << LayerMask.NameToLayer("Chemical")))
            {
                Debug.Log("hit");

                Renderer renderer = hit[i].collider.GetComponent<Renderer>();
                MeshCollider meshCollider = hit[i].collider as MeshCollider;
                if (renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null || meshCollider == null)
                {
                   
                    continue;
                }

                Texture2D tex = (Texture2D)renderer.material.mainTexture;
                Vector2 pixelUV = hit[i].textureCoord;

                Color c1 = tex.GetPixel((int)(pixelUV.x * tex.width), (int)(pixelUV.y * tex.height));
                Color c2 = tex.GetPixel((int)(pixelUV.x * tex.width * 1.1), (int)(pixelUV.y * tex.height));
                Color c3 = tex.GetPixel((int)(pixelUV.x * tex.width), (int)(pixelUV.y * tex.height * 1.1));
                Color c4 = tex.GetPixel((int)(pixelUV.x * tex.width * 0.9), (int)(pixelUV.y * tex.height));
                Color c5 = tex.GetPixel((int)(pixelUV.x * tex.width), (int)(pixelUV.y * tex.height * 0.9));
              
                Color c = new Color();
                c.a = (c1.a + c2.a + c3.a + c4.a + c5.a) / 5;
                Debug.Log(c.a);
                if (c.a > 0.1)
                {
                    string name = hit[i].collider.transform.parent.name;
                    Game game = Game.getInstance();
                    Item item = game.getItem(name);
                    Transform btn = itemList.transform.FindChild("btn_" + item.name);
                    if(btn != null)
                    {
                       Toggle tgl=  btn.GetComponent<Toggle>();
                        if (tgl != null) tgl.isOn = true;
                    }
                    
                    if (hit[i].collider.gameObject.CompareTag("Fingerprint"))
                    {
                        
                        scanFingerPrint(game, item);
                    }
                    else if (hit[i].collider.gameObject.CompareTag("Biological"))
                    {
                        Debug.Log("Biological");
                        scanBiological(game, item);
                    }
                    else
                    {
                        scanChemical(game, item);
                    }
                    GameObject.Find("MenuManager").GetComponent<InitMenu>().needsRefresh = true;
                    //item
                }
                //else Debug.Log("not klicked");
                return;
            }
        }
    }

    void scanFingerPrint(Game game, Item item)
    {
        if (item == null) return;
        for (int x = 0; x < item.fingerprint.Length; x++)
        {
            if (item.fingerprint[x].time.Contains(game.getTime()))
                item.fingerprint[x].status = 1;

        }
      //  needsRefresh = true;
    }
    void scanBiological(Game game, Item item)
    {
        if (item == null) return;
        for (int x = 0; x < item.biological.Length; x++)
        {
            if (item.biological[x].time.Contains(game.getTime()))
                item.biological[x].status = 1;
        }
        //needsRefresh = true;
    }
    void scanChemical(Game game, Item item)
    {

        if (item == null) return;
        for (int x = 0; x < item.chemical.Length; x++)
        {
            if (item.chemical[x].time.Contains(game.getTime()))
                item.chemical[x].status = 1;

        }
        //needsRefresh = true;
    }

}
