using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Bird : NetworkBehaviour
{

    Rigidbody rb;
    public float force;

    [HideInInspector] public FlappyBird flappyBird;

    [HideInInspector] public UnityEvent hitPipe;

    [HideInInspector] public UnityEvent increaseScore;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            increaseScore.Invoke();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        hitPipe.Invoke();
    }



}
