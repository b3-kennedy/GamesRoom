using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Shop : NetworkBehaviour
{
    GameObject player;
    public Button closeButton;
    public GameObject shopUI;
    public Transform layout;
    GameObject shop;

    void Awake()
    {
        closeButton.onClick.AddListener(CloseShop);
        shop = transform.GetChild(0).gameObject;
        shop.SetActive(false);
    }

    void Start()
    {
        foreach (var item in ItemHolder.Instance.items)
        {
            var name = item.Key;
            var itemObject = item.Value;
            Item itemScript = itemObject.GetComponent<Item>();
            ShopItemUI ui = Instantiate(shopUI, layout).GetComponent<ShopItemUI>();
            ui.nameTMP.text = name;
            ui.priceTMP.text = $"${itemScript.price}";
            ui.itemObject = itemObject;
            ui.buyButton.onClick.AddListener(delegate { BuyItemServerRpc(NetworkManager.Singleton.LocalClientId,itemScript.name, itemScript.price); });
        }
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    void BuyItemServerRpc(ulong clientID, string itemName, float price)
    {
        var player = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
        SteamPlayer steamPlayer = player.GetComponent<SteamPlayer>();
        if(steamPlayer.credits.Value >= price)
        {
            steamPlayer.credits.Value -= (int)price;
            BuyItemClientRpc(clientID, itemName);
        }
        
        
    }
    
    [ClientRpc]
    void BuyItemClientRpc(ulong clientID,string itemName)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID) return;
        
        Item item = ItemHolder.Instance.GetItem(itemName).GetComponent<Item>();
        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        playerObject.GetComponent<ItemManager>().OnPickUpItem(item);
        CheckPrice();
    }
    
    void CheckPrice()
    {
        for (int i = 0; i < layout.childCount; i++)
        {
            ShopItemUI ui = layout.GetChild(i).GetComponent<ShopItemUI>();
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            float price = ui.itemObject.GetComponent<Item>().price;
            var credits = player.GetComponent<SteamPlayer>().credits.Value;

            if (price > credits)
            {
                ui.buyButton.gameObject.SetActive(false);
            }
            else if (credits >= price)
            {
                ui.buyButton.gameObject.SetActive(true);
            }

        }
    }
    
    

    public void OpenShop(GameObject p)
    {
        player = p;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerLook>().enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        shop.SetActive(true);

        CheckPrice();
    }
    
    public void CloseShop()
    {
        if (!player) return;

        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerLook>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player = null;
        shop.SetActive(false);
    }
}
