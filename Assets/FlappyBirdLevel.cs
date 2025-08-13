using System.Collections.Generic;
using UnityEngine;

public class FlappyBirdLevel : MonoBehaviour
{
    public GameObject pipeSet;
    public float minHeight;
    public float maxHeight;

    public Transform spawnPoint;

    public List<GameObject> spawnedPipes;

    public float distanceBetweenPipes = 3f;

    public float speed = 3f;

    public void SpawnPipes()
    {
        float height = Random.Range(minHeight, maxHeight);
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, height, spawnPoint.position.z);
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

        // Spawn a new pipe if needed
        if (spawnedPipes.Count == 0 ||
            spawnedPipes[spawnedPipes.Count - 1].transform.position.x <= spawnPoint.position.x - distanceBetweenPipes)
        {
            SpawnPipes();
        }

        if (spawnedPipes.Count == 0 ||
            spawnedPipes[spawnedPipes.Count - 1].transform.position.x <= spawnPoint.position.x - distanceBetweenPipes)
        {
            SpawnPipes();
        }
    }
}
