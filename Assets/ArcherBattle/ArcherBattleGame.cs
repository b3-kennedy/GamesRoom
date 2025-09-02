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
        public ArcherBattle.GameOver gameOverState;

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
            }

        }


        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            leftPlayer.GetComponent<ArcheryPlayer>().playerObject.GetComponent<NetworkObject>().Despawn();
            rightPlayer.GetComponent<ArcheryPlayer>().playerObject.GetComponent<NetworkObject>().Despawn();
            leftPlayer.GetComponent<NetworkObject>().ChangeOwnership(0);
            rightPlayer.GetComponent<NetworkObject>().ChangeOwnership(0);
            connectedPlayers.Clear();
            ResetClientRpc();
            ChangeStateServerRpc(GameState.MAIN_MENU);

        }

        [ClientRpc]
        void ResetClientRpc()
        {
            ClearArrows();
        }

        void ClearArrows()
        {
            for (int i = gameState.firedArrows.Count - 1; i >= 0; i--)
            {
                Destroy(gameState.firedArrows[i]);
            }
            gameState.firedArrows.Clear();
        }



        public void AssignPlayers()
        {
            if (connectedPlayers.Count == 0) return;

            if (IsServer)
            {
                leftPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
                rightPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);
                var leftID = NetworkManager.Singleton.ConnectedClients[connectedPlayers[0].OwnerClientId].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
                var rightID = NetworkManager.Singleton.ConnectedClients[connectedPlayers[1].OwnerClientId].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
                DisableMovementClientRpc(leftID, rightID);
            }

            int turn = Random.Range(0, 2);


            ArcheryPlayer left = leftPlayer.GetComponent<ArcheryPlayer>();
            ArcheryPlayer right = rightPlayer.GetComponent<ArcheryPlayer>();
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
            Assign(left);
            Assign(right);
        }

        void Assign(ArcheryPlayer player)
        {
            player.AssignPlayer();
        }

        [ClientRpc]
        void DisableMovementClientRpc(ulong leftID, ulong rightID)
        {
            GameObject leftPlayer = null;
            GameObject rightPlayer = null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(leftID, out var left))
            {
                leftPlayer = left.gameObject;
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(rightID, out var right))
            {
                rightPlayer = right.gameObject;
            }

            leftPlayer.GetComponent<PlayerMovement>().canJump = false;
            rightPlayer.GetComponent<PlayerMovement>().canJump = false;
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
        