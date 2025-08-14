using Unity.Netcode;
using UnityEngine;

public class Interact : NetworkBehaviour
{

    public float range = 5f;
    Transform cam;

    public KeyCode interactKey = KeyCode.E;

    public PlayerInteractPanel playerInteractMenu;

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
                Debug.Log(machine);
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
                        playerInteractMenu.title.text = $"Interact With {steamPlayer.playerName}";
                        playerInteractMenu.gameObject.SetActive(true);
                    }
                }

            }
        }
        else if (Input.GetKeyUp(interactKey) && playerInteractMenu.gameObject.activeSelf)
        {
            playerInteractMenu.gameObject.SetActive(false);
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
