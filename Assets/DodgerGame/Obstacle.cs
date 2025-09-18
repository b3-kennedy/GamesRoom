using Unity.Netcode;
using UnityEngine;

public class Obstacle : NetworkBehaviour
{

    Rigidbody rb;

    public float speed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(0, -speed, 0);
    }
}
