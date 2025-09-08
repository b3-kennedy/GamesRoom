using TMPro;
using System.IO;
using Unity.Netcode;
using UnityEngine;


public class FlappyBird : Game
{
    public GameObject mainMenu;
    public GameObject gameScene;

    public GameObject gameOverScene;

    public GameObject leaderboardScene;

    public FlappyBirdLevel level;

    public GameObject birdPrefab;
    public GameObject bird;

    public AudioSource musicAudioSource;

    public NetworkVariable<int> score = new NetworkVariable<int>();

    public enum GameState { MAIN_MENU, GAME, GAME_OVER, LEADERBOARDS}

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

    ulong playerID;

    ulong serverPlayerID;

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

        leaderboardScene.SetActive(false);
        gameOverScene.SetActive(false);
        gameScene.SetActive(false);
        
        

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
        if (netGameState.Value == GameState.MAIN_MENU)
        {
            netGameState.Value = GameState.LEADERBOARDS;
            serverPlayerID = clientID;
        }


    }

    [ClientRpc]
    void HookBirdEventsClientRpc(ulong clientID,ulong birdNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(birdNetId, out var netObj))
        {
            var b = netObj.GetComponent<Bird>();
            b.hitPipe.AddListener(HitPipe);
            b.increaseScore.AddListener(IncreaseScore);
        }
        playerID = clientID;
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

    [ServerRpc(RequireOwnership = false)]
    void CheckScoreServerRpc(ulong playerID)
    {
        var playerObj = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
        CheckScoreClientRpc(playerObj.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    void CheckScoreClientRpc(ulong objectID)
    {
        

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var player))
        {
            if (score.Value > LeaderboardHolder.Instance.GetHighScore())
            {
                BeatServerHighScoreServerRpc(objectID, score.Value);
                if (IsServer)
                {
                    player.GetComponent<PlayerSaver>().fbHighScore.Value = score.Value;
                }
            }
            else if (score.Value > player.GetComponent<PlayerSaver>().fbHighScore.Value)
            {
                if (IsServer)
                {
                    player.GetComponent<PlayerSaver>().fbHighScore.Value = score.Value;
                }

                BeatPersonalHighScoreServerRpc(objectID, player.GetComponent<PlayerSaver>().fbHighScore.Value);
            }


        }
        LeaderboardHolder.Instance.UpdateLeaderboardServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void BeatPersonalHighScoreServerRpc(ulong objectID, int score)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var player))
        {
            player.GetComponent<SteamPlayer>().credits.Value += score * 2;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void BeatServerHighScoreServerRpc(ulong objectID, int score)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var player))
        {
            player.GetComponent<SteamPlayer>().credits.Value += score * 5;
        }
    }




    void Update()
    {
        if (netGameState.Value == GameState.GAME)
        {
            level.Move();
        }
        else if (Input.GetKeyDown(KeyCode.R) && netGameState.Value == GameState.GAME_OVER)
        {

            ChangeStateServerRpc(GameState.MAIN_MENU);
        }
        else if (Input.GetKeyDown(KeyCode.E) && netGameState.Value == GameState.LEADERBOARDS)
        {
            SpawnBirdServerRpc();
            ChangeStateServerRpc(GameState.GAME);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnBirdServerRpc()
    {
        if (bird != null)
        {
            Destroy(bird);
        }
        bird = Instantiate(birdPrefab);
        bird.transform.position = birdPosition;
        var netObj = bird.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(serverPlayerID);

        HookBirdEventsClientRpc(serverPlayerID, netObj.NetworkObjectId);
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
                leaderboardScene.SetActive(false);
                gameScene.SetActive(false);
                gameOverScene.SetActive(false);
                break;

            case GameState.GAME:
                gameOverScene.SetActive(false);
                leaderboardScene.SetActive(false);
                musicAudioSource.Play();
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                break;

            case GameState.GAME_OVER:
                musicAudioSource.Stop();
                leaderboardScene.SetActive(false);
                gameOverScene.SetActive(true);
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                gameOverScoreText.text = $"SCORE: {score.Value}";
                if (IsServer)
                {
                    bird.GetComponent<NetworkObject>().Despawn(true);
                }
                CheckScoreServerRpc(playerID);
                break;

            case GameState.LEADERBOARDS:
                leaderboardScene.SetActive(true);
                gameOverScene.SetActive(false);
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                break;
        }
    }
}

    
