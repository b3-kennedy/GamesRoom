using Unity.Netcode;
using UnityEngine;

public class FarkleDice : NetworkBehaviour
{

    public NetworkVariable<int> diceValue;
    // Call this to randomize dice at start

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RandomizeDiceServerRpc();
        }
        
    }

    void Update()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    void RandomizeDiceServerRpc()
    {
        // Random top face (1 to 6)
        int topFace = Random.Range(1, 7);

        // Base rotation for each top face (Euler angles)
        Vector3 faceRotation = Vector3.zero;
        diceValue.Value = 0; // The value the dice will return

        switch (topFace)
        {
            case 2: faceRotation = new Vector3(0, 0, 0); diceValue.Value = 2; break; //2
            case 6: faceRotation = new Vector3(90, 0, 0); diceValue.Value = 6; break; //6
            case 3: faceRotation = new Vector3(0, 0, -90); diceValue.Value = 3; break; //3
            case 4: faceRotation = new Vector3(0, 0, 90); diceValue.Value = 4; break; //4
            case 1: faceRotation = new Vector3(-90, 0, 0); diceValue.Value = 1; break; //1
            case 5: faceRotation = new Vector3(180, 0, 0); diceValue.Value = 5; break; //5
        }

        // Add random spin around vertical axis (Y-axis)
        float randomYaw = Random.Range(0f, 360f);
        faceRotation.y += randomYaw;

        // Apply rotation instantly
        transform.rotation = Quaternion.Euler(faceRotation);

        // Return the dice value
        
    }

}
