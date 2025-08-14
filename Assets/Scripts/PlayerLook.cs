using Unity.Netcode;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [Header("References")]
    public Transform playerBody; // Assign the player's body (usually the parent of the camera)
    public Transform cam;        // Assign the camera

    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public float clampAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            cam.gameObject.SetActive(false);
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

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player body left/right
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
