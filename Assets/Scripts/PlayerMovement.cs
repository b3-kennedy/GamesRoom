using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    public enum PlayerState { NORMAL, SPRINT }
    public PlayerState state;

    [Header("Movement")]
    public float normalSpeed = 5f;
    public float sprintSpeed = 10f;
    public float acceleration = 10f;
    public float jumpForce = 5f;
    public float airMultiplier = 0.5f;
    public float groundDrag = 4f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public float groundCheckHeight = 1.0f;
    public LayerMask groundMask;

    [Header("References")]
    public Transform orientation;
    public Animator anim;

    public Transform model;
    private float horizontal;
    private float vertical;
    private bool isSprinting = false;

    private Rigidbody rb;

    public bool canJump = true;

    RagdollEnabler ragdollEnabler;
    float getUpTimer;
    public bool getUp = false;
    bool hasRagdollHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        state = PlayerState.NORMAL;
        ragdollEnabler = GetComponent<RagdollEnabler>();
        ragdollEnabler.spine.GetComponent<RagdollCollision>().hitObject.AddListener(OnRagdollHit);
    }

    void Update()
    {

        GetUp();

        if (!IsOwner) return;

        // Get input
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Sprint input
        isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && IsGrounded();
        state = isSprinting ? PlayerState.SPRINT : PlayerState.NORMAL;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && canJump)
        {
            Jump();
        }
        
        if(Input.GetKeyDown(KeyCode.O) && !GetComponent<RagdollEnabler>().isRagdoll)
        {
            
            RagdollAndAddForceServerRpc(NetworkObjectId, 100, Vector3.up);
        }

        // Apply drag based on grounded state
        rb.linearDamping = IsGrounded() ? groundDrag : 0f;

        Animation();

        if(hasRagdollHit)
        {
            getUpTimer += Time.deltaTime;
            if(getUpTimer >= 2)
            {
                GetUpServerRpc(NetworkObjectId);
                getUpTimer = 0;
            }
        }

        

    }
    
    void OnRagdollHit()
    {
        hasRagdollHit = true;
    }
    
    void GetUp()
    {
        
        if (getUp)
        {
            if (ragdollEnabler.head.position.y <= 1.5f)
            {
                ragdollEnabler.head.GetComponent<Rigidbody>().linearVelocity = Vector3.up * 5;
            }
            else
            {
                getUp = false;
                hasRagdollHit = false;
                ragdollEnabler.head.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                ragdollEnabler.SetRagdollServerRpc(false);
            }
            
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void GetUpServerRpc(ulong networkObjectID)
    {
        GetUpClientRpc(networkObjectID);
    }
    
    [ClientRpc]
    void GetUpClientRpc(ulong networkObjectID)
    {
        Debug.Log("get up");
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out var player))
        {
            Debug.Log(player);
            player.GetComponent<PlayerMovement>().getUp = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RagdollAndAddForceServerRpc(ulong networkObjectID, float force, Vector3 dir)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out var player))
        {
            RagdollEnabler ragdollEnabler = player.GetComponent<RagdollEnabler>();
            ragdollEnabler.EnableRagdoll();

            Rigidbody root = ragdollEnabler.ragdollRoot.GetComponent<Rigidbody>();
            root.AddForce(dir * force, ForceMode.Impulse);
        }

        // Tell everyone (including server/host client) to simulate it locally too
        RagdollAndAddForceClientRpc(networkObjectID, force, dir);
    }

    [ClientRpc]
    void RagdollAndAddForceClientRpc(ulong networkObjectID, float force, Vector3 dir)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out var player))
        {
            RagdollEnabler ragdollEnabler = player.GetComponent<RagdollEnabler>();
            ragdollEnabler.EnableRagdoll();

            Rigidbody root = ragdollEnabler.ragdollRoot.GetComponent<Rigidbody>();
            root.AddForce(dir * force, ForceMode.Impulse); // each client does this locally
        }
    }



    void Animation()
    {
        if (IsGrounded() && (horizontal != 0 || vertical != 0))
        {
            // Normalize input so diagonals don't exceed 1
            Vector2 input = new Vector2(horizontal, vertical).normalized;
            Vector3 moveDir = orientation.forward * input.y + orientation.right * input.x;

            if (input.x != 0 && input.y != 0)
            {
                if (input.magnitude > 0.1f)
                {
                    Quaternion targetRotation;

                    // If moving backward, rotate 180 degrees from moveDir
                    if (input.y < -0.1f)
                    {
                        targetRotation = Quaternion.LookRotation(-moveDir, Vector3.up); // back faces forward
                    }
                    else // forward or sideways movement
                    {
                        targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                    }

                    // Smoothly rotate model
                    model.transform.rotation = Quaternion.Slerp(model.transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(orientation.forward, Vector3.up);
                model.transform.rotation = Quaternion.Slerp(model.transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            // Forward/backward
            anim.SetFloat("isBackwards", input.y);  // -1 = backward, 1 = forward

            // Left/right
            anim.SetFloat("isRight", input.x);       // -1 = left, 1 = right
            
            if(input.x != 0 && input.y == 0)
            {
                anim.SetBool("isWalkStrafing", true);
                anim.SetBool("isWalking", false);
            }
            else
            {
                anim.SetBool("isWalking", true);
                anim.SetBool("isWalkStrafing", false);
            }

            
        }
        else
        {
            anim.SetFloat("isBackwards", 0f);
            anim.SetFloat("isRight", 0f);
            anim.SetBool("isWalking", false);
            anim.SetBool("isWalkStrafing", false);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        MovePlayer();
        SpeedControl();
    }

    void MovePlayer()
    {
        Vector3 moveDir = orientation.forward * vertical + orientation.right * horizontal;
        float currentSpeed = state == PlayerState.SPRINT ? sprintSpeed : normalSpeed;

        if (IsGrounded())
        {
            rb.AddForce(moveDir.normalized * currentSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDir.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float maxSpeed = state == PlayerState.SPRINT ? sprintSpeed : normalSpeed;

        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        Vector3 capsuleBottom = groundCheck.position;
        Vector3 capsuleTop = groundCheck.position + Vector3.up * groundCheckHeight;

        return Physics.CheckCapsule(capsuleBottom, capsuleTop, groundCheckRadius, groundMask);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Vector3 capsuleBottom = groundCheck.position;
        Vector3 capsuleTop = groundCheck.position + Vector3.up * groundCheckHeight;

        // Draw a capsule approximation using spheres and lines
        Gizmos.DrawWireSphere(capsuleBottom, groundCheckRadius);
        Gizmos.DrawWireSphere(capsuleTop, groundCheckRadius);
        Gizmos.DrawLine(capsuleBottom + Vector3.right * groundCheckRadius, capsuleTop + Vector3.right * groundCheckRadius);
        Gizmos.DrawLine(capsuleBottom - Vector3.right * groundCheckRadius, capsuleTop - Vector3.right * groundCheckRadius);
        Gizmos.DrawLine(capsuleBottom + Vector3.forward * groundCheckRadius, capsuleTop + Vector3.forward * groundCheckRadius);
        Gizmos.DrawLine(capsuleBottom - Vector3.forward * groundCheckRadius, capsuleTop - Vector3.forward * groundCheckRadius);
    }
}
