using Assets.CreditClicker;
using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{

    public Transform itemUIParent;
    [HideInInspector] public ItemHotbarGraphic[] itemSlots;

    ItemHotbarGraphic prevSlot;
    ItemHotbarGraphic selectedSlot;
    int index;

    public Transform hand;

    public Item hammer;
    public Item soundboard;

    [HideInInspector] public bool canSwitch = true;
    public override void OnNetworkSpawn()
    {
        itemSlots = new ItemHotbarGraphic[5];
        itemUIParent = transform.Find("PlayerUI").transform.GetChild(0);
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = itemUIParent.GetChild(i).GetComponent<ItemHotbarGraphic>();
            itemSlots[i].GetComponent<ItemHotbarGraphic>().holdPos = hand;
            itemSlots[i].GetComponent<ItemHotbarGraphic>().manager = this;
            itemSlots[i].GetComponent<ItemHotbarGraphic>().index = i;
        }
        
        if(!IsOwner)
        {
            itemUIParent.gameObject.SetActive(false);
        }
    }
    
    public void OnPickUpItem(Item item)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            ItemHotbarGraphic slot = itemSlots[i];
            if(slot.item == null)
            {
                slot.item = item;
                slot.item.cam = GetComponent<PlayerLook>().normalCamera;
                slot.item.player = gameObject;
                slot.item.anim = GetComponent<PlayerMovement>().anim;
                return;
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        if(canSwitch)
        {
            SlotSelection();
        }
        
        if(selectedSlot != null && selectedSlot.item != null)
        {
            if(Input.GetButtonDown("Fire1"))
            {
                selectedSlot.spawnedItem.GetComponent<Item>().Use();
            }
            
            if(Input.GetButtonDown("Fire2"))
            {
                selectedSlot.spawnedItem.GetComponent<Item>().isAltUse = true;
            }
            else if(Input.GetButtonUp("Fire2"))
            {
                selectedSlot.spawnedItem.GetComponent<Item>().isAltUse = false;
            }
            
            
            
        }
    }
    
    public Item GetItemInSlot(int index)
    {
        return itemSlots[index].item;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnItemServerRpc(ulong clientID, string itemName, int slotIndex)
    {
        ulong playerObjectID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.NetworkObjectId;
        SpawnItemClientRpc(playerObjectID, itemName, clientID, slotIndex);
    }

    [ClientRpc]
    void SpawnItemClientRpc(ulong netObjID, string itemName, ulong clientID, int slotIndex)
    {
        if (NetworkManager.Singleton.LocalClientId == clientID) return;

        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var player))
        {
            Transform hand = player.GetComponent<BodyPartManager>().hand;
            GameObject item = ItemHolder.Instance.GetItem(itemName);
            GameObject newItem = Instantiate(item, hand);
            player.GetComponent<ItemManager>().itemSlots[slotIndex].spawnedItem = newItem;
            player.GetComponent<ItemManager>().itemSlots[slotIndex].item = newItem.GetComponent<Item>();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeItemVisibilityServerRpc(ulong clientID, bool value, int slotIndex)
    {
        ulong playerObjectID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.NetworkObjectId;
        ChangeItemVisibilityClientRpc(playerObjectID, clientID, value, slotIndex);
    }

    [ClientRpc]
    void ChangeItemVisibilityClientRpc(ulong netObjID, ulong clientID, bool value, int slotIndex)
    {
        if (NetworkManager.Singleton.LocalClientId == clientID) return;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var player))
        {
            ItemHotbarGraphic slot = player.GetComponent<ItemManager>().itemSlots[slotIndex];
            if(value)
            {
                slot.spawnedItem.SetActive(true);
            }
            else
            {
                slot.spawnedItem.SetActive(false);
            }
        }


    }

    void SlotSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            index = 0;
            SelectSlot(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            index = 1;
            SelectSlot(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            index = 2;
            SelectSlot(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            index = 3;
            SelectSlot(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            index = 4;
            SelectSlot(index);
        }


        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll < 0)
        {
            index++;
            if (index >= 5)
            {
                index = 0;
            }

            SelectSlot(index);
        }
        else if (scroll > 0)
        {
            index--;
            if (index < 0)
            {
                index = 4;
            }
            SelectSlot(index);
        }
    }

    void SelectSlot(int index)
    {
        if (selectedSlot != null)
        {
            if(selectedSlot.item != null)
            {
                selectedSlot.item.OnUnequip();
            }
            
            selectedSlot.OnDeselect();
        }

        selectedSlot = itemSlots[index];
        selectedSlot.OnSelect();
        if(selectedSlot.item != null)
        {
            selectedSlot.item.OnEquip();
        }
        
    }
}
