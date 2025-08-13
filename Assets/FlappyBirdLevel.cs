using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FlappyBirdLevel : NetworkBehaviour
{
    public GameObject pipeSet;
    public float minHeight;
    public float maxHeight;

    public Transform spawnPoint;

    public List<GameObject> spawnedPipes;

    public float distanceBetweenPipes = 3f;

    public float speed = 3f;

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
    

    public void Move()
    {

        // Move all pipes
        for (int i = spawnedPipes.Count - 1; i >= 0; i--)
        {
            GameObject pipe = spawnedPipes[i];
            pipe.transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);

            if (pipe.transform.position.x < spawnPoint.position.x - 25f)
            {
                Destroy(pipe);
                spawnedPipes.RemoveAt(i);
            }
        }

        if (!IsServer) return;

        // Spawn a new pipe if needed
        if (spawnedPipes.Count == 0 ||
            spawnedPipes[spawnedPipes.Count - 1].transform.position.x <= spawnPoint.position.x - distanceBetweenPipes)
        {
            SpawnPipesServerRpc();
        }
    }
}
