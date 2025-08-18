using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;

[System.Serializable]
public class RythmSpawn
{
    public GameObject leftPlayerSpawn;
    public GameObject rightPlayerSpawn;
}

public class RythmDuel : ArcadeGame
{
    public enum GameState { MAIN_MENU, GAME, GAME_OVER }

    public GameObject mainMenu;

    public GameObject gameScene;
    public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

    public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
    public TextMeshPro connectedPlayersText;
    public TextMeshPro joinText;

    public List<RythmSpawn> spawnZones;

    public float baseSpawnInterval = 1f;
    float spawnInterval;

    float timer;
    float pingTimer;

    public GameObject target;
    GameObject spawnedTargetLeft;
    GameObject spawnedTargetRight;

    public GameObject leftPlayer;
    public GameObject rightPlayer;

    public TextMeshPro leftPlayerName;
    public TextMeshPro rightPlayerName;

    public Transform leftLives;
    public Transform rightLives;

    int leftLivesCount = 3;
    int rightLivesCount = 3;





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
        spawnInterval = baseSpawnInterval;
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
        if (connectedPlayersCount.Value == 1 && netGameState.Value != GameState.GAME)
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
            Debug.Log("left dead");
            return;
        }

        if (rightLivesCount <= 0)
        {
            Debug.Log("right dead");
            return;
        }

        if (isLeft)
        {
            leftLives.GetChild(0).gameObject.SetActive(false);
            leftLivesCount--;
        }
        else
        {
            rightLives.GetChild(0).gameObject.SetActive(false);
            rightLivesCount--;
        }
    }


    void Game()
    {
        if (!IsServer) return;

        float ping = GetClientWithHighestRTT();

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            int spawnType = Random.Range(0, 3);
            if (spawnType == 0)
            {
                SpawnSingle(ping);
            }
            else if (spawnType == 1)
            {
                SpawnDouble(ping);
            }
            else
            {
                SpawnTriple(ping);
            }
            

            timer = 0;
        }
    }

    void SpawnTriple(float ping)
    {
        for (int i = 0; i < 3; i++)
        {
            spawnedTargetLeft = SpawnTarget(spawnZones[i].leftPlayerSpawn.transform.position, true);
            spawnedTargetRight = SpawnTarget(spawnZones[i].rightPlayerSpawn.transform.position, false);
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
        StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));

        spawnedTargetLeft = SpawnTarget(spawnZones[secondLane].leftPlayerSpawn.transform.position, true);
        spawnedTargetRight = SpawnTarget(spawnZones[secondLane].rightPlayerSpawn.transform.position, false);
        StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));

    }

    void SpawnSingle(float ping)
    {
        // Choose random spawn zone safely
        int spawnIndex = Random.Range(0, spawnZones.Count);

        // Spawn both sides
        spawnedTargetLeft = SpawnTarget(spawnZones[spawnIndex].leftPlayerSpawn.transform.position, true);
        spawnedTargetRight = SpawnTarget(spawnZones[spawnIndex].rightPlayerSpawn.transform.position, false);

        // Delay showing them until ping-adjusted time
        StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));
    }

    private GameObject SpawnTarget(Vector3 position, bool isLeft)
    {
        // Instantiate prefab
        var obj = Instantiate(target, position, Quaternion.identity);

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


    [ServerRpc(RequireOwnership = false)]
    public void GameOverServerRpc()
    {
        netGameState.Value = GameState.GAME_OVER;
    }

    private void ApplyState(GameState state)
    {
        switch (state)
        {
            case GameState.MAIN_MENU:
                ResetServerRpc();
                mainMenu.SetActive(true);
                gameScene.SetActive(false);
                break;

            case GameState.GAME:
                mainMenu.SetActive(false);
                gameScene.SetActive(true);
                break;

            case GameState.GAME_OVER:
                mainMenu.SetActive(false);
                gameScene.SetActive(false);
                break;
        }
    }
}
