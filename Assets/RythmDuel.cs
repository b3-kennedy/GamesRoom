using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public class RythmDuel : ArcadeGame
{
    public enum GameState { MAIN_MENU, GAME, GAME_OVER }

    public GameObject mainMenu;

    public GameObject gameScene;
    public List<NetworkObject> connectedPlayers = new List<NetworkObject>();

    public NetworkVariable<int> connectedPlayersCount = new NetworkVariable<int>();
    public TextMeshPro connectedPlayersText;
    public TextMeshPro joinText;


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






    void Update()
    {
        if (netGameState.Value == GameState.MAIN_MENU)
        {
            connectedPlayersText.text = $"{connectedPlayersCount.Value}/2";
            if (connectedPlayersCount.Value == 2)
            {
                ChangeStateServerRpc(GameState.GAME);
            }
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
