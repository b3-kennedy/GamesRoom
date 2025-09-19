using UnityEngine;
using Unity.Netcode;
using Assets.DodgerGame;

namespace Assets.Dodger
{
    public class DodgerGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER, LEADERBOARDS }

        public MainMenu mainMenuState;
        public Dodger.GameState gameState;
        public GameOver gameOverState;
        public Leaderboards leaderboardState;
        public GameObject playerPrefab;
        [HideInInspector] public DodgerPlayer player;

        public Transform playerSpawn;

        ulong playerID;

        

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            // Apply initial state locally
            ApplyState(netGameState.Value);

            mainMenuState.gameObject.SetActive(true);
            gameState.gameObject.SetActive(false);
            gameOverState.gameObject.SetActive(false);
            leaderboardState.gameObject.SetActive(false);

        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            //resultState.game = this;
            gameOverState.game = this;
            //wagerState.game = this;

        }

        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {

            if(netGameState.Value == GameState.MAIN_MENU)
            {
                player = Instantiate(playerPrefab).GetComponent<DodgerPlayer>();
                player.transform.position = new Vector3(-212.630005f, -120f, 143.293686f); 
                player.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);
                ChangeStateServerRpc(GameState.LEADERBOARDS);
                ulong playerObjID = player.GetComponent<NetworkObject>().NetworkObjectId;
                ulong netObjID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
                OnBeginClientRpc(netObjID, playerObjID, clientID);
            }

        }

        [ClientRpc]
        void OnBeginClientRpc(ulong netID, ulong playerObjID, ulong clientID)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerObjID, out var p))
            {
                player = p.GetComponent<DodgerPlayer>();


            }

            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(netID, out var playerObj))
            {
                player.playerObject = playerObj.gameObject;
                player.playerObject.GetComponent<PlayerMovement>().canJump = false;
                player.game = this;


            }

            playerID = clientID;


        }

        [ServerRpc(RequireOwnership = false)]
        void CheckScoreServerRpc(ulong playerID)
        {
            var playerObj = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
            CheckScoreClientRpc(playerObj.GetComponent<NetworkObject>().NetworkObjectId);
        }

        [ClientRpc]
        void CheckScoreClientRpc(ulong objectID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var player))
            {
                if (gameState.score.Value > LeaderboardHolder.Instance.GetHighScore(LeaderboardHolder.GameType.DODGER))
                {
                    if (IsServer)
                    {
                        player.GetComponent<PlayerSaver>().dodgerHighScore.Value = gameState.score.Value;
                    }
                }
                else if (gameState.score.Value > player.GetComponent<PlayerSaver>().dodgerHighScore.Value)
                {
                    if (IsServer)
                    {
                        player.GetComponent<PlayerSaver>().dodgerHighScore.Value = gameState.score.Value;
                    }
                }


            }
            LeaderboardHolder.Instance.UpdateDodgeLeaderboardServerRpc();
        }


        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            ChangeStateServerRpc(GameState.MAIN_MENU);
            gameState.score.Value = 0;
            player.GetComponent<NetworkObject>().Despawn(true);
            ResetClientRpc();
        }
        
        [ClientRpc]
        void ResetClientRpc()
        {
            if (player.playerObject)
            {
                player.playerObject.GetComponent<PlayerMovement>().canJump = true;
                gameState.speed = gameState.baseSpeed;
                
                for (int i = gameState.pipeList.Count - 1; i >= 0 ; i--)
                {
                    Destroy(gameState.pipeList[i]);
                }
                gameState.pipeList.Clear();
                gameState.speedTimer = 0;
                
            }
        }

        void Update()
        {
            if (NetworkManager.Singleton.LocalClientId != playerID) return;
            
            if(Input.GetKeyDown(KeyCode.E) && netGameState.Value == GameState.LEADERBOARDS)
            {
                ChangeStateServerRpc(GameState.GAME);
            }
        }

        private void OnNetworkGameStateChanged(GameState oldState, GameState newState)
        {
            LeaveState(oldState);
            Debug.Log($"Game state changed from {oldState} to {newState}");
            ApplyState(newState);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeStateServerRpc(GameState newState)
        {
            netGameState.Value = newState;
        }

        void LeaveState(GameState state)
        {
            switch (state)
            {
                case GameState.MAIN_MENU:
                    mainMenuState.OnStateExit();             
                    break;
                case GameState.GAME:
                    gameState.OnStateExit();             
                    break;
                case GameState.GAME_OVER:
                    gameOverState.OnStateExit();
                    break;
                case GameState.LEADERBOARDS:
                    leaderboardState.OnStateExit();
                    break;
            }
        }

        private void ApplyState(GameState state)
        {
            switch (state)
            {
                case GameState.MAIN_MENU:
                    mainMenuState.OnStateEnter();
                    break;

                case GameState.GAME:
                    gameState.OnStateEnter();
                    break;

                case GameState.GAME_OVER:
                    gameOverState.OnStateEnter();
                    CheckScoreServerRpc(playerID);
                    break;
                case GameState.LEADERBOARDS:
                    leaderboardState.OnStateEnter();
                    break;
            }
        }
    }
}
        