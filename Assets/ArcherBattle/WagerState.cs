using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.ArcherBattle
{
    public class WagerState : State
    {
        ArcherBattleGame archerBattleGame;
        public NetworkVariable<int> wagerAmount;

        public GameObject player1ButtonOptions;

        public GameObject player2ButtonOptions;
        public TextMeshPro wagerAmountText;
        public TextMeshPro chooseText;
        public TextMeshPro rightPlayerConfirmText;

        string player1Name;
        string player2Name;

        SteamPlayer player1;
        SteamPlayer player2;

        public NetworkVariable<bool> isPlayer1Locked = new NetworkVariable<bool>(false);

        void Start()
        {
            if (game is ArcherBattleGame g)
            {
                archerBattleGame = g;
            }

            wagerAmount.OnValueChanged += UpdateText;
        }

        private void UpdateText(int previousValue, int newValue)
        {
            wagerAmountText.text = newValue.ToString();
        }

        public override void OnStateEnter()
        {
            if (IsServer)
            {
                var leftPlayerObject = NetworkManager.Singleton.ConnectedClients[archerBattleGame.leftPlayer.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;
                player1Name = leftPlayerObject.GetComponent<SteamPlayer>().playerName;
                player1 = leftPlayerObject.GetComponent<SteamPlayer>();
                var rightPlayerObject = NetworkManager.Singleton.ConnectedClients[archerBattleGame.rightPlayer.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;
                player2Name = rightPlayerObject.GetComponent<SteamPlayer>().playerName;
                player2 = rightPlayerObject.GetComponent<SteamPlayer>();
                SetTextValuesClientRpc(player1Name, player2Name);
            }
            gameObject.SetActive(true);
        }

        [ClientRpc]
        void SetTextValuesClientRpc(string player1Name, string player2Name)
        {
            chooseText.text = $"{player1Name} choose an amount to wager";
            rightPlayerConfirmText.text = $"{player2Name} do you agree to this amount?";
        }

        public override void OnStateUpdate()
        {
            if (isPlayer1Locked.Value)
            {
                chooseText.gameObject.SetActive(false);
                rightPlayerConfirmText.gameObject.SetActive(true);
                player2ButtonOptions.SetActive(true);
                player1ButtonOptions.SetActive(false);
            }
            else
            {
                chooseText.gameObject.SetActive(true);
                rightPlayerConfirmText.gameObject.SetActive(false);
                player2ButtonOptions.SetActive(false);
                player1ButtonOptions.SetActive(true);
            }
        }

        public override void OnStateExit()
        {
            if (IsServer)
            {
                archerBattleGame.AssignPlayers();
            }
            
            gameObject.SetActive(false);

        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeWagerAmountServerRpc(int amount)
        {
            int player1Credits = player1.credits.Value;
            int player2Credits = player2.credits.Value;

            int maxWager = Mathf.Min(player1Credits, player2Credits);

            wagerAmount.Value += amount;

            wagerAmount.Value = Mathf.Clamp(wagerAmount.Value, 0, maxWager);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ZeroWagerAmountServerRpc()
        {
            wagerAmount.Value = 0;
        }


        [ServerRpc(RequireOwnership = false)]
        public void ChangePlayer1LockedInStateServerRpc()
        {
            isPlayer1Locked.Value = !isPlayer1Locked.Value;
        }
    }
}

