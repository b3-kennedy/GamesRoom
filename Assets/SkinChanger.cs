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
        newModel.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        yield return new WaitForSeconds(1f);
        newModel.transform.position = pos;
        SetPositionClientRpc(pos, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientID }
            }
        });
    }

    [ClientRpc]
    private void SetPositionClientRpc(Vector3 pos, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return; // Extra safety check

        transform.position = pos;
    }
}
