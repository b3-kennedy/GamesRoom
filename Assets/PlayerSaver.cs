using UnityEngine;
using Unity.Netcode;
using System.IO;
using System.Collections;

public class PlayerSaver : NetworkBehaviour
{

    public int fbHighScore;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Debug.Log("load data");
        Load();
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        SaveOnDisconnect();
    }

    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "savegame.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found at: " + path);
            if (IsServer)
            {
                GetComponent<SteamPlayer>().credits.Value = 100;
            }
            return;
        }

        string json = File.ReadAllText(path);

        // Deserialize into your wrapper class
        SaveData saveDataWrapper = JsonUtility.FromJson<SaveData>(json);

        if (saveDataWrapper == null || saveDataWrapper.playerData == null)
        {
            Debug.LogWarning("Save file missing playerData section.");
            return;
        }

        int creditCount = saveDataWrapper.playerData.creditCount;

        Debug.Log("Loaded creditCount = " + creditCount);

        if (IsServer && IsOwner)
        {
            GetComponent<SteamPlayer>().credits.Value = creditCount;
        }
        else
        {
            SetPlayerCreditsServerRpc(NetworkManager.Singleton.LocalClientId, creditCount);
        }

        fbHighScore = saveDataWrapper.playerData.flappyBirdHighScore;
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
        LeaderboardHolder.Instance.UpdateLeaderboardServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerCreditsServerRpc(ulong playerID, int credits)
    {
        var playerObject = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
        playerObject.GetComponent<SteamPlayer>().credits.Value = credits;
    }



    public void SaveOnDisconnect()
    {
        if (GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId) return;

        string path = Path.Combine(Application.persistentDataPath, "savegame.json");
        SaveData saveDataWrapper;

        // Load existing save if present
        if (File.Exists(path))
        {
            string existingJson = File.ReadAllText(path);
            saveDataWrapper = JsonUtility.FromJson<SaveData>(existingJson);
            if (saveDataWrapper == null) saveDataWrapper = new SaveData();
        }
        else
        {
            saveDataWrapper = new SaveData();
        }

        // Update playerData
        saveDataWrapper.playerData = new PlayerData
        {
            creditCount = GetComponent<SteamPlayer>().credits.Value,
            flappyBirdHighScore = fbHighScore
            
        };

        // Write back to file
        string json = JsonUtility.ToJson(saveDataWrapper, true);
        File.WriteAllText(path, json);

        Debug.Log("PlayerData saved on disconnect: " + path);
    }
}
