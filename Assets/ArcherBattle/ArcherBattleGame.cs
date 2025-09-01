using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Assets.CreditClicker;

namespace Assets.ArcherBattle
{
    public class ArcherBattleGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER }

        public GameObject leftPlayer;
        public GameObject rightPlayer;

        public MainMenu mainMenuState;
        public ArcherBattle.GameState gameState;

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

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
        public void AssignPlayersServerRpc()
        {
            if (connectedPlayers.Count == 0) return;

            leftPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
            rightPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);

            ArcheryPlayer left = leftPlayer.GetComponent<ArcheryPlayer>();
            ArcheryPlayer right = rightPlayer.GetComponent<ArcheryPlayer>();

            int turn = Random.Range(0, 2);
            if (turn == 0)
            {
                left.isTurn.Value = true;
                right.isTurn.Value = false;
            }
            else
            {
                left.isTurn.Value = false;
                right.isTurn.Value = true;
            }
            AssignClientRpc(left.GetComponent<NetworkObject>().NetworkObjectId, right.GetComponent<NetworkObject>().NetworkObjectId);
        }

        [ClientRpc]
        void AssignClientRpc(ulong leftPlayerID, ulong rightPlayerID)
        {
            GameObject leftPlayer = null;
            GameObject rightPlayer = null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(leftPlayerID, out var left))
            {
                leftPlayer = left.gameObject;
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(rightPlayerID, out var right))
            {
                rightPlayer = right.gameObject;
            }



            leftPlayer.GetComponent<ArcheryPlayer>().game = this;
            leftPlayer.GetComponent<ArcheryPlayer>().AssignPlayer();

            rightPlayer.GetComponent<ArcheryPlayer>().game = this;
            rightPlayer.GetComponent<ArcheryPlayer>().AssignPlayer();
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
                    break;
            }
        }
    }
}
        