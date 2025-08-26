using Unity.Netcode;
using UnityEngine;
using System.Collections;


namespace Assets.CreditClicker
{
    public class Player : NetworkBehaviour
    {


        [HideInInspector] public GameObject playerObject;
        SteamPlayer steamPlayer;
        [HideInInspector] public CreditClickerGame game;

        public GameObject sphere;

        public float pulseScale = 1.5f;   // How big the pulse gets
        public KeyCode pulseKey = KeyCode.Space; // Button to trigger pulse

        private Vector3 originalScale;
        private bool isPulsing = false;

        public GameObject moneyObjectPrefab;

        ulong ownerID;

        public int upgradeSelectionIndex;

        [HideInInspector] public GameState gameState;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            originalScale = sphere.transform.localScale;

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
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            if (game && game.netGameState.Value != CreditClickerGame.GameState.GAME) return;

            if (Input.GetKeyDown(KeyCode.Space) && !isPulsing)
            {
                StartCoroutine(Pulse(OwnerClientId));
                PulseServerRpc(OwnerClientId);
                AddCreditsServerRpc(sphere.transform.position, game.clickCredits, OwnerClientId);
            }

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
                    GetComponent<UpgradeManager>().BuyUpgradeServerRpc(upgradeSelectionIndex, ownerID);
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

        [ServerRpc(RequireOwnership = false)]
        public void AddCreditsServerRpc(Vector3 spawnPos,int amount, ulong id)
        {
            NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<SteamPlayer>().credits.Value += amount;
            for (int i = 0; i < amount; i++)
            {
                GameObject spawnedMoneyObject = Instantiate(moneyObjectPrefab, spawnPos, Quaternion.identity);
                spawnedMoneyObject.GetComponent<NetworkObject>().Spawn();
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

