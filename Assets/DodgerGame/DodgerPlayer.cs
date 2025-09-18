using UnityEngine;


namespace Assets.Dodger
{
    public class DodgerPlayer : MonoBehaviour
    {
        public float speed = 10f;
        Vector3 moveVec;

        [HideInInspector] public DodgerGame game;

        [HideInInspector] public GameObject playerObject;

        Rigidbody rb;

        public float dashSpeed = 25f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;

        private bool isDashing = false;
        private float dashEndTime = 0f;
        private float lastDashTime = -999f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            float x = Input.GetAxisRaw("ArrowHorizontal");
            float y = Input.GetAxisRaw("ArrowVertical");

            moveVec = new Vector3(x, y, 0);

            if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
            {
                StartDash();
            }

            if (isDashing && Time.time >= dashEndTime)
            {
                isDashing = false;
            }
        }

        void FixedUpdate()
        {
            if (isDashing)
            {
                rb.linearVelocity = moveVec * dashSpeed;
            }
            else
            {
                rb.linearVelocity = moveVec * speed;
            }
        }


        void StartDash()
        {
            if (moveVec == Vector3.zero) return; // don’t dash if not moving

            isDashing = true;
            dashEndTime = Time.time + dashDuration;
            lastDashTime = Time.time;
        }


    }
}

