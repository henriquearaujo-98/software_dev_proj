using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    #region Connection packets
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myId = myId;

        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void UDPTest(Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {msg}");
        ClientSend.UDPTestReceived();
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        
        int _id = _packet.ReadInt();
        string msg = $"Player {GameManager.players[_id].GetComponent<PlayerManager>().username} has disconnected.";
        Debug.Log(msg);
        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);

        GameManager.players[Client.instance.myId].pc.killFeedHandler.InstantiateKillFeedItem(msg);
    }
    #endregion

    public static void SpawnPlayer(Packet packet)
    {

        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        string msg = $"{username} has joined the game.";
        
        GameManager.instance.SpawnPlayer(id, username, position, rotation);

        if(GameManager.players.ContainsKey(Client.instance.myId))
            GameManager.players[Client.instance.myId].pc.killFeedHandler.InstantiateKillFeedItem(msg);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.players.ContainsKey(_id))
            GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();
        Debug.Log(_id + "-" +_rotation);

        if (GameManager.players.ContainsKey(_id))
            GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);

    }

    public static void DamageIndicator(Packet _packet)
    {
        int _damageFrom = _packet.ReadInt();

        GameManager.instance.myPlayer.RegisterDamageIndicator(GameManager.players[_damageFrom].transform.position);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void KillFeed(Packet _packet)
    {
        int _fromPlayer = _packet.ReadInt();
        int _fromPlayer_Kills = _packet.ReadInt();
        int _weapon = _packet.ReadInt();
        int _victimPlayer = _packet.ReadInt();

        int _victimPlayer_Deaths = _packet.ReadInt();


        string _killfeedtext = $"{GameManager.players[_fromPlayer].username} [{GameManager.instance.weapons[_weapon].name}] {GameManager.players[_victimPlayer].username}";

        GameManager.players[Client.instance.myId].pc.killFeedHandler.InstantiateKillFeedItem(_killfeedtext);


        Debug.Log($"{GameManager.players[_fromPlayer].username} [{GameManager.instance.weapons[_weapon].name}] {GameManager.players[_victimPlayer].username}");


        GameManager.players[_fromPlayer].kills = _fromPlayer_Kills;
        GameManager.players[_victimPlayer].deaths = _victimPlayer_Deaths;
    }

    public static void PlayerInputs(Packet _packet)
    {
        int _fromPlayer = _packet.ReadInt();
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }

        if (GameManager.players.ContainsKey(_fromPlayer))
            GameManager.players[_fromPlayer].gameObject.GetComponent<Enemy>().serverInputs = _inputs;
    }

    public static void KillNotification(Packet _packet)
    {
        int _id = _packet.ReadInt();

        if (GameManager.players.ContainsKey(_id))
            GameManager.players[_id].gameObject.GetComponent<PlayerController>().KillNotification();
    }
}