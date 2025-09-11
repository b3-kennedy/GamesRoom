using Steamworks;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;


namespace Assets.Football
{

    public struct BallState
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    public class Ball : NetworkBehaviour
    {
        [Header("Smoothing Settings")]
        [SerializeField] private float correctionSpeed = 15f;
        [SerializeField] private float velocityCorrectionRate = 10f;
        [SerializeField] private float snapThreshold = 2f;

        float syncRate = 0.01f;
        float syncTimer;

        public GameState gameState;

        private Rigidbody rb;
        private Vector3 targetPosition;
        private Vector3 targetVelocity;
        private bool needsCorrection = false;

        public GameObject ghostBall;
        GameObject ghostInstance;

        public bool useServerBall;

        public Dictionary<int, BallState> history = new Dictionary<int, BallState>();

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                //
                if (!useServerBall)
                {
                    GetComponent<MeshRenderer>().enabled = false;

                    // Turn off collider so ghost ball is not interfered with
                    GetComponent<Collider>().enabled = false;

                    // Rigidbody is already kinematic on clients
                    GetComponent<Rigidbody>().isKinematic = true;


                    ghostInstance = Instantiate(ghostBall, transform.position, transform.rotation);
                    GhostBall ghostScript = ghostInstance.GetComponent<GhostBall>();
                    ghostScript.BindToServerBall(this);
                }

            }
        }

        void FixedUpdate()
        {
            if (IsServer)
            {
                if (useServerBall)
                {
                    SyncBallStateClientRpc(rb.position, rb.linearVelocity);
                }
                else
                {
                    WithGhostBall();
                }
                
            }
        }

        void WithGhostBall()
        {
            // Save current state in history
            int tick = NetworkManager.Singleton.ServerTime.Tick;
            BallState state = new BallState
            {
                position = rb.position,
                velocity = rb.linearVelocity
            };
            history[tick] = state;

            // Remove old ticks
            List<int> oldTicks = new List<int>();
            foreach (int t in history.Keys)
            {
                if (t < tick - 200) oldTicks.Add(t);
            }
            foreach (int t in oldTicks) history.Remove(t);
            SyncBallClientRpc(tick, rb.position, rb.linearVelocity);
        }


        void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if (other.CompareTag("LeftGoal"))
            {
                gameState.OnGoalServerRpc(false);
            }
            else if (other.CompareTag("RightGoal"))
            {
                gameState.OnGoalServerRpc(true);
            }
        }
        // void FixedUpdate()
        // {
        //     if (!IsServer) return;

        //     // Ball physics is server-authoritative
        //     // Periodically sync state to clients
        //     SyncBallStateClientRpc(rb.position, rb.linearVelocity);
        // }

        [ClientRpc]
        void SyncBallStateClientRpc(Vector3 position, Vector3 velocity)
        {
            if (IsServer) return; // host already authoritative

            

            if (useServerBall)
            {
                rb.linearVelocity = velocity;
            }

        }

        [ClientRpc]
        void SyncBallClientRpc(int t, Vector3 pos, Vector3 vel)
        {
            if (!ghostInstance) return;

            rb.position = pos;

            BallState state = new BallState
            {
                position = pos,
                velocity = vel
            };
            ghostInstance.GetComponent<GhostBall>().history[t] = state;
        }
    }
}

