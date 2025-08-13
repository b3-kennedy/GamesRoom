using Unity.Netcode;
using UnityEngine;

public class Bird : NetworkBehaviour
{

    Rigidbody rb;
    public float force;

    FlappyBird flappyBird;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flappyBird = transform.parent.parent.GetComponent<FlappyBird>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (!gameObject.activeSelf) return;

        if (Input.GetButtonDown("Fire1"))
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(transform.up * force, ForceMode.Impulse);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        
        if (other.CompareTag("FlappyBirdScore"))
        {
            flappyBird.IncreaseScoreServerRpc();
        }
    }
}
