using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    public List<Weapon> weapons; // Make sure weapon's ID correspond to their index on this list. Also that client and server are synch

    [HideInInspector] public PlayerController myPlayer;

    [SerializeField] private LevelManager levelManager;

    private void Awake()
    {

        DontDestroyOnLoad(gameObject);
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

    /// <summary>Loads scene, then spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_name">The player's name.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    /// <param name="_sceneID">SceneID of the server</param>
    public async void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, int _sceneID)
    {
        //Load scene
        await levelManager.LoadSceneAsync(_sceneID);

        //TODO: Pre game screen

        //Spawn Player
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().Initialize(_id, _username);
        players.Add(_id, _player.GetComponent<PlayerManager>());

        if (_player.GetComponent<PlayerController>())
        {
            myPlayer = _player.GetComponent<PlayerController>();
        }
    }
}
