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

        public GameObject arrowPrefab;

        public GameObject playerPrefab;

        public GameObject cam;

        public GameObject leftPlayerObject;
        public GameObject rightPlayerObject;

        public NetworkVariable<int> playerSpawnCount;

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
            GameObject spawnedPlayer;
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
            playerSpawnCount.Value++;
            SpawnPlayerClientRpc(isPlayer1, spawnedPlayer.GetComponent<NetworkObject>().NetworkObjectId);

            if (playerSpawnCount.Value == 2)
            {
                OnTurnEndServerRpc();
            }
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
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().arrowSpawn = player.transform.GetChild(4).GetChild(1);
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().chargeBar = player.transform.GetChild(5).GetChild(0).GetChild(0);
            }
            else
            {
                rightPlayerObject = player;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().playerObject = player;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().rotater = player.transform.GetChild(4);
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().arrowSpawn = player.transform.GetChild(4).GetChild(1);
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().chargeBar = player.transform.GetChild(5).GetChild(0).GetChild(0);
            }



        }

        [ServerRpc(RequireOwnership = false)]
        public void OnTurnEndServerRpc()
        {
            isLeftPlayerTurn.Value = !isLeftPlayerTurn.Value;
            Vector3 leftPos = leftPlayerObject.transform.position;
            Vector3 rightPos = rightPlayerObject.transform.position;
            if (isLeftPlayerTurn.Value)
            {
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = true;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = false;
                cam.transform.position = new Vector3(leftPos.x, leftPos.y, cam.transform.position.z);
            }
            else
            {
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = false;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = true;
                cam.transform.position = new Vector3(rightPos.x, rightPos.y, cam.transform.position.z);

            }


            MoveCameraClientRpc(cam.transform.position);
        }



        [ClientRpc]
        void MoveCameraClientRpc(Vector3 pos)
        {
            cam.transform.position = pos;
        }

        [ServerRpc(RequireOwnership = false)]
        public void LaunchArrowServerRpc(Vector3 spawn, Vector3 dir, float force)
        {
            LaunchArrowClientRpc(spawn, dir, force);
        }

        [ClientRpc]
        void LaunchArrowClientRpc(Vector3 spawn, Vector3 dir, float force)
        {
            GameObject arrow = Instantiate(arrowPrefab, spawn, Quaternion.identity);
            arrow.GetComponent<Arrow>().Hit.AddListener(OnArrowHit);
            cam.GetComponent<CameraFollow>().startPos = cam.transform.position;
            cam.GetComponent<CameraFollow>().target = arrow.transform;
            cam.GetComponent<CameraFollow>().isFollow = true;
            arrow.GetComponent<Rigidbody>().AddForce(dir * force, ForceMode.Impulse);
        }


        void OnArrowHit()
        {
            Debug.Log("hit");
            cam.GetComponent<CameraFollow>().isFollow = false;
            OnTurnEndServerRpc();
            StartCoroutine(SwitchTurnAfterTime());
        }



        IEnumerator SwitchTurnAfterTime()
        {
            yield return new WaitForSeconds(3f);
            
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



