using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{

    Transform itemUIParent;
    ItemHotbarGraphic[] itemSlots;
    ItemHotbarGraphic selectedSlot;
    int index;

    public Item hammer;
    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            itemSlots = new ItemHotbarGraphic[5];
            itemUIParent = GameObject.Find("PlayerUI").transform.GetChild(0);
            for (int i = 0; i < itemSlots.Length; i++)
            {
                itemSlots[i] = itemUIParent.GetChild(i).GetComponent<ItemHotbarGraphic>();
            }

            OnPickUpItem(hammer);
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
                return;
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        
        SlotSelection();
        if(selectedSlot != null && selectedSlot.item != null)
        {
            if(Input.GetButtonDown("Fire1"))
            {
                selectedSlot.item.Use();
            }
            else if(Input.GetButtonDown("Fire2"))
            {
                selectedSlot.item.AltUse();
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

        if (scroll > 0)
        {
            index++;
            if (index >= 5)
            {
                index = 0;
            }

            SelectSlot(index);
        }
        else if (scroll < 0)
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
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if(i == index)
            {
                itemSlots[i].OnSelect();
                selectedSlot = itemSlots[i];
            }
            else
            {
                itemSlots[i].OnDeselect();
            }
        }
    }
}
