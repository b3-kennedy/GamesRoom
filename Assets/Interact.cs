using Unity.Netcode;
using UnityEngine;

public class Interact : NetworkBehaviour
{

    public float range = 5f;
    Transform cam;

    public KeyCode interactKey = KeyCode.E;

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
                    machine.arcadeGame.Begin();
                }
            }
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
