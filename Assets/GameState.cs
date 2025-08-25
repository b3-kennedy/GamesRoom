using Unity.Netcode;
using UnityEngine;
using System.Collections;

namespace Assets.CreditClicker
{
    public class GameState : State
    {
        public GameObject upgradePanel;
        public Transform upgradePanelStart;
        public Transform upgradePanelFinish;

        [HideInInspector] public Transform upgradeParent;

        public float lerpDuration = 0.5f;

        [HideInInspector] public NetworkVariable<bool> isUpgradePanelOpen;

        void Start()
        {
            upgradeParent = upgradePanel.transform.GetChild(0).GetChild(0).GetChild(0);
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
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

