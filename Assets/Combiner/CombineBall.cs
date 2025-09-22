using Assets.Football;
using Unity.Netcode;
using UnityEngine;

public class CombineBall : NetworkBehaviour
{
    public enum BallType {SMALL, MEDIUM, BIG, HUGE, MASSIVE};
    public BallType ballType;

    public GameObject nextBall;

    [HideInInspector] public Transform follower;

    public NetworkVariable<bool> isDropped = new NetworkVariable<bool>(false);


    void Update()
    {
        if (!IsOwner) return;
    
        if(!isDropped.Value)
        {
            transform.position = follower.position;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        CombineBall otherBall = other.gameObject.GetComponent<CombineBall>();
        if (otherBall != null && otherBall.ballType == ballType)
        {
            if (GetInstanceID() < otherBall.GetInstanceID())
            {
                Destroy(gameObject);
                Destroy(other.gameObject);
                SpawnNextBallServerRpc();
                
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SpawnNextBallServerRpc()
    {
        if (!nextBall) return;
        GameObject ball = Instantiate(nextBall, transform.position, Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn();
        ball.GetComponent<CombineBall>().isDropped.Value = true;
    }
}
