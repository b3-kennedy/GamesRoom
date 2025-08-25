using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.CreditClicker
{
    public class UpgradeManager : NetworkBehaviour
    {
        public Transform upgradePanel;
        Transform upgradeParent;

        Player player;

        void Start()
        {
            player = GetComponent<Player>();
            upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyUpgradeServerRpc(int index, ulong playerID)
        {
            Upgrade upgrade = upgradeParent.GetChild(index).GetComponent<UpgradeUI>().upgrade;
            var player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
            if (player.GetComponent<SteamPlayer>().credits.Value >= upgrade.cost)
            {
                player.GetComponent<SteamPlayer>().credits.Value -= upgrade.cost;
                ApplyUpgradeClientRpc(index, playerID);
            }
            

        }

        [ClientRpc]
        void ApplyUpgradeClientRpc(int index, ulong playerID)
        {
            if (NetworkManager.Singleton.LocalClientId != playerID) return;

            Upgrade upgrade = upgradeParent.GetChild(index).GetComponent<UpgradeUI>().upgrade;
            if (upgrade.upgradeType == Upgrade.UpgradeType.CLICK_SPEED)
            {
                upgrade.tier++;
                upgrade.cost *= Mathf.RoundToInt(upgrade.costIncreasePerTier);
                player.game.incomeSpeed *= 0.9f;
                Debug.Log("Upgraded click speed");
            }
        }
    }
}

