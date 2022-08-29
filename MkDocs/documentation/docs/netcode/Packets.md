# Packets

<sub>Author: Henrique Araújo</sup>

It is impossible to send, for example, an integer or string through a wire. Instead, you need to, send a byte array that represents a certain data structure and its value. To accomplish this, we use the in-house implementation of the Packet class.

The premise is that a packet has a unique ID known to both client and server. So, in that sense, we have 2 enumerator types on the Packet.cs class. One to keep track of the available packets that a client can send/server can receive and one to keep track of the packets that the client can receive/server can send.

## Server Packets
A server packet is a packet that is sent from the server to all, one or all but one client.
A enumerator is responsible for giving each of these packets an ID.

```C#
public enum ServerPackets
{
    welcome = 1,
    udpTest,
    spawnPlayer,
    playerInputs,
    playerPosition,
    playerRotation,
    playerDisconnected,
    playerHealth,
    damageIndicator,
    playerRespawned,
    killFeed,
    killNotification
}
```

## Client Packets
A client packet is a packet that is sent from a client to the server.
A enumerator is responsible for giving each of these packets an ID.

```C#
public enum ClientPackets
{
    welcomeReceived = 1,
    updTestReceived,
    playerMovement,
    playerShoot,
    weaponIndex
}
```

## The Packet class

### Constructors

The Packet class has 3 constructors overrides. They are

#### Default Constructor

Used to generate an empty packet.

```C#
public Packet(){    // Creates empty packet
    buffer = new List<byte>();
    readPos = 0;
}
```

#### Parameterized Constructor (Used for sending)

Used for sending data to the client/server

```C#
public Packet(){    // Creates a new packet with a given ID.
    buffer = new List<byte>();
    readPos = 0;
}
```

#### Parameterized Constructor (Used for receiving)

Used for receiving data to the client/server

```C#
public Packet(){    // Used for reconstructing the packet object with the byte array coming from the stream
    buffer = new List<byte>();
    readPos = 0;
}
```

### Helper methods

#### SetBytes
Initializes the Packet's data using a given byte array

#### Write Length
Inserts the byte length of the packet at the start of the list. This is crucial for unpacking a packet.

#### InsertInt
Inserts the ID of the packet. Not to be confused with the Insert(int value) method which inserts an integer that is not associated with the ID of the packet.

#### ToArray
Returns the buffer’s content as an array.

#### Length
Returns the length of the buffer

#### UnreadLength
Returns the Length of the buffer minus the current reading position of the cursor.

#### Reset 
Resets the buffer according to Boolean. If false, we just unread the last integer (4 is the number of bytes for an integer)

#### Writting data to the packet
Writing data to the packet is straightforward. You just need to add a sequence of bytes to the byte List. These are automatically converted from a primitive data structure (int, bool etc) to byte format, as the following example demonstrates.

```C#
public void Write(int _value){
    buffer.AddRange(BitConverter.GetBytes(_value));
}
```

### Reading data from the packet
Reading data is a little bit more complex. Let’s take as an example the ReadInt method. If the size of the buffer is greater than the read position (cursor), that means there are no integers to read in the byte array and we throw an exception. Otherwise we take the next set of bytes from the list and convert them to an integer. If the local variable _moveReadPos is true, we increase the read position by 4 (because an integer is represented by 4 bytes). The read position tells us which bytes in the packet we have converted and which ones are next. At the end, we return the value.

```C#
public int ReadInt(bool _moveReadPos = true){

    if(buffer.Count > readPos){

        int _value = BitConverter.ToInt32(readableBuffer, readPos);
        if(_moveReadPos){
            readPos += 4;
        }
        return _value;

    }else{
        throw new Exception(!"Could not read value of type 'int'!");
    }

}
```