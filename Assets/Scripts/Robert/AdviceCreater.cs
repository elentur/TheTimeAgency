using UnityEngine;
using System.Collections;
using Assets.TheTimeAgency.Scripts;

public class AdviceCreater {

    private readonly GameObject _pingBox;
    private readonly int _numberAdvices;
    private readonly GameObject AdvicePlaceHolder;

    public AdviceCreater(int numberAdvices, GameObject aph)
    {
        _numberAdvices = numberAdvices;
        AdvicePlaceHolder = aph;
        _pingBox = new GameObject("pingBox");
        _pingBox.transform.SetParent(AdvicePlaceHolder.transform.parent.gameObject.transform, false);
    }

    public GameObject[] fillAdviceList()
    {

        GameObject[] AdviceList = new GameObject[_numberAdvices];

        var average = Vector3.zero;
        var color = Random.ColorHSV();

        for (int i = 0; i < _numberAdvices; i++)
        {
            GameObject advice = AddCube("advice_" + average.x + "/" + average.y + "/" + average.z, average, color, new Vector3(10, 10, 10));

            AdvicePlaceHolder aph = advice.GetComponent<AdvicePlaceHolder>();
            aph.Importance = i;
            advice.SetActive(false);
            AdviceList[i] = advice;
        }

        return AdviceList;
    }

    private GameObject AddCube(string name, Vector3 position, Color color, Vector3 scale)
    {
        // copy of the maker
        GameObject cube = Object.Instantiate<GameObject>(AdvicePlaceHolder);

        cube.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        cube.transform.SetParent(_pingBox.transform, false);
        // Place the marker at the center of the screen at the found floor height.

        // adding a Colider for ping state
        BoxCollider bc = (BoxCollider)cube.gameObject.AddComponent(typeof(BoxCollider));
        bc.center = Vector3.zero;

        cube.transform.position = position;

        cube.transform.localScale = scale;

        cube.GetComponent<Renderer>().material.color = color;

        cube.SetActive(false);

        // Debug.Log(string.Format("Cube {0} set on {1}", cubeCopy.name, cubeCopy.transform.position));

        return cube;
    }
}
