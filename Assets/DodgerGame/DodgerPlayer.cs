using UnityEngine;


namespace Assets.DodgerGame
{
    public class DodgerPlayer : MonoBehaviour
    {
        public float speed = 10f;
        Vector3 moveVec;

        Rigidbody rb;
    
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
        }

        void FixedUpdate()
        {
            rb.linearVelocity = speed * Time.fixedDeltaTime * moveVec;
        }
    }
}

