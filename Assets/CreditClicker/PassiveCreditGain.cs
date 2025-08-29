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

        float interval = 10f;
        int moneyPerPulse = 3;

        int percent = 0;

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
            if (timer >= interval)
            {
                PulseServerRpc();
                timer = 0;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void IncreaseValueServerRpc(int value)
        {
            IncreaseValueClientRpc(value);
        }

        [ClientRpc]
        void IncreaseValueClientRpc(int value)
        {
            moneyPerPulse += value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void DecreaseIntervalServerRpc(float value)
        {
            DecreaseIntervalClientRpc(value);
        }

        [ClientRpc]
        void DecreaseIntervalClientRpc(float value)
        {
            interval *= value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void PulseServerRpc()
        {
            int total = moneyPerPulse;

            if (player.game.interestAmount > 0)
            {
                int credits = player.steamPlayer.credits.Value;
                total += Mathf.RoundToInt(credits * (player.game.interestAmount / 100f));
            }

            if (player.game.hasPlayerCountUpgrade)
            {
                float multiplier = 1f + (SteamManager.Instance.playerCount.Value / 10f);
                total = Mathf.RoundToInt(total * multiplier);
            }

            if (player.game.hasTimeUpgrade)
            {
                total *= 1 + (player.game.minutesInGameState.Value / 100);
            }

            player.AddCreditsServerRpc(transform.position, total, OwnerClientId);
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

