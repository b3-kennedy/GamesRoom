using Unity.Netcode;
using UnityEngine;
using System.Collections;


namespace Assets.CreditClicker
{
    public class PassiveCreditGain : NetworkBehaviour
    {
        [HideInInspector] public Player player;
        float timer;

        bool isPulsing;
        float pulseScale = 0.9f;

        Vector3 originalScale;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            originalScale = transform.localScale;

        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= 10)
            {
                PulseServerRpc();
                timer = 0;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void PulseServerRpc()
        {
            player.AddCreditsServerRpc(transform.position, 10, OwnerClientId);
            PulseClientRpc();
        }

        [ClientRpc]
        void PulseClientRpc()
        {
            StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            isPulsing = true;
            // Scale up
            Vector3 targetScale = originalScale * pulseScale;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / 0.25f;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }



            // Scale back down
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / 0.25f;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }



            isPulsing = false;
        }
    }
}

