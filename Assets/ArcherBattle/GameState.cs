using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using TMPro;
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

        public GameObject floor;

        public NetworkVariable<int> playerSpawnCount;

        public NetworkVariable<bool> isLeftPlayerTurn = new NetworkVariable<bool>(false);

        [HideInInspector] public SteamPlayer winningPlayer;
        [HideInInspector] public SteamPlayer losingPlayer;

        [HideInInspector] public List<GameObject> firedArrows = new List<GameObject>();

        bool isGameOver;

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
            isGameOver = false;
            if (IsServer)
            {
                playerSpawnCount.Value = 0;
            }

            floor.SetActive(true);

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
                SetTurnServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void SetTurnServerRpc()
        {
            int turn = Random.Range(0, 2);
            if (turn == 0)
            {
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = true;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = false;
            }
            else
            {
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = false;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = true;
            }
            OnTurnEndServerRpc();
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
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().playerNameText = player.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>();
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().AddListeners();
                
            }
            else
            {
                rightPlayerObject = player;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().playerObject = player;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().rotater = player.transform.GetChild(4);
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().arrowSpawn = player.transform.GetChild(4).GetChild(1);
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().chargeBar = player.transform.GetChild(5).GetChild(0).GetChild(0);
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().playerNameText = player.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>();
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().AddListeners();
                
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnTurnEndServerRpc()
        {
            Vector3 leftPos = leftPlayerObject.transform.position;
            Vector3 rightPos = rightPlayerObject.transform.position;
            if (archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value)
            {
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = false;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = true;
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().ChangeShotValueServerRpc(false);
                cam.transform.position = new Vector3(rightPos.x, rightPos.y, cam.transform.position.z);
            }
            else
            {
                archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = true;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().isTurn.Value = false;
                archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().ChangeShotValueServerRpc(false);
                cam.transform.position = new Vector3(leftPos.x, leftPos.y, cam.transform.position.z);
            }
            MoveCameraClientRpc(cam.transform.position);
        }



        [ClientRpc]
        void MoveCameraClientRpc(Vector3 pos)
        {
            cam.GetComponent<CameraFollow>().isFollow = false;
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
            cam.GetComponent<CameraFollow>().startPos = cam.transform.position;
            cam.GetComponent<CameraFollow>().target = arrow.transform;
            cam.GetComponent<CameraFollow>().isFollow = true;
            arrow.GetComponent<Rigidbody>().AddForce(dir * force, ForceMode.Impulse);
            arrow.GetComponent<Arrow>().Hit.AddListener(OnArrowHit);
            firedArrows.Add(arrow);
        }


        void OnArrowHit()
        {
            Debug.Log("hit");
            if (gameObject.activeSelf)
            {
                StartCoroutine(SwitchTurnAfterTime());
            }
            
        }

        public void OnGameOver(string playerName)
        {
            Debug.Log($"{playerName} has lost");
            isGameOver = true;

            SteamPlayer client1 = archerBattleGame.connectedPlayers[0].GetComponent<SteamPlayer>();
            SteamPlayer client2 = archerBattleGame.connectedPlayers[1].GetComponent<SteamPlayer>();

            if (client1.playerName == playerName)
            {
                winningPlayer = client2;
                losingPlayer = client1;
                archerBattleGame.gameOverState.SetWinnerServerRpc(client2.playerName);
            }
            else
            {
                winningPlayer = client1;
                losingPlayer = client2;
                archerBattleGame.gameOverState.SetWinnerServerRpc(client1.playerName);
            }

            if (IsServer)
            {
                winningPlayer.credits.Value += archerBattleGame.wagerState.wagerAmount.Value;
                losingPlayer.credits.Value -= archerBattleGame.wagerState.wagerAmount.Value;
            }

            archerBattleGame.leftPlayer.GetComponent<ArcheryPlayer>().OnGameEndServerRpc();
            archerBattleGame.rightPlayer.GetComponent<ArcheryPlayer>().OnGameEndServerRpc();

            StartCoroutine(EndGameAfterTime());
        }

        IEnumerator EndGameAfterTime()
        {
            yield return new WaitForSeconds(3);
            archerBattleGame.ChangeStateServerRpc(ArcherBattleGame.GameState.GAME_OVER);
        }

        IEnumerator SwitchTurnAfterTime()
        {
            yield return new WaitForSeconds(3f);
            if (!isGameOver)
            {
                OnTurnEndServerRpc();
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



