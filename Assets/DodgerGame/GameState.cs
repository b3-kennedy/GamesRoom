using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Dodger
{
    public class GameState : State
    {
        DodgerGame dodgerGame;
        public GameObject playerPrefab;
        public Transform minSpawn;
        public Transform maxSpawn;
        public Transform topLeft;
        public Transform topRight;
        public Transform bottomLeft;
        public Transform bottomRight;
        public Transform obstacleParent;
        public GameObject obstaclePrefab;
        public float baseSpeed = 1f;
        [HideInInspector] public float speed;
        public float distanceBetweenPipes = 3f;
        [HideInInspector] public float speedTimer;
        public float speedIncreaseInterval = 30f;
        public float speedIncrease = 0.1f;
        public List<GameObject> pipeList;
        public NetworkVariable<int> score;
        public TextMeshPro scoreTMP;
        
        public GameObject powerUpPrefab;

        float powerUpTimer;

        float powerUpSpawnTime;
        
        public float minPowerUpSpawnTime = 5f;
        public float maxPowerUpSpawnTime = 120f;
        
        void Start()
        {
            if(game is DodgerGame dg)
            {
                dodgerGame = dg;
            }

            speed = baseSpeed;

            score.OnValueChanged += UpdateScoreText;
        }

        private void UpdateScoreText(int previousValue, int newValue)
        {
            scoreTMP.text = newValue.ToString();
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            if(IsServer)
            {
                SpawnServerRpc();
                powerUpSpawnTime = Random.Range(minPowerUpSpawnTime, maxPowerUpSpawnTime);
            }
            
            
        }
        

        public override void OnStateUpdate()
        {

            for (int i = 0; i < pipeList.Count; i++)
            {
                GameObject pipe = pipeList[i];
                pipe.transform.position -= new Vector3(0, speed, 0) * Time.deltaTime;

                if (pipe.transform.position.y < minSpawn.position.y - 25f)
                {
                    Destroy(pipe);
                    pipeList.RemoveAt(i);
                }
            }

            speedTimer += Time.deltaTime;
            if (speedTimer >= speedIncreaseInterval)
            {
                speed += speedIncrease;
                speedTimer = 0;
            }


            if (!IsServer) return;
            


            Spawn();

            powerUpTimer += Time.deltaTime;
            if(powerUpTimer >= powerUpSpawnTime)
            {
                powerUpSpawnTime = Random.Range(minPowerUpSpawnTime, maxPowerUpSpawnTime);
                Vector3 spawn = GetRandomPoint();
                SpawnPowerUpClientRpc(spawn);
                powerUpTimer = 0;
            }
         

        }
        
        [ClientRpc]
        void SpawnPowerUpClientRpc(Vector3 spawn)
        {
            GameObject powerUp = Instantiate(powerUpPrefab, spawn, Quaternion.identity);
        }
        
        void Spawn()
        {
            if (!IsServer) return;
            
            if (pipeList.Count > 0)
            {
                Vector3 pipePos = new Vector3(0, pipeList[pipeList.Count - 1].transform.position.y, 0);
                Vector3 spawnPos = new Vector3(0, minSpawn.position.y, 0);
                if (Vector3.Distance(pipePos, spawnPos) > distanceBetweenPipes)
                {
                    SpawnServerRpc();
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void IncreaseScoreServerRpc(int value)
        {
            score.Value += value;
        }
        
        [ServerRpc(RequireOwnership = false)]        
        void SpawnServerRpc()
        {
            float x = Random.Range(minSpawn.position.x, maxSpawn.position.x);
            float y = minSpawn.position.y;
            float z = minSpawn.position.z;

            Vector3 pos = new Vector3(x, y, z);
            SpawnClientRpc(pos);
        }
        
        [ClientRpc]
        void SpawnClientRpc(Vector3 pos)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, obstacleParent);
            pipeList.Add(obstacle);
            obstacle.transform.position = pos;
        }
        
        Vector3 GetRandomPoint()
        {
            float randomX = Random.Range(topLeft.position.x, topRight.position.x);
            float randomY = Random.Range(topLeft.position.y, bottomLeft.position.y);
            return new Vector3(randomX, randomY, topLeft.position.z);
        }
        
        public float GetSpawnPos()
        {
            return Random.Range(minSpawn.localPosition.x, maxSpawn.localPosition.x);
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}


