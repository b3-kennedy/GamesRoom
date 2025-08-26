using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using System;

namespace Assets.CreditClicker
{
    public class GameState : State
    {
        public GameObject upgradePanel;
        public Transform upgradePanelStart;
        public Transform upgradePanelFinish;
        public Transform sphereSpawnsParent;
        public GameObject activeUpgrades;
        public GameObject passiveUpgrades;

        public TextMeshPro creditCountText;

        [HideInInspector] public Transform upgradeParent;

        public float lerpDuration = 0.5f;

        [HideInInspector] public NetworkVariable<bool> isUpgradePanelOpen;

        [HideInInspector] public Player player;

        SteamPlayer steamPlayer;

        void Start()
        {
            upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            activeUpgrades.SetActive(true);
            passiveUpgrades.SetActive(false);
            Debug.Log(player);

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
            base.OnStateEnter();
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }

    }
}

