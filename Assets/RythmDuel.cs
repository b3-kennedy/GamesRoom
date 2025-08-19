using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

[System.Serializable]
public class RythmSpawn
{
    public GameObject leftPlayerSpawn;
    public GameObject rightPlayerSpawn;
}

public class RythmDuel : ArcadeGame
{
    public enum GameState { MAIN_MENU, GAME, GAME_OVER, WAVE_CLEARED, WAGER}

    [Header("Scenes")]
    public GameObject mainMenu;
    public GameObject gameScene;
    public GameObject waveScene;

    public GameObject gameOverScene;

    public GameObject wagerScene;

    [Header("Game Settings")]
    public float timeBetweenWaves = 5f;
    float waveTimer;

    public int startingWaveTarget = 15;

    public float waveIncreaseTargetMultiplier = 1.5f;

    public float baseTargetSpeed = 2f;

    public float targetSpeedIncreaseMultiplier = 1.25f;

    public float baseSpawnInterval = 1f;

    public float spawnIntervalDecreaseMultiplier = 1.25f;

    public float maxTargets = 50f;

    public float maxSpeed = 10f;

    public float minSpawnInterval = 0.2f;

    [Header("Main Menu Scene")]

    List<NetworkObject> connectedPlayers = new List<NetworkObject>();
    NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
    public TextMeshPro connectedPlayersText;
    public TextMeshPro joinText;

    [Header("Wager Scene")]

    public TextMeshPro playerName;
    public TextMeshPro wagerAmountText;
    public TextMeshPro rightPlayerConfirmText;
    public NetworkVariable<int> wagerAmount;

    public Transform buttonParent;

    public Transform rightPlayerButtonOptions;

    

    public bool isLeftPlayerLocked;

    [Header("Game Scene")]

    public GameObject leftPlayer;
    public GameObject rightPlayer;
    public TextMeshPro leftPlayerName;
    public TextMeshPro rightPlayerName;
    public List<RythmSpawn> spawnZones;
    public GameObject target;

    public Transform leftLives;
    public Transform rightLives;

    [Header("Wave Scene")]

    public TextMeshPro waveClearedText;

    public int waveNumber = 1;

    [Header("Game Over Scene")]

    public TextMeshPro winnerText;
    SteamPlayer winner;
    SteamPlayer loser;







    
    float spawnInterval;

    float timer;
    float pingTimer;


    GameObject spawnedTargetLeft;
    GameObject spawnedTargetRight;

    int leftLivesCount = 3;
    int rightLivesCount = 3;
    float targetSpeed;

    int waveTarget;

    bool waveCleared = false;

    List<GameObject> targetSpawnedList = new List<GameObject>();

    // Network variable for syncing game state across clients
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

        


    }

    [ServerRpc(RequireOwnership = false)]
    public override void BeginServerRpc(ulong clientID)
    {
        waveTarget = startingWaveTarget;
        spawnInterval = baseSpawnInterval;
        targetSpeed = baseTargetSpeed;
        if (connectedPlayers.Count < 2)
        {
            if (connectedPlayers.Contains(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject)) return;

            connectedPlayers.Add(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject);
            connectedPlayersCount.Value = connectedPlayers.Count;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ResetServerRpc()
    {
        connectedPlayersCount.Value = 0;
        wagerAmount.Value = 0;
        ResetClientRpc();
    }

    [ClientRpc]
    void ResetClientRpc()
    {
        
        rightLivesCount = 3;
        leftLivesCount = 3;
        connectedPlayers.Clear();
        for (int i = targetSpawnedList.Count - 1; i >= 0; i--)
        {
            Destroy(targetSpawnedList[i].gameObject);

        }
        for (int i = 0; i < leftLives.childCount; i++)
        {
            leftLives.GetChild(i).gameObject.SetActive(true);
            rightLives.GetChild(i).gameObject.SetActive(true);
        }


        targetSpawnedList.Clear();
        targetSpeed = baseTargetSpeed;
        spawnInterval = baseSpawnInterval;
        waveTarget = startingWaveTarget;
        Debug.Log(connectedPlayersCount.Value);
        ChangeStateServerRpc(GameState.MAIN_MENU);

    }

    void MainMenu()
    {
        connectedPlayersText.text = $"{connectedPlayersCount.Value}/2";
        if (connectedPlayersCount.Value == 2 && netGameState.Value != GameState.WAGER)
        {
            ChangeStateServerRpc(GameState.WAGER);
            if (IsServer)
            {
                leftPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
                leftPlayer.GetComponent<RhythmPlayer>().isLeftPlayer = true;
                rightPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);
                rightPlayer.GetComponent<RhythmPlayer>().isLeftPlayer = false;
                var leftPlayerObject = NetworkManager.Singleton.ConnectedClients[connectedPlayers[0].OwnerClientId].PlayerObject;
                var rightPlayerObject = NetworkManager.Singleton.ConnectedClients[connectedPlayers[1].OwnerClientId].PlayerObject;
                string leftPlayerName = leftPlayerObject.GetComponent<SteamPlayer>().playerName;
                string rightPlayerName = rightPlayerObject.GetComponent<SteamPlayer>().playerName;
                SetPlayerNamesClientRpc(leftPlayerName, rightPlayerName);

            }
            
            
        }
    }

    [ClientRpc]
    void SetPlayerNamesClientRpc(string player1Name, string player2Name)
    {
        leftPlayerName.text = player1Name;
        rightPlayerName.text = player2Name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveLifeServerRpc(bool isLeft)
    {
        RemoveLifeClientRpc(isLeft);
    }

    [ClientRpc]
    void RemoveLifeClientRpc(bool isLeft)
    {
        var lPlayer = leftPlayer.GetComponent<RhythmPlayer>();
        var rPlayer = rightPlayer.GetComponent<RhythmPlayer>();
        var leftPlayerObject = NetworkManager.Singleton.ConnectedClients[rightPlayer.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;
        var rightPlayerObject = NetworkManager.Singleton.ConnectedClients[leftPlayer.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;

        if (isLeft && lPlayer.canLoseLife)
        {
            leftLives.GetChild(3 - leftLivesCount).gameObject.SetActive(false);
            lPlayer.canLoseLife = false;
            lPlayer.LostLife();
            leftLivesCount--;

        }
        else if (!isLeft && rPlayer.canLoseLife)
        {
            rightLives.GetChild(3 - rightLivesCount).gameObject.SetActive(false);
            rPlayer.canLoseLife = false;
            rPlayer.LostLife();
            rightLivesCount--;
        }

        if (leftLivesCount <= 0)
        {
            ChangeStateServerRpc(GameState.GAME_OVER);

            winner = rightPlayerObject.GetComponent<SteamPlayer>();
            loser = leftPlayerObject.GetComponent<SteamPlayer>();
            return;
        }

        if (rightLivesCount <= 0)
        {
            ChangeStateServerRpc(GameState.GAME_OVER);
            winner = leftPlayerObject.GetComponent<SteamPlayer>();
            loser = rightPlayerObject.GetComponent<SteamPlayer>();
            return;
        }
    }


    void Game()
    {
        if (!IsServer) return;

        float ping = GetClientWithHighestRTT();

        if (!waveCleared)
        {
            if (targetSpawnedList.Count < waveTarget)
            {
                timer += Time.deltaTime;
                if (timer >= spawnInterval)
                {
                    int spawnType = Random.Range(0, 3);
                    if (spawnType == 0) SpawnSingle(ping);
                    else if (spawnType == 1) SpawnDouble(ping);
                    else SpawnTriple(ping);

                    timer = 0;
                }
            }

            if (targetSpawnedList.Count >= waveTarget && targetSpawnedList.All(t => t == null))
            {
                waveCleared = true;
                StartCoroutine(EndOfRoundBuffer());
                
            }
        }
    }

    IEnumerator EndOfRoundBuffer()
    {
        yield return new WaitForSeconds(1f);
        ChangeStateServerRpc(GameState.WAVE_CLEARED);
    }

    void WaveCleared()
    {
        waveTimer += Time.deltaTime;
        if (waveTimer >= timeBetweenWaves)
        {
            waveNumber++;

            waveClearedText.text = $"WAVE {waveNumber} COMPLETE";

            targetSpawnedList.Clear();
            waveCleared = false;
            if (targetSpeed < maxSpeed)
            {
                targetSpeed *= targetSpeedIncreaseMultiplier;
            }
            else
            {
                targetSpeed = maxSpeed;
            }

            if (spawnInterval > minSpawnInterval)
            {
                spawnInterval /= spawnIntervalDecreaseMultiplier;
            }
            else
            {
                spawnInterval = minSpawnInterval;
            }

            if (waveTarget < maxTargets)
            {
                waveTarget = Mathf.RoundToInt(waveTarget * waveIncreaseTargetMultiplier);
            }
            else
            {
                waveTarget = Mathf.RoundToInt(maxTargets);
            }
            

            ChangeStateServerRpc(GameState.GAME);
            waveTimer = 0;
        }
    }

    void GameOver()
    {
        winnerText.text = $"{winner.playerName} Wins!";
        PayoutWagerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void PayoutWagerServerRpc()
    {
        winner.credits.Value += wagerAmount.Value;
        loser.credits.Value -= wagerAmount.Value;
    }

    void Wager()
    {
        if (!IsServer) return;

        var leftPlayerObject = NetworkManager.Singleton.ConnectedClients[leftPlayer.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;
        var leftPlayerName = leftPlayerObject.GetComponent<SteamPlayer>().playerName;
        var rightPlayerObject = NetworkManager.Singleton.ConnectedClients[rightPlayer.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;
        var rightPlayerName = rightPlayerObject.GetComponent<SteamPlayer>().playerName;
        WagerClientRpc(leftPlayerName, rightPlayerName);


    }

    [ClientRpc]
    void WagerClientRpc(string leftPlayerName, string rightPlayerName)
    {

        playerName.text = $"{leftPlayerName} pick an amount to wager";
        wagerAmountText.text = wagerAmount.Value.ToString();

        if (isLeftPlayerLocked)
        {
            buttonParent.gameObject.SetActive(false);
            rightPlayerConfirmText.text = $"{rightPlayerName}, do you agree to this wager?";
            rightPlayerConfirmText.gameObject.SetActive(true);
            rightPlayerButtonOptions.gameObject.SetActive(true);
        }
        else
        {
            rightPlayer.GetComponent<RhythmPlayer>().canInput = true;
            buttonParent.gameObject.SetActive(true);
            rightPlayerConfirmText.gameObject.SetActive(false);
            rightPlayerButtonOptions.gameObject.SetActive(false);
        }
    }

    void SpawnTriple(float ping)
    {
        for (int i = 0; i < 3; i++)
        {
            spawnedTargetLeft = SpawnTarget(spawnZones[i].leftPlayerSpawn.transform.position, true);
            spawnedTargetRight = SpawnTarget(spawnZones[i].rightPlayerSpawn.transform.position, false);
            targetSpawnedList.Add(spawnedTargetLeft);
            targetSpawnedList.Add(spawnedTargetRight);
            StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));

        }
    }

    void SpawnDouble(float ping)
    {
        // Pick two unique lanes
        int firstLane = Random.Range(0, spawnZones.Count);
        int secondLane;

        do
        {
            secondLane = Random.Range(0, spawnZones.Count);
        } while (secondLane == firstLane);

        spawnedTargetLeft = SpawnTarget(spawnZones[firstLane].leftPlayerSpawn.transform.position, true);
        spawnedTargetRight = SpawnTarget(spawnZones[firstLane].rightPlayerSpawn.transform.position, false);
        targetSpawnedList.Add(spawnedTargetLeft);
        targetSpawnedList.Add(spawnedTargetRight);
        StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));

        spawnedTargetLeft = SpawnTarget(spawnZones[secondLane].leftPlayerSpawn.transform.position, true);
        spawnedTargetRight = SpawnTarget(spawnZones[secondLane].rightPlayerSpawn.transform.position, false);
        targetSpawnedList.Add(spawnedTargetLeft);
        targetSpawnedList.Add(spawnedTargetRight);
        StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));

    }

    void SpawnSingle(float ping)
    {
        // Choose random spawn zone safely
        int spawnIndex = Random.Range(0, spawnZones.Count);

        // Spawn both sides
        spawnedTargetLeft = SpawnTarget(spawnZones[spawnIndex].leftPlayerSpawn.transform.position, true);
        spawnedTargetRight = SpawnTarget(spawnZones[spawnIndex].rightPlayerSpawn.transform.position, false);
        targetSpawnedList.Add(spawnedTargetLeft);
        targetSpawnedList.Add(spawnedTargetRight);


        // Delay showing them until ping-adjusted time
        StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));
    }

    private GameObject SpawnTarget(Vector3 position, bool isLeft)
    {
        // Instantiate prefab
        var obj = Instantiate(target, position, Quaternion.identity);

        obj.GetComponent<Move>().speed = targetSpeed;

        

        // Assign Move component if present
        if (obj.TryGetComponent<Move>(out var move))
        {
            move.duel = this;
            move.isLeft = isLeft;
        }

        // Disable visuals until reveal
        if (obj.TryGetComponent<MeshRenderer>(out var renderer))
        {
            renderer.enabled = false;
        }

        // Network spawn
        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
        else
        {
            Debug.LogError("[RythmDuel] Target prefab missing NetworkObject!");
        }

        return obj;
    }

    void PlayerInput()
    {
        if (!IsOwner) return;

    }



    void Update()
    {
        if (netGameState.Value == GameState.MAIN_MENU)
        {
            MainMenu();
        }
        else if (netGameState.Value == GameState.GAME)
        {
            PlayerInput();
            Game();
        }
        else if (netGameState.Value == GameState.WAVE_CLEARED)
        {
            WaveCleared();
        }
        else if (netGameState.Value == GameState.GAME_OVER)
        {
            GameOver();
        }
        else if (netGameState.Value == GameState.WAGER)
        {
            Wager();
        }
    }

    IEnumerator EnableOnServer(float time, GameObject leftTarget, GameObject rightTarget)
    {
        yield return new WaitForSeconds(time);
        leftTarget.GetComponent<MeshRenderer>().enabled = true;
        rightTarget.GetComponent<MeshRenderer>().enabled = true;
    }

    float GetClientWithHighestRTT()
    {
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        float maxRTT = 0f;
        ulong clientIdWithMaxRTT = 0;

        foreach (var client in connectedPlayers)
        {
            float rtt = transport.GetCurrentRtt(client.OwnerClientId) / 1000f; // Convert ms to seconds
            if (rtt > maxRTT)
            {
                maxRTT = rtt;
                clientIdWithMaxRTT = client.OwnerClientId;
            }
        }

        return maxRTT/2f;
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
    }


    private void ApplyState(GameState state)
    {
        switch (state)
        {
            case GameState.MAIN_MENU:
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                waveScene.SetActive(false);
                gameOverScene.SetActive(false);
                wagerScene.SetActive(false);
                break;

            case GameState.GAME:
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                waveScene.SetActive(false);
                gameOverScene.SetActive(false);
                wagerScene.SetActive(false);
                break;
            case GameState.WAVE_CLEARED:
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                waveScene.SetActive(true);
                gameOverScene.SetActive(false);
                wagerScene.SetActive(false);
                break;
            case GameState.GAME_OVER:
                for (int i = targetSpawnedList.Count - 1; i >= 0; i--)
                {
                    Destroy(targetSpawnedList[i].gameObject);

                }
                targetSpawnedList.Clear();
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                waveScene.SetActive(false);
                gameOverScene.SetActive(true);
                wagerScene.SetActive(false);
                break;
            case GameState.WAGER:
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                waveScene.SetActive(false);
                gameOverScene.SetActive(false);
                wagerScene.SetActive(true);
                break;

        }
    }
}
