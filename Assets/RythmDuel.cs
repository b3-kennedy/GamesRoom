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


    void Game()
    {
        if (!IsServer) return;

        float ping = GetClientWithHighestRTT();

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            int spawnIndex = Random.Range(0, 3);
            spawnedTargetLeft = Instantiate(target, spawnZones[spawnIndex].leftPlayerSpawn.transform.position, Quaternion.identity);
            spawnedTargetRight = Instantiate(target, spawnZones[spawnIndex].rightPlayerSpawn.transform.position, Quaternion.identity);
            spawnedTargetLeft.GetComponent<MeshRenderer>().enabled = false;
            spawnedTargetRight.GetComponent<MeshRenderer>().enabled = false;
            spawnedTargetLeft.GetComponent<NetworkObject>().Spawn();
            spawnedTargetRight.GetComponent<NetworkObject>().Spawn();
            StartCoroutine(EnableOnServer(ping, spawnedTargetLeft, spawnedTargetRight));
            timer = 0;
        }
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
