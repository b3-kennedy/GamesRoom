using UnityEngine;

namespace Assets.Football
{
    public class GhostBall : MonoBehaviour
    {
        [Header("Reconciliation Settings")]
        public float positionCorrectionSpeed = 10f;
        public float velocityCorrectionRate = 5f;
        public float snapThreshold = 2f;

        private Rigidbody rb;
        private Ball serverBall;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false; // now uses physics
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public void BindToServerBall(Ball server)
        {
            serverBall = server;

            // Prevent colliding with the server ball
            Physics.IgnoreCollision(server.GetComponent<Collider>(), GetComponent<Collider>());
        }

        void FixedUpdate()
        {
            if (serverBall == null) return;

            // Reconcile with server ball
            Vector3 posDiff = serverBall.transform.position - transform.position;
            if (posDiff.magnitude > snapThreshold)
            {
                // Snap if too far
                rb.position = serverBall.transform.position;
                rb.linearVelocity = serverBall.GetComponent<Rigidbody>().linearVelocity;
            }
            else
            {
                // Smoothly adjust
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, serverBall.GetComponent<Rigidbody>().linearVelocity, velocityCorrectionRate * Time.fixedDeltaTime);
                rb.position = Vector3.Lerp(rb.position, serverBall.transform.position, positionCorrectionSpeed * Time.fixedDeltaTime);
            }
        }

        public void ApplyLocalKick(Vector3 impulse)
        {
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}

