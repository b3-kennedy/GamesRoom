using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractPanel : NetworkBehaviour
{
    public TextMeshProUGUI title;
    public ulong clientID;
    public ulong otherClientID;

    public void RockPaperScissorsInvite()
    {
        RockPaperScissorsInviteServerRpc(otherClientID, clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RockPaperScissorsInviteServerRpc(ulong receiverClientID, ulong senderClientID)
    {
        RockPaperScissorsInviteClientRpc(receiverClientID, senderClientID);
    }

    [ClientRpc]
    void RockPaperScissorsInviteClientRpc(ulong receiverClientID, ulong senderClientID)
    {
        if (NetworkManager.Singleton.LocalClientId == receiverClientID)
        {
            var senderPlayer = NetworkManager.Singleton.ConnectedClients[senderClientID].PlayerObject;
            var senderName = senderPlayer.GetComponent<SteamPlayer>().playerName;
            Debug.Log($"Received a rps invite from {senderName}");
        }
    }
}
