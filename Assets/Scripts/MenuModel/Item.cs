using System.Collections.Generic;
public class Item
{
    public string name { get; set; }


    public int[] time;
    public string src;
    public Evidence[] fingerprint;
    public Evidence[] chemical;
    public Evidence[] biological;

    public List<string> test = new List<string>();
    public List<string> compares = new List<string>();
    public Item()
    {

    }
}