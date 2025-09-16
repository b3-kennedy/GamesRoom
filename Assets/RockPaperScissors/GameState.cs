using TMPro;
using UnityEngine;
using Unity.Netcode;
using System;


namespace Assets.RockPaperScissors
{
    public class GameState : State
    {
        RockPaperScissorsGame rpsGame;

        public TextMeshPro pickingTMP;

        public GameObject pickScreen;

        public GameObject roundWinnerScreen;

        public GameObject[] items;
        

        public enum SelectedItem {ROCK, PAPER, SCISSORS};
        
        public NetworkVariable<SelectedItem> LeftSelectedItem = new NetworkVariable<SelectedItem>(
            SelectedItem.ROCK, // default value
            NetworkVariableReadPermission.Everyone,  // all clients can read
            NetworkVariableWritePermission.Owner     // only the owner can write
        );

        public NetworkVariable<SelectedItem> RightSelectedItem = new NetworkVariable<SelectedItem>(
            SelectedItem.ROCK,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        void Start()
        {
            if (game is RockPaperScissorsGame rps)
            {
                rpsGame = rps;
            }

            LeftSelectedItem.OnValueChanged += SelectItemLeft;
            RightSelectedItem.OnValueChanged += SelectItemRight;
        }

        private void SelectItemRight(SelectedItem previousValue, SelectedItem newValue)
        {
            if (!IsServer) return;

            RightSelectedItem.Value = newValue;


        }

        private void SelectItemLeft(SelectedItem previousValue, SelectedItem newValue)
        {
            if (!IsServer) return;

            LeftSelectedItem.Value = newValue;
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);


        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SelectItemServerRpc(bool isLeft, SelectedItem item)
        {
            Debug.Log("hello?");
            if(isLeft)
            {
                LeftSelectedItem.Value = item;
                rpsGame.leftPlayer.GetComponent<RPSPlayer>().isLockedIn.Value = true;
                rpsGame.leftPlayer.GetComponent<RPSPlayer>().isPicking.Value = false;
                rpsGame.rightPlayer.GetComponent<RPSPlayer>().isPicking.Value = true;
            }
            else
            {
                RightSelectedItem.Value = item;
                rpsGame.rightPlayer.GetComponent<RPSPlayer>().isLockedIn.Value = true;
                rpsGame.leftPlayer.GetComponent<RPSPlayer>().isPicking.Value = true;
                rpsGame.rightPlayer.GetComponent<RPSPlayer>().isPicking.Value = false;
            }

            if (rpsGame.leftPlayer.GetComponent<RPSPlayer>().isLockedIn.Value && rpsGame.rightPlayer.GetComponent<RPSPlayer>().isLockedIn.Value)
            {
                rpsGame.ChangeStateServerRpc(RockPaperScissorsGame.GameState.ROUND_RESULTS);
            }

        }        

        public override void OnStateUpdate()
        {
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

