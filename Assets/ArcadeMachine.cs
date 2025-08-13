using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArcadeMachine : NetworkBehaviour
{
    public ArcadeGame arcadeGame;
    public GameObject screen;

    public List<GameObject> nearPlayers = new List<GameObject>();

    public void StartGame()
    {
        arcadeGame.Begin();
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
        if (nearPlayers.Count == 0)
        {
            screen.SetActive(false);    
        }
    }
}
