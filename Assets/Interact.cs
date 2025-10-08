using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Interact : NetworkBehaviour
{

    public float range = 5f;
    Transform cam;

    public KeyCode interactKey = KeyCode.E;

    NetworkObject playerInteractingWith;

    ulong clientID;
    ulong otherClientID;

    void Start()
    {
        cam = GetComponent<PlayerLook>().normalCamera;
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
                Table table = hit.collider.transform.root.GetComponent<Table>();
                if (machine)
                {
                    machine.SetActivePlayerServerRpc(NetworkManager.Singleton.LocalClientId);
                    machine.arcadeGame.BeginServerRpc(NetworkManager.Singleton.LocalClientId);
                }
                else if (table)
                {
                    Debug.Log(table.tableGame);
                    table.tableGame.BeginServerRpc(NetworkManager.Singleton.LocalClientId);
                }
                
                if(hit.collider.CompareTag("Player"))
                {
                    Debug.Log("hit player");
                    Vector3 direction = (hit.collider.transform.position - transform.position).normalized;
                    ulong id = hit.collider.GetComponent<NetworkObject>().NetworkObjectId;
                    hit.collider.GetComponent<PlayerMovement>().RagdollAndAddForceServerRpc(id, 200, direction);
                }
                
                if(hit.collider.CompareTag("Shop"))
                {

                    hit.collider.GetComponent<Shop>().OpenShop(gameObject);
                }

            }
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
