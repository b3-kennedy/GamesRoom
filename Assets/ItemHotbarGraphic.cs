using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ItemHotbarGraphic : MonoBehaviour
{
    [HideInInspector] public Image selectGraphic;
    public bool isSelected;

    public Item item;

    public Transform holdPos;

    [HideInInspector] public int index;

    [HideInInspector]public GameObject spawnedItem;

    Color normalOutlineColour;

    [HideInInspector] public ItemManager manager;

    void Start()
    {
        selectGraphic = transform.GetChild(0).GetComponent<Image>();
        normalOutlineColour = selectGraphic.color;
    }

    public void OnSelect()
    {
        isSelected = true;
        selectGraphic.color = Color.white;
        if (spawnedItem == null && item)
        {
            spawnedItem = Instantiate(item.gameObject, holdPos);
            spawnedItem.name = item.name;
            manager.SpawnItemServerRpc(manager.OwnerClientId, spawnedItem.name, index);
        }
        else if (spawnedItem != null)
        {
            manager.ChangeItemVisibilityServerRpc(manager.OwnerClientId, true, index);
            spawnedItem.SetActive(true);
        }
    }

    public void OnDeselect()
    {
        isSelected = false;
        selectGraphic.color = normalOutlineColour;
        if(spawnedItem)
        {
            manager.ChangeItemVisibilityServerRpc(manager.OwnerClientId, false, index);
            spawnedItem.SetActive(false);
        }
    }
}
