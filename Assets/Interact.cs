using UnityEngine;

public class Interact : MonoBehaviour
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
}
