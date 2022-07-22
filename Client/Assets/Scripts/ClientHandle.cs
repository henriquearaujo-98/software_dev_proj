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
        Debug.Log($"Player {GameManager.players[_id].GetComponent<PlayerManager>().username} has disconnected.");
        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }
    #endregion

    public static void SpawnPlayer(Packet packet)
    {

        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();
        int _damageFrom = _packet.ReadInt();

        GameManager.players[_id].SetHealth(_health);
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
        int _weapon = _packet.ReadInt();
        int _victimPlayer = _packet.ReadInt();

        Debug.Log("WeaponID: " + _weapon);

        Debug.Log($"{GameManager.players[_fromPlayer].username} [{GameManager.instance.weapons[_weapon].name}] {GameManager.players[_victimPlayer].username}");
    }
}