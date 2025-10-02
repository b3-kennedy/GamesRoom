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
        // Server tells all clients what to do
        SetRagdollClientRpc(enable, targetClientId);
    }

    [ClientRpc]
    private void SetRagdollClientRpc(bool enable, ulong targetClientId)
    {
        // If targeting a specific player
        if (targetClientId != 0 && targetClientId != OwnerClientId)
            return;

        if (enable)
        {
            EnableRagdoll();
        }
        else
        {
            EnableAnimator();
        }
            
    }


    public void EnableRagdoll()
    {
        
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
            rigidbody.linearVelocity = GetComponent<Rigidbody>().linearVelocity;
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

    public void EnableAnimator()
    {
        
        transform.position = ragdollRoot.position;
        animator.enabled = true;
        
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
        
        if(IsOwner)
        {
            ragdollCamera.SetActive(false);
            normalCamera.SetActive(true);
        }

        isRagdoll = false;
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

}
