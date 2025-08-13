using Steamworks.Data;
using UnityEngine;

public class LobbyHolder : MonoBehaviour
{
    public static LobbyHolder Instance;
    public Lobby currentLobby;

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
}
