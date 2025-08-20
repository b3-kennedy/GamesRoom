using Unity.Netcode;
using UnityEngine;

public class FarkleDice : NetworkBehaviour
{

    public NetworkVariable<int> diceValue;
    public NetworkVariable<bool> isSelected = new NetworkVariable<bool>(false);

    public GameObject selectGraphic;
    GameObject spawnedSelectGraphic;
    // Call this to randomize dice at start

    void Start()
    {
        isSelected.OnValueChanged += SelectedValueChange;
    }

    private void SelectedValueChange(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            //spawn select graphic
            if (IsServer)
            {
                spawnedSelectGraphic = Instantiate(selectGraphic, transform.position, Quaternion.identity);
                spawnedSelectGraphic.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                SetSelectGraphicClientRpc(spawnedSelectGraphic.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
        else
        {
            //hide select graphic
            if (IsServer && spawnedSelectGraphic)
            {
                spawnedSelectGraphic.GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    void SetSelectGraphicClientRpc(ulong netObjID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var graphic))
        {
            spawnedSelectGraphic = graphic.gameObject;
            spawnedSelectGraphic.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

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
