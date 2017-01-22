// GameDoc.cs
// compile with: /doc:XMLsample.xml
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


public class Game
{

    //*************ATTRIBUTES************//
    public struct Attributes
    {
        public string name;
        public string[] items;
        public string[] persons;
        public string[] time;
        public solution[] solutions;
    }
    public struct solution
    {
        public string[] offenders;
        public string[] victims;
    }
    public Attributes[] gameAttributes;

    private Item[] items;
    private Person[] persons;

    private List<GameObject> itemgamObjects = new List<GameObject>();
    private static Game instance = null;
    private static int game = 0;

    private Item selectedItem;
    private List<Item> testItems = new List<Item>();

    private int timeValue;

    public bool preTest = false;

    public int testProgress = -1;
    //*************CONSTRUCTOR************//
    private Game()
    {

    }
    //*************GETER&SETTER************//
    public static Game getInstance(int game)
    {
        if (instance == null)
        {
            TextAsset file = Resources.Load(Path.Combine("GameItems", "Games")) as TextAsset;
            instance = Newtonsoft.Json.JsonConvert.DeserializeObject<Game>(file.text);
        }
        instance.setGame(game);
        return instance;
    }
    public static Game getInstance()
    {
        return instance;
    }


    public List<GameObject> getItemGameObjects()
    {
        return itemgamObjects;
    }
    public GameObject getGameObjectFor(Item item)
    {
        for(int i = 0; i < itemgamObjects.Count; i++)
        {
            if (items[i] == item) return itemgamObjects[i];
        }
        return null;
    }
    
    public void addItemGameObjects(GameObject obj)
    {
        itemgamObjects.Add(obj);
    }
    public int itemLength()
    {
        return items.Length;
    }
    public Item[] getItems()
    {
        return items;
    }
    public int personLength()
    {
        return persons.Length;
    }
    public Person[] getPersons()
    {
        return persons;
    }
    public Item getSelectedItem()
    {
        return selectedItem;
    }

    public void setSelectedItem(Item item)
    {
        selectedItem = item;
    }
    public List<Item> getTestItems()
    {
        return testItems;
    }

    public void addTestItems(Item item)
    {
        if (testItems.Count < 2 && !testItems.Contains(item))
        {
            testItems.Add(item);
        }
    }
    public void clearTestItems()
    {
        testItems.Clear();
    }
    public void setTime(int time)
    {
        this.timeValue = time;
    }
    public string getRealTime()
    {
        return gameAttributes[game].time[timeValue];
    }
    public int getTime()
    {
        return this.timeValue;
    }
    //*************PUBLIC FUNCTIONS************//

        //Validates Game Solution 
    public GameStateMessage validate()
    {

        foreach (Person p in persons)
        {
            if (p.isOffender)
            {
                bool isOffender = false;
                foreach (Game.solution solution in gameAttributes[game].solutions)
                {
                    if (solution.offenders.Contains(p.name)) isOffender = true;
                }
                if (!isOffender) return new GameStateMessage("Der Täter ist Falsch.");
            }
            if (p.isVictim)
            {
                bool isVictim = false;
                foreach (Game.solution solution in gameAttributes[game].solutions)
                {
                    if (solution.victims.Contains(p.name)) isVictim = true;
                }
                if (!isVictim) return new GameStateMessage("Ein Opfer ist Falsch.");
            }
            if (!p.isOffender && !p.isVictim) return new GameStateMessage("Jede Person muss entweder Täter und/oder Opfer sein.");
            
        }
        return new GameStateMessage(true, "Du hast eine richtige Lösung gefunden.");
    }
    /// <summary>
    /// Returns for the selected Item a List of strings with
    /// all known evidences
    /// </summary>
    /// <returns>List of strings</returns>
    public List<Evidence> getEvidenceList()
    {
        List<Evidence> returnValue = new List<Evidence>();
        returnValue.AddRange(setEvidence(selectedItem.fingerprint));
        returnValue.AddRange(setEvidence(selectedItem.chemical));
        returnValue.AddRange(setEvidence(selectedItem.biological));
        foreach (string s in selectedItem.compares)
        {
            Evidence info = new Evidence();
            info.name = "info";
            info.text = s;
            returnValue.Add(info);
        }
        return returnValue;
    }


    /// <summary>
    /// This function returns a specific item for a name
    /// If there are multible items with the same name it returns the first item
    /// </summary>
    /// <param name="name"> The search string</param>
    /// <returns>the item that was found or null if there is no item for this name</returns>
    public Item getItem(string name)
    {
        foreach (Item i in items)
        {
            if (i.name == name)
            {
                return i;
            }
        }
        return null;
    }

    /// <summary>
    /// Tests all Items that ar addet to testList (1 or 2)
    /// </summary>
    public void test_Items()
    {
        testProgress++;

        if(testProgress > 100)
        {
            testProgress = -1;
            preTest = false;
             if (testItems.Count == 1) setUpSingleTest();
             else if (testItems.Count == 2) setUpDoubleTest();
            clearTestItems();
        }

    }
    //*************PRIVATE FUNCTIONS************//

    private List<Evidence> setEvidence(Evidence[] evidence)
    {
        List<Evidence> returnValue = new List<Evidence>();
        for (int i = 0; i < evidence.Length; i++)
        {
            if (evidence[i].status == 1 && evidence[i].time.Contains(timeValue))
            {
                evidence[i].text = evidence[i].name;
                if (evidence[i].test != null && selectedItem.test.Contains(evidence[i].test)) evidence[i].text += " - Test:" + evidence[i].test;
                returnValue.Add(evidence[i]);
            }
        }
        return returnValue;
    }
    private void setUpSingleTest()
    {
        Item item = (Item)testItems[0];
        atItemsToTestlist(item, item.fingerprint);
        atItemsToTestlist(item, item.chemical);
        atItemsToTestlist(item, item.biological);
    }
    private void setUpDoubleTest()
    {
        Item item1 = testItems[0];
        Item item2 = testItems[1];
        matchEvidence(item1, item2);
        matchEvidence(item1, item2, 1);
        matchEvidence(item1, item2, 2);

    }
    private void matchEvidence(Item item1, Item item2, int t = 0)
    {
        Evidence[] e1 = item1.fingerprint;
        Evidence[] e2 = item2.fingerprint;
        if (t == 1)
        {
            e1 = item1.chemical;
            e2 = item2.chemical;
        }
        else if (t == 2)
        {
            e1 = item1.biological;
            e2 = item2.biological;
        }
        // Evidence[] e1 = item1.fingerprint.Concat(item1.biological).Concat(item1.chemical).ToArray();
        //Evidence[] e2 = item2.fingerprint.Concat(item2.biological).Concat(item2.chemical).ToArray();


        if (e1.Length <= 0 || e2.Length <= 0) return;

        if (e1.Length == 1 && e2.Length == 1)
        {
            if (e1[0].status != 0 && e2[0].status != 0 &&
                e1[0].time.Contains(timeValue) && e2[0].time.Contains(timeValue))
            {
                string not = "nicht";
                string name="";
                if (e1[0].person == e2[0].person)
                {
                    not = "";
                    if (e1[0].name.Contains("Fingerabdruck")) name = " Die DNA Spuren reichen für eine Bestimmung: " + e1[0].person; ;
                }
                string answer = e2[0].name + " von " + item2.name + " ist " + not + " identisch mit " + e1[0].name + " von " + item1.name + ". "+name;

                if (!item1.compares.Contains(answer)) item1.compares.Add(answer);
                if (!item2.compares.Contains(answer)) item2.compares.Add(answer);
            }
        }
        else
        {
            bool identity = false;
            foreach (Evidence ev1 in e1)
            {
                if (ev1.time.Contains(timeValue))
                {
                    foreach (Evidence ev2 in e2)
                    {
                        if (ev2.time.Contains(timeValue))
                        {
                            if (ev1.person == ev2.person)
                            {
                                identity = true;
                                break;
                            }
                        }
                    }
                }
                if (identity) break;
            }
            string not = " keine";
            if (identity)
            {
                not = " auch";
            }
            string type = "Biologische Spuren";
            if (t == 0) type = "Abdrücke";
            if (t == 1) type = "Chemische Spuren";
            string answer = type + " die an " + item1.name + " vorhanden sind, sind " + not + " an " + item2.name + " vorhanden.";
            if (!item1.compares.Contains(answer)) item1.compares.Add(answer);
            if (!item2.compares.Contains(answer)) item2.compares.Add(answer);
        }


    }
    private void atItemsToTestlist(Item item, Evidence[] evidence)
    {
        foreach (Evidence ev in evidence)
        {
            if (!item.test.Contains(ev.test) &&
                ev.time.Contains(timeValue) &&
                ev.test != null &&
                ev.status != 0
                ) item.test.Add(ev.test);

        }
    }
    private void setGame(int game)
    {
   
        //TextAsset t = Resources.Load("item" + i) as TextAsset;
        Game.game = game;
        try
        {
            items = new Item[gameAttributes[game].items.Length];
            for (int i = 0; i < gameAttributes[game].items.Length; i++)
            {
                TextAsset file = Resources.Load(Path.Combine("GameItems", gameAttributes[game].items[i])) as TextAsset;
                items[i] = Newtonsoft.Json.JsonConvert.DeserializeObject<Item>(file.text);
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Fehler beim einlesen der Items\n" + e.StackTrace);
        }
        try
        {
            persons = new Person[gameAttributes[game].persons.Length];
            for (int i = 0; i < gameAttributes[game].persons.Length; i++)
            {
                TextAsset file = Resources.Load(Path.Combine("GameItems", gameAttributes[game].persons[i])) as TextAsset;

                persons[i] = Newtonsoft.Json.JsonConvert.DeserializeObject<Person>(file.text);
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Fehler beim einlesen der Personen\n" + e.StackTrace);
        }

    }
}

