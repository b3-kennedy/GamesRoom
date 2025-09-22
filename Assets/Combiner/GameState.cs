using System.Collections;
using System.Collections.Generic;
using Assets.Football;
using Unity.Netcode;
using UnityEngine;


namespace Assets.Combiner
{
    public class GameState : State
    {
        CombinerGame combinerGame;
        CombinerPlayer combinerPlayer;
        public Transform ballSpawn;
        public List<GameObject> spawnBalls;

        [HideInInspector] public GameObject spawnedBall;

        float spawnTimer;

        ulong playerOwnerID;
        
        void Start()
        {
            if (game is CombinerGame cg)
            {
                combinerGame = cg;
            }
            combinerPlayer = combinerGame.player;
            playerOwnerID = combinerPlayer.GetComponent<NetworkObject>().OwnerClientId;

        }
        public override void OnStateEnter()
        {

            gameObject.SetActive(true);
            SpawnBallServerRpc();
        }

        public override void OnStateUpdate()
        {

        }
        

        
        public void SpawnBall()
        {
            if (NetworkManager.Singleton.LocalClientId != playerOwnerID) return;

            StartCoroutine(SpawnBallAfterTime());
        }
        
        IEnumerator SpawnBallAfterTime()
        {
            yield return new WaitForSeconds(1f);
            SpawnBallServerRpc();

        }


        [ServerRpc(RequireOwnership = false)]
        void SpawnBallServerRpc()
        {
            int randomNum = Random.Range(0, spawnBalls.Count);
            spawnedBall = Instantiate(spawnBalls[randomNum], ballSpawn);
            spawnedBall.GetComponent<CombineBall>().isDropped.Value = false;
            spawnedBall.GetComponent<NetworkObject>().SpawnWithOwnership(combinerPlayer.GetComponent<NetworkObject>().OwnerClientId);
            SpawnBallClientRpc(spawnedBall.GetComponent<NetworkObject>().NetworkObjectId);
        }
        
        [ClientRpc]
        void SpawnBallClientRpc(ulong ballID)
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballID, out var ball))
            {
                ball.GetComponent<CombineBall>().follower = ballSpawn;
                ball.GetComponent<Rigidbody>().isKinematic = true;
                ball.transform.localPosition = Vector3.zero;
                
            }
            
        }
        

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

