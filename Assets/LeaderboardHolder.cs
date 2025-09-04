using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int highScore;
}

public class LeaderboardHolder : MonoBehaviour
{
    public static LeaderboardHolder Instance;
    public List<LeaderboardEntry> leaderboard;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddEntryServerRpc(string playerName, int score)
    {
        AddEntryClientRpc(playerName, score);
    }

    [ClientRpc]
    void AddEntryClientRpc(string playerName, int score)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            playerName = playerName,
            highScore = score
        };
        leaderboard.Add(entry);
    }
}
