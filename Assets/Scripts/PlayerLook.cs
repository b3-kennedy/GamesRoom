using Unity.Netcode;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [Header("References")]
    public Transform playerBody; // Assign the player's body (usually the parent of the camera)
    public Transform normalCamera;        // Assign the camera
    public Transform ragdollCamera;

    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public float clampAngle = 90f;

    private float xRotation = 0f;

    RagdollEnabler ragdollEnabler;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ragdollEnabler = GetComponent<RagdollEnabler>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            normalCamera.gameObject.SetActive(false);
            ragdollCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

        normalCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        ragdollCamera.localRotation = normalCamera.localRotation;

        // Rotate player body left/right
        if(!ragdollEnabler.isRagdoll)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
        
    }
}
