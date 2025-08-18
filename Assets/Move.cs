using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LowLevelPhysics;

public class Move : NetworkBehaviour
{
    public float speed = 1f;

    public bool isLeft;
    [HideInInspector] public RythmDuel duel;

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zone1") || other.CompareTag("Zone2") || other.CompareTag("Zone3"))
        {
            Debug.Log("entered zone");
            RhythmGameZone zone = other.GetComponent<RhythmGameZone>();
            if (zone)
            {
                zone.target = gameObject;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Zone1") || other.CompareTag("Zone2") || other.CompareTag("Zone3"))
        {
            Debug.Log("left zone");
            RhythmGameZone zone = other.GetComponent<RhythmGameZone>();
            if (zone)
            {
                zone.target = null;
            }
        }
        else if (other.CompareTag("KillZone"))
        {
            gameObject.SetActive(false);
            duel.RemoveLifeServerRpc(isLeft);
            DestroyTargetServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyTargetServerRpc()
    {

        GetComponent<NetworkObject>().Despawn(true);
    }
}
