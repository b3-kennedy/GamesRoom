using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Assets.Football
{
    public class FootballGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER, WAGER }

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public MainMenu mainMenuState;
        public Football.GameState gameState;
        public GameOverState gameOverState;

        public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
        public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            // Apply initial state locally
            ApplyState(netGameState.Value);
        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            gameOverState.game = this;
            // wagerState.game = this;
        }



        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {
            if (netGameState.Value == GameState.MAIN_MENU)
            {
                Debug.Log("begun");
                if (connectedPlayers.Count < 2)
                {
                    if (connectedPlayers.Contains(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject)) return;

                    connectedPlayers.Add(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject);
                    connectedPlayersCount.Value = connectedPlayers.Count;
                }
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            connectedPlayers.Clear();
            connectedPlayersCount.Value = 0;
            gameState.OnReset();
            ChangeStateServerRpc(GameState.MAIN_MENU);

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
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetWinnerServerRpc(string winner)
        {
            SetWinnerClientRpc(winner);
        }

        [ClientRpc]
        void SetWinnerClientRpc(string winner)
        {
            gameOverState.winner = winner;
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
                    break;
            }
        }
    }
}
        