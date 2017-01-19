using UnityEngine;
using System.Collections;

public class SuspectScript : MonoBehaviour {

    private Person person;
    void Start()
    {
        person = GetComponent<PersonContainer>().person;
    }

    public void isVictim()
    {
        person.isVictim = !person.isVictim;
    }

    public void isOffender()
    {
        person.isOffender = !person.isOffender;
    }
}
