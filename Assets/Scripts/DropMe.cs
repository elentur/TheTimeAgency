using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Image containerImage;
	public Image receivingImage;

    public Sprite defaultSprite;
	private Color normalColor;
	public Color highlightColor = Color.yellow;

    private bool lockImage = false;
    private GameObject obj;

    private  Game game;

    

  
    public void resetSprite()
    {
        receivingImage.overrideSprite = defaultSprite;
    }
    public void lock_Image()
    {
         receivingImage.color = Color.gray;
        
    }
    public void unlock_Image()
    {
   
            receivingImage.color = Color.white;

            if (obj != null)
            {
                obj.GetComponent<Image>().color = Color.white;
                obj.GetComponent<ItemContainer>().isLocked = false;
            }

    }

    public void OnEnable ()
	{
		if (containerImage != null)
			normalColor = containerImage.color;
	}
	
	public void OnDrop(PointerEventData data)
	{
        if (receivingImage.overrideSprite != defaultSprite ) return;
        game = Game.getInstance();
        if (game != null && game.testProgress >= 0) return;
		containerImage.color = normalColor;
		
		if (receivingImage == null)
			return;
		
		Sprite dropSprite = GetDropSprite (data);
        Item item = GetDropItem(data);
        Evidence evidence = GetDropEvidence(data);
		if (dropSprite != null)
        {
            receivingImage.overrideSprite = dropSprite;
            if (item != null)
            {
                Debug.Log(item.name);
                
                if (game == null) return;
                game.addTestItems(item);
                lock_Image();
            } 
            if (evidence != null)
            {
                Debug.Log(evidence.name);
            }
        }
			
	}

   
    public void OnPointerEnter(PointerEventData data)
	{
		if (containerImage == null)
			return;
		
		Sprite dropSprite = GetDropSprite (data);
		if (dropSprite != null)
			containerImage.color = highlightColor;
	}

	public void OnPointerExit(PointerEventData data)
	{
		if (containerImage == null)
			return;
		
		containerImage.color = normalColor;
	}
	
	private Sprite GetDropSprite(PointerEventData data)
	{
		var originalObj = data.pointerDrag;
		if (originalObj == null)
			return null;
		
		var dragMe = originalObj.GetComponent<DragMe>();
		if (dragMe == null)
			return null;
		
		var srcImage = originalObj.GetComponent<Image>();
		if (srcImage == null)
			return null;
		
		return srcImage.sprite;
	}

    private Item GetDropItem(PointerEventData data)
    {

        var originalObj = data.pointerDrag;
        if(originalObj is GameObject)
        {
            obj = originalObj;
            obj.GetComponent<Image>().color = Color.gray;
            obj.GetComponent<ItemContainer>().isLocked = true;
        }
        var dragMe = originalObj.GetComponent<DragMe>();

        ItemContainer container = originalObj.GetComponent<ItemContainer>();
        if (container == null)
            return null;

        return container.item;
    }
    private Evidence GetDropEvidence(PointerEventData data)
    {
        var originalObj = data.pointerDrag;
        var dragMe = originalObj.GetComponent<DragMe>();
        EvidenceContainer container = originalObj.GetComponent<EvidenceContainer>();
        if (container == null)
            return null;

        return container.evidence;
    }
 

}
