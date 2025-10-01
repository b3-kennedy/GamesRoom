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
        }
        Physics.IgnoreLayerCollision(8, 9);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRagdollServerRpc(bool enable, Vector3 force = default, ulong targetClientId = 0)
    {
        // Server tells all clients what to do
        SetRagdollClientRpc(enable, force, targetClientId);
    }

    [ClientRpc]
    private void SetRagdollClientRpc(bool enable, Vector3 force, ulong targetClientId)
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
        if (!IsOwner) return;
        
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
            rigidbody.linearVelocity = GetComponent<Rigidbody>().linearVelocity;
            rigidbody.detectCollisions = true;
            rigidbody.useGravity = true;
        }
        ragdollCamera.SetActive(true);
        normalCamera.SetActive(false);
        isRagdoll = true;
        GetComponent<CapsuleCollider>().height = 0.1f;
    }

    public void EnableAnimator()
    {
        if (!IsOwner) return;

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
        }
        ragdollCamera.SetActive(false);
        normalCamera.SetActive(true);
        isRagdoll = false;
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<CapsuleCollider>().height = 2f;
    }

}
