# Sending Packets

## From Client to Server

Sending a packet from a client to the server requires 3 steps:

1. Adding a new entry to the client packet enumerator.
2. Creating a packet with the new client packet enumerator entry.
3. Map the packet to a method on the server.

In the upcoming examples, we will be sending a packet named myPacket containing the ID, position and username of a player.

### Adding an entry to the client packet enumerator

Every packet needs to have an ID so that when it reaches the Server, we can map that packet to a method, triggering a behaviour. In this example, we are going to send the **myPacket** packet.

So, on the Packet.cs file, we add a new entry to the ClientPackets enumerator:

```C#
public enum ClientPackets
{
    welcomeReceived = 1,
    myPacket
}
```

### Creating a packet with the new client packet enumerator entry.

On the **ClientSend** class, we need a new method. By covention, this method will be named after the packet that it is going to send.

Inside this method, we create a packet, initializing it with the ID found on the enumerator entry that we added on the previous step, and we write to the packet the data that we want the server to receive.

After that we can send the packet through the SendTCPData or SendUDPData methods found within the same class.

```C#
public static void myPacket(int ID, Vector3 position, string username){

    using (Packet _packet = new Packet((int)ClientPackets.myPacket)){
        _packet.Write(ID);
        _packet.Write(position);
        _packet.Write(username);

        SendTCPData(_packet);
    }
}
```

#### <sup>Usage</sup>

Send the data to the client, on the Player class:

```C#
// Player class
int ID;
string username;
Vector3 position;

void Start(){
    ClientSend.myPacket(ID, position, username);
}

```

### Map the packet to a method on the server

To map a packet to a method, we first need to create the method that the packet is going to trigger.

This is done on the Server's ServerHandle class. We name this method the same name as the packet to ensure consistency.

Sending a packet from the client to the server makes it so the packet holds the connected client's ID. And it is passed to the method that the packet triggers.

To read the data from the packet, we must do it in the same order that it was written since it has been transmited in a sequencial byte array (stream).

```C#

public static void MyPacket(int _fromClient, Packet packet){

    int ID = packet.ReadInt();
    string username = packet.ReadString();
    Vector3 position = packet.ReadVector3();

    if(ID != _fromClient){
        Debug.Log("Impostor!");
        return;
    }

    Server.clients[_fromClient].username = username;
    Server.clients[_fromClient].transform.position = position;

}

```

Next we need to map the packet to the method. This is done on the Server class **InitializeServerData** method.

This method gets called when the Server boots up and it holds a Dictionary mapping packet IDs coming from the ClientPackets enumerator to a method on the ServerHandle class.

In this sense, we can map our packet like so.

```C#
packetHandlers = new Dictionary<int, PacketHandler>()
{
    { (int)ClientPackets.myPacket, ServerHandle.MyPacket }
}
```

**IMPORTANT** : The Packet class must be exactly the same both of the Client and on the Server.


## From Server to Client

Sending a packet from a server to all, one or all but one client requires 3 steps:

1. Adding a new entry to the server packet enumerator.
2. Creating a packet with the new server packet enumerator entry.
3. Map the packet to a method on the client.

In the upcoming examples, we will be sending a packet named myPacket containing the dummy variables.

### Adding an entry to the server packet enumerator

Every packet needs to have an ID so that when it reaches the Client, we can map that packet to a method, triggering a behaviour. In this example, we are going to send the **myPacket** packet.

So, on the Packet.cs file, we add a new entry to the ClientPackets enumerator:

```C#
//Packet.cs
public enum ServerPackets
{
    welcome = 1,
    myPacket
}
```

### Creating a packet with the new server packet enumerator entry.

On the **ServerSend** class, we need a new method. By covention, this method will be named after the packet that it is going to send.

Inside this method, we create a packet, initializing it with data that we want the client(s) to receive.

After that we can send the packet through the SendTCPData or SendUDPData methods found within the same class.

Unlike the client, we now have multiple options when it comes to sending both UDP and TCP classes:

+ Send the packet to every connected client
+ Send the packet to every connected client execpt one, given the ID
+ Send the packet to only one specific connected client

```C#
//ServerSend.cs
public static void myPacket(int ID, Vector3 position, string username){

    using (Packet _packet = new Packet((int)ClientPackets.myPacket)){

        _packet.Write(ID);
        _packet.Write(position);
        _packet.Write(username);

        SendTCPData(ID, _packet);
        // SendTCPDataToAll(_packet);
        // SendTCPDataToAllExeptOne(ID, _packet);

        // SendUDPData(ID, _packet);
        // SendUDPDataToAll(_packet);
        // SenUDPPDataToAllExeptOne(ID, _packet);
    }
}
```

#### <sup>Usage</sup>

Send the data to the client.

```C#
    ServerSend.myPacket(1, transform.position, "player");
```

### Map the packet to a method on the client

To map a packet to a method, we first need to create the method that the packet is going to trigger.

This is done on the Client's ClientHandle class. We name this method the same name as the packet to ensure consistency.

To read the data from the packet, we must do it in the same order that it was written since it has been transmited in a sequencial byte array (stream).

```C#
// ClientHandle.cs
public static void MyPacket(int _fromClient, Packet packet){

    int ID = packet.ReadInt();
    string username = packet.ReadString();
    Vector3 position = packet.ReadVector3();

}

```

Next we need to map the packet to the method. This is done on the Client class **InitializeClientData** method.

This method gets called when the Client laucnhes and it holds a Dictionary mapping packet IDs coming from the ClientPackets enumerator to a method on the ServerHandle class.

In this sense, we can map our packet like so.

```C#
packetHandlers = new Dictionary<int, PacketHandler>()
{
    { (int)ServerPackets.myPacket, ClientHandle.MyPacket }
}
```

**IMPORTANT** : The Packet class must be exactly the same both of the Client and on the Server.