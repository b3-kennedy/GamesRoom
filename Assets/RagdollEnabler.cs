using Unity.Netcode;
using Unity.Netcode.Components;
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

    GameObject fakeRagDoll;

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
    private void SetRagdollClientRpc(bool enable, Vector3 velocity, ulong targetClientId)
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
        animator.enabled = false;
        foreach (var joint in joints)
        {
            joint.enableCollision = true;
        }
        foreach (var collider in colliders)
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

        if (IsOwner)
        {
            ragdollCamera.SetActive(true);
            normalCamera.SetActive(false);
        }

        isRagdoll = true;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void RepositionPlayerServerRpc(ulong netID, Vector3 pos)
    {
        RepositionPlayerClientRpc(netID, pos);
    }

    [ClientRpc]
    void RepositionPlayerClientRpc(ulong netID, Vector3 pos)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netID, out var player))
        {            
            player.GetComponent<RagdollEnabler>().animator.enabled = true;
            player.GetComponent<RagdollEnabler>().isRagdoll = false;
            player.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<PlayerMovement>().enabled = true;
            player.GetComponent<CapsuleCollider>().enabled = true;
            player.GetComponent<RagdollEnabler>().ragdollRoot.parent.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
            player.GetComponent<RagdollEnabler>().ragdollRoot.parent.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = true;
            player.GetComponent<NetworkTransform>().Teleport(pos, player.transform.rotation, player.transform.localScale);
        }
    }

    public void EnableAnimator()
    {
        if (!isRagdoll)
            return;

        foreach (var joint in joints)
            joint.enableCollision = false;

        foreach (var collider in colliders)
            collider.enabled = false;

        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }

        ragdollRoot.parent.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        ragdollRoot.parent.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = false;

        // Owner handles camera
        if (IsOwner)
        {

            Vector3 ragdollHipPosition = ragdollRoot.GetComponent<Rigidbody>().position;
            RepositionPlayerServerRpc(GetComponent<NetworkObject>().NetworkObjectId, ragdollHipPosition);
            ragdollCamera.SetActive(false);
            normalCamera.SetActive(true);
        }
    }

}