using Unity.Netcode;
using UnityEngine;

public class RotateToLocalPlayer : MonoBehaviour
{
    private Transform localPlayer;

    void Update()
    {
        if (localPlayer == null && NetworkManager.Singleton.IsListening)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var client))
            {
                if (client.PlayerObject != null)
                    localPlayer = client.PlayerObject.transform;
            }
        }

        if (localPlayer != null)
        {
            Vector3 direction = localPlayer.position - transform.position;
            direction.y = 0f; // keep rotation on Y-axis only

            if (direction.sqrMagnitude > 0.001f)
            {
                // Rotate so left (-X) faces the player
                transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f);
            }
        }
    }
}
