using Assets.Football;
using Unity.Netcode;
using UnityEngine;

public class CombineBall : MonoBehaviour
{
    public enum BallType {SMALL, MEDIUM, BIG, HUGE, MASSIVE};
    public BallType ballType;

    public GameObject nextBall;

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
        SpawnBallClientRpc();
    }
    
    [ClientRpc]
    void SpawnBallClientRpc()
    {
        if (!nextBall) return;
        Instantiate(nextBall, transform.position, Quaternion.identity);
    }
}
