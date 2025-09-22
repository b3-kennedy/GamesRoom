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
        }

        public override void OnStateUpdate()
        {
            SpawnBall();
        }
        

        
        void SpawnBall()
        {
            if (NetworkManager.Singleton.LocalClientId != playerOwnerID) return;
        
            if(ballSpawn.childCount == 0)
            {
                spawnTimer += Time.deltaTime;
                if(spawnTimer >= 1f)
                {
                    SpawnBallServerRpc();
                    spawnTimer = 0;
                }
                
            }
        }


        [ServerRpc(RequireOwnership = false)]
        void SpawnBallServerRpc()
        {
            int randomNum = Random.Range(0, spawnBalls.Count);
            SpawnBallClientRpc(randomNum);
        }
        
        [ClientRpc]
        void SpawnBallClientRpc(int index)
        {
            Debug.Log("spawn");
            GameObject spawnedBall = Instantiate(spawnBalls[index], ballSpawn);
            spawnedBall.GetComponent<Rigidbody>().isKinematic = true;
            spawnedBall.transform.localPosition = Vector3.zero;
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

