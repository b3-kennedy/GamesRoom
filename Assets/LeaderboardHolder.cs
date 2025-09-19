using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;




public class LeaderboardHolder : NetworkBehaviour
{
    public static LeaderboardHolder Instance;
    public Dictionary<string, int> flappyBirdLeaderboard = new Dictionary<string, int>();
    public Dictionary<string, int> dodgerLeaderboard = new Dictionary<string, int>();


    public GameObject leaderboardEntryPrefab;

    public Transform flappyBirdlayout;
    public Transform dodgeLayout;
    
    public enum GameType {FLAPPY_BIRD, DODGER};


    [ServerRpc(RequireOwnership = false)]
    public void UpdateFlappyBirdLeaderboardServerRpc()
    {
        foreach (var pair in NetworkManager.Singleton.ConnectedClients)
        {
            ulong id = pair.Key;
            NetworkClient client = pair.Value;
            NetworkObject playerObject = client.PlayerObject;
            string playerName = playerObject.GetComponent<SteamPlayer>().playerName;
            int score = playerObject.GetComponent<PlayerSaver>().fbHighScore.Value;
            AddEntryClientRpc(playerName, score, GameType.FLAPPY_BIRD);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateDodgeLeaderboardServerRpc()
    {
        foreach (var pair in NetworkManager.Singleton.ConnectedClients)
        {
            ulong id = pair.Key;
            NetworkClient client = pair.Value;
            NetworkObject playerObject = client.PlayerObject;
            string playerName = playerObject.GetComponent<SteamPlayer>().playerName;
            int score = playerObject.GetComponent<PlayerSaver>().dodgerHighScore.Value;
            AddEntryClientRpc(playerName, score, GameType.DODGER);
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



    [ClientRpc]
    void AddEntryClientRpc(string playerName, int score, GameType gameType)
    {
        Transform layout = null;
        Dictionary<string, int> leaderboard = null;
        switch (gameType)
        {
            case GameType.FLAPPY_BIRD:
                layout = flappyBirdlayout;
                leaderboard = flappyBirdLeaderboard;
                break;
            case GameType.DODGER:
                layout = dodgeLayout;
                leaderboard = dodgerLeaderboard;
                break;
        }
        if (leaderboard.ContainsKey(playerName))
        {
            // Update the dictionary
            leaderboard[playerName] = score;
            Debug.Log($"Updated leaderboard: {playerName} with new score {score}");
            


            // Update the existing UI
            foreach (Transform entry in layout)
            {
                TextMeshProUGUI playerNameText = entry.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (playerNameText.text.StartsWith(playerName))
                {
                    TextMeshProUGUI scoreText = entry.GetChild(2).GetComponent<TextMeshProUGUI>();
                    scoreText.text = score.ToString();
                    break;
                }
            }
        }
        else
        {
            // Add new entry
            leaderboard.Add(playerName, score);
            Debug.Log($"Added to leaderboard: {playerName} with score {score}");
            GameObject spawnedEntry = Instantiate(leaderboardEntryPrefab, layout);
            TextMeshProUGUI playerNameText = spawnedEntry.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = spawnedEntry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            playerNameText.text = $"{playerName}:";
            scoreText.text = score.ToString();
        }

        SortLeaderboardUI(layout);
    }

    public int GetHighScore(GameType type)
    {
        Transform layout = null;
    
        switch(type)
        {
            case GameType.FLAPPY_BIRD:
                layout = flappyBirdlayout;
                break;
            case GameType.DODGER:
                layout = dodgeLayout;
                break;
        }
    
        TextMeshProUGUI scoreText = layout.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();
        if (int.TryParse(scoreText.text, out int highScore))
        {
            return highScore;
        }
        else
        {
            Debug.LogWarning($"Failed to parse high score from text: {scoreText.text}");
            return 0; // fallback if parsing fails
        }

    }

    private void SortLeaderboardUI(Transform layout)
    {
    
        
        //Get all child entries
        List<Transform> entries = new List<Transform>();
        foreach (Transform child in layout)
            entries.Add(child);

        // Sort by the score in descending order
        entries.Sort((a, b) =>
        {
            int scoreA = int.Parse(a.GetChild(2).GetComponent<TextMeshProUGUI>().text);
            int scoreB = int.Parse(b.GetChild(2).GetComponent<TextMeshProUGUI>().text);
            return scoreB.CompareTo(scoreA); // descending
        });

        // Re-assign sibling index to rearrange in hierarchy
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].SetSiblingIndex(i);
        }
    }
}
