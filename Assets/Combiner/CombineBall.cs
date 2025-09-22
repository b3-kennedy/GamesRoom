using Assets.Football;
using Unity.Netcode;
using UnityEngine;

public class CombineBall : NetworkBehaviour
{
    public enum BallType {SMALL, MEDIUM, BIG, HUGE, MASSIVE, HUMONGOUS, COLLOSAL, MOUNTAINOUS, GARGANTUAN, COSMIC, OMEGA};
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
                if(otherBall.ballType != BallType.OMEGA)
                {
                    ulong ball1ID = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                    ulong ball2ID = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                    SpawnNextBallServerRpc(ball1ID, ball2ID, other.transform.position);
                }

                
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SpawnNextBallServerRpc(ulong ball1ID, ulong ball2ID, Vector3 pos)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ball1ID, out var ball1))
        {
            ball1.Despawn();
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ball2ID, out var ball2))
        {
            ball2.Despawn();
        }

        if (!nextBall) return;
        GameObject ball = Instantiate(nextBall, pos, Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn();
        ball.GetComponent<CombineBall>().isDropped.Value = true;
    }
}
