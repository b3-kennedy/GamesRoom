using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

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
                GameObject p1 = Instantiate(playerPrefab, leftPlayerSpawn.position, Quaternion.identity);
                p1.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

            }
            else if (footballGame.connectedPlayers[1] == playerObject)
            {
                GameObject p2 = Instantiate(playerPrefab, rightPlayerSpawn.position, Quaternion.identity);
                p2.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
            }
        }

        void SpawnBall()
        {
            GameObject ball = Instantiate(ballPrefab, ballSpawn.position, Quaternion.identity);
            ball.GetComponent<NetworkObject>().Spawn();
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