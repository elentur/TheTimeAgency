using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour {

	public Animator initiallyOpen;

	private int m_OpenParameterId;
	private Animator m_Open;
	private GameObject m_PreviouslySelected;
    private GameObject barrier;

	const string k_OpenTransitionName = "Open";
	const string k_ClosedStateName = "Closed";

	public void OnEnable()
	{
        
            GameObject obj = GameObject.Find("SetBarrierInterface");
           if(obj != null) barrier = obj.gameObject;
    

        m_OpenParameterId = Animator.StringToHash (k_OpenTransitionName);

		if (initiallyOpen == null)
			return;
        Debug.Log("Inititaly Open: " + initiallyOpen.gameObject.name);
		OpenPanel(initiallyOpen);
	}

	public void OpenPanel (Animator anim)
	{

        if (m_Open == anim)
        {
            CloseCurrent();
            m_Open = null;  
            return;
        }


        anim.gameObject.SetActive(true);
		var newPreviouslySelected = EventSystem.current.currentSelectedGameObject;

		anim.transform.SetAsLastSibling();
      
	    CloseCurrent();
        m_PreviouslySelected = newPreviouslySelected;
       
        m_Open = anim;
		m_Open.SetBool(m_OpenParameterId, !m_Open.GetBool(m_OpenParameterId));

		GameObject go = FindFirstEnabledSelectable(anim.gameObject);
    
        SetSelected(go);
      //  m_Open = null;
    }

	static GameObject FindFirstEnabledSelectable (GameObject gameObject)
	{
		GameObject go = null;
		var selectables = gameObject.GetComponentsInChildren<Selectable> (true);
		foreach (var selectable in selectables) {
			if (selectable.IsActive () && selectable.IsInteractable ()) {
				go = selectable.gameObject;
				break;
			}
		}
		return go;
	}
    public void CloseCurrent()
    {
        CloseCurrent(m_Open);
    }
    public void CloseCurrent(Animator m_Open)
	{
		if (m_Open == null)
			return;
        if(barrier != null && barrier.GetComponent<Animator>()  == m_Open)
        {
            m_Open = null;
            Destroy(barrier);
            barrier = null;
            return;
        }
		m_Open.SetBool(m_OpenParameterId, false);
		SetSelected(m_PreviouslySelected);
		StartCoroutine(DisablePanelDeleyed(m_Open));
		m_Open = null;
	}

	IEnumerator DisablePanelDeleyed(Animator anim)
	{
		bool closedStateReached = false;
		bool wantToClose = true;
		while (!closedStateReached && wantToClose)
		{
			if (!anim.IsInTransition(0))
				closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

			wantToClose = !anim.GetBool(m_OpenParameterId);

			yield return new WaitForEndOfFrame();
		}

		if (wantToClose)
			anim.gameObject.SetActive(false);
	}

	private void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
	}
}
