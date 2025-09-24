using UnityEngine;
using Unity.Netcode;
using Assets.Combiner;

public class CombinerPlayer : NetworkBehaviour
{

    Rigidbody rb;
    public float speed;
    [HideInInspector] public CombinerGame game;
    [HideInInspector] public GameObject playerObject;

    float xMove;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (game && game.netGameState.Value == CombinerGame.GameState.GAME)
        {
            xMove = Input.GetAxisRaw("ArrowHorizontal") * speed;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!game.gameState.spawnedBall.GetComponent<CombineBall>().isDropped.Value)
                {
                    game.gameState.SpawnBall();
                    DropBallServerRpc();

                }
            }
        }
        else if(game && game.netGameState.Value == CombinerGame.GameState.LEADERBOARDS)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                game.ChangeStateServerRpc(CombinerGame.GameState.GAME);
            }
        }
        
        

    }
    
    [ServerRpc(RequireOwnership = false)]
    void DropBallServerRpc()
    {
        GameObject ball = game.gameState.spawnedBall;
        ball.GetComponent<CombineBall>().isDropped.Value = true;
        DropBallClientRpc();
    }

    [ClientRpc]
    void DropBallClientRpc()
    {
        GameObject ball = game.gameState.spawnedBall;
        ball.transform.SetParent(null);
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Collider>().enabled = true;
        ball.GetComponent<Rigidbody>().AddForce(-Vector3.up * 3f, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(xMove, 0f, 0f);
    }
}
