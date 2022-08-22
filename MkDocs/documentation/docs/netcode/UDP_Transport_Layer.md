# UDP Transport Layer

The UDP/IP protocol suite serves the same purpose as its counterpart, the TCP/IP protocol suite. The main difference between these two is that whilst the TCP guarantees the delivery of packets in their correct order at the cost of latency, the UDP just sends it to the client disregarding if it gets there or not. This is mostly appropriate if we need to send packets every tick of the server.

The UDP connection is only established after the TCP connection. In this sense, we use the connection packets from the TCP to trigger the UDP connection.

In this section we'll walk through how a UDP packet is created, sent and received in great detail.

## Client

The client connects via UDP to the server, once the first TCP packet has been received (Welcome packet, to be discussed further on). When this packet is received, on the client’s ClientHandle.cs Welcome method, the Client.cs subclass UDP’s method Connect is called, passing the already established TCP local port.

```C#
Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
```

On the Connect method, we start by initiating the socket (UdpClient) with the local port passed in previously as an IPEndPoint. Note that this local port is a port in which the client is communicating and not the port to which we’re trying to send data to.

After that, we call the socket’s Connect method to start a UDP connection with the server and the BeginReceive method to start receiving data from the server which will be talked more in detail further on. 

We also send an empty packet with the Client ID to the server so that our attempt to connect via UDP gets recognized and handled. This packet’s purpose is only to initiate a connection with the server. The SendData method is responsible for adding the client ID to the packet since it is vital for proper UDP usage.

```C#
 public void Connect(int _localPort)
{
    socket = new UdpClient(_localPort);

    socket.Connect(endPoint);
    socket.BeginReceive(ReceiveCallback, null);

    using (Packet _packet = new Packet()) // Initialize the connection with the server and open localport so that the client can receive data
    {
        SendData(_packet);
    }

    LevelManager.instance.SetServerConnectionProgress(50);
}
```

On the SendData method, we start by inserting the client ID on the packet. We do this because we’ll be using this value on the server to determine who sent it. Because of the way UDP works, we can’t give each client their own UDP client instance on the server without running into issues with closing of ports. We can validate the ID by checking if the endpoints match on the server.

After that, we check if the socket isn’t null and start sending our packet. The UdpClient’s BeginSend method is similar to the TcpClient’s.

```C#
public void SendData(Packet _packet)
{
    try
    {
        _packet.InsertInt(instance.myId);
        if (socket != null)
        {
            socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
        }
    }
    catch (Exception _ex)
    {
        Debug.Log($"Error sending data to server via UDP: {_ex}");
    }
}
```

On the ReceiveCallback method, we start by assigning a byte array to the value returned by the socket’s EndReceive method, passing the IASyncResult as a parameter. 

Immediately after, we call the socket’s BeginReceive method again, passing it the same parameters as before.

After that, we check if the byte array length is less than 4 (again, because 4 if the size of an integer) and, in that case, we disconnect the Client because the connection has terminated.

After that we call the HandleData method to retrieve the information from the byte array. We wrap all of this logic in a try-catch block and, should an exception be throw, we disconnect the UDP.

```C#
private void ReceiveCallback(IAsyncResult _result)
{
    try
    {
        byte[] _data = socket.EndReceive(_result, ref endPoint);
        socket.BeginReceive(ReceiveCallback, null);

        if (_data.Length < 4)
        {
            instance.Disconnect();
            return;
        }

        HandleData(_data);
    }
    catch
    {
        Disconnect();
    }
}
```

On the handle data method, we start by creating a new packet with the bytes we received. Next, we read the length of the packet and store the packet’s information except its size on the “_data” variable. In this way, the next integer we read from the packet will be a packet ID.

After that, we use the ThreadManager.cs main thread to read the packet ID and invoke the corresponding method using the packetHandlers.

```C#
private void HandleData(byte[] _data)
{
    using (Packet _packet = new Packet(_data))
    {
        int _packetLength = _packet.ReadInt();
        _data = _packet.ReadBytes(_packetLength);
    }

    ThreadManager.ExecuteOnMainThread(() =>
    {
        using (Packet _packet = new Packet(_data))
        {
            int _packetId = _packet.ReadInt();
            packetHandlers[_packetId](_packet);
        }
    });
}
```

Now, on the ClientSend.cs file, we can add a SendUDPData method for sending UDP packets. Just like its TCP counterpart, we first write the length of the packet and then call the Client.cs instance’s UDP class to send the data.

```C#
private static void SendUDPData(Packet packet)
{
    packet.WriteLength();
    Client.instance.udp.SendData(packet);
}
```

## Server

Similarly to its client counterpart, on the Client.cs UDP class, you can find a Connect method that assigns and endpoint to the connection.

```C#
public void Connect(IPEndPoint _endPoint)
{
    endPoint = _endPoint;
}
```

After that, we have a SendData method so we can send UDP packets to a certain client. Inside it, we simply call the Server.cs SendUDPData method, passing our assigned endpoint and a packet.

```C#
public void SendData(Packet _packet)
{
    Server.SendUDPData(endPoint, _packet);
}
```

Much to the liking of the client, the UDP’s HandleData’s first two lines of code aims to remove the length of the packet from the byte array so that the next integer to be read will be the packet ID.

We use the ThreadManager.cs main thread to then read out the packet ID and invoke the appropriate method.

```C#
public void HandleData(Packet _packetData)
{
    int _packetLength = _packetData.ReadInt();
    byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

    ThreadManager.ExecuteOnMainThread(() =>
    {
        using (Packet _packet = new Packet(_packetBytes))
        {
            int _packetId = _packet.ReadInt();
            Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
        }
    });
}
```

On the Server.cs, we have a UdpClient object named udpListener to keep the consistency with the TCP implementation.

```C#
private static TcpListener tcpListener;
private static UdpClient udpListener;
```

On the Server.cs Start method, initialize this new variable with the server’s port number and, after that, we call the udpListener’s (UdpClient) BeginReceive method, passing the UDPReceiveCallback method as a parameter.

```C#
udpListener = new UdpClient(Port);
udpListener.BeginReceive(UDPReceiveCallback, null);
```

On the UDPReceiveCallback method, we start by creating an IPEndPoint with no specific IP address or port number. After that, we assign a byte array to the udpListener’s EndReceive method passing as second parameter a reference to the IPEndPoint we just created. This will not only get the bytes coming from the connection but will also populate the IPEndPoint with the information from the machine that the connection came from. Next we call the BeginReceive method again in case there are more connections that need handling.

```C#
private static void UDPReceiveCallback(IAsyncResult _result)
{
    try
    {
        IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
        udpListener.BeginReceive(UDPReceiveCallback, null);
```

After that we check if the byte array is less than 4 bytes longs, in which case we return out of the method because there is no integer with the packet’s byte size to be read.

```C#
if (_data.Length < 4)
{
    return;
}
```

After that, we create a new packet with the bytes received in the “_data” variable and start by reading the client ID. We do a quick check to see if it’s 0 which it should never be (starts at 1).

Immediately after, we check if the client that sent the packet has an active UDP connection. If it doesn’t, we connect it using the Client.cs UDP class Connect method and passing in the endpoint that was populated by the udpListener’s EndReceive method. Note that if this is the case, we know that the packet sent was the initial connection packet that opens up the client’s port. 

```C#
using (Packet _packet = new Packet(_data))
    {
        int _clientId = _packet.ReadInt();

        if (_clientId == 0)
        {
            return;
        }

        if (clients[_clientId].udp.endPoint == null)
        {
            clients[_clientId].udp.Connect(_clientEndPoint);
            return;
        }
```

If a connection is present, we then check the endpoints of the client in our server’s dictionary and compare it to the one that we got over the network. If these match, we can be sure that there is no impersonation (ID and endpoints match), and we call the UDP’s HandleData packet.

```C#
        if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
        {
            clients[_clientId].udp.HandleData(_packet);
        }
    }
}
```

Lastly, on the ServerSend.cs class, we add a SendUDPData, SendUDPDataToAll and SendUDPDataToAllExeptOne methods, much to the liking of its TCP counterpart.

```C#
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
```