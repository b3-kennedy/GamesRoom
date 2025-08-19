using UnityEngine;
using Unity.Netcode;
using Steamworks;

public class SteamPlayer : NetworkBehaviour
{
    public string playerName;
    public NetworkVariable<int> credits;

    public void Start()
    {
        if (!IsOwner) return;
        if (SteamClient.IsLoggedOn && SteamClient.IsValid)
        {
            playerName = SteamClient.Name;
            SetPlayerNameServerRpc(GetComponent<NetworkObject>().NetworkObjectId, playerName);
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerNameServerRpc(ulong netObjId, string name)
    {
        SetPlayerNameClientRpc(netObjId, name);
    }

    [ClientRpc]
    void SetPlayerNameClientRpc(ulong netObjId, string name)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out var player))
        {
            player.GetComponent<SteamPlayer>().playerName = name;
        }
    }
}
