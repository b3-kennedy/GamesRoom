using UnityEngine;
using Unity.Netcode;

namespace Assets.Snake
{
    public class SnakeGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER, LEADERBOARD }

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public MainMenu mainMenuState;
        public Snake.GameState gameState;
        public GameOver gameOverState;
        public Leaderboard leaderboardState;

        public SnakePlayer player;

        ulong playerID;

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            // Apply initial state locally
            ApplyState(netGameState.Value);
            gameState.gameObject.SetActive(false);
            gameOverState.gameObject.SetActive(false);
            mainMenuState.gameObject.SetActive(true);
            leaderboardState.gameObject.SetActive(false);
        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            gameOverState.game = this;
            leaderboardState.game = this;

        }

        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {

            if (netGameState.Value == GameState.MAIN_MENU)
            {
                ChangeStateServerRpc(GameState.LEADERBOARD);
                player.GetComponent<NetworkObject>().ChangeOwnership(clientID);
                ulong playerObjID = player.GetComponent<NetworkObject>().NetworkObjectId;
                ulong netObjID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
                OnBeginClientRpc(netObjID, playerObjID, clientID);
            }

        }

        [ClientRpc]
        void OnBeginClientRpc(ulong netID, ulong playerObjID, ulong clientID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjID, out var p))
            {
                player = p.GetComponent<SnakePlayer>();


            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netID, out var playerObj))
            {
                player.playerObject = playerObj.gameObject;
                player.playerObject.GetComponent<PlayerMovement>().canJump = false;
                player.game = this;


            }

            playerID = clientID;


        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            ChangeStateServerRpc(GameState.MAIN_MENU);
            for (int i = 1; i < player.snakePositions.Count; i++)
            {
                player.snakePositions.RemoveAt(i);
            }
            player.snakePositions[0] = new Vector2Int(12, 12);
            //ResetClientRpc();
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
                if (gameState.score.Value > LeaderboardHolder.Instance.GetHighScore(LeaderboardHolder.GameType.SNAKE))
                {
                    if (IsServer)
                    {
                        player.GetComponent<PlayerSaver>().snakeHighScore.Value = gameState.score.Value;
                    }
                }
                else if (gameState.score.Value > player.GetComponent<PlayerSaver>().snakeHighScore.Value)
                {
                    if (IsServer)
                    {
                        player.GetComponent<PlayerSaver>().snakeHighScore.Value = gameState.score.Value;
                    }
                }


            }
            LeaderboardHolder.Instance.UpdateSnakeLeaderboardServerRpc();
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
                case GameState.LEADERBOARD:
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

                case GameState.LEADERBOARD:
                    leaderboardState.OnStateEnter();
                    break;
            }
        }
    }
}
        