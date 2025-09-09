using Unity.Netcode;
using UnityEngine;


namespace Assets.Football
{
    public class Ball : NetworkBehaviour
    {
        Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (!IsServer) return;

            // Ball physics is server-authoritative
            // Periodically sync state to clients
            SyncBallStateClientRpc(rb.position, rb.linearVelocity);
        }

        [ClientRpc]
        void SyncBallStateClientRpc(Vector3 position, Vector3 velocity)
        {
            if (IsServer) return; // host already authoritative

            rb.position = position;
            rb.linearVelocity = velocity;
        }
    }
}

