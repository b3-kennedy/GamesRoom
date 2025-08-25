using Unity.Netcode;
using UnityEngine;

namespace Assets.CreditClicker
{
    public class MoneyObject : NetworkBehaviour
    {
        public float force;
        Rigidbody rb;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Destroy(gameObject, 5f);
            rb = GetComponent<Rigidbody>();
            float randomX = Random.Range(-1f, 1f);
            float randomY = Random.Range(-1f, 1f);

            if (Mathf.Abs(randomX) < 0.2f)
                randomX = 0.2f * Mathf.Sign(randomX != 0 ? randomX : 1);

            if (Mathf.Abs(randomY) < 0.2f)
                randomY = 0.2f * Mathf.Sign(randomY != 0 ? randomY : 1);

            Vector3 randomDir = new Vector3(randomX, randomY, 0).normalized;

            rb.AddForce(randomDir * force, ForceMode.Impulse);
        }
    }
}

