using UnityEngine;
using Unity.Netcode;
using Assets.DodgerGame;

namespace Assets.Dodger
{
    public class DodgerGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER }

        public MainMenu mainMenuState;
        public Dodger.GameState gameState;
        public DodgerPlayer player;

        

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
            //resultState.gameObject.SetActive(false);
            //gameOverState.gameObject.SetActive(false);
            //wagerState.gameObject.SetActive(false);
        }

        void Awake()
        {
            mainMenuState.game = this;
            gameState.game = this;
            //resultState.game = this;
            //gameOverState.game = this;
            //wagerState.game = this;

        }

        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {

            if(netGameState.Value == GameState.MAIN_MENU)
            {
                player.GetComponent<NetworkObject>().ChangeOwnership(clientID);
                ChangeStateServerRpc(GameState.GAME);
                ulong netObjID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
                OnBeginClientRpc(netObjID);
            }

        }

        [ClientRpc]
        void OnBeginClientRpc(ulong netID)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(netID, out var playerObj))
            {
                player.playerObject = playerObj.gameObject;
                player.playerObject.GetComponent<PlayerMovement>().canJump = false;
                player.game = this;


            }


        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            ChangeStateServerRpc(GameState.MAIN_MENU);
            ResetClientRpc();
        }
        
        [ClientRpc]
        void ResetClientRpc()
        {
            if (player.playerObject)
            {
                player.playerObject.GetComponent<PlayerMovement>().canJump = true;
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
        