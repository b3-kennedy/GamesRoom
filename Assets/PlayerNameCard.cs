using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerNameCard : NetworkBehaviour
{
    public TextMeshProUGUI nameTMP;
    GameObject player;
    GameObject cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayer != null)
        {
            cam = localPlayer.GetComponent<PlayerLook>().normalCamera.gameObject;
        }
    }

    public override void OnNetworkSpawn()
    {
    
    }
    
    public void SetPlayerName(ulong playerNetID, string name)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetID, out var player))
        {
            PlayerNameCard card = player.transform.GetChild(4).GetComponent<PlayerNameCard>();
            card.nameTMP.text = name;
            Debug.Log(name);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (cam == null) return;

        // Make the name card face the camera
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0);
    }
}
