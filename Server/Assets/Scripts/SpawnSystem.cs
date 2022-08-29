using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{

    public List<Transform> spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
            spawnPoints.Add(child);
    }

    public Vector3 GetNewSpawnPosition()
    {
        return spawnPoints[ Random.Range(0, spawnPoints.Count) ].position;
    }
}
