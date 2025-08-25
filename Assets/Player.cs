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

        GameState gameState;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            originalScale = sphere.transform.localScale;

        }

        public void OnPlayerAssigned()
        {
            steamPlayer = playerObject.GetComponent<SteamPlayer>();
            ownerID = playerObject.GetComponent<NetworkObject>().OwnerClientId;
            //sphere.GetComponent<NetworkObject>().ChangeOwnership(ownerID);
            if (game.gameState is GameState g)
            {
                gameState = g;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Space) && !isPulsing)
            {
                StartCoroutine(Pulse());
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                if (!gameState.isUpgradePanelOpen)
                {
                    gameState.ChangeUpgradePanelStateServerRpc(true);
                }
                else
                {
                    gameState.ChangeUpgradePanelStateServerRpc(false);
                }
            }



            if (Input.GetKeyDown(KeyCode.UpArrow) && gameState.isUpgradePanelOpen && upgradeSelectionIndex < gameState.upgradeParent.childCount-1)
            {
                gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().DeselectServerRpc();
                upgradeSelectionIndex++;
                gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().SelectServerRpc();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && gameState.isUpgradePanelOpen && upgradeSelectionIndex > 0)
            {
                gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().DeselectServerRpc();
                upgradeSelectionIndex--;
                gameState.upgradeParent.GetChild(upgradeSelectionIndex).GetComponent<UpgradeUI>().SelectServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void AddCreditsServerRpc(int amount, ulong id)
        {
            NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<SteamPlayer>().credits.Value += amount;
            GameObject spawnedMoneyObject = Instantiate(moneyObjectPrefab, sphere.transform.position, Quaternion.identity);
            spawnedMoneyObject.GetComponent<NetworkObject>().Spawn();
        }


        private IEnumerator Pulse()
        {
            isPulsing = true;
            AddCreditsServerRpc(game.clickCredits, ownerID);
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

