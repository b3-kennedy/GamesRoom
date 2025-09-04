using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;




public class LeaderboardHolder : NetworkBehaviour
{
    public static LeaderboardHolder Instance;
    public Dictionary<string, int> leaderboard = new Dictionary<string, int>();

    public GameObject leaderboardEntryPrefab;

    public Transform layout;


    [ServerRpc(RequireOwnership = false)]
    public void UpdateLeaderboardServerRpc()
    {
        foreach (var pair in NetworkManager.Singleton.ConnectedClients)
        {
            ulong id = pair.Key;
            NetworkClient client = pair.Value;
            NetworkObject playerObject = client.PlayerObject;
            string playerName = playerObject.GetComponent<SteamPlayer>().playerName;
            int score = playerObject.GetComponent<PlayerSaver>().fbHighScore;
            AddEntryClientRpc(playerName, score);
        }
        
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddEntryServerRpc(string playerName, int score)
    {
        AddEntryClientRpc(playerName, score);
    }

    [ClientRpc]
    void AddEntryClientRpc(string playerName, int score)
    {
        if (!leaderboard.ContainsKey(playerName))
        {
            leaderboard.Add(playerName, score);
            Debug.Log($"Added to leaderboard: {playerName} with score {score}");
            GameObject spawnedEntry = Instantiate(leaderboardEntryPrefab, layout);
            TextMeshProUGUI playerNameText = spawnedEntry.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = spawnedEntry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            playerNameText.text = $"{playerName}:";
            scoreText.text = score.ToString();
        }
    }
}
