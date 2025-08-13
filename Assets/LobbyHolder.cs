using Steamworks.Data;
using UnityEngine;

public class LobbyHolder : MonoBehaviour
{
    public static LobbyHolder Instance;
    public Lobby currentLobby;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
