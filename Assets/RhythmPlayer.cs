using Unity.Netcode;
using UnityEngine;

public class RhythmPlayer : NetworkBehaviour
{

    public bool inZone1;
    public bool inZone2;
    public bool inZone3;

    [HideInInspector] public GameObject leftTarget;
    [HideInInspector] public GameObject middleTarget;
    [HideInInspector] public GameObject rightTarget;

    public override void OnGainedOwnership()
    {
        Debug.Log($"{OwnerClientId} gained ownership of this object");
    }

    public override void OnLostOwnership()
    {
        Debug.Log($"{OwnerClientId} lost ownership of this object");
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && inZone1)
        {
            leftTarget.SetActive(false);
            DestroyTargetServerRpc(leftTarget.GetComponent<NetworkObject>().NetworkObjectId);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && inZone2)
        {
            middleTarget.SetActive(false);
            DestroyTargetServerRpc(middleTarget.GetComponent<NetworkObject>().NetworkObjectId);

        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && inZone3)
        {
            rightTarget.SetActive(false);
            DestroyTargetServerRpc(rightTarget.GetComponent<NetworkObject>().NetworkObjectId);

        }


    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyTargetServerRpc(ulong netObjID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var target))
        {
            Destroy(target);
        }
    }
}
