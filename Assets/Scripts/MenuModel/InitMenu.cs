using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class InitMenu : MonoBehaviour
{
    
    private Game game;

    private GameObject itemList;
    private GameObject evidences;
    private GameObject evidenceList;
    private GameObject testProgressBar;
    private GameObject btnAnalyze;
    private GameObject suspectList;


    
    private GameObject gameMessage;

    private RectTransform testProgress;

    private PanelManager panelManager;
    private PanelManager evidencePanelManager;



    private Animator evidencesMenuAnimator;

    private Text evidenceTitel;
    private Text timeSliderTitel;
    private Text testProgessTitel;

    private GameObject cam;


    private Button pingButton;

    private Slider timeSlider;
    public bool needsRefresh = false;
    // Use this for initialization
    void Awake()
    {
        game = Game.getInstance(0);
    }
    void Start()
    {
        try
        {
           
            Debug.Log("Game initialized");
            itemList = GameObject.Find("ItemList");
            evidences = GameObject.Find("Evidences");
            suspectList = GameObject.Find("SuspectList");
            evidencesMenuAnimator = evidences.GetComponent<Animator>();
            evidenceTitel = GameObject.Find("EvidenceTitleLabel").GetComponent<Text>();
            evidenceList = GameObject.Find("EvidenceList");

            panelManager = gameObject.GetComponent<PanelManager>();
            evidencePanelManager = GameObject.Find("EvidenceManager").GetComponent<PanelManager>();

            pingButton = GameObject.Find("Ping").GetComponent<Button>();
            timeSlider = GameObject.Find("TimeSlider").GetComponent<Slider>();
            timeSlider.onValueChanged.AddListener((a) => slideTime(a));
            timeSliderTitel = GameObject.Find("TimeSliderTitel").GetComponent<Text>();

            gameMessage = GameObject.Find("GameMessage");
            gameMessage.SetActive(false);



            btnAnalyze =  GameObject.Find("BtnAnalyze");
            testProgressBar = GameObject.Find("TestProgressBar");
            testProgress = GameObject.Find("TestProgressHandle").GetComponent<RectTransform>();
            testProgessTitel = GameObject.Find("TestProgessTitel").GetComponent<Text>();
            testProgressBar.SetActive(false);


            //setItems();


            setSuspects();
           

            placeItems();
            timeSlider.value = 2;
                 }
        catch(Exception e) {
            Debug.Log(e.StackTrace);
        }
      //  cam =  Camera.main.transform.FindChild("Camera").gameObject;
    }
 
    internal void addItemButton(string name, Sprite sprite)
    {

        Item item = game.getItem(name);
        if (item == null) return;
        GameObject button = (GameObject)Instantiate(Resources.Load(Path.Combine("Prefabs", "Item")), new Vector3(0.0f, -130.0f * itemList.transform.childCount, 0.0f), Quaternion.identity);
        button.transform.SetParent(itemList.transform, false);
        Button b = button.GetComponent<Button>();
        Item it = item;
        b.onClick.AddListener(() =>
        {
            if (game.getSelectedItem() == it) game.setSelectedItem(null);
            else
            {
                if (evidencesMenuAnimator.GetBool("Open")) evidencePanelManager.OpenPanel(evidencesMenuAnimator);
                game.setSelectedItem(it);
            }
            evidencePanelManager.OpenPanel(evidencesMenuAnimator);
            needsRefresh = true;
        });

        ItemContainer ic = button.GetComponentInChildren<ItemContainer>();
        ic.item = item;
        GameObject image = button.transform.Find("Image").gameObject;
        image.GetComponent<Image>().sprite = sprite;


        RectTransform rt = itemList.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(156.0f, itemList.transform.childCount * 130);

    }

    private void placeItems()
    {

        Game game = Game.getInstance();
        foreach (Item item in game.getItems())
        {

            try
            {
                if (item.src != null)
                {
                    GameObject i = (GameObject)Instantiate(Resources.Load(Path.Combine("Prefabs", item.src)), new Vector3(0, 0,0), Quaternion.identity);
                    i.SetActive(false);
                    i.name = item.name;
                    game.addItemGameObjects(i);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Fehler beim setzen von " + item.name);
            }
        }
    }

    void slideTime(float a)
    {
        game.setTime((int)a);
        timeSliderTitel.text = "Zeit: " + game.getRealTime();
        foreach (GameObject obj in game.getItemGameObjects())
        {
            Item item = game.getItem(obj.name);
            for (int i = 0; i < item.fingerprint.Length; i++)
            {
                Transform t = obj.transform.Find("Fingerprint" + i);
                if (t != null)
                {

                    if (!item.fingerprint[i].time.Contains(game.getTime()))
                    {
                        t.gameObject.SetActive(false);
                    }
                    else
                    {
                        t.gameObject.SetActive(true);
                    }
                }
            }
            for (int i = 0; i < item.biological.Length; i++)
            {
                Transform t = obj.transform.Find("Biological" + i);
                if (t != null)
                {

                    if (!item.biological[i].time.Contains(game.getTime()))
                    {
                        t.gameObject.SetActive(false);
                    }
                    else
                    {
                        t.gameObject.SetActive(true);
                    }
                }
            }
            for (int i = 0; i < item.chemical.Length; i++)
            {
                Transform t = obj.transform.Find("Chemical" + i);
                if (t != null)
                {

                    if (!item.chemical[i].time.Contains(game.getTime()))
                    {
                        t.gameObject.SetActive(false);
                    }
                    else
                    {
                        t.gameObject.SetActive(true);
                    }
                }
            }
        }
    }


    void setSuspects()
    {
        float counter = 0.0f;
        foreach (Person person in game.getPersons())
        {
            GameObject suspectCont = (GameObject)Instantiate(Resources.Load(Path.Combine("Prefabs", "Suspect")), new Vector3(0.0f, -350.0f * counter, 0.0f), Quaternion.identity);
            suspectCont.transform.SetParent(suspectList.transform, false);
            suspectCont.transform.Find("Name").GetComponent<Text>().text = person.name;

            PersonContainer pc = suspectCont.GetComponentInChildren<PersonContainer>();
            pc.person = person;
            counter++;
        }
        RectTransform rt = suspectList.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(490.0f, suspectList.transform.childCount * 350);

    }

    // Update is called once per frame
    void Update()
    {
        if (needsRefresh) refresh();
        if (game.testProgress < 0 && game.preTest)
        {
                InvokeRepeating("test_Items", 0.0f, 0.1f*game.getTestItems().Count);
        }
      //  Debug.Log(Camera.main.GetComponent<Camera>().fieldOfView + "  " + cam.GetComponent<Camera>().fieldOfView);
    }
    public void test_Items()
    {
        game.test_Items();
        testProgressBar.SetActive(true);
            testProgress.sizeDelta = new Vector2(400.0f * (game.testProgress /100.0f), 0.0f);
           testProgessTitel.text = game.testProgress + "%";
   
        if (game.testProgress == -1)
        {
            CancelInvoke("test_Items");
            testProgressBar.SetActive(false);
            DropMe script = GameObject.Find("Test1Image").GetComponent<DropMe>();
            script.resetSprite();
            script.unlock_Image();
            script = GameObject.Find("Test2Image").GetComponent<DropMe>();
            script.resetSprite();
            script.unlock_Image();
            needsRefresh = true;
        }
    }
    private void refresh()
    {
        var children = new List<GameObject>();
        foreach (Transform child in evidenceList.transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));

        if (game.getSelectedItem() == null) return;
        evidenceTitel.text = "Hinweise\n " + game.getSelectedItem().name;

        addEvidences();

        needsRefresh = false;
    }
    void addEvidences()
    {
        float counter = 0.0f;
        foreach (Evidence e in game.getEvidenceList())
        {
            GameObject button = (GameObject)Instantiate(Resources.Load(Path.Combine("Prefabs", "EvidenceItem")), new Vector3(0.0f, -160.0f * counter, 0.0f), Quaternion.identity);
            button.transform.SetParent(evidenceList.transform, false);
            Debug.Log(button);
            Text text = button.GetComponentInChildren<Text>();
            text.text = e.text;
            EvidenceContainer ec = button.GetComponent<EvidenceContainer>();
            ec.evidence = e;
            counter++;
        }
        RectTransform rt = evidenceList.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(490.0f, evidenceList.transform.childCount * 160);

    }
    public void solve()
    {
        if (evidencesMenuAnimator.GetBool("Open")) evidencePanelManager.OpenPanel(evidencesMenuAnimator);
        GameStateMessage m = game.validate();
        panelManager.CloseCurrent();
        GameObject.Find("BarrierManager").GetComponent<PanelManager>().CloseCurrent();
        GameObject.Find("BarrierManager").GetComponent<PanelManager>().CloseCurrent();
        gameMessage.SetActive(true);
        Text[] texts = gameMessage.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            Debug.Log(t.name);
            if (t.name == "Titel")
            {
                if (m.win) t.text = "Du hast gewonnen!";
                else t.text = "Du hast verloren!";
            }
            else if (t.name == "Message")
            {
                t.text = m.message;
            }
        }


        
    }

    public void endGame()
    {
        SceneManager.LoadScene(0);
    }
}
