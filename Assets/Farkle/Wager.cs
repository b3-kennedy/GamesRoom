using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Farkle
{
    public class Wager : State
    {
        FarkleGame farkleGame;
        public TextMeshPro wagerText;
        public TextMeshPro player2Confirm;
        public TextMeshPro player2Buttons;
        public TextMeshPro enterButtonPrompt;
        public TextMeshPro wagerAmountText;
        public NetworkVariable<int> wagerAmount;

        void Start()
        {
            gameObject.SetActive(false);
            if (game is FarkleGame fg)
            {
                farkleGame = fg;
            }

            wagerAmount.OnValueChanged += WagerAmountChanged;
        }

        private void WagerAmountChanged(int previousValue, int newValue)
        {
            wagerAmountText.text = wagerAmount.Value.ToString();
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            SetWagerTextServerRpc();
        }

        public override void OnStateUpdate()
        {

        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }


        [ServerRpc(RequireOwnership = false)]
        public void LockInAmountServerRpc(bool value)
        {
            var ownerID = farkleGame.player2.GetComponent<NetworkObject>().OwnerClientId;
            var player2 = NetworkManager.Singleton.ConnectedClients[ownerID].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
            LockedInClientRpc(player2, value);

        }

        [ClientRpc]
        void LockedInClientRpc(ulong player2ID, bool confirm)
        {
            if (confirm)
            {
                player2Confirm.gameObject.SetActive(true);
                player2Buttons.gameObject.SetActive(true);
                wagerText.gameObject.SetActive(false);
                enterButtonPrompt.gameObject.SetActive(false);
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2ID, out var player))
                {
                    var playerName = player.GetComponent<SteamPlayer>().playerName;
                    player2Confirm.text = $"{playerName} do you agree to this wager?";
                }
            }
            else
            {
                player2Confirm.gameObject.SetActive(false);
                player2Buttons.gameObject.SetActive(false);
                wagerText.gameObject.SetActive(true);
                enterButtonPrompt.gameObject.SetActive(true);
            }


        }

        [ServerRpc(RequireOwnership = false)]
        public void SetWagerAmountServerRpc(int value)
        {
            wagerAmount.Value += value;
        }
        


        [ServerRpc(RequireOwnership = false)]
        void SetWagerTextServerRpc()
        {
            var ownerID = farkleGame.player1.GetComponent<NetworkObject>().OwnerClientId;
            var player1 = NetworkManager.Singleton.ConnectedClients[ownerID].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
            SetWagerTextClientRpc(player1);
        }

        [ClientRpc]
        void SetWagerTextClientRpc(ulong player1ID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1ID, out var player))
            {
                Debug.Log(player);
                var playerName = player.GetComponent<SteamPlayer>().playerName;
                wagerText.text = $"{playerName} pick an amount to wager";
            }
        }
    }
}

