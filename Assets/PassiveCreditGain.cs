using Unity.Netcode;
using UnityEngine;


namespace Assets.CreditClicker
{
    public class PassiveCreditGain : NetworkBehaviour
    {
        [HideInInspector] public Player player;
        float timer;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= 10)
            {
                player.AddCreditsServerRpc(transform.position,10, OwnerClientId);
                timer = 0;
            }
        }
    }
}

