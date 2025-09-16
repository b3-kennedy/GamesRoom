using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.RockPaperScissors
{
    public class Result : State
    {

        RockPaperScissorsGame rpsGame;

        public enum ResultState { LEFT_WIN, RIGHT_WIN, DRAW}

        public NetworkVariable<ResultState> resultState = new NetworkVariable<ResultState>(
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
        public TextMeshPro leftPlayerNameTMP;
        public TextMeshPro rightPlayerNameTMP;
        public TextMeshPro leftScoreTMP;
        public TextMeshPro rightScoreTMP;

        public NetworkVariable<int> leftScore;
        public NetworkVariable<int> rightScore;

        void Start()
        {
            if (game is RockPaperScissorsGame rps)
            {
                rpsGame = rps;
            }

            leftScore.OnValueChanged += LeftScoreChanged;
            rightScore.OnValueChanged += RightScoreChanged;
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
                resultState.Value = ResultState.LEFT_WIN;
                leftScore.Value++;
            }
            else if(rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                resultState.Value = ResultState.LEFT_WIN;
                leftScore.Value++;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                resultState.Value = ResultState.LEFT_WIN;
                leftScore.Value++;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.ROCK && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.PAPER)
            {
                resultState.Value = ResultState.RIGHT_WIN;
                rightScore.Value++;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.PAPER && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.SCISSORS)
            {
                resultState.Value = ResultState.RIGHT_WIN;
                rightScore.Value++;
            }
            else if (rpsGame.gameState.LeftSelectedItem.Value == GameState.SelectedItem.SCISSORS && rpsGame.gameState.RightSelectedItem.Value == GameState.SelectedItem.ROCK)
            {
                resultState.Value = ResultState.RIGHT_WIN;
                rightScore.Value++;
            }
            else if(rpsGame.gameState.LeftSelectedItem.Value == rpsGame.gameState.RightSelectedItem.Value)
            {
                resultState.Value = ResultState.DRAW;
            }
            ShowWinnerClientRpc(rpsGame.gameState.LeftSelectedItem.Value, rpsGame.gameState.RightSelectedItem.Value, resultState.Value);
        }
        
        [ClientRpc]
        void ShowWinnerClientRpc(GameState.SelectedItem left, GameState.SelectedItem right, ResultState result)
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

            leftPlayerNameTMP.text = rpsGame.leftPlayerName;
            rightPlayerNameTMP.text = rpsGame.rightPlayerName;
            
            
            if(result == ResultState.LEFT_WIN)
            {
                winnerTMP.text = $"{rpsGame.leftPlayerName} Wins!";
            }
            else if(result == ResultState.RIGHT_WIN)
            {
                winnerTMP.text = $"{rpsGame.rightPlayerName} Wins!";
            }
            else if(result == ResultState.DRAW)
            {
                winnerTMP.text = $"Draw!";
            }
            
            if(IsServer)
            {
                StartCoroutine(BackToGame());
            }
        }

        private void RightScoreChanged(int previousValue, int newValue)
        {
            rightScoreTMP.text = newValue.ToString();
        }

        private void LeftScoreChanged(int previousValue, int newValue)
        {
            leftScoreTMP.text = newValue.ToString();
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

