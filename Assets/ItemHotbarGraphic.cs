using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ItemHotbarGraphic : NetworkBehaviour
{
    [HideInInspector] public Image selectGraphic;
    public bool isSelected;

    public Item item;

    public Transform holdPos;

    [HideInInspector]public GameObject spawnedItem;

    Color normalOutlineColour;

    [HideInInspector] public ItemManager manager;

    void Start()
    {
        selectGraphic = transform.GetChild(0).GetComponent<Image>();
        normalOutlineColour = selectGraphic.color;
    }


    
    // [ServerRpc(RequireOwnership = false)]
    // void SpawnItemServerRpc(ulong clientID, string itemName)
    // {
    //     ulong playerObjectID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.NetworkObjectId;
    //     SpawnItemClientRpc(playerObjectID, itemName, clientID);
    // }
    
    // [ClientRpc]
    // void SpawnItemClientRpc(ulong netObjID, string itemName, ulong clientID)
    // {
    //     if (NetworkManager.Singleton.LocalClientId == clientID) return;
    
    //     if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var player))
    //     {
    //         Transform hand = player.GetComponent<BodyPartManager>().hand;
    //         GameObject item = ItemHolder.Instance.GetItem(itemName);
    //         spawnedItem = Instantiate(item, hand);
    //     }
    // }
    
    // [ServerRpc(RequireOwnership = false)]
    // void ChangeItemVisibilityServerRpc(ulong clientID, bool value)
    // {
    //     ChangeItemVisibilityClientRpc(clientID, value);
    // }
    
    // [ClientRpc]
    // void ChangeItemVisibilityClientRpc(ulong clientID, bool value)
    // {
    //     if (NetworkManager.Singleton.LocalClientId == clientID) return;
    //     if(spawnedItem)
    //     {
    //         spawnedItem.SetActive(value);
    //     }
    // }

    public void OnSelect()
    {
        isSelected = true;
        selectGraphic.color = Color.white;
        if (spawnedItem == null && item)
        {
            spawnedItem = Instantiate(item.gameObject, holdPos);
            spawnedItem.name = item.name;
            manager.SpawnItemServerRpc(manager.OwnerClientId, spawnedItem.name);
        }
        else if (spawnedItem != null)
        {
            spawnedItem.SetActive(true);
        }
    }

    public void OnDeselect()
    {
        isSelected = false;
        selectGraphic.color = normalOutlineColour;
        if(spawnedItem)
        {
            spawnedItem.SetActive(false);
        }
    }
}
