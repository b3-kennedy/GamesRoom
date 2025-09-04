using Unity.Netcode;
using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace Assets.CreditClicker
{

    [System.Serializable]
    public class CreditClickerSaveData
    {
        public GameSaveData creditClicker;
    }

    [System.Serializable]
    public class UpgradeSaveData
    {
        public string upgradeName;
        public int tier;
    }

    [System.Serializable]
    public class GameSaveData
    {
        public List<UpgradeSaveData> activeUpgrades;
        public List<UpgradeSaveData> passiveUpgrades;
    }
    public class GameState : State
    {
        public GameObject upgradePanel;
        public Transform upgradePanelStart;
        public Transform upgradePanelFinish;
        public Transform sphereSpawnsParent;
        public GameObject activeUpgrades;
        public GameObject passiveUpgrades;

        public GameObject background;

        MeshRenderer backgroundMeshRenderer;

        public TextMeshPro creditCountText;

        [HideInInspector] public Transform upgradeParent;

        public float lerpDuration = 0.5f;

        [HideInInspector] public NetworkVariable<bool> isUpgradePanelOpen;

        [HideInInspector] public Player player;

        SteamPlayer steamPlayer;

        CreditClickerGame creditClickerGame;


        float redTimer;

        void Start()
        {
            upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
            if (game is CreditClickerGame g)
            {
                creditClickerGame = g;
            }
            backgroundMeshRenderer = background.GetComponent<MeshRenderer>();
        }



        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            activeUpgrades.SetActive(true);
            passiveUpgrades.SetActive(false);
            Debug.Log(player);

        }

        public override void OnReset()
        {
            var passiveParent = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0);
            var activeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
            if (player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                SaveGameState();
            }
            
            for (int i = 0; i < passiveParent.childCount; i++)
            {
                UpgradeUI ui = passiveParent.GetChild(i).GetComponent<UpgradeUI>();
                ui.currentTier = 0;
                ui.cost = ui.upgrade.cost;
                for (int j = 0; j < ui.layout.childCount; j++)
                {
                    ui.layout.GetChild(j).GetComponent<Image>().color = Color.gray;
                }
            }

            for (int i = 0; i < activeParent.childCount; i++)
            {
                UpgradeUI ui = activeParent.GetChild(i).GetComponent<UpgradeUI>();
                ui.currentTier = 0;
                ui.cost = ui.upgrade.cost;
                for (int j = 0; j < ui.layout.childCount; j++)
                {
                    ui.layout.GetChild(j).GetComponent<Image>().color = Color.gray;
                }
            }
            
        }

        public void OnCreditsChanged(int previousValue, int newValue)
        {
            var credits = NetworkManager.Singleton.ConnectedClients[player.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.GetComponent<SteamPlayer>().credits;
            creditCountText.text = $"${credits.Value}";
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeUpgradesServerRpc(bool activeValue, bool passiveValue)
        {
            ChangeUpgradesClientRpc(activeValue, passiveValue);
        }

        [ClientRpc]
        void ChangeUpgradesClientRpc(bool activeValue, bool passiveValue)
        {
            activeUpgrades.SetActive(activeValue);
            passiveUpgrades.SetActive(passiveValue);
            if (activeValue)
            {
                upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
            }
            else
            {
                upgradeParent = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0);
            }

            if (IsServer)
            {
                for (int i = 0; i < upgradeParent.childCount; i++)
                {
                    upgradeParent.GetChild(i).GetComponent<UpgradeUI>().isSelected.Value = false;
                }
                upgradeParent.GetChild(0).GetComponent<UpgradeUI>().isSelected.Value = true;
            }

            player.upgradeSelectionIndex = 0;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeUpgradePanelStateServerRpc(bool value, ulong clientID)
        {
            ChangeUpgradePanelStateClientRpc(value, clientID);
        }

        // public override void OnNetworkSpawn()
        // {
        //     gameObject.SetActive(false);
        // }

        [ClientRpc]
        void ChangeUpgradePanelStateClientRpc(bool value, ulong clientID)
        {
            //if (NetworkManager.Singleton.LocalClientId == clientID) return;

            StartCoroutine(LerpUpgradePanel(value));
        }

        public IEnumerator LerpUpgradePanel(bool open)
        {
            Vector3 startPos = open ? upgradePanelStart.position : upgradePanelFinish.position;
            Vector3 endPos = open ? upgradePanelFinish.position : upgradePanelStart.position;



            float elapsed = 0f;
            while (elapsed < lerpDuration)
            {
                elapsed += Time.deltaTime;
                Vector3 newPos = Vector3.Lerp(startPos, endPos, elapsed / lerpDuration);
                upgradePanel.transform.position = newPos;
                yield return null;
            }

            upgradePanel.transform.position = endPos;

            if (IsServer)
            {
                isUpgradePanelOpen.Value = open;
            }
        }


        public override void OnStateUpdate()
        {
            if (creditClickerGame.redInterval > 0)
            {
                redTimer += Time.deltaTime;
                if (redTimer >= UnityEngine.Random.Range(creditClickerGame.redInterval, creditClickerGame.redInterval * 5))
                {
                    StartCoroutine(FlashRed(UnityEngine.Random.Range(1, 5)));
                    redTimer = 0;
                }
            }
        }


        IEnumerator FlashRed(float duration)
        {

            backgroundMeshRenderer.material.color = Color.red;
            ChangeBackgroundColourServerRpc(true);


            yield return new WaitForSeconds(duration);


            backgroundMeshRenderer.material.color = Color.black;
            ChangeBackgroundColourServerRpc(false);


        }

        [ServerRpc(RequireOwnership = false)]
        void ChangeBackgroundColourServerRpc(bool isRed)
        {
            ChangeBackgroundColourClientRpc(isRed);
        }

        [ClientRpc]
        void ChangeBackgroundColourClientRpc(bool isRed)
        {
            if (isRed)
            {
                backgroundMeshRenderer.material.color = Color.red;
            }
            else
            {
                backgroundMeshRenderer.material.color = Color.black;
            }
        }

        public void LoadGameState()
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId) return;

            string path = Path.Combine(Application.persistentDataPath, "savegame.json");

            if (!File.Exists(path))
            {
                Debug.LogWarning("Save file not found at: " + path);
                return;
            }

            string json = File.ReadAllText(path);
            CreditClickerSaveData wrapper = JsonUtility.FromJson<CreditClickerSaveData>(json);

            GameSaveData saveData = wrapper.creditClicker;
            if (saveData == null)
            {
                Debug.LogWarning("Save file is invalid or missing creditclicker data.");
                return;
            }
            // Clear current upgrades in the UI (optional, depending on your setup)
            Transform activeUpgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
            Transform passiveUpgradeParent = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0);
            //UpgradeManager upgradeManager = player.GetComponent<UpgradeManager>();

            foreach (var savedData in saveData.activeUpgrades)
            {
                // Loop through children of the active upgrade panel
                for (int i = 0; i < activeUpgradeParent.childCount; i++)
                {
                    UpgradeUI ui = activeUpgradeParent.GetChild(i).GetComponent<UpgradeUI>();
                    if (ui != null && ui.upgrade.name == savedData.upgradeName)
                    {
                        // Match found, update the tier
                        ui.currentTier = savedData.tier;
                        Debug.Log(savedData.tier);
                        player.GetComponent<UpgradeManager>().BuyUpgradeServerRpc(i, player.GetComponent<NetworkObject>().OwnerClientId, ui.currentTier, false, true);
                        break; // Stop looping once we found the match
                    }
                }
            }

            foreach (var savedData in saveData.passiveUpgrades)
            {
                // Loop through children of the active upgrade panel
                for (int i = 0; i < passiveUpgradeParent.childCount; i++)
                {
                    UpgradeUI ui = passiveUpgradeParent.GetChild(i).GetComponent<UpgradeUI>();
                    if (ui != null && ui.upgrade.name == savedData.upgradeName)
                    {
                        Debug.Log("here");
                        // Match found, update the tier
                        ui.currentTier = savedData.tier;
                        player.GetComponent<UpgradeManager>().BuyUpgradeServerRpc(i, player.GetComponent<NetworkObject>().OwnerClientId, ui.currentTier, false, false);
                        break; // Stop looping once we found the match
                    }
                }
            }

        }

        public void SaveGameState()
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId) return;

            string path = Path.Combine(Application.persistentDataPath, "savegame.json");
            SaveData saveDataWrapper;

            // If file exists, load it so we don’t overwrite other sections
            if (File.Exists(path))
            {
                string existingJson = File.ReadAllText(path);
                saveDataWrapper = JsonUtility.FromJson<SaveData>(existingJson);
                if (saveDataWrapper == null) saveDataWrapper = new SaveData();
            }
            else
            {
                saveDataWrapper = new SaveData();
            }

            // Build your creditClicker section
            GameSaveData creditClickerSave = new GameSaveData
            {
                activeUpgrades = new List<UpgradeSaveData>(),
                passiveUpgrades = new List<UpgradeSaveData>()
            };

            Transform activeUpgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
            Transform passiveUpgradeParent = upgradePanel.transform.GetChild(1).GetChild(0).GetChild(0);

            for (int i = 0; i < activeUpgradeParent.childCount; i++)
            {
                UpgradeUI ui = activeUpgradeParent.GetChild(i).GetComponent<UpgradeUI>();
                creditClickerSave.activeUpgrades.Add(new UpgradeSaveData
                {
                    upgradeName = ui.upgrade.name,
                    tier = ui.currentTier
                });
            }

            for (int i = 0; i < passiveUpgradeParent.childCount; i++)
            {
                UpgradeUI ui = passiveUpgradeParent.GetChild(i).GetComponent<UpgradeUI>();
                creditClickerSave.passiveUpgrades.Add(new UpgradeSaveData
                {
                    upgradeName = ui.upgrade.name,
                    tier = ui.currentTier
                });
            }

            // Assign to wrapper
            saveDataWrapper.creditClicker = creditClickerSave;

            // Save whole wrapper back to file
            string json = JsonUtility.ToJson(saveDataWrapper, true);
            File.WriteAllText(path, json);

            Debug.Log("Game saved to: " + path);
        }

    }
}

