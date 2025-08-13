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

        scoreText.text = "SCORE: 0";

        score.OnValueChanged += OnScoreChanged;

    }

    [ServerRpc(RequireOwnership = false)]
    public override void BeginServerRpc(ulong clientID)
    {
        netGameState.Value = GameState.GAME;
        bird.GetComponent<NetworkObject>().ChangeOwnership(clientID);
    }

    public override void Reset()
    {

        bird.transform.localPosition = new Vector3(0f, 0f, 1.59f);
        bird.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        scoreText.text = "SCORE: 0";
        level.ClearPipes();
        if (IsServer)
        {
            score.Value = 0;
            netGameState.Value = GameState.MAIN_MENU;
        }
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

    // Server only: increment score
    [ServerRpc(RequireOwnership = false)]
    public void IncreaseScoreServerRpc()
    {
        score.Value++;
    }

    // Called automatically on all clients when score changes
    private void OnScoreChanged(int oldValue, int newValue)
    {
        scoreText.text = $"SCORE: {newValue}";
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

    
