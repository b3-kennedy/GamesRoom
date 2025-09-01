using UnityEngine;

public class Arrow : MonoBehaviour
{

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }
    }
}
