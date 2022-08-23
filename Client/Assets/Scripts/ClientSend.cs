using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength(); // Writes the length of the packet at the start of the list of bytes
        Client.instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }


    #region Connection Packets
    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myId);
            packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.updTestReceived))
        {
            packet.Write("Received a UDP packet.");

            SendUDPData(packet);
        }
    }
    #endregion

    #region Game Packets
    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

            SendTCPData(_packet);
        }
    }

    public static void PlayerShoot(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }

    public static void WeaponSelected(int _weaponSelected)
    {
        using (Packet _packet = new Packet((int)ClientPackets.weaponSelected))
        {
            _packet.Write(_weaponSelected);

            SendTCPData(_packet);

            
        }
    }

    public static void WeaponsID(int _primary, int _secondary)
    {
        using (Packet _packet = new Packet((int)ClientPackets.weaponsID))
        {
            _packet.Write(_primary);
            _packet.Write(_secondary);

            SendTCPData(_packet);


        }
    }
    #endregion
}