using Unity.Netcode;
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

    private float horizontal;
    private float vertical;
    private bool isSprinting = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        state = PlayerState.NORMAL;
    }

    void Update()
    {

        if (!IsOwner) return;

        // Get input
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Sprint input
        isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && IsGrounded();
        state = isSprinting ? PlayerState.SPRINT : PlayerState.NORMAL;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        // Apply drag based on grounded state
        rb.linearDamping = IsGrounded() ? groundDrag : 0f;
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
