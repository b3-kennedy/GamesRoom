using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


namespace Assets.Football
{
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

            float x = Input.GetAxis("ArrowHorizontal") * speed;
            moveVec = new Vector3(x, 0, 0);

            if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded())
            {
                jumpRequest = true;
            }
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
        }

        bool isGrounded()
        {
            return Physics.Raycast(groundCheck.position, Vector3.down, rb.GetComponent<Collider>().bounds.extents.y + 0.1f);
        }
    }
}

