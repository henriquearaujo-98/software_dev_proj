using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle : MonoBehaviour
{
    public static void WelcomeReceived(int clientId, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($" {username} ({Server.clients[clientId].tcp.socket.Client.RemoteEndPoint}) " +
                                                        $"connected successfully and is now player {clientId}.");
        if (clientId != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {clientId}) has assumed the wrong client ID ({clientIdCheck})!");
            return;
        }
        Server.clients[clientId].SendIntoGame(username);
    }

    public static void UDPTestReceived(int fromClient, Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {msg}");
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_shootDirection);
    }

    public static void WeaponSelected(int _fromClient, Packet _packet)
    {
        int _weaponIndex = _packet.ReadInt();

        Server.clients[_fromClient].player.weaponSwitching.Select(_weaponIndex);
    }

    public static void WeaponsID(int _fromClient, Packet _packet)
    {
        int _primary = _packet.ReadInt();
        int _secondary = _packet.ReadInt();

        Server.clients[_fromClient].player.weaponSwitching.primaryWeaponID = _primary;
        Server.clients[_fromClient].player.weaponSwitching.secondaryWeaponID = _secondary;
    }
}
