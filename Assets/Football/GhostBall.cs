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
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public void BindToServerBall(Ball server)
        {
            serverBall = server;
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Ignore collisions with server ball
            Physics.IgnoreCollision(server.GetComponent<Collider>(), rb.GetComponent<Collider>());
        }

        void FixedUpdate()
        {
            if (serverBall == null) return;

            // Reconcile with server ball
            float posDiff = Vector3.Distance(serverBall.transform.position, transform.position);
            if (posDiff > snapThreshold)
            {
                // Snap if too far
                rb.position = serverBall.transform.position;
                rb.linearVelocity = serverBall.GetComponent<Rigidbody>().linearVelocity;
            }
            else
            {
                // Smoothly adjust
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, serverBall.GetComponent<Rigidbody>().linearVelocity, velocityCorrectionRate * Time.fixedDeltaTime);
            }
        }

        public void ApplyLocalKick(Vector3 impulse)
        {
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}

