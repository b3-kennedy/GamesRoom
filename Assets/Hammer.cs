using Unity.Netcode;
using UnityEngine;

public class Hammer : Item
{
    public float range = 5f;

    public override void OnEquip()
    {
        transform.SetLocalPositionAndRotation(new Vector3(-0.727999985f, -0.0649999976f, -0.172999993f), Quaternion.Euler(344.841003f, 344.714081f, 94.4443436f));
        base.OnEquip();
        Debug.Log(anim.GetLayerName(1));
        anim.SetLayerWeight(1, 1f);
    }
    public override void Use()
    {
        anim.SetLayerWeight(2, 1f);
        anim.SetTrigger("Swing");
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("hit player");
                Vector3 direction = (hit.collider.transform.position - player.transform.position).normalized;
                ulong id = hit.collider.GetComponent<NetworkObject>().NetworkObjectId;
                hit.collider.GetComponent<PlayerMovement>().RagdollAndAddForceServerRpc(id, 200, direction);
            }
        }
    }

    public override void OnUnequip()
    {
        anim.SetLayerWeight(1, 0f);
    }
    
    
    
}
