using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;

public class SkinChanger : NetworkBehaviour
{
    public GameObject testSkin;

    [ServerRpc(RequireOwnership = false)]
    public void ChangeServerRpc(ulong clientID)
    {
        var playerObj = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
        Vector3 playerPos = playerObj.transform.position;
        playerObj.Despawn(true);
        StartCoroutine(RespawnPlayerNextFrame(clientID, playerPos));
        
    }

    private IEnumerator RespawnPlayerNextFrame(ulong clientID, Vector3 pos)
    {
        yield return null; // wait one frame
        GameObject newModel = Instantiate(testSkin, pos, Quaternion.identity);
        newModel.transform.position = pos;
        newModel.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        TeleportClientRpc(newModel.GetComponent<NetworkObject>().NetworkObjectId, pos);
    }
    
    [ClientRpc]
    void TeleportClientRpc(ulong netObjID, Vector3 pos)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var player))
        {
            player.GetComponent<NetworkTransform>().Teleport(pos, Quaternion.identity, Vector3.one);
        }
    }
}
