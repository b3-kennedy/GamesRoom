using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CreditClicker
{
    [System.Serializable]
    public class UpgradeValues
    {
        public Upgrade baseUpgrade;
        public int cost;
        public int tier;

        public float value;
    }
    public class UpgradeManager : NetworkBehaviour
    {
        public Transform upgradePanel;
        Transform upgradeParent;

        public GameObject passiveCreditObject;

        Player creditPlayer;

        public Dictionary<Upgrade, UpgradeValues> upgrades = new Dictionary<Upgrade, UpgradeValues>();

        public List<GameObject> passiveGainers;

        void Start()
        {
            creditPlayer = GetComponent<Player>();
            upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyUpgradeServerRpc(int index, ulong playerID, int upgradeCount, bool useMoney, bool isActive)
        {
            UpgradeUI upgradeUI = null;
            if (isActive)
            {
                upgradeUI = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(index).GetComponent<UpgradeUI>();
            }
            else
            {
                upgradeUI = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(index).GetComponent<UpgradeUI>();
            }
            
            Upgrade upgrade = upgradeUI.upgrade;
            var player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
            if (useMoney)
            {
                UsingMoney(upgradeCount, player, upgrade, upgradeUI, index, playerID, isActive);
            }
            else
            {
                NotUsingMoney(upgradeCount, player, upgrade, upgradeUI, index, playerID, isActive);
            }




        }

        void NotUsingMoney(int upgradeCount, NetworkObject player, Upgrade upgrade, UpgradeUI upgradeUI, int index, ulong playerID, bool isActive)
        {
            for (int i = 0; i < upgradeCount; i++)
            {
                if (upgradeUI.currentTier <= upgrade.maxTiers)
                {
                    if (!upgrades.ContainsKey(upgrade))
                    {
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
                        UpgradeValues values = upgrades[upgrade];
                        values.tier++;
                        values.cost = Mathf.RoundToInt(values.cost * upgrade.costIncreasePerTier);
                        upgrades[upgrade] = values;
                    }

                    int currentCost = upgrades[upgrade].cost;


                    ApplyUpgradeClientRpc(index, playerID, currentCost, upgrades[upgrade].tier, isActive);
                }
            }
        }

        void UsingMoney(int upgradeCount, NetworkObject player, Upgrade upgrade, UpgradeUI upgradeUI, int index, ulong playerID, bool isActive)
        {
            for (int i = 0; i < upgradeCount; i++)
            {
                Debug.Log(upgradeUI.cost);
                if (player.GetComponent<SteamPlayer>().credits.Value >= upgradeUI.cost && upgradeUI.currentTier < upgrade.maxTiers)
                {
                    if (!upgrades.ContainsKey(upgrade))
                    {
                        player.GetComponent<SteamPlayer>().credits.Value -= upgradeUI.cost;

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
                        player.GetComponent<SteamPlayer>().credits.Value -= upgradeUI.cost;
                        UpgradeValues values = upgrades[upgrade];
                        values.tier++;
                        values.cost = Mathf.RoundToInt(values.cost * upgrade.costIncreasePerTier);
                        upgrades[upgrade] = values;
                    }

                    int currentCost = upgrades[upgrade].cost;


                    ApplyUpgradeClientRpc(index, playerID, currentCost, upgrades[upgrade].tier, isActive);
                }
            }
        }

        [ClientRpc]
        void ApplyUpgradeClientRpc(int index, ulong playerID, int newCost, int tier, bool isActive)
        {
            if (NetworkManager.Singleton.LocalClientId != playerID) return;

            UpgradeUI ui = null;
            if (isActive)
            {
                ui = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(index).GetComponent<UpgradeUI>();
            }
            else
            {
                ui = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(index).GetComponent<UpgradeUI>();
            }
            ui.currentTier = tier;
            Upgrade upgrade = ui.upgrade;
            if (upgrade.upgradeType == Upgrade.UpgradeType.CLICK_SPEED && ui.currentTier <= upgrade.maxTiers)
            {
                creditPlayer.game.incomeSpeed *= upgrade.value;
                Debug.Log("Upgraded click speed");
                ui.UpgradeCostServerRpc(newCost);
                AddTierToUpgradeUIServerRpc(index, tier, true);
            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.PASSIVE && ui.currentTier <= upgrade.maxTiers)
            {
                Debug.Log("Add passive income");
                ui.UpgradeCostServerRpc(newCost);
                SpawnPassiveCreditServerRpc(playerID, tier);
                AddTierToUpgradeUIServerRpc(index, tier, false);
            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.ACTIVE && ui.currentTier <= upgrade.maxTiers)
            {
                creditPlayer.game.clickCredits += (int)upgrade.value;
                ui.UpgradeCostServerRpc(newCost);
                AddTierToUpgradeUIServerRpc(index, tier, true);
            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.PASSIVE_INCREASE && ui.currentTier <= upgrade.maxTiers)
            {
                if (passiveGainers.Count == 0) return;

                foreach (var gainer in passiveGainers)
                {
                    gainer.GetComponent<PassiveCreditGain>().DecreaseIntervalServerRpc(upgrade.value);

                }

                ui.UpgradeCostServerRpc(newCost);
                AddTierToUpgradeUIServerRpc(index, tier, false);

            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.PASSIVE_MONEY_INCREASE && ui.currentTier <= upgrade.maxTiers)
            {
                if (passiveGainers.Count == 0) return;

                foreach (var gainer in passiveGainers)
                {
                    gainer.GetComponent<PassiveCreditGain>().IncreaseValueServerRpc((int)upgrade.value);

                }

                ui.UpgradeCostServerRpc(newCost);
                AddTierToUpgradeUIServerRpc(index, tier, false);

            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.DOUBLE_CREDIT_CHANCE && ui.currentTier <= upgrade.maxTiers)
            {
                creditPlayer.game.doubleChance += (int)upgrade.value;
                ui.UpgradeCostServerRpc(newCost);
                AddTierToUpgradeUIServerRpc(index, tier, true);

            }
            else if (upgrade.upgradeType == Upgrade.UpgradeType.INTEREST && ui.currentTier <= upgrade.maxTiers)
            {
                creditPlayer.game.interestAmount += upgrade.value;

                ui.UpgradeCostServerRpc(newCost);
                AddTierToUpgradeUIServerRpc(index, tier, false);

            }

        }

        [ServerRpc(RequireOwnership = false)]
        void AddTierToUpgradeUIServerRpc(int index, int tier, bool active)
        {
            AddTierToUpgradeUIClientRpc(index, tier, active);
        }

        [ClientRpc]
        void AddTierToUpgradeUIClientRpc(int index, int tier, bool active)
        {
            UpgradeUI ui;
            if (active)
            {
                ui = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(index).GetComponent<UpgradeUI>();
            }
            else
            {
                ui = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(index).GetComponent<UpgradeUI>();
            }
            ui.layout.GetChild(tier - 1).GetComponent<Image>().color = Color.white;
        }

        [ServerRpc(RequireOwnership = false)]
        void SpawnPassiveCreditServerRpc(ulong playerID, int tier)
        {
            GameObject spawner = Instantiate(passiveCreditObject, creditPlayer.gameState.sphereSpawnsParent.GetChild(tier - 1).position, Quaternion.identity);
            spawner.GetComponent<PassiveCreditGain>().player = creditPlayer;
            spawner.GetComponent<NetworkObject>().SpawnWithOwnership(playerID);
            AddPassiveGainersClientRpc(spawner.GetComponent<NetworkObject>().NetworkObjectId);
        }

        [ClientRpc]
        void AddPassiveGainersClientRpc(ulong objectID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var gainer))
            {
                passiveGainers.Add(gainer.gameObject);
            }
        }
    }
}

