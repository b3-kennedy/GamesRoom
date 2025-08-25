using UnityEngine;
using Unity.Netcode;

namespace Assets.CreditClicker
{
    public class CreditClickerGame : Game
    {
        public enum GameState { MAIN_MENU, GAME, GAME_OVER }

        public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
            GameState.MAIN_MENU,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public State mainMenu;
        public State game;
        public Player player;

        [Header("Game Settings")]
        public int baseClickCredits;
        [HideInInspector] public int clickCredits;

        public float baseIncomeSpeed;
        [HideInInspector] public float incomeSpeed;

        void Start()
        {
            clickCredits = baseClickCredits;
            incomeSpeed = baseIncomeSpeed;
            mainMenu.game = this;
            game.game = this;
            // Listen for state changes
            netGameState.OnValueChanged += OnNetworkGameStateChanged;

            // Apply initial state locally
            ApplyState(netGameState.Value);
        }

        [ServerRpc(RequireOwnership = false)]
        public override void BeginServerRpc(ulong clientID)
        {

            if (netGameState.Value == GameState.MAIN_MENU)
            {
                player.gameObject.GetComponent<NetworkObject>().ChangeOwnership(clientID);
                player.playerObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject;
                player.playerObject.GetComponent<PlayerMovement>().canJump = false;
                player.game = this;
                player.OnPlayerAssigned();
                netGameState.Value = GameState.GAME;
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            player.gameObject.GetComponent<NetworkObject>().ChangeOwnership(0);
            netGameState.Value = GameState.MAIN_MENU;
            ResetClientRpc();
        }

        [ClientRpc]
        void ResetClientRpc()
        {
            player.playerObject.GetComponent<PlayerMovement>().canJump = true;
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
                    mainMenu.OnStateExit();                
                    break;
                case GameState.GAME:
                    game.OnStateExit();                    
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
                    game.OnStateEnter();
                    break;

                case GameState.GAME_OVER:
                    break;
            }
        }
    }
            
            
            
}
        