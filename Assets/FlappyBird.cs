using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class FlappyBird : ArcadeGame
{
    public GameObject mainMenu;
    public GameObject gameScene;
    public FlappyBirdLevel level;

    public GameObject bird;

    public NetworkVariable<int> score = new NetworkVariable<int>();

    public enum GameState { MAIN_MENU, GAME, GAME_OVER }

    // Network variable for syncing game state across clients
    public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
        GameState.MAIN_MENU,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public TextMeshPro scoreText;

    void Start()
    {
        // Listen for state changes
        netGameState.OnValueChanged += OnNetworkGameStateChanged;

        // Apply initial state locally
        ApplyState(netGameState.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void BeginServerRpc(ulong clientID)
    {
        netGameState.Value = GameState.GAME;
        bird.GetComponent<NetworkObject>().ChangeOwnership(clientID);
    }


    void Update()
    {
        if (netGameState.Value == GameState.GAME)
        {
            level.Move();
        }
    }

    private void OnNetworkGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"Game state changed from {oldState} to {newState}");
        ApplyState(newState);

        if (IsServer && newState == GameState.GAME)
        {
            // Server triggers pipe spawning when game starts
            level.SpawnPipesServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseScoreServerRpc()
    {
        score.Value++;
    }

    private void ApplyState(GameState state)
    {
        switch (state)
        {
            case GameState.MAIN_MENU:
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                break;

            case GameState.GAME:
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                break;

            case GameState.GAME_OVER:
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                // TODO: Add game over UI logic here
                break;
        }
    }
}

    
