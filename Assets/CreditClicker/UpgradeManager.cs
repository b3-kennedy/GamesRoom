using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.CreditClicker
{
    [System.Serializable]
    public class UpgradeValues
    {
        public Upgrade baseUpgrade;
        public int cost;
        public int tier;

        public float value;

        public string GetDescription()
        {
            return $"{baseUpgrade.upgradeDescription}\n" +
                   $"Tier: {tier}\n" +
                   $"Effect: x{baseUpgrade.value * tier}";
        }
    }
    public class UpgradeManager : NetworkBehaviour
    {
        public Transform upgradePanel;
        Transform upgradeParent;

        public GameObject passiveCreditObject;

        Player creditPlayer;

        public Dictionary<Upgrade, UpgradeValues> upgrades = new Dictionary<Upgrade, UpgradeValues>();

        void Start()
        {
            creditPlayer = GetComponent<Player>();
            upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyUpgradeServerRpc(int index, ulong playerID)
        {
            Upgrade upgrade = creditPlayer.gameState.upgradeParent.GetChild(index).GetComponent<UpgradeUI>().upgrade;
            var player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
            if (player.GetComponent<SteamPlayer>().credits.Value >= upgrade.cost)
            {
                if (!upgrades.ContainsKey(upgrade))
                {
                    player.GetComponent<SteamPlayer>().credits.Value -= upgrade.cost;
                    UpgradeValues values = new UpgradeValues
                    {
                        baseUpgrade = upgrade,
                        cost = Mathf.RoundToInt(upgrade.cost * upgrade.costIncreasePerTier),
                        tier = 1,
                        value = upgrade.value
                    };
                    upgrades.Add(upgrade, values);
                }
                else
                {
                    player.GetComponent<SteamPlayer>().credits.Value -= upgrades[upgrade].cost;
                    UpgradeValues values = upgrades[upgrade];
                    values.tier++;
                    values.cost = Mathf.RoundToInt(values.cost * upgrade.costIncreasePerTier);
                    upgrades[upgrade] = values;
                }

                int currentCost = upgrades[upgrade].cost;


                ApplyUpgradeClientRpc(index, playerID, currentCost);
            }


        }

        [ClientRpc]
        void ApplyUpgradeClientRpc(int index, ulong playerID, int newCost)
        {
            if (NetworkManager.Singleton.LocalClientId != playerID) return;

            Upgrade upgrade = creditPlayer.gameState.upgradeParent.GetChild(index).GetComponent<UpgradeUI>().upgrade;
            if (upgrade.upgradeType == Upgrade.UpgradeType.CLICK_SPEED)
            {
                creditPlayer.game.incomeSpeed *= upgrade.value;
                Debug.Log("Upgraded click speed");
                upgradeParent.GetChild(index).GetComponent<UpgradeUI>().UpgradeCostServerRpc(newCost);
            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.PASSIVE)
            {
                Debug.Log("Add passive income");
                upgradeParent.GetChild(index).GetComponent<UpgradeUI>().UpgradeCostServerRpc(newCost);
                SpawnPassiveCreditServerRpc(playerID);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void SpawnPassiveCreditServerRpc(ulong playerID)
        {
            GameObject spawner = Instantiate(passiveCreditObject);
            spawner.GetComponent<PassiveCreditGain>().player = creditPlayer;
            spawner.GetComponent<NetworkObject>().SpawnWithOwnership(playerID);
        }
    }
}

