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



