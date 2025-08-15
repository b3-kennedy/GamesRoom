using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArcadeMachine : NetworkBehaviour
{
    public ArcadeGame arcadeGame;
    public GameObject screen;

    public List<GameObject> nearPlayers = new List<GameObject>();


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

    [ClientRpc]
    void TurnOnClientRpc()
    {
        screen.SetActive(true);
    }

    void Update()
    {
        if (!IsServer) return;

        if (nearPlayers.Count == 0)
        {
            TurnOffScreenClientRpc();
        }
    }

    [ClientRpc]
    void TurnOffScreenClientRpc()
    {
        screen.SetActive(false);
        arcadeGame.ResetServerRpc();
    }
}
