using Unity.Netcode;
using UnityEngine;

public class RagdollEnabler : NetworkBehaviour
{
    public Animator animator;
    public Transform ragdollRoot;
    public Transform head;
    public Transform spine;
    public bool startRagdoll = false;
    Rigidbody[] rigidbodies;
    CharacterJoint[] joints;
    Collider[] colliders;
    
    public GameObject normalCamera;
    public GameObject ragdollCamera;

    [HideInInspector] public bool isRagdoll = false;
    public Vector3 headPosition;

    bool hasTeleported = false;

    void Awake()
    {

        
    }

    public override void OnNetworkSpawn()
    {
        Physics.IgnoreLayerCollision(8, 9);
        rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        joints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        colliders = ragdollRoot.GetComponentsInChildren<Collider>();
        foreach (var joint in joints)
        {
            joint.enableCollision = false;
        }
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRagdollServerRpc(bool enable, ulong targetClientId = 0)
    {
        var player = NetworkManager.Singleton.ConnectedClients[targetClientId].PlayerObject;
        Vector3 vel = player.GetComponent<Rigidbody>().linearVelocity;
        SetRagdollClientRpc(enable, vel, targetClientId);
    }

    [ClientRpc]
    private void SetRagdollClientRpc(bool enable, Vector3 velocity,ulong targetClientId)
    {
        // If targeting a specific player
        if (targetClientId != 0 && targetClientId != OwnerClientId)
            return;

        if (enable)
        {
            EnableRagdoll(velocity);
        }
        else
        {
            EnableAnimator();
        }
            
    }


    public void EnableRagdoll(Vector3 vel)
    {
        hasTeleported = false;
        animator.enabled = false;
        foreach(var joint in joints)
        {
            joint.enableCollision = true;
        }
        foreach(var collider in colliders)
        {
            collider.enabled = true;
        }
        foreach (var rigidbody in rigidbodies)
        {
            
            rigidbody.detectCollisions = true;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.linearVelocity = vel;
        }
        
        if(IsOwner)
        {
            ragdollCamera.SetActive(true);
            normalCamera.SetActive(false);
        }

        isRagdoll = true;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportServerRpc(ulong netObjID, Vector3 pos, ulong clientID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjID, out var player))
        {
            Debug.Log($"Server teleporting to: {pos}");
            TeleportClientRpc(pos, clientID);
        }
    }

    [ClientRpc]
    void TeleportClientRpc(Vector3 pos, ulong clientID)
    {
        if (clientID != OwnerClientId)
            return;

        Debug.Log($"Client teleporting to: {pos}, current position: {transform.position}");
        transform.position = pos;
        Debug.Log($"After teleport: {transform.position}");
    }

    public void EnableAnimator()
    {
        // STORE the ragdoll end position FIRST before changing anything
        Vector3 ragdollEndPosition = Vector3.zero;
        if (IsOwner)
        {
            ragdollEndPosition = ragdollRoot.GetComponent<Rigidbody>().position;
            Debug.Log($"Ragdoll ended at: {ragdollEndPosition}");
            Debug.Log($"Player currently at: {transform.position}");
        }

        // Disable ragdoll physics
        foreach (var joint in joints)
        {
            joint.enableCollision = false;
        }
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }

        // Teleport IMMEDIATELY on owner, then sync to server
        if (IsOwner && !hasTeleported)
        {
            // Teleport locally first
            transform.position = ragdollEndPosition;
            Debug.Log($"Teleported locally to: {transform.position}");

            // Then tell server about the new position
            TeleportServerRpc(GetComponent<NetworkObject>().NetworkObjectId, ragdollEndPosition, OwnerClientId);

            ragdollCamera.SetActive(false);
            normalCamera.SetActive(true);
            hasTeleported = true;
        }

        animator.enabled = true;
        isRagdoll = false;
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

}
