using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArcadeMachine : NetworkBehaviour
{
    public Game arcadeGame;
    public GameObject screen;

    public List<GameObject> nearPlayers = new List<GameObject>();

    public GameObject activePlayer;


    void Start()
    {
    }

    public void StartGame()
    {
        arcadeGame.BeginServerRpc(NetworkManager.Singleton.LocalClientId);
    }


    [ServerRpc(RequireOwnership = false)]
    public void TurnOnServerRpc()
    {
        TurnOnClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetActivePlayerServerRpc(ulong clientID)
    {
        activePlayer = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject;
    }

    [ClientRpc]
    void TurnOnClientRpc()
    {
        screen.SetActive(true);
    }

    void Update()
    {
        if (!IsServer) return;

        if (nearPlayers.Count == 0 && screen.activeSelf)
        {
            TurnOffScreenClientRpc();
        }

        if (activePlayer && !nearPlayers.Contains(activePlayer))
        {
            arcadeGame.ResetServerRpc();
        }
    }

    [ClientRpc]
    void TurnOffScreenClientRpc()
    {
        screen.SetActive(false);
        arcadeGame.ResetServerRpc();
    }
}
