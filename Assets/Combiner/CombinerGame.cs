using UnityEngine;
using Unity.Netcode;

namespace Assets.Combiner
{
    public class CombinerGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER, LEADERBOARDS }

        public MainMenu mainMenuState;
        public Combiner.GameState gameState;
        public GameOver gameOverState;
        public Leaderboards leaderboardState;
        public CombinerPlayer player;
        
        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        [HideInInspector] public ulong playerID;

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

            Physics.IgnoreLayerCollision(6, 7);
        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            gameOverState.game = this;

        }

        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {

            if (netGameState.Value == GameState.MAIN_MENU)
            {
                ChangeStateServerRpc(GameState.LEADERBOARDS);
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
                player = p.GetComponent<CombinerPlayer>();


            }

            Debug.Log(player);

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netID, out var playerObj))
            {
                player.playerObject = playerObj.gameObject;
                player.playerObject.GetComponent<PlayerMovement>().canJump = false;
                player.game = this;


            }

            playerID = clientID;
            player.transform.localPosition = new Vector3(2f, 4.48999977f, 1.27999997f);


        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            ChangeStateServerRpc(GameState.MAIN_MENU);
            for (int i = gameState.spawnedBalls.Count - 1; i >= 0 ; i--)
            {
                Destroy(gameState.spawnedBalls[i]);
            }
            gameState.spawnBalls.Clear();
            gameState.score.Value = 0;
            ResetClientRpc();
        }

        [ClientRpc]
        void ResetClientRpc()
        {
            if (player.playerObject)
            {
                player.playerObject.GetComponent<PlayerMovement>().canJump = true;
            }
            player.transform.localPosition = new Vector3(2f, 4.48999977f, 1.27999997f);
            gameState.scoreTMP.text = "Score: 0";
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
                if (gameState.score.Value > LeaderboardHolder.Instance.GetHighScore(LeaderboardHolder.GameType.COMBINER))
                {
                    if (IsServer)
                    {
                        player.GetComponent<PlayerSaver>().combinerHighScore.Value = gameState.score.Value;
                    }
                }
                else if (gameState.score.Value > player.GetComponent<PlayerSaver>().combinerHighScore.Value)
                {
                    if (IsServer)
                    {
                        player.GetComponent<PlayerSaver>().combinerHighScore.Value = gameState.score.Value;
                    }
                }


            }
            LeaderboardHolder.Instance.UpdateCombinerLeaderboardServerRpc();
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
        