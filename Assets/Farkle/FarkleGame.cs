using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;


namespace Assets.Farkle
{
    public class FarkleGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER, WAGER}

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public State mainMenu;
        public State gameState;
        public State wagerState;

        public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
        public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

        public GameObject player1;
        public GameObject player2;

        void Start()
        {
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            mainMenu.game = this;
            gameState.game = this;
            wagerState.game = this;

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

        public void AssignPlayers()
        {
            player1.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
            player1.GetComponent<FarklePlayer>().isPlayer1 = true;
            player1.GetComponent<FarklePlayer>().farkleGame = this;
            player2.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);
            player2.GetComponent<FarklePlayer>().isPlayer1 = false;
            player2.GetComponent<FarklePlayer>().farkleGame = this;
        }

        public void SetFirstTurn(int player)
        {
            Debug.Log("first turn");
            if (player == 0)
            {
                player1.GetComponent<FarklePlayer>().isTurn.Value = true;
                player2.GetComponent<FarklePlayer>().isTurn.Value = false;
            }
            else
            {
                player1.GetComponent<FarklePlayer>().isTurn.Value = false;
                player2.GetComponent<FarklePlayer>().isTurn.Value = true;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SwitchTurnServerRpc(bool isPlayer1)
        {
            if (isPlayer1)
            {
                player1.GetComponent<FarklePlayer>().isTurn.Value = false;
                player1.GetComponent<FarklePlayer>().isTurn.Value = true;
            }
            else
            {
                player1.GetComponent<FarklePlayer>().isTurn.Value = true;
                player1.GetComponent<FarklePlayer>().isTurn.Value = false;
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
                    gameState.OnStateExit();
                    break;
                case GameState.WAGER:
                    wagerState.OnStateExit();
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
                    gameState.OnStateEnter();
                    break;
                case GameState.WAGER:
                    wagerState.OnStateEnter();
                    break;
                case GameState.GAME_OVER:
                    break;
            }
        }
    }

}


