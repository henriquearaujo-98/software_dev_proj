using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    public static NetworkManager instance;

    public SpawnSystem spawnSystem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 400;

        Server.Start(50, 9443);

        spawnSystem = GameObject.FindGameObjectWithTag("SpawnSystem").GetComponent<SpawnSystem>();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, spawnSystem.GetNewSpawnPosition(), Quaternion.identity).GetComponent<Player>();
    }
}
