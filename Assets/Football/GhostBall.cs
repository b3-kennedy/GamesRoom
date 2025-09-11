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

            // Get the latest server tick received
            int latestTick = -1;
            foreach (var tick in history.Keys)
            {
                if (tick > latestTick) latestTick = tick;
            }

            if (latestTick != -1)
            {
                BallState serverState = history[latestTick]; // <-- use ghost's history
                Vector3 posDiff = serverState.position - rb.position;
                float distance = posDiff.magnitude;

                if (distance > snapThreshold)
                {
                    rb.position = serverState.position;
                    rb.linearVelocity = serverState.velocity;
                }
                else
                {
                    rb.position = Vector3.Lerp(rb.position, serverState.position, positionCorrectionFactor);
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, serverState.velocity, velocityCorrectionFactor);
                }

                Debug.Log("reconcile");
            }

            //     // Optional: remove old history to save memory
            //     List<int> oldTicks = new List<int>();
            // foreach (int t in history.Keys)
            // {
            //     if (t < currentTick - 200) // keep last 200 ticks
            //         oldTicks.Add(t);
            // }
            // foreach (int t in oldTicks)
            //     history.Remove(t);
        }

        public void ApplyLocalKick(Vector3 impulse)
        {
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}

