using System.Runtime.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.ArcherBattle
{
    public class Arrow : NetworkBehaviour
    {

        Rigidbody rb;
        bool hasHit = false;

        [HideInInspector] public UnityEvent Hit;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (hasHit) return;

            Vector3 velocity = rb.linearVelocity;

            if (velocity != Vector3.zero)
            {
                // Make +Z point in travel direction
                Quaternion lookRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);

                // Rotate extra 90° so +X (right) becomes the forward nose
                transform.rotation = lookRot * Quaternion.Euler(0f, -90f, 0f);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!IsServer) return;

            Hit.Invoke();
            rb.isKinematic = true;
            hasHit = true;
        }
    }
}

