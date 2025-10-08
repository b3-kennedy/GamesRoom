using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    GameObject player;
    public Button closeButton;

    GameObject shop;

    void Awake()
    {
        closeButton.onClick.AddListener(CloseShop);
        shop = transform.GetChild(0).gameObject;
        shop.SetActive(false);
    }

    public void OpenShop(GameObject p)
    {
        player = p;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerLook>().enabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        shop.SetActive(true);
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
