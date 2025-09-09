using System;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


namespace Assets.Football
{


    public struct PlayerInput : INetworkSerializable
    {
        public float horizontal;
        public bool jump;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref horizontal);
            serializer.SerializeValue(ref jump);
        }
    }

    public class FootballPlayer : NetworkBehaviour
    {
        FootballGame footballGame;
        public float speed = 5f;
        public float jumpForce = 5f;

        public float fallMultiplier = 2.5f;
        public float riseMultiplier = 2.5f;
        Rigidbody rb;
        Vector3 moveVec;
        bool jumpRequest;
        public Transform groundCheck;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            var input = GetInput();

            ApplyMovement(input);
            if (!IsServer)
            {
                SendInputServerRpc(input);
            }
            
            
            
        }

        PlayerInput GetInput()
        {
            PlayerInput input = new PlayerInput
            {
                horizontal = Input.GetAxis("ArrowHorizontal"),
                jump = Input.GetKeyDown(KeyCode.UpArrow) && isGrounded()

            };
            return input;
        }

        void ApplyMovement(PlayerInput input)
        {
            moveVec = new Vector3(input.horizontal * speed, 0, 0);

            if (input.jump)
            {
                jumpRequest = true;
            }
                
        }

        [ServerRpc(RequireOwnership = false)]
        void SendInputServerRpc(PlayerInput input)
        {
            ApplyMovement(input);
        }

        void FixedUpdate()
        {
            rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, 0);
            if (jumpRequest)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpRequest = false;
            }

            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += (fallMultiplier - 1) * Physics.gravity.y * Time.fixedDeltaTime * Vector3.up;
            }

            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity += (riseMultiplier - 1) * Physics.gravity.y * Time.fixedDeltaTime * Vector3.up;
            }

            if (IsServer)
            {
                SyncStateClientRpc(rb.position, rb.linearVelocity);
            }
        }

        [ClientRpc]
        private void SyncStateClientRpc(Vector3 pos, Vector3 vel)
        {
            if (IsOwner) return; // Don’t override the local owner’s prediction

            rb.position = pos;
            rb.linearVelocity = vel;
        }

        bool isGrounded()
        {
            return Physics.Raycast(groundCheck.position, Vector3.down, rb.GetComponent<Collider>().bounds.extents.y + 0.1f);
        }
    }
}

