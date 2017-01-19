using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SetObjectOnCameraPos : MonoBehaviour
{
    
  
    float floor = -1.3f;

    List<GameObject> barriers = new List<GameObject>();
    public void setObject()
    {
        Camera cam = Camera.main;
        if (barriers.Count >= 4) return;
        GameObject instance =  (GameObject)Instantiate(
        Resources.Load(Path.Combine("Prefabs", "Barrier")),
        cam.transform.position,
        cam.transform.rotation);
        instance.transform.Translate(0.0f, 0.0f, 4.0f);
        instance.transform.rotation = Quaternion.identity; 
      //  instance.GetComponent<Animator>().Play("Take 001");
        // instance.transform.LookAt(cam.transform.position);
        instance.transform.position = new Vector3(instance.transform.position.x, floor, instance.transform.position.z);
       // instance.transform.rotation = new Quaternion();
        if (barriers.Count > 0)
        {
            createMesh(instance, barriers[barriers.Count - 1]);

        }
        barriers.Add(instance);
        if (barriers.Count >= 4)
        {
            createMesh(barriers[0], instance);
        }
        instance.SetActive(true);
    }

    void createMesh(GameObject instance, GameObject lastInstance)
    {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        Transform tra = lastInstance.transform;
        float height = tra.position.y + tra.localScale.y +0.04f;
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        float scale = Vector3.Distance(instance.transform.position, lastInstance.transform.position);
        plane.transform.localScale = new Vector3(scale,0.08f,0.0f);
        Vector3 p =( instance.transform.position + lastInstance.transform.position) *0.5f;
        p.y = height;
        plane.transform.position = p;
        instance.transform.LookAt(lastInstance.transform);
        plane.transform.rotation = instance.transform.rotation;
        plane.transform.Rotate(0, 90, 0);
       
        Material newMat = Resources.Load("Banner", typeof(Material)) as Material;
        Renderer renderer = plane.GetComponent<Renderer>();
           renderer.material = newMat;
        renderer.material.SetTextureScale("_MainTex", new Vector2(scale/0.1f/8.8f, 1));
    }
}
