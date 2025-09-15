using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.RockPaperScissors
{
    public class Result : State
    {

        RockPaperScissorsGame rpsGame;

        public enum ResultState { LEFT_WIN, RIGHT_WIN, DRAW}

        public NetworkVariable<ResultState> resutState = new NetworkVariable<ResultState>(
            ResultState.DRAW,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

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
            Debug.Log("winner");
            if(rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.ROCK && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                resutState.Value = ResultState.LEFT_WIN;
            }
            else if(rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                resutState.Value = ResultState.LEFT_WIN;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                resutState.Value = ResultState.LEFT_WIN;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.ROCK && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                resutState.Value = ResultState.RIGHT_WIN;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                resutState.Value = ResultState.RIGHT_WIN;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                resutState.Value = ResultState.RIGHT_WIN;
            }
            else if(rpsGame.gameState.LeftSelectedItem.Value == rpsGame.gameState.RightSelectedItem.Value)
            {
                resutState.Value = ResultState.DRAW;
            }
            ShowWinnerClientRpc(rpsGame.gameState.LeftSelectedItem.Value, rpsGame.gameState.RightSelectedItem.Value);
        }
        
        [ClientRpc]
        void ShowWinnerClientRpc(GameState.SelectedItem left, GameState.SelectedItem right)
        {
            if(left == GameState.SelectedItem.ROCK)
            {
                leftRock.SetActive(true);
                leftPaper.SetActive(false);
                leftScissors.SetActive(false);
            }
            else if (left == GameState.SelectedItem.PAPER)
            {
                leftRock.SetActive(false);
                leftPaper.SetActive(true);
                leftScissors.SetActive(false);
            }
            else if (left == GameState.SelectedItem.SCISSORS)
            {
                leftRock.SetActive(false);
                leftPaper.SetActive(false);
                leftScissors.SetActive(true);
            }
            
            if(right == GameState.SelectedItem.ROCK)
            {
                rightRock.SetActive(true);
                rightPaper.SetActive(false);
                rightScissors.SetActive(false);
            }
            else if (right == GameState.SelectedItem.PAPER)
            {
                rightRock.SetActive(false);
                rightPaper.SetActive(true);
                rightScissors.SetActive(false);
            }
            else if (right == GameState.SelectedItem.SCISSORS)
            {
                rightRock.SetActive(false);
                rightPaper.SetActive(false);
                rightScissors.SetActive(true);
            }
            
            if(resutState.Value == ResultState.LEFT_WIN)
            {
                winnerTMP.text = $"{rpsGame.leftPlayerName} Wins!";
            }
            else if(resutState.Value == ResultState.RIGHT_WIN)
            {
                winnerTMP.text = $"{rpsGame.rightPlayerName} Wins!";
            }
            else if(resutState.Value == ResultState.DRAW)
            {
                winnerTMP.text = $"Draw!";
            }
            
            if(IsServer)
            {
                StartCoroutine(BackToGame());
            }
        }
        
        IEnumerator BackToGame()
        {
            yield return new WaitForSeconds(3);
            rpsGame.ResetPlayersServerRpc();
            rpsGame.ChangeStateServerRpc(RockPaperScissorsGame.GameState.GAME);
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

