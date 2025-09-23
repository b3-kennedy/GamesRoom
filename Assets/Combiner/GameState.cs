using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;


namespace Assets.Combiner
{
    public class GameState : State
    {
        CombinerGame combinerGame;
        public Transform ballSpawn;
        public List<GameObject> spawnBalls;

        [HideInInspector] public GameObject spawnedBall;

        public NetworkVariable<int> score;

        public List<GameObject> spawnedBalls;

        public Transform overFlowTrigger;

        public TextMeshPro scoreTMP;

        float spawnTimer;

        ulong playerOwnerID;
        
        void Start()
        {
            if (IsServer)
            {
                AssignGame();
                SpawnBallServerRpc();
                

            }

            score.OnValueChanged += UpdateScoreText;
        }



        void AssignGame()
        {
            if (game is CombinerGame cg)
            {
                combinerGame = cg;
                playerOwnerID = combinerGame.player.GetComponent<NetworkObject>().OwnerClientId;

            }
        }
        public override void OnStateEnter()
        {

            gameObject.SetActive(true);
            
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
            spawnedBall.GetComponent<CombineBall>().game = combinerGame;
            spawnedBall.GetComponent<NetworkObject>().SpawnWithOwnership(combinerGame.player.GetComponent<NetworkObject>().OwnerClientId);
            
            spawnedBalls.Add(spawnedBall);
            SpawnBallClientRpc(spawnedBall.GetComponent<NetworkObject>().NetworkObjectId);
        }
        
        [ClientRpc]
        void SpawnBallClientRpc(ulong ballID)
        {
            if(!combinerGame)
            {
                AssignGame();
            }
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballID, out var ball))
            {
                spawnedBall = ball.gameObject;
                ball.GetComponent<CombineBall>().follower = ballSpawn;
                ball.GetComponent<Rigidbody>().isKinematic = true;
                ball.transform.localPosition = Vector3.zero;
                ball.GetComponent<CombineBall>().game = combinerGame;
                ball.GetComponent<Collider>().enabled = false;
                Debug.Log(combinerGame);
                combinerGame.player.GetComponent<BoxCollider>().size = ball.transform.localScale;
                
            }
            
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void IncreaseScoreServerRpc(int amount)
        {
            score.Value += amount;
        }

        private void UpdateScoreText(int previousValue, int newValue)
        {
            scoreTMP.text = $"Score: {newValue}";
        }


        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

