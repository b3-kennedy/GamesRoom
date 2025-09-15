using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.RockPaperScissors
{
    public class Result : State
    {

        RockPaperScissorsGame rpsGame;
        public NetworkVariable<bool> isLeftWinner;

        public GameObject leftRock;
        public GameObject leftPaper;
        public GameObject leftScissors;
        public GameObject rightRock;
        public GameObject rightPaper;
        public GameObject rightScissors;

        public TextMeshPro winnerTMP;

        void Start()
        {
            if (game is RockPaperScissorsGame rps)
            {
                rpsGame = rps;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            if (IsServer)
            {
                DecideWinner();
            }
        }
        
        void DecideWinner()
        {
            if(rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.ROCK && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                isLeftWinner.Value = true;
            }
            else if(rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                isLeftWinner.Value = true;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                isLeftWinner.Value = true;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.ROCK && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                isLeftWinner.Value = false;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                isLeftWinner.Value = false;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                isLeftWinner.Value = false;
            }
            ShowWinnerClientRpc();
        }
        
        [ClientRpc]
        void ShowWinnerClientRpc()
        {
            if(rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                leftRock.SetActive(true);
                leftPaper.SetActive(false);
                leftScissors.SetActive(false);
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                leftRock.SetActive(false);
                leftPaper.SetActive(true);
                leftScissors.SetActive(false);
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                leftRock.SetActive(false);
                leftPaper.SetActive(false);
                leftScissors.SetActive(true);
            }
            
            if(rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                rightRock.SetActive(true);
                rightPaper.SetActive(false);
                rightScissors.SetActive(false);
            }
            else if (rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                rightRock.SetActive(false);
                rightPaper.SetActive(true);
                rightScissors.SetActive(false);
            }
            else if (rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                rightRock.SetActive(false);
                rightPaper.SetActive(false);
                rightScissors.SetActive(true);
            }
            
            if(isLeftWinner.Value)
            {
                winnerTMP.text = $"{rpsGame.leftPlayerName} Wins!";
            }
            else
            {
                winnerTMP.text = $"{rpsGame.rightPlayerName} Wins!";
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

