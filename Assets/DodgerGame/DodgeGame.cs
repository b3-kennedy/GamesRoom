using UnityEngine;
using Unity.Netcode;

namespace Assets.DodgerGame
{
    public class DodgerGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER }

        public MainMenu mainMenuState;

        public GameObject player;
        
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
            //gameState.gameObject.SetActive(false);
            //resultState.gameObject.SetActive(false);
            //gameOverState.gameObject.SetActive(false);
            //wagerState.gameObject.SetActive(false);
        }

        void Awake()
        {
            mainMenuState.game = this;
            //gameState.game = this;
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
                    break;

                case GameState.GAME:
                    break;

                case GameState.GAME_OVER:
                    break;
            }
        }
    }
}
        