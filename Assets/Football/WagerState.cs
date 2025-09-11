using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using System;

namespace Assets.Football
{
    public class WagerState : State
    {
        FootballGame footballGame;
        public TextMeshPro chooseTMP;
        public TextMeshPro amountTMP;
        public TextMeshPro rightPlayerConfirmTMP;
        public GameObject player1ButtonOptions;
        public GameObject player2ButtonOptions;

        public NetworkVariable<int> wagerAmount;

        public NetworkVariable<bool> isPlayer1Locked;

        GameObject player1;
        GameObject player2;

        string player1Name;
        string player2Name;

        void Start()
        {
            if (game is FootballGame fg)
            {
                footballGame = fg;
            }

            if (IsServer)
            {
                player1 = footballGame.connectedPlayers[0].gameObject;
                player2 = footballGame.connectedPlayers[1].gameObject;
                player1Name = player1.GetComponent<SteamPlayer>().playerName;
                player2Name = player2.GetComponent<SteamPlayer>().playerName;
                SetChooseTextClientRpc(player1Name, player2Name);
            }

            wagerAmount.OnValueChanged += OnWagerChanged;
        }

        private void OnWagerChanged(int previousValue, int newValue)
        {
            amountTMP.text = wagerAmount.Value.ToString();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeWagerAmountServerRpc(int amount)
        {
            int player1Credits = player1.GetComponent<SteamPlayer>().credits.Value;
            int player2Credits = player2.GetComponent<SteamPlayer>().credits.Value;

            int maxWager = Mathf.Min(player1Credits, player2Credits);

            wagerAmount.Value += amount;

            wagerAmount.Value = Mathf.Clamp(wagerAmount.Value, 0, maxWager);
        }

        public override void OnStateEnter()
        {

            gameObject.SetActive(true);

        }

        [ClientRpc]
        void SetChooseTextClientRpc(string p1Name, string p2Name)
        {
            chooseTMP.text = $"{p1Name} choose an amount";
            rightPlayerConfirmTMP.text = $"{p2Name} do you agree to this amount?";
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

        public override void OnStateUpdate()
        {
            if (isPlayer1Locked.Value)
            {
                chooseTMP.gameObject.SetActive(false);
                rightPlayerConfirmTMP.gameObject.SetActive(true);
                player2ButtonOptions.SetActive(true);
                player1ButtonOptions.SetActive(false);
            }
            else
            {
                chooseTMP.gameObject.SetActive(true);
                rightPlayerConfirmTMP.gameObject.SetActive(false);
                player2ButtonOptions.SetActive(false);
                player1ButtonOptions.SetActive(true);
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}
