using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;


namespace Assets.Farkle
{
    public class FarkleGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER}

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public State mainMenu;

        public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
        public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;
            mainMenu.game = this;

            // Apply initial state locally
            ApplyState(netGameState.Value);


        }

        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {
            Debug.Log("begun");
            if (connectedPlayers.Count < 2)
            {
                if (connectedPlayers.Contains(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject)) return;

                connectedPlayers.Add(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject);
                connectedPlayersCount.Value = connectedPlayers.Count;
            }
        }

        private void OnNetworkGameStateChanged(GameState oldState, GameState newState)
        {

            LeaveState(oldState);
            Debug.Log($"Game state changed from {oldState} to {newState}");
            ApplyState(newState);

        }

        // Update is called once per frame
        void Update()
        {

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
                    mainMenu.OnStateExit();
                    break;

                case GameState.GAME:
                    break;

                case GameState.GAME_OVER:
                    break;
            }
        }


        private void ApplyState(GameState state)
        {
            switch (state)
            {
                case GameState.MAIN_MENU:
                    mainMenu.OnStateEnter();
                    break;

                case GameState.GAME:
                    break;

                case GameState.GAME_OVER:
                    break;
            }
        }
    }

}


