using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CreditClicker
{
    public class MoneyObject : NetworkBehaviour
    {
        public float force;
        Rigidbody rb;

        [HideInInspector] public int tier;

        TextMeshPro text;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

            text = GetComponent<TextMeshPro>();

            if (IsServer)
            {
                Destroy(gameObject, 5f);
            }

            switch (tier)
            {
                case 1:
                    text.text = "$";
                    text.color = Color.white;
                    break;
                case 10:
                    text.text = "$$";
                    text.color = Color.green;
                    break;
                case 100:
                    text.text = "$$$";
                    text.color = Color.cyan;
                    break;
                case 1000:
                    text.text = "$$$$";
                    text.color = Color.yellow;
                    break;
                case 10000:
                    text.text = "$$$$$";
                    text.color = new Color(1f, 0.65f, 0f);
                    break;
                case 100000:
                    text.text = "$$$$$$";
                    text.color = Color.magenta;
                    break;
                case 1000000:
                    text.text = "$$$$$$$";
                    text.color = new Color(1f, 0.84f, 0f);
                    break;
                case 10000000:
                    text.text = "$$$$$$$$";
                    text.color = Color.red;
                    break;

            }
            
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

