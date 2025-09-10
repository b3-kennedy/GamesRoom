using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System;

namespace Assets.Football
{
    public class GameState : State
    {
        FootballGame footballGame;
        public Camera cam;
        public FootballPlayer player1;
        public FootballPlayer player2;
        public GameObject playerPrefab;
        public GameObject ballPrefab;
        public Transform leftPlayerSpawn;
        public Transform rightPlayerSpawn;
        public Transform ballSpawn;
        public NetworkVariable<int> leftPlayerScore;
        public NetworkVariable<int> rightPlayerScore;

        public TextMeshPro player1ScoreTMP;
        public TextMeshPro player2ScoreTMP;

        GameObject ball;
        GameObject player1GO;
        GameObject player2GO;

        string player1Name;
        string player2Name;
        int playersAssigned = 0;

        public GameObject winner;
        public GameObject loser;

        void Start()
        {
            if (game is FootballGame fg)
            {
                footballGame = fg;
            }
            leftPlayerScore.OnValueChanged += UpdateLeftScoreText;
            rightPlayerScore.OnValueChanged += UpdateRightScoreText;
        }


        public override void OnStateEnter()
        {
            SpawnAndAssignPlayersServerRpc(NetworkManager.Singleton.LocalClientId);
            if (IsServer)
            {
                SpawnBall();
            }
            gameObject.SetActive(true);
            cam.orthographicSize = 10f;

        }

        public override void OnReset()
        {
            if (IsServer)
            {
                ball.GetComponent<NetworkObject>().Despawn();
                player1GO.GetComponent<NetworkObject>().Despawn();
                player2GO.GetComponent<NetworkObject>().Despawn();
                leftPlayerScore.Value = 0;
                rightPlayerScore.Value = 0;
            }


        }

        [ServerRpc(RequireOwnership = false)]
        void SpawnAndAssignPlayersServerRpc(ulong clientID)
        {
            NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
            if (footballGame.connectedPlayers[0] == playerObject)
            {
                player1GO = Instantiate(playerPrefab, leftPlayerSpawn.position, Quaternion.identity);
                player1GO.GetComponent<FootballPlayer>().footballGame = footballGame;
                player1GO.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                playersAssigned++;
                player1Name = playerObject.GetComponent<SteamPlayer>().playerName;


            }
            else if (footballGame.connectedPlayers[1] == playerObject)
            {
                player2GO = Instantiate(playerPrefab, rightPlayerSpawn.position, Quaternion.identity);
                player2GO.GetComponent<FootballPlayer>().footballGame = footballGame;
                player2GO.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                playersAssigned++;
                player2Name = playerObject.GetComponent<SteamPlayer>().playerName;
            }

            if (playersAssigned >= 2)
            {
                SetPlayerNamesClientRpc(player1Name, player2Name);
                SetScoreTextClientRpc();
            }
        }

        [ClientRpc]
        void SetPlayerNamesClientRpc(string p1Name, string p2Name)
        {
            player1Name = p1Name;
            player2Name = p2Name;
        }

        void SpawnBall()
        {
            ball = Instantiate(ballPrefab, ballSpawn.position, Quaternion.identity);
            ball.GetComponent<Ball>().gameState = this;
            ball.GetComponent<NetworkObject>().Spawn();



            
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnGoalServerRpc(bool isRight)
        {
            if (isRight)
            {
                leftPlayerScore.Value++;
            }
            else
            {
                rightPlayerScore.Value++;
            }
            ResetPositions();


        }

        [ClientRpc]
        void SetScoreTextClientRpc()
        {
            player1ScoreTMP.text = $"{player1Name}: {0}";
            player2ScoreTMP.text = $"{0}: {player2Name}";
        }

        private void UpdateLeftScoreText(int previousValue, int newValue)
        {
            player1ScoreTMP.text = $"{player1Name}: {newValue}";
            if (IsServer)
            {
                if (newValue == 5)
                {
                    winner = NetworkManager.Singleton.ConnectedClients[player1GO.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.gameObject;
                    loser = NetworkManager.Singleton.ConnectedClients[player2GO.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.gameObject;
                    footballGame.gameOverState.SetWinnerServerRpc(winner.GetComponent<SteamPlayer>().playerName);
                    footballGame.ChangeStateServerRpc(FootballGame.GameState.GAME_OVER);
                }
            }

        }

        private void UpdateRightScoreText(int previousValue, int newValue)
        {
            player2ScoreTMP.text = $"{newValue}: {player2Name}";
            if (IsServer)
            {
                if (newValue == 5)
                {
                    winner = NetworkManager.Singleton.ConnectedClients[player2GO.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.gameObject;
                    loser = NetworkManager.Singleton.ConnectedClients[player1GO.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.gameObject;
                    footballGame.gameOverState.SetWinnerServerRpc(winner.GetComponent<SteamPlayer>().playerName);
                    footballGame.ChangeStateServerRpc(FootballGame.GameState.GAME_OVER);
                }
            }
        }

        void ResetPositions()
        {
            ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            ball.transform.position = ballSpawn.position;

            player1GO.transform.position = leftPlayerSpawn.position;
            player2GO.transform.position = rightPlayerSpawn.position;


        }

        [ClientRpc]
        void ResetPositionsClientRpc()
        {
            
        }


        public override void OnStateUpdate()
        {

        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
            cam.orthographicSize = 5f;
        }
    }
}