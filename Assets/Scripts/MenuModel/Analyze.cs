using UnityEngine;
using System.Collections;

public class Analyze : MonoBehaviour {
    Game game;
    public GameObject btn;
    public GameObject wheatley;
    // Use this for initialization
    void Start () {
         game = Game.getInstance();
    }
	
	// Update is called once per frame
    void Update()
    {
        if (game != null && !game.preTest && !btn.activeSelf)
        {
            // wheatley.SetActive(false);
           // wheatley.GetComponent<WheatleyScript>().randomPosition();

            btn.SetActive(true);
        }
    }
    public void startAnalyze()
    {
        if (game != null && game.getTestItems().Count > 0)
        {
            game.preTest = true;
            btn.SetActive(false);
            wheatley.GetComponent<WheatleyScript>().state = 0;
            wheatley.GetComponent<WheatleyScript>().randomDistance = -1;




        }
    }

}
