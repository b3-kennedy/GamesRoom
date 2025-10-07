using UnityEngine;
using UnityEngine.UI;

public class ItemHotbarGraphic : MonoBehaviour
{
    [HideInInspector] public Image selectGraphic;
    public bool isSelected;

    public Item item;

    Color normalOutlineColour;

    void Start()
    {
        selectGraphic = transform.GetChild(0).GetComponent<Image>();
        normalOutlineColour = selectGraphic.color;
    }

    public void OnSelect()
    {
        isSelected = true;
        selectGraphic.color = Color.white;
    }
    
    public void OnDeselect()
    {
        isSelected = false;
        selectGraphic.color = normalOutlineColour;
    }
}
