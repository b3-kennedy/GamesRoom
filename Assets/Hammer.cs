using Unity.Netcode;
using UnityEngine;

public class Hammer : Item
{
    public float range = 5f;
    public override void Use()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("hit player");
                Vector3 direction = (hit.collider.transform.position - cam.transform.position).normalized;
                ulong id = hit.collider.GetComponent<NetworkObject>().NetworkObjectId;
                hit.collider.GetComponent<PlayerMovement>().RagdollAndAddForceServerRpc(id, 200, direction);
            }
        }

    }
}
