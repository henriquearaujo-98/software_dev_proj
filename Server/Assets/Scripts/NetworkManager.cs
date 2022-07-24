using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    public static NetworkManager instance;

    public List<Weapon> weapons; // Make sure weapon's ID correspond to their index on this list. Also that client and server are synch

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
        Application.targetFrameRate = 64;

        Server.Start(50, 9443);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(1f, 1f, 1f), Quaternion.identity).GetComponent<Player>();
    }
}
