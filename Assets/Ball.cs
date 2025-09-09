using Unity.Netcode;
using UnityEngine;


namespace Assets.Football
{
    public class Ball : NetworkBehaviour
    {
        [Header("Smoothing Settings")]
        [SerializeField] private float correctionSpeed = 15f;
        [SerializeField] private float velocityCorrectionRate = 10f;
        [SerializeField] private float snapThreshold = 2f;

        float syncRate = 0.1f;
        float syncTimer;

        private Rigidbody rb;
        private Vector3 targetPosition;
        private Vector3 targetVelocity;
        private bool needsCorrection = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (IsServer)
            {
                syncTimer += Time.fixedDeltaTime;
                if (syncTimer >= syncRate)
                {
                    SyncBallStateClientRpc(rb.position, rb.linearVelocity);
                    syncTimer = 0f;
                }
                
            }
            else if (needsCorrection)
            {
                ApplyConstantSpeedCorrection();
            }
        }

        [ClientRpc]
        void SyncBallStateClientRpc(Vector3 position, Vector3 velocity)
        {
            if (IsServer) return;

            float distance = Vector3.Distance(rb.position, position);

            if (distance > snapThreshold)
            {
                // Snap for large discrepancies
                rb.position = position;
                rb.linearVelocity = velocity;
                needsCorrection = false;
            }
            else
            {
                // Set targets for smooth correction
                targetPosition = position;
                targetVelocity = velocity;
                needsCorrection = true;
            }
        }

        private void ApplyConstantSpeedCorrection()
        {
            // Move towards target at constant speed (not exponential decay)
            Vector3 positionDiff = targetPosition - rb.position;

            if (positionDiff.magnitude > 0.01f)
            {
                // Move at constant speed towards target
                Vector3 moveDirection = positionDiff.normalized;
                float moveDistance = Mathf.Min(correctionSpeed * Time.fixedDeltaTime, positionDiff.magnitude);
                rb.MovePosition(rb.position + moveDirection * moveDistance);
            }

            // Correct velocity
            Vector3 velocityDiff = targetVelocity - rb.linearVelocity;
            if (velocityDiff.magnitude > 0.1f)
            {
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity,
                    velocityCorrectionRate * Time.fixedDeltaTime);
            }

            // Stop correction when close enough
            if (positionDiff.magnitude < 0.05f && velocityDiff.magnitude < 0.1f)
            {
                needsCorrection = false;
            }
        }

        // void FixedUpdate()
        // {
        //     if (!IsServer) return;

        //     // Ball physics is server-authoritative
        //     // Periodically sync state to clients
        //     SyncBallStateClientRpc(rb.position, rb.linearVelocity);
        // }

        // [ClientRpc]
        // void SyncBallStateClientRpc(Vector3 position, Vector3 velocity)
        // {
        //     if (IsServer) return; // host already authoritative

        //     rb.position = position;
        //     rb.linearVelocity = velocity;
        // }
    }
}

