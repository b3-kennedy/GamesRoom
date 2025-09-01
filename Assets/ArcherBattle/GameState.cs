using System.Collections;
using Mono.Cecil.Cil;
using Unity.Netcode;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class GameState : State
    {
        ArcherBattleGame archerBattleGame;

        public Transform player1SpawnPos;
        public Transform minSpawnPos;
        public Transform maxSpawnPos;

        public GameObject playerPrefab;

        public GameObject cam;

        public GameObject leftPlayerObject;
        public GameObject rightPlayerObject;

        public NetworkVariable<bool> isLeftPlayerTurn = new NetworkVariable<bool>(false);

        void Start()
        {
            if (game is ArcherBattleGame g)
            {
                archerBattleGame = g;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            if (IsServer)
            {
                
            }



        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayersServerRpc(bool isPlayer1, ulong clientID)
        {
            GameObject spawnedPlayer = null;
            if (isPlayer1)
            {
                spawnedPlayer = Instantiate(playerPrefab, player1SpawnPos.position, Quaternion.identity);
            }
            else
            {
                float randomX = Random.Range(minSpawnPos.position.x, maxSpawnPos.position.x);
                Vector3 spawn = new Vector3(randomX, minSpawnPos.position.y, minSpawnPos.position.z);
                spawnedPlayer = Instantiate(playerPrefab, spawn, Quaternion.Euler(0, 180, 0));
            }
            spawnedPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
            SpawnPlayerClientRpc(isPlayer1, spawnedPlayer.GetComponent<NetworkObject>().NetworkObjectId);
        }

        [ClientRpc]
        void SpawnPlayerClientRpc(bool isPlayer1, ulong objectID)
        {
            GameObject player = null;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var playerObject))
            {
                player = playerObject.gameObject;
            }

            if (!player)
            {
                Debug.Log("Object not found");
                return;
            }


            if (isPlayer1)
            {
                leftPlayerObject = player;
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().playerObject = player;
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().rotater = player.transform.GetChild(4);
            }
            else
            {
                rightPlayerObject = player;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().playerObject = player;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().rotater = player.transform.GetChild(4);
            }

            MoveCameraClientRpc();

        }

        [ServerRpc(RequireOwnership = false)]
        public void OnTurnEndServerRpc()
        {
            isLeftPlayerTurn.Value = !isLeftPlayerTurn.Value;
            if (isLeftPlayerTurn.Value)
            {
                leftPlayerObject.GetComponent<ArcheryPlayer>().isTurn.Value = true;
                rightPlayerObject.GetComponent<ArcheryPlayer>().isTurn.Value = false;
            }
            else
            {
                leftPlayerObject.GetComponent<ArcheryPlayer>().isTurn.Value = false;
                rightPlayerObject.GetComponent<ArcheryPlayer>().isTurn.Value = true;
            }


            MoveCameraClientRpc();
        }

        [ClientRpc]
        void MoveCameraClientRpc()
        {
            Vector3 leftPos = leftPlayerObject.transform.position;
            Vector3 rightPos = rightPlayerObject.transform.position;
            if (isLeftPlayerTurn.Value)
            {
                cam.transform.position = new Vector3(leftPos.x, leftPos.y, cam.transform.position.z);
            }
            else
            {
                cam.transform.position = new Vector3(rightPos.x, rightPos.y, cam.transform.position.z);
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



