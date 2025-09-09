using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;


namespace Assets.CreditClicker
{
    public class Player : NetworkBehaviour
    {


        [HideInInspector] public GameObject playerObject;
        [HideInInspector] public SteamPlayer steamPlayer;
        [HideInInspector] public CreditClickerGame game;

        UpgradeManager upgradeManager;

        public GameObject sphere;

        public float pulseScale = 1.5f;   // How big the pulse gets
        public KeyCode pulseKey = KeyCode.Space; // Button to trigger pulse

        private Vector3 originalScale;
        private bool isPulsing = false;

        public GameObject moneyObjectPrefab;

        ulong ownerID;

        public int upgradeSelectionIndex;

        [HideInInspector] public GameState gameState;

        public int[] moneyTiers;

        int percent = 0;

        public int activePassiveGainers = 0;

        public bool noCap;



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            originalScale = sphere.transform.localScale;
            upgradeManager = GetComponent<UpgradeManager>();

        }

        public void OnPlayerAssigned()
        {
            steamPlayer = playerObject.GetComponent<SteamPlayer>();
            ownerID = playerObject.GetComponent<NetworkObject>().OwnerClientId;
            if (game.gameState is GameState g)
            {
                gameState = g;
            }
            
            gameState.player = this;
            gameState.passiveUpgrades.SetActive(false);
            gameState.LoadGameState();
            steamPlayer.credits.OnValueChanged += gameState.OnCreditsChanged;
        }

        void Space()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isPulsing)
            {
                StartCoroutine(Pulse(OwnerClientId));
                PulseServerRpc(OwnerClientId);

                int creditsToAdd = game.clickCredits;



                if (game.doubleChance > 0)
                {
                    int randomNum = Random.Range(0, 100);
                    if (randomNum < game.doubleChance)
                    {
                        creditsToAdd *= 2;
                    }
                }

                if (game.interestAmount > 0)
                {
                    int credits = steamPlayer.credits.Value;
                    int percent = Mathf.RoundToInt(credits * (game.interestAmount / 100));
                    creditsToAdd += percent;
                }

                if (gameState.background.GetComponent<MeshRenderer>().material.color == Color.red)
                {
                    creditsToAdd *= 2;
                }

                if (game.hasPlayerCountUpgrade)
                {
                    creditsToAdd *= 1 + (LobbyHolder.Instance.currentLobby.MemberCount/10);
                }

                if (game.hasTimeUpgrade)
                {
                    creditsToAdd *= 1 + (game.minutesInGameState.Value / 100); 
                }

                if (activePassiveGainers > 0)
                {
                    for (int i = 0; i < activePassiveGainers; i++)
                    {
                        upgradeManager.passiveGainers[i].GetComponent<PassiveCreditGain>().PulseServerRpc();
                    }
                }

                if (noCap)
                {
                    game.incomeSpeed = 0;
                }

                AddCreditsServerRpc(sphere.transform.position, creditsToAdd, OwnerClientId);

            }
        }

        void Upgrades()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (!gameState.isUpgradePanelOpen.Value)
                {
                    //gameState.LerpUpgradePanel(true);
                    gameState.ChangeUpgradePanelStateServerRpc(true, ownerID);
                    gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().SelectServerRpc();
                    upgradeSelectionIndex = 0;
                }
                else
                {
                    //gameState.LerpUpgradePanel(false);
                    gameState.ChangeUpgradePanelStateServerRpc(false, ownerID);
                }
            }

            if (gameState && gameState.isUpgradePanelOpen.Value)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) && upgradeSelectionIndex < gameState.upgradeParent.childCount - 1)
                {
                    gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().DeselectServerRpc();
                    upgradeSelectionIndex++;
                    gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().SelectServerRpc();
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) && upgradeSelectionIndex > 0)
                {
                    gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().DeselectServerRpc();
                    upgradeSelectionIndex--;
                    gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().SelectServerRpc();
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (gameState.activeUpgrades.activeSelf)
                    {
                        GetComponent<UpgradeManager>().BuyUpgradeServerRpc(upgradeSelectionIndex, ownerID, 1, true, true);
                    }
                    else
                    {
                        GetComponent<UpgradeManager>().BuyUpgradeServerRpc(upgradeSelectionIndex, ownerID, 1, true, false);
                    }

                }
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (gameState.passiveUpgrades.activeSelf)
                    {
                        gameState.ChangeUpgradesServerRpc(true, false);
                    }
                    else if (gameState.activeUpgrades.activeSelf)
                    {
                        gameState.ChangeUpgradesServerRpc(false, true);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            if (!game) return;

            if (game && game.netGameState.Value != CreditClickerGame.GameState.GAME) return;

            Space();
            Upgrades();





        }

        [ServerRpc(RequireOwnership = false)]
        public void AddCreditsServerRpc(Vector3 spawnPos,int amount, ulong id)
        {
            NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<SteamPlayer>().credits.Value += amount;

            int remaining = amount;

            foreach (int tier in moneyTiers)
            {
                while (remaining >= tier)
                {
                    GameObject spawnedMoneyObject = Instantiate(moneyObjectPrefab, spawnPos, Quaternion.identity);
                    spawnedMoneyObject.GetComponent<NetworkObject>().Spawn();
                    spawnedMoneyObject.GetComponent<MoneyObject>().tier.Value = tier;
                    remaining -= tier;
                }
            }

        }


        [ServerRpc(RequireOwnership = false)]
        void PulseServerRpc(ulong clientID)
        {
            PulseClientRpc(clientID);
        }

        [ClientRpc]
        void PulseClientRpc(ulong clientID)
        {
            if(NetworkManager.Singleton.LocalClientId == clientID) return;
            StartCoroutine(Pulse(clientID));
        }


        private IEnumerator Pulse(ulong clientId)
        {
            isPulsing = true;      
            // Scale up
            Vector3 targetScale = originalScale * pulseScale;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / game.incomeSpeed;
                sphere.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }



            // Scale back down
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / game.incomeSpeed;
                sphere.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }



            isPulsing = false;
        }
    }
}

