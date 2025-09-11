using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Assets.Farkle;

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

        public WagerState wagerState;

        public GameOverState gameOverState;

        public FootballMenuControl footballMenuController1;
        public FootballMenuControl footballMenuController2;

        public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
        public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            // Apply initial state locally
            ApplyState(netGameState.Value);

            wagerState.gameObject.SetActive(false);
            gameOverState.gameObject.SetActive(false);
            gameState.gameObject.SetActive(false);
            mainMenuState.gameObject.SetActive(true);
        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            gameOverState.game = this;
            wagerState.game = this;
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

                if (connectedPlayers.Count == 2)
                {
                    footballMenuController1.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
                    footballMenuController2.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);
                    BeginClientRpc(footballMenuController1.GetComponent<NetworkObject>().NetworkObjectId,
                    footballMenuController2.GetComponent<NetworkObject>().NetworkObjectId); 
                }
            }

        }

        [ClientRpc]
        void BeginClientRpc(ulong p1ObjectID, ulong p2ObjectID)
        {
            FootballMenuControl player1 = null;
            FootballMenuControl player2 = null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(p1ObjectID, out var p1))
            {
                player1 = p1.GetComponent<FootballMenuControl>();
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(p2ObjectID, out var p2))
            {
                player2 = p2.GetComponent<FootballMenuControl>();
            }

            player1.footballGame = this;
            player1.isPlayer1 = true;
            player2.isPlayer1 = false;
            player2.footballGame = this;
        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            connectedPlayers.Clear();
            connectedPlayersCount.Value = 0;
            gameState.OnReset();
            wagerState.wagerAmount.Value = 0;
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
                case GameState.WAGER:
                    wagerState.OnStateExit();
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
            gameOverState.winnerTMP.text = $"{winner} has Won!";
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
                case GameState.WAGER:
                    wagerState.OnStateEnter();
                    break;
            }
        }
    }
}
        