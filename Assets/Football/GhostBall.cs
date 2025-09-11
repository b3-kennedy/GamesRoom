using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
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

        public Dictionary<int, BallState> history = new Dictionary<int, BallState>();

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

            // 1️⃣ Predict physics for current tick (already handled by Rigidbody)
            // Save predicted state in history
            int currentTick = NetworkManager.Singleton.LocalTime.Tick;
            history[currentTick] = new BallState
            {
                position = rb.position,
                velocity = rb.linearVelocity
            };

            // 2️⃣ Reconcile if authoritative state exists for this tick
            if (history.TryGetValue(currentTick, out BallState predictedState))
            {
                
                // Check if server has sent a state for this tick
                if (serverBall.history.TryGetValue(currentTick, out BallState serverState))
                {
                    Debug.Log("reconcile");
                    Vector3 posDiff = serverState.position - predictedState.position;
                    float distance = posDiff.magnitude;

                    if (distance > snapThreshold)
                    {
                        // Snap if very far off
                        rb.position = serverState.position;
                        rb.linearVelocity = serverState.velocity;
                    }
                    else
                    {
                        // Smoothly correct toward server state
                        rb.MovePosition(rb.position + posDiff * positionCorrectionFactor);
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, serverState.velocity, velocityCorrectionFactor);
                    }
                }
            }

            // Optional: remove old history to save memory
            List<int> oldTicks = new List<int>();
            foreach (int t in history.Keys)
            {
                if (t < currentTick - 200) // keep last 200 ticks
                    oldTicks.Add(t);
            }
            foreach (int t in oldTicks)
                history.Remove(t);
        }

        public void ApplyLocalKick(Vector3 impulse)
        {
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}

