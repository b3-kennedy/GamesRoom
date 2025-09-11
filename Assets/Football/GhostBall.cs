using UnityEngine;

namespace Assets.Football
{
    public class GhostBall : MonoBehaviour
    {
        [Header("Reconciliation Settings")]
        public float positionCorrectionFactor = 0.1f;
        public float velocityCorrectionFactor = 0.1f;
        public float snapThreshold = 2f;

        private Rigidbody rb;
        private Ball serverBall;

        Rigidbody serverRb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public void BindToServerBall(Ball server)
        {
            serverBall = server;
            serverRb = server.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Ignore collisions with server ball
            Physics.IgnoreCollision(server.GetComponent<Collider>(), rb.GetComponent<Collider>());
        }

        void FixedUpdate()
        {
            if (serverBall == null) return;

            Vector3 serverPos = serverBall.transform.position;
            Vector3 serverVel = serverRb.linearVelocity;

            float posDiff = Vector3.Distance(serverPos, rb.position);

            if (posDiff > snapThreshold)
            {
                // Way too far off → snap
                rb.position = serverPos;
                rb.linearVelocity = serverVel;
            }
            else
            {
                // Smooth reconciliation
                Vector3 correction = (serverPos - rb.position) * positionCorrectionFactor;
                rb.MovePosition(rb.position + correction);

                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, serverVel, velocityCorrectionFactor);
            }
        }

        public void ApplyLocalKick(Vector3 impulse)
        {
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}

