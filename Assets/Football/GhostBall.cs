using UnityEngine;

namespace Assets.Football
{
    public class GhostBall : MonoBehaviour
    {
        [Header("Smoothing Settings")]
        public float positionCorrectionSpeed = 15f;
        public float velocityCorrectionRate = 10f;

        // References
        private Rigidbody rb;
        private Ball serverBall; // the authoritative networked ball

        // Prediction variables
        private Vector3 predictedVelocity;
        private bool hasPrediction = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // we move manually
        }

        public void BindToServerBall(Ball server)
        {
            serverBall = server;
        }

        public void ApplyLocalKick(Vector3 impulse)
        {
            predictedVelocity += impulse / 1f; // assume mass = 1 for simplicity
            hasPrediction = true;
        }

        void Update()
        {
            if (serverBall == null) return;

            // Predict local movement
            if (hasPrediction)
            {
                predictedVelocity += Physics.gravity * Time.deltaTime;
                transform.position += predictedVelocity * Time.deltaTime;
            }

            // Smooth correction toward server-authoritative position
            Vector3 targetPos = serverBall.transform.position;
            Vector3 targetVel = serverBall.GetComponent<Rigidbody>().linearVelocity;

            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > 2f) // snap if too far
            {
                transform.position = targetPos;
                predictedVelocity = targetVel;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, positionCorrectionSpeed * Time.deltaTime);
                predictedVelocity = Vector3.Lerp(predictedVelocity, targetVel, velocityCorrectionRate * Time.deltaTime);
            }
        }
    }
}

