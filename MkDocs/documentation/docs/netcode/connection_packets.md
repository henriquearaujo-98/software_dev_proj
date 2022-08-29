# Connection packets

<sub>Author: Henrique Araújo</sup>

This section is going to cover some concepts such as Server/Client synchornization aswell as how the first players are connected to the server.

## Welcome packet

The Welcome packet is a [server packet](../netcode/Packets.md#server-packets).

After a client successfuly connects itself to the server via TCP, the server then adds that client to the Client class clients list, giving it an ID 
The next step is to send that ID to the newly connected player so that he can initialize its Client class. This is done in the Client class's TCP class Connect method.

For more on this, please check [TCP Connection](../netcode/TCP_connection.md).

In the ServerSend class Welcome method, we send the welcome packet. This packet will contain the newly connected client's ID and a welcome to the server method.
We use the SendTCPData method because we only want to provide this information to a specific client.

```C#
// ServerSend
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

On the Client's ClientHandle class, we receive the data on the packet and we initialize our Client's ID and initialize a [UDP Connection](../netcode/UDP_Transport_Layer.md).
Furthermore, we send back to the server a [Welcome Received](#welcome-received-packet) packet.

```C#
//ClientHandle
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

## Welcome Received packet

The Welcome packet is a [client packet](../netcode/Packets.md#client-packets).

Immediately after a client receives a Welcome packet from the server, it sends a Welcome Received back.

This packet contains the ID of the now initialized Client and a username given by the user.

```C#
using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
{
    packet.Write(Client.instance.myId);
    packet.Write(UIManager.instance.usernameField.text);

    SendTCPData(packet);
}
```

On the Server's Server Handle class, we read the data inside the package, and check if the client ID both on the server and client match. If it doesn't, it most likely means it is a bad actor trying to connect.

If every thing is fine, we send the player into the game. In other words, we [Spawn a Player](#spawn-a-player).

### Spawn a Player

Spawning a player is the visual representation that the client is connected to the server.

The Client Packet Welcome Received is responsible for sending a player into the game world on the **server**. To do this, it accesses the Server class clients list by the ID passed through the network and calls the Client class SendIntoGame method.

Inside this method, we instantiate the player game object and initialize its Player class with the ID and username passed through the network to the server. We take this game object to populate the Client class player attribute which represents the player in the game world.

```C#
player = NetworkManager.instance.InstantiatePlayer();
player.Initialize(id, _playerName);
```

After that, it sends information about every other connected client to the newly connected client so that it can render the server's game state.

```C#
// Send all players to the new player
foreach (Client _client in Server.clients.Values)
{
    if (_client.player != null)
    {
        if (_client.id != id)
        {
            ServerSend.SpawnPlayer(id, _client.player, sceneID);
        }
    }
}
```

And after that it sends information about the newly connected client to every other connected client.

```C#
// Send the new player to all players (including himself)
foreach (Client _client in Server.clients.Values)
{
    if (_client.player != null)
    {
        ServerSend.SpawnPlayer(_client.id, player, sceneID);
    }
}
```

This information is sent through the [SpawnPlayer packet](#spawn-player-packet)

## Spawn Player Packet

The SpawnPlayer packet is a Server Packet that is used to render the server's game state in each individual connected clients.

It gets called whenever a new player spawns within the Server game world. Namingly, on the Server's Client class SendIntoGame method [see Spawn a Player](#spawn-a-player).

This packet is send in the ServerSend class SpawnPlayer method. It consists on sending 5 pieces of data:

+ Player ID
+ player username
+ Initial player position
+ Initial player rotation
+ Current scene ID: might not be used if we're sending information about a newly connected client to the ones that are already rendering the server's scene (game map).

```C#
public static void SpawnPlayer(int toClient, Player player, int sceneID)
{
    
    using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
    {
        packet.Write(player.id);
        packet.Write(player.username);
        packet.Write(player.transform.position);
        packet.Write(player.transform.rotation);
        packet.Write(sceneID);

        SendTCPData(toClient, packet);
    }
}
```

Over on the Client, we begin by reading the data and initializing the player, wether it is itself or a remote player. After that, it initializes a feed item, informing the client that a new remote client has connected.

```C#
public static void SpawnPlayer(Packet packet)
{

    int id = packet.ReadInt();
    string username = packet.ReadString();
    Vector3 position = packet.ReadVector3();
    Quaternion rotation = packet.ReadQuaternion();
    int sceneID = packet.ReadInt();
    string msg = $"{username} has joined the game.";
    
    GameManager.instance.SpawnPlayer(id, username, position, rotation, sceneID);

    if(GameManager.players.ContainsKey(Client.instance.myId))
        GameManager.players[Client.instance.myId].pc.killFeedHandler.InstantiateKillFeedItem(msg);
}
```

At this point, users can connect to the server and see other users that are connected.

## PlayerDisconnected packet

The PlayerDisconnected packet is a Server Packet that is responsible for letting other connected clients that a certain client has terminated its connection to the server.

It gets called on the Server's Client class Disconnect method and it sends a packet to all connected clients with the ID of the client that has left.

```C#
//ServerSend
public static void PlayerDisconnected(int clientId)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(clientId);

            SendTCPDataToAll(packet);

        }
    }
```

Over on the client, we read the ID and, with that ID, we remove an entry on the GameManager class list of players, destroying the object that that entry is associated with from the client's game state.

We also push a Feed Item informing that a player has left the session.

```C#
public static void PlayerDisconnected(Packet _packet)
{
    
    int _id = _packet.ReadInt();
    string msg = $"Player {GameManager.players[_id].GetComponent<PlayerManager>().username} has disconnected.";
    Debug.Log(msg);
    Destroy(GameManager.players[_id].gameObject);
    GameManager.players.Remove(_id);

    GameManager.players[Client.instance.myId].pc.killFeedHandler.InstantiateKillFeedItem(msg);
}
```