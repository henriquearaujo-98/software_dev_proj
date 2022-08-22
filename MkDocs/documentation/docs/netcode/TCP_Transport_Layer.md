# TCP Transport Layer

The TCP/IP protocol suite is the most widely used protocol suite on the world wide web. Its premise is to guarantee that the packet reaches its destination and there is no packet loss. However, this approach will cause an increase in server latency since every packet needs to be checked.

In this section we'll walk through how a TCP packet is created, sent and received in great detail.

## Transmitting data from the server and receiving in the client

To send a TCP packet, first we created a SendData method on the Client.cs class’s TCP class. In that method, we check if the socket (TcpClient) is not null, and after that we call the stream’s BeginWrite. In this method we pass the packet in array format, offset of 0, the packet’s length as the length and null as the object state. We wrap this logic in a try-catch block to avoid any runtime errors that might crash our server.

```C#
public void SendData(Packet packet)
{
    try
    {
        if (socket != null)
        {
            stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
        }
    }
    catch (Exception e)
    {
        Debug.Log($"Error sending data to server via TCP: {e}");
    }
}
```

Next, on the ServerSend.cs class, we add a SendTCPData method that takes in the client’s ID and the packet itself. Within this method we call the packet’s WriteLength method  and use the correct client’s ID to send a packet to a specific client.

```C#
private static void SendTCPData(int clientId, Packet packet)
{
    packet.WriteLength();
    Server.clients[clientId].tcp.SendData(packet);
}
```

Following the same logic as the previous method, we also have a method that send a TCP packet to all connected clients as well as a method to send a TCP packet to all connected clients except one.

```C#
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
```

After this, we can use these methods to send a TCP packet to the client. The following is how the Welcome packet is being sent on the ServerSend.cs.

```C#
public static void Welcome(int clientId, string msg)
{
    using (Packet packet = new Packet((int)ServerPackets.welcome))
    {
        packet.Write(msg);
        packet.Write(clientId);

        SendTCPData(clientId, packet);
    }
}
```

And we call this packet on the Client.css TCP class, on the connect method.

```C#
public void Connect(TcpClient _socket)
{
    socket = _socket;
    socket.ReceiveBufferSize = dataBufferSize;
    socket.SendBufferSize = dataBufferSize;

    stream = socket.GetStream();

    receivedData = new Packet();
    receiveBuffer = new byte[dataBufferSize];

    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

    ServerSend.Welcome(id, "Welcome to the server!");
}
```

At this point, the packet is being sent to the client.

On the client side, we need to update the packetHandler’s dictionary on the Client.css and link the packet to a new method in ClientHandle.css. This was previously discussed on Packets/Receiving a packet topic.

Right before we call the BeginRead method from the stream attribute, we call the receivedData (Packet.css) Reset method. This will take a Boolean return from the HandleData method and determine if we reset the packet or not.

Since TCP is stream based, it sends a continuous stream of information. It will ensure all packets we send are delivered and in the correct order. While the chunks of data being sent are guaranteed to arrive, they aren’t guaranteed to be delivered in one piece. In this sense, when we send a packet it will be added to a larger list of bytes, and once enough bytes accumulate they’re sent in one bigger delivery. TCP leaves it up to the developer to handle cases where a packet is split between two deliveries which is why we don’t always want to reset the received bytes. There could still be a piece of a packet in it that the rest of it hasn’t arrived.

```C#
private void ReceiveCallback(IAsyncResult _result)
{
    try
    {
        int _byteLength = stream.EndRead(_result);

        if (_byteLength <= 0)
        {
            
            Server.clients[id].Disconnect();
            return;
        }

        byte[] _data = new byte[_byteLength];
        Array.Copy(receiveBuffer, _data, _byteLength);

        receivedData.Reset(HandleData(_data)); // Reset receivedData if all data was handled
        stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
    }
    catch (Exception _ex)
    {
        Debug.Log($"Error receiving TCP data: {_ex}");
        Server.clients[id].Disconnect();
    }
}
```

On the HandleData method, we check if the receivedData has more than 4 unread bytes. If it does, it means that we have the start of one of our packets because a integer consists of 4 byes and the start of our packet always begins with an integer representing the length of the packet.
Next we check if that integer is less than 1. If it is, we reset the data.

```C#
 private bool HandleData(byte[] _data)
{
    int _packetLength = 0;

    receivedData.SetBytes(_data);

    if (receivedData.UnreadLength() >= 4)
    {
        // If client's received data contains a packet
        _packetLength = receivedData.ReadInt();
        if (_packetLength <= 0)
        {
            // If packet contains no data
            return true; // Reset receivedData instance to allow it to be reused
        }
    }
```

Next, and still no the same method, we do a while loop in which we check if the packet length is greater than 0 and less than the unread length of the received data. If this is true, it means we still have another complete packet that we need to handle.
Inside a different thread from what the Unity framework is running, we then create a new packet using the bytes that were sent over the network. With this new packet, grab the correct packet handler (discussed previously) using our packet ID and invoke it passing it the packet instance. This is how the ID get’s mapped to the actual byte array.

```C#
    while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
        {
 // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
            byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                }
            });
```

After that we check for the same condition as the beginning of the method to check if the packet has been processed in its entirety.

```C#
        _packetLength = 0; // Reset packet length
        if (receivedData.UnreadLength() >= 4)
        {
            // If client's received data contains another packet
            _packetLength = receivedData.ReadInt();
            if (_packetLength <= 0)
            {
                // If packet contains no data
                return true; // Reset receivedData instance to allow it to be reused
            }
        }
}
```

Outside the loop, we check if the packet length is less or equal to 1, in which case we return true. If it is not, that means that what was processed was part of the packet and not its entirety and we should continue to handle it to its completion.

```C#
    if (_packetLength <= 1)
    {
        return true; // Reset receivedData instance to allow it to be reused
    }

    return false;
```

At this stage, the server can send TCP packets to the server but the client can not respond.

## Transmitting data from the client and receiving it on the server

Like the server, in the client’s Client.cs class, we have a TCP sub-class with a method named SendData. This method is responsible for sending TCP data over to the server. We start by checking if the socket (TcpClient) is null and, if it’s not, we call the stream’s BeginWrite method, passing the packet as an array of bytes, an offset of 0, the packet length null for a callback and null for a object state.

We wrap this logic in a try-catch block since the method can produce an exception that could potentially crash the server.

```C#
public void SendData(Packet packet)
{
    try
    {
        if (socket != null)
        {
            stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
        }
    }
    catch (Exception e)
    {
        Debug.Log($"Error sending data to server via TCP: {e}");
    }
}
```

Much like to the server, we have a ClientSend.cs class where the sending logic is written on the client side. Inside that class, you will find a SendTCPData method that sends TCP packets to the server.

```C#
private static void SendTCPData(Packet packet)
{
    packet.WriteLength(); // Writes the length of the packet at the start of the list of bytes
    Client.instance.tcp.SendData(packet);
}
```

At this stage the client can now send TCP packets to the server. To illustrate this, let’s take a look at the WelcomeReceived packet. Much like the server, we use the using keyword to dispose of the packet after we have sent it.

```C#
public static void WelcomeReceived()
{
    using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
    {
        packet.Write(Client.instance.myId);
        packet.Write(UIManager.instance.usernameField.text);

        SendTCPData(packet);
    }
}
```

On the ClientHandle.cs Welcome method, we call the WelcomeReceived method over on the ClientSend.cs. This is because we want to inform the server that the transmission was successful as well as some other topics that will be discussed further on.

```C#
public static void Welcome(Packet packet)
{
    string msg = packet.ReadString();
    int myId = packet.ReadInt();

    Debug.Log($"Message from server: {msg}");
    Client.instance.myId = myId;

    ClientSend.WelcomeReceived();

    Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
}
```

Back on the server side, we need to be able to handle the incoming TCP packets coming from the client. This happens within the ServerHandle.cs class WelcomeReceived method which will be the method that we call once we detect the WelcomeReceived packet coming from the client.

In that method, we start by reading the data of the packet **in the same order that it was written**. This data will be used to spawn the player and print some good logging statements that we will take a look later on.

```C#
public static void WelcomeReceived(int clientId, Packet packet)
{
    int clientIdCheck = packet.ReadInt();
    string username = packet.ReadString();

    Debug.Log($" {username} ({Server.clients[clientId].tcp.socket.Client.RemoteEndPoint}) " +
                                                    $"connected successfully and is now player {clientId}.");
    if (clientId != clientIdCheck)
    {
        Debug.Log($"Player \"{username}\" (ID: {clientId}) has assumed the wrong client ID ({clientIdCheck})!");
    }
    Server.clients[clientId].SendIntoGame(username);
}
```

On the Server.cs, much like the Client.cs on the client side, we need a dictionary of integers, PacketHandlers. Since the server is handling multiple clients at the same time, the PacketHandler itself will also have an ID.

We initialize that dictionary on the InitializeServerData method.

```C#
private static void InitializeServerData()
{
    for (int i = 1; i <= MaxPlayers; i++)
    {
        clients.Add(i, new Client(i));
    }

    packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.updTestReceived, ServerHandle.UDPTestReceived },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
            { (int)ClientPackets.weaponIndex, ServerHandle.WeaponIndex },
        };
    Debug.Log("Initialized packets.");
}
```

Next, we need to handle the data we receive. Since the HandleData method on the client side works also for the server side, we just copy it and don’t need to change anything. We just need to also add the receivedData’s (Packet) Reset method, and pass the HandleData method as the parameter, much to the likes of the client side.

```C#
 private bool HandleData(byte[] data)
{
    int packetLength = 0;

    receivedData.SetBytes(data);

    if (receivedData.UnreadLength() >= 4)
    {
        packetLength = receivedData.ReadInt();
        if (packetLength <= 0)
        {
            return true;
        }
    }

    while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
    {
        byte[] packetBytes = receivedData.ReadBytes(packetLength);
        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (Packet packet = new Packet(packetBytes))
            {
                int packetId = packet.ReadInt();
                packetHandlers[packetId](packet);
            }
        });

        packetLength = 0;
        if (receivedData.UnreadLength() >= 4)
        {
            packetLength = receivedData.ReadInt();
            if (packetLength <= 0)
            {
                return true;
            }
        }
    }

    if (packetLength <= 1)
    {
        return true;
    }

    return false;
}
```

At this stage, the client and both send and receive TCP packets to and from the server and the server can send packets to one, or more clients whilst receiving packets from multiple clients.

