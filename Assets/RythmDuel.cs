using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[System.Serializable]
public class RythmSpawn
{
    public GameObject leftPlayerSpawn;
    public GameObject rightPlayerSpawn;
}

public class RythmDuel : ArcadeGame
{
    public enum GameState { MAIN_MENU, GAME, GAME_OVER, WAVE_CLEARED}

    [Header("Scenes")]
    public GameObject mainMenu;
    public GameObject gameScene;
    public GameObject waveScene;

    public GameObject gameOverScene;

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


    List<NetworkObject> connectedPlayers = new List<NetworkObject>();

    NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();

    [Header("Other")]
    public TextMeshPro connectedPlayersText;
    public TextMeshPro joinText;

    public TextMeshPro leftPlayerName;
    public TextMeshPro rightPlayerName;

    public TextMeshPro waveClearedText;

    public TextMeshPro winnerText;

    public List<RythmSpawn> spawnZones;

    
    float spawnInterval;

    float timer;
    float pingTimer;

    public GameObject target;
    GameObject spawnedTargetLeft;
    GameObject spawnedTargetRight;

    public GameObject leftPlayer;
    public GameObject rightPlayer;



    public Transform leftLives;
    public Transform rightLives;

    int leftLivesCount = 3;
    int rightLivesCount = 3;




    float targetSpeed;

    int waveTarget;

    bool waveCleared = false;

    public int waveNumber = 1;

    public string winner;





    public List<GameObject> targetSpawnedList = new List<GameObject>();





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
    }

    [ClientRpc]
    void ResetClientRpc()
    {

    }

    void MainMenu()
    {
        connectedPlayersText.text = $"{connectedPlayersCount.Value}/2";
        if (connectedPlayersCount.Value == 2 && netGameState.Value != GameState.GAME)
        {
            ChangeStateServerRpc(GameState.GAME);
            if (IsServer)
            {
                leftPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[0].OwnerClientId);
                rightPlayer.GetComponent<NetworkObject>().ChangeOwnership(connectedPlayers[1].OwnerClientId);
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

        if (leftLivesCount <= 0)
        {
            ChangeStateServerRpc(GameState.GAME_OVER);
            winner = connectedPlayers[0].GetComponent<SteamPlayer>().playerName;
            return;
        }

        if (rightLivesCount <= 0)
        {
            ChangeStateServerRpc(GameState.GAME_OVER);
            winner = connectedPlayers[1].GetComponent<SteamPlayer>().playerName;
            return;
        }

        if (isLeft)
        {
            leftLives.GetChild(3 - leftLivesCount).gameObject.SetActive(false);
            leftLivesCount--;
            
        }
        else
        {
            rightLives.GetChild(3 - rightLivesCount).gameObject.SetActive(false);
            rightLivesCount--;
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
        winnerText.text = $"{winner} Wins!";
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
                ResetServerRpc();
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                waveScene.SetActive(false);
                gameOverScene.SetActive(false);
                break;

            case GameState.GAME:
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                waveScene.SetActive(false);
                gameOverScene.SetActive(false);
                break;
            case GameState.WAVE_CLEARED:
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                waveScene.SetActive(true);
                gameOverScene.SetActive(false);
                break;

            case GameState.GAME_OVER:
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                waveScene.SetActive(false);
                gameOverScene.SetActive(true);
                break;
        }
    }
}
