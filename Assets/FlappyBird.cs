using TMPro;
using Unity.Burst;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class FlappyBird : ArcadeGame
{
    public GameObject mainMenu;
    public GameObject gameScene;

    public GameObject gameOverScene;

    public FlappyBirdLevel level;

    public GameObject birdPrefab;
    public GameObject bird;

    public AudioSource musicAudioSource;

    public NetworkVariable<int> score = new NetworkVariable<int>();

    public enum GameState { MAIN_MENU, GAME, GAME_OVER }

    // Network variable for syncing game state across clients
    public NetworkVariable<GameState> netGameState = new NetworkVariable<GameState>(
        GameState.MAIN_MENU,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public TextMeshPro scoreText;
    public TextMeshPro gameOverScoreText;

    public TextMeshPro speedText;

    Vector3 birdPosition;

    void Start()
    {
        // Listen for state changes
        netGameState.OnValueChanged += OnNetworkGameStateChanged;

        // Apply initial state locally
        ApplyState(netGameState.Value);

        scoreText.text = "SCORE: 0";

        speedText.text = "SPEED x1";

        score.OnValueChanged += OnScoreChanged;

        birdPosition = new Vector3(-97.4796448f, -120f, 143.583679f);
        

    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSpeedTextServerRpc()
    {
        UpdateSpeedTextClientRpc();
    }

    [ClientRpc]
    void UpdateSpeedTextClientRpc()
    {
        float baseSpeed = level.baseSpeed;
        float currentSpeed = level.GetSpeed();

        float multiplier = currentSpeed / baseSpeed;

        speedText.text = $"SPEED x{multiplier:F2}";
    }

    [ServerRpc(RequireOwnership = false)]
    public override void BeginServerRpc(ulong clientID)
    {

        netGameState.Value = GameState.GAME;
        if (bird != null)
        {
            Destroy(bird);
        }
        bird = Instantiate(birdPrefab);

        bird.transform.position = birdPosition;
        var netObj = bird.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientID);
        HookBirdEventsClientRpc(netObj.NetworkObjectId);

    }

    [ClientRpc]
    void HookBirdEventsClientRpc(ulong birdNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(birdNetId, out var netObj))
        {
            var b = netObj.GetComponent<Bird>();
            b.hitPipe.AddListener(HitPipe);
            b.increaseScore.AddListener(IncreaseScore);
        }
    }

    void HitPipe()
    {
        GameOverServerRpc();
    }

    void IncreaseScore()
    { 
        IncreaseScoreServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]    
    public override void ResetServerRpc()
    {
        score.Value = 0;
        netGameState.Value = GameState.MAIN_MENU;
        ResetClientRpc();
    }

    [ClientRpc]
    void ResetClientRpc()
    {
        if (bird)
        {
            bird.transform.localPosition = birdPosition;
            bird.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }

        scoreText.text = "SCORE: 0";
        level.ClearPipes();

    }






    void Update()
    {
        if (netGameState.Value == GameState.GAME)
        {
            level.Move();
        }
        else if (Input.GetKeyDown(KeyCode.E) && netGameState.Value == GameState.GAME_OVER)
        {
            
            ChangeStateServerRpc(GameState.MAIN_MENU);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeStateServerRpc(GameState newState)
    {
        netGameState.Value = newState;
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

    [ServerRpc(RequireOwnership = false)]
    public void GameOverServerRpc()
    {
        netGameState.Value = GameState.GAME_OVER;
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
                musicAudioSource.Stop();
                ResetServerRpc();
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                gameOverScene.SetActive(false);
                break;

            case GameState.GAME:
                gameOverScene.SetActive(false);
                musicAudioSource.Play();
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                break;

            case GameState.GAME_OVER:
                musicAudioSource.Stop();
                gameOverScene.SetActive(true);
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                gameOverScoreText.text = $"SCORE: {score.Value}";
                break;
        }
    }
}

    
