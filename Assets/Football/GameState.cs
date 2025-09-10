using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

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

        void Start()
        {
            if (game is FootballGame fg)
            {
                footballGame = fg;
            }
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

        [ServerRpc(RequireOwnership = false)]
        void SpawnAndAssignPlayersServerRpc(ulong clientID)
        {
            NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
            if (footballGame.connectedPlayers[0] == playerObject)
            {
                player1GO = Instantiate(playerPrefab, leftPlayerSpawn.position, Quaternion.identity);
                player1GO.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                playersAssigned++;
                player1Name = playerObject.GetComponent<SteamPlayer>().playerName;


            }
            else if (footballGame.connectedPlayers[1] == playerObject)
            {
                player2GO = Instantiate(playerPrefab, rightPlayerSpawn.position, Quaternion.identity);
                player2GO.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                playersAssigned++;
                player2Name = playerObject.GetComponent<SteamPlayer>().playerName;
            }

            if (playersAssigned >= 2)
            {
                SetPlayerNamesClientRpc(player1Name, player2Name);
                UpdateScoreTextClientRpc();
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
            UpdateScoreTextClientRpc();


        }

        [ClientRpc]
        void UpdateScoreTextClientRpc()
        {
            player1ScoreTMP.text = $"{player1Name}: {leftPlayerScore.Value}";
            player2ScoreTMP.text = $"{rightPlayerScore.Value}: {player2Name}";
        }

        void ResetPositions()
        {
            ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            ball.transform.position = ballSpawn.position;

            player1GO.transform.position = leftPlayerSpawn.position;
            player2GO.transform.position = rightPlayerSpawn.position;


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