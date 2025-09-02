using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


namespace Assets.ArcherBattle
{
    public class Health : NetworkBehaviour
    {

        public float maxHealth = 100f;
        public NetworkVariable<float> health = new NetworkVariable<float>();

        [HideInInspector] public UnityEvent Death;


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                health.Value = maxHealth;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float dmg)
        {
            health.Value -= dmg;
            if (health.Value <= 0)
            {
                Debug.Log("player has died");
                Death.Invoke();
            }
        }
    }
}

