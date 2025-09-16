using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Assets.RockPaperScissors
{
    public class RockPaperScissorsGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER, ROUND_RESULTS}

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public List<NetworkObject> connectedPlayers = new List<NetworkObject>();
        public NetworkVariable<int> connectedPlayersCount;

        public MainMenu mainMenuState;
        public RockPaperScissors.GameState gameState;

        public GameOver gameOverState;

        public Result resultState;

        public GameObject leftPlayer;
        public GameObject rightPlayer;

        public string leftPlayerName;
        public string rightPlayerName;

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            // Apply initial state locally
            ApplyState(netGameState.Value);

            mainMenuState.gameObject.SetActive(true);
            gameState.gameObject.SetActive(false);
            resultState.gameObject.SetActive(false);
            gameOverState.gameObject.SetActive(false);
        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            resultState.game = this;
            gameOverState.game = this;

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
                    leftPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
                    rightPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);
                    ulong left = connectedPlayers[0].NetworkObjectId;
                    ulong right = connectedPlayers[1].NetworkObjectId;
                    BeginClientRpc(left, right);
                    SetTurns();
                }
            }
        }

        [ClientRpc]
        void BeginClientRpc(ulong leftPlayerObjectID, ulong rightPlayerObjectID)
        {
            NetworkObject player1 = null;
            NetworkObject player2 = null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(leftPlayerObjectID, out var p1))
            {
                player1 = p1;
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(rightPlayerObjectID, out var p2))
            {
                player2 = p2;
            }
            leftPlayer.GetComponent<RPSPlayer>().playerObject = player1.gameObject;
            rightPlayer.GetComponent<RPSPlayer>().playerObject = player2.gameObject;
            leftPlayer.GetComponent<RPSPlayer>().rpsGame = this;
            rightPlayer.GetComponent<RPSPlayer>().rpsGame = this;

            leftPlayerName = player1.GetComponent<SteamPlayer>().playerName;
            rightPlayerName = player2.GetComponent<SteamPlayer>().playerName;

            leftPlayer.GetComponent<RPSPlayer>().OnTurnChange();
            rightPlayer.GetComponent<RPSPlayer>().OnTurnChange();
        }

        public void SetTurns()
        {
            leftPlayer.GetComponent<RPSPlayer>().isPicking.Value = true;
            rightPlayer.GetComponent<RPSPlayer>().isPicking.Value = false;
        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            connectedPlayers.Clear();
            connectedPlayersCount.Value = 0;
            ChangeStateServerRpc(GameState.MAIN_MENU);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetPlayersServerRpc()
        {
            leftPlayer.GetComponent<RPSPlayer>().isPicking.Value = true;
            rightPlayer.GetComponent<RPSPlayer>().isPicking.Value = false;
            leftPlayer.GetComponent<RPSPlayer>().isLockedIn.Value = false;
            rightPlayer.GetComponent<RPSPlayer>().isLockedIn.Value = false;

        }
        
        [ClientRpc]
        void ResetPlayersClientRpc()
        {
            leftPlayer.GetComponent<RPSPlayer>().index = 0;
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
                case GameState.ROUND_RESULTS:
                    resultState.OnStateExit();
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
                    break;

                case GameState.ROUND_RESULTS:
                    resultState.OnStateEnter();
                    break;
            }
        }
    }
}
        