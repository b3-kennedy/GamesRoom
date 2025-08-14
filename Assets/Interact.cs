using Unity.Netcode;
using UnityEngine;

public class Interact : NetworkBehaviour
{

    public float range = 5f;
    Transform cam;

    public KeyCode interactKey = KeyCode.E;

    public PlayerInteractPanel playerInteractMenu;

    NetworkObject playerInteractingWith;

    ulong clientID;
    ulong otherClientID;

    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
            {
                ArcadeMachine machine = hit.collider.GetComponent<ArcadeMachine>();
                if (machine)
                {
                    machine.arcadeGame.BeginServerRpc(NetworkManager.Singleton.LocalClientId);
                }

            }
        }
        if (Input.GetKey(interactKey))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    SteamPlayer steamPlayer = hit.collider.GetComponent<SteamPlayer>();
                    if (!playerInteractMenu.gameObject.activeSelf)
                    {
                        playerInteractingWith = hit.collider.GetComponent<NetworkObject>();
                        clientID = NetworkManager.Singleton.LocalClientId;
                        otherClientID = playerInteractingWith.OwnerClientId;
                        
                        playerInteractMenu.title.text = $"Interact With {steamPlayer.playerName}";
                        playerInteractMenu.clientID = NetworkManager.Singleton.LocalClientId;
                        playerInteractMenu.otherClientID = playerInteractingWith.OwnerClientId;
                        playerInteractMenu.gameObject.SetActive(true);
                        Cursor.lockState = CursorLockMode.Confined;
                        
                    }
                }

            }
        }
        else if (Input.GetKeyUp(interactKey) && playerInteractMenu.gameObject.activeSelf)
        {
            playerInteractMenu.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            playerInteractingWith = null;
        }

    }

    public void RockPaperScissorsInvite()
    {
        RockPaperScissorsInviteServerRpc(otherClientID, clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RockPaperScissorsInviteServerRpc(ulong receiverClientID, ulong senderClientID)
    {
        RockPaperScissorsInviteClientRpc(receiverClientID, senderClientID);
    }

    [ClientRpc]
    void RockPaperScissorsInviteClientRpc(ulong receiverClientID, ulong senderClientID)
    {
        if (NetworkManager.Singleton.LocalClientId == receiverClientID)
        {
            var senderPlayer = NetworkManager.Singleton.ConnectedClients[senderClientID].PlayerObject;
            var senderName = senderPlayer.GetComponent<SteamPlayer>().playerName;
            Debug.Log($"Received a rps invite from {senderName}");
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ArcadeMachine"))
        {
            ArcadeMachine machine = other.GetComponent<ArcadeMachine>();
            if (machine)
            {
                machine.TurnOnServerRpc();
                if (!machine.nearPlayers.Contains(gameObject))
                {
                    machine.nearPlayers.Add(gameObject);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ArcadeMachine"))
        {
            ArcadeMachine machine = other.GetComponent<ArcadeMachine>();
            if (machine)
            {
                if (machine.nearPlayers.Contains(gameObject))
                {
                    machine.nearPlayers.Remove(gameObject);
                }
            }
        }
    }
}
