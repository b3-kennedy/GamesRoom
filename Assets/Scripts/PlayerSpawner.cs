using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;

    public static PlayerSpawner Instance;

    public GameObject testModel;


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

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoad;
    }

    private void OnSceneLoad(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsHost && sceneName == "GameScene")
        {
            foreach (var id in clientsCompleted)
            {
                GameObject player = Instantiate(playerPrefab);
                GameObject model = player.transform.GetChild(0).GetChild(1).gameObject;
                Destroy(model);
                GameObject newModel = Instantiate(testModel, player.transform.GetChild(0));
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
            }
        }
    }

}
