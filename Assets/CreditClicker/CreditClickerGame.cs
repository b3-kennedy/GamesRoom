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
        public State gameState;
        public Player player;

        [Header("Game Settings")]
        public int baseClickCredits;
        [HideInInspector] public int clickCredits;

        public int baseDoubleChance;
        [HideInInspector] public int doubleChance;

        public float baseIncomeSpeed;
        public float incomeSpeed;

        public float interestAmount;


        void Start()
        {
            clickCredits = baseClickCredits;
            incomeSpeed = baseIncomeSpeed;
            mainMenu.game = this;
            gameState.game = this;
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
                ulong netObjID = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
                netGameState.Value = GameState.GAME;
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
                player.OnPlayerAssigned();


            }


        }

        [ServerRpc(RequireOwnership = false)]
        public override void ResetServerRpc()
        {
            //player.gameObject.GetComponent<NetworkObject>().ChangeOwnership(0);
            netGameState.Value = GameState.MAIN_MENU;
            UpgradeManager upgradeManager = player.GetComponent<UpgradeManager>();
            for (int i = upgradeManager.passiveGainers.Count - 1; i >= 0; i--)
            {
                Destroy(upgradeManager.passiveGainers[i].gameObject);
            }
            upgradeManager.passiveGainers.Clear();
            player.GetComponent<UpgradeManager>().upgrades.Clear();
            ResetClientRpc();
            Debug.Log("RESET");
        }

        [ClientRpc]
        void ResetClientRpc()
        {
            if (player.playerObject)
            {
                player.playerObject.GetComponent<PlayerMovement>().canJump = true;
            }
            incomeSpeed = baseIncomeSpeed;
            clickCredits = baseClickCredits;
            doubleChance = baseDoubleChance;
            interestAmount = 0;
            gameState.OnReset();

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
                    mainMenu.OnStateEnter();
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
        