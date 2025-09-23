using Assets.Combiner;
using Assets.Football;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class CombineBall : NetworkBehaviour
{
    public enum BallType {SMALL, MEDIUM, BIG, HUGE, MASSIVE, HUMONGOUS, COLLOSAL, MOUNTAINOUS, GARGANTUAN, COSMIC, OMEGA};
    public BallType ballType;

    public GameObject nextBall;

    public int spawnScore;

    [HideInInspector] public CombinerGame game;

    [HideInInspector] public Transform follower;

    public NetworkVariable<bool> isDropped = new NetworkVariable<bool>(false);

    Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
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
                    ulong ball2ID = other.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                    SpawnNextBallServerRpc(ball1ID, ball2ID, transform.position);
                }

                
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("CombinerGameOver"))
        {
            StartCoroutine(CheckAboveLine(gameObject));
        }
    }

    IEnumerator CheckAboveLine(GameObject ball)
    {
        yield return new WaitForSeconds(0.5f); // half-second grace period
        if (ball != null && ball.transform.position.y > game.gameState.overFlowTrigger.position.y)
        {
            game.ChangeStateServerRpc(CombinerGame.GameState.GAME_OVER);
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
        int score = ball.GetComponent<CombineBall>().spawnScore;
        ball.GetComponent<CombineBall>().game = game;
        ball.GetComponent<NetworkObject>().Spawn();
        ball.GetComponent<CombineBall>().isDropped.Value = true;
        game.gameState.IncreaseScoreServerRpc(score);
    }
}
