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
            // Make +Z point in travel direction
            Quaternion lookRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            // Rotate extra 90° so +X (right) becomes the forward nose
            transform.rotation = lookRot * Quaternion.Euler(0f, -90f, 0f);
        }
    }
}
