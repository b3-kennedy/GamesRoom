using UnityEngine;
using Unity.Netcode;
using Assets.Combiner;

public class CombinerPlayer : MonoBehaviour
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
        xMove = Input.GetAxisRaw("ArrowHorizontal") * speed;
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(transform.GetChild(0).childCount > 0)
            {

                DropBallServerRpc();

            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DropBallServerRpc()
    {
        DropBallClientRpc();
    }
    
    [ClientRpc]
    void DropBallClientRpc()
    {
        GameObject ball = transform.GetChild(0).GetChild(0).gameObject;
        ball.transform.SetParent(null);
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Rigidbody>().AddForce(-Vector3.up * 3f, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(xMove, 0f, 0f);
    }
}
