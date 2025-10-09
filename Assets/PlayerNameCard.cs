using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerNameCard : MonoBehaviour
{
    public TextMeshProUGUI nameTMP;
    GameObject player;
    GameObject cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = transform.root.gameObject;
        cam = player.GetComponent<PlayerLook>().normalCamera.gameObject;
        nameTMP.text = player.GetComponent<SteamPlayer>().playerName; 
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (cam == null) return;

        // Make the name card face the camera
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0);
    }
}
