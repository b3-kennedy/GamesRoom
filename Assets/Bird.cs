using UnityEngine;

public class Bird : MonoBehaviour
{

    Rigidbody rb;
    public float force;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!gameObject.activeSelf) return;

        if (Input.GetButtonDown("Fire1"))
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(transform.up * force, ForceMode.Impulse);
        }
    }
}
