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

        float syncRate = 0.01f;
        float syncTimer;

        public GameState gameState;

        private Rigidbody rb;
        private Vector3 targetPosition;
        private Vector3 targetVelocity;
        private bool needsCorrection = false;

        public GameObject ghostBall;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                GetComponent<MeshRenderer>().enabled = false;

                // Turn off collider so ghost ball is not interfered with
                GetComponent<Collider>().enabled = false;

                // Rigidbody is already kinematic on clients
                GetComponent<Rigidbody>().isKinematic = true;

                
                GameObject ghostInstance = Instantiate(ghostBall, transform.position, transform.rotation);
                GhostBall ghostScript = ghostInstance.GetComponent<GhostBall>();
                ghostScript.BindToServerBall(this);
            }
        }

        void FixedUpdate()
        {
            if (IsServer)
            {
                SyncBallStateClientRpc(rb.position, rb.linearVelocity);
            }
        }


        void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if (other.CompareTag("LeftGoal"))
            {
                gameState.OnGoalServerRpc(false);
            }
            else if(other.CompareTag("RightGoal"))
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

            rb.position = position;
            //rb.linearVelocity = velocity;
        }
    }
}

