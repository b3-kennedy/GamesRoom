using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FlappyBirdLevel : NetworkBehaviour
{
    public GameObject pipeSet;
    public float minHeight;
    public float maxHeight;

    public Transform spawnPoint;

    FlappyBird flappyBird;

    public List<GameObject> spawnedPipes;

    public float distanceBetweenPipes = 3f;

    public float baseSpeed = 3f;
    public float maxSpeed = 5f;

    public NetworkVariable<float> speed;

    float gameTimer;

    int lastSecond = -1;

    void Start()
    {
        flappyBird = transform.parent.parent.GetComponent<FlappyBird>();
        if (IsServer)
        {
            speed.Value = baseSpeed;
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPipesServerRpc()
    {
        float height = Random.Range(minHeight, maxHeight);
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + height, spawnPoint.position.z);
        SpawnPipesClientRpc(spawnPos);
    }

    [ClientRpc]
    void SpawnPipesClientRpc(Vector3 spawnPos)
    {

        GameObject pipe = Instantiate(pipeSet, spawnPos, pipeSet.transform.rotation);
        pipe.transform.SetParent(transform);
        spawnedPipes.Add(pipe);
    }

    public float GetSpeed()
    {
        return speed.Value;
    }

    public void ClearPipes()
    {
        gameTimer = 0;
        if (IsServer)
        {
            speed.Value = baseSpeed;
        }
        
        for (int i = spawnedPipes.Count - 1; i >= 0; i--)
        {
            if (spawnedPipes[i] != null)
            {
                Destroy(spawnedPipes[i]);
            }
        }

        spawnedPipes.Clear();
    }


    public void Move()
    {

        if (!gameObject.activeSelf) return;



        for (int i = spawnedPipes.Count - 1; i >= 0; i--)
        {
            GameObject pipe = spawnedPipes[i];
            pipe.transform.position -= new Vector3(speed.Value * Time.deltaTime, 0, 0);

            if (pipe.transform.position.x < spawnPoint.position.x - 25f)
            {
                Destroy(pipe);
                spawnedPipes.RemoveAt(i);
            }
        }

        if (!IsServer) return;

        gameTimer += Time.deltaTime;

        int currentSecond = Mathf.FloorToInt(gameTimer);
        if (currentSecond % 2 == 0 && currentSecond != lastSecond && speed.Value < maxSpeed)
        {
            speed.Value += 0.01f;
            flappyBird.UpdateSpeedTextServerRpc();
        }
        lastSecond = currentSecond;

        // Spawn a new pipe if needed
        if (spawnedPipes.Count == 0 ||
            spawnedPipes[spawnedPipes.Count - 1].transform.position.x <= spawnPoint.position.x - distanceBetweenPipes)
        {
            SpawnPipesServerRpc();
        }
    }
}
