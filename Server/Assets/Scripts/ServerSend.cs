using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendTCPData(int clientId, Packet packet)
    {
        packet.WriteLength();
        Server.clients[clientId].tcp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }
    private static void SendTCPDataToAllExeptOne(int clientId, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != clientId)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAllExeptOne(int clientId, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != clientId)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    //Packets
    #region Connection packets
    public static void Welcome(int clientId, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(clientId);

            SendTCPData(clientId, packet);
        }
    }

    public static void UDPTest(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.udpTest))
        {
            packet.Write("A test packet for UDP.");

            SendUDPData(toClient, packet);
        }
    }

    public static void PlayerDisconnected(int clientId)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(clientId);

            SendTCPDataToAll(packet);

        }
    }
    #endregion

    #region Game packets
    public static void SpawnPlayer(int toClient, Player player)
    {
        
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(toClient, packet);
        }
    }
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAllExeptOne(_player.id, _packet);
        }
    }
    
    public static void PlayerScale(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerScale))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.localScale);

            SendUDPDataToAllExeptOne(_player.id, _packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id); 
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }
    #endregion
}
