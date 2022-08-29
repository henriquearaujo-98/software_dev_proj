# Shooting

<sub>Author: Henrique Araújo</sup>


On the client side’s PlayerController.cs script, we check every frame of the client if he has pressed the Mouse0 button. We do this in the Unity’s MonoBehaviour Update method because it is imperative that a shot is registered. When we detect such input, we then call the ClientSend.cs PlayerShoot method, passing in the camera’s transform which will be our shooting origin.

```C#
if (Input.GetKey(KeyCode.Mouse0))
{
    ClientSend.PlayerShoot(camTransform.forward);
}
```

On the ClientSend.cs PlayerShoot method, we initialize a packet passing in the “playerShoot” ID and write the vector passed in through the previous method. We send this packet through TCP because we can not afford to lose it, as losing it could possibly be game breaking.

```C#
public static void PlayerShoot(Vector3 _facing)
{
    using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
    {
        _packet.Write(_facing);

        SendTCPData(_packet);
    }
}
```

On the server side’s ServerHandle.cs script, we receive the packet in the PlayerShoot method and start by reading the direction in which the player was facing when he pressed mouse0 on the client side. After that, we access that client’s Player.cs script and call the Shoot method, passing in that same vector.

It is worth noting that we could simply use the server’s render of the player’s rotation for this but since the rotation is not server authoritative, it is better for the client to send in which direction he actually was facing instead of relying on a possible outdated state of game that the server holds.

```C#
public static void PlayerShoot(int _fromClient, Packet _packet)
{
    Vector3 _shootDirection = _packet.ReadVector3();

    Server.clients[_fromClient].player.Shoot(_shootDirection);
}
```

On the Player.cs Shoot method, we can access the current weapon that the player is using and set the both the ```_viewDirection``` and ```shootOrigin``` variables for shooting.
Note that the weapon is not yet shooting.

```c#
public void Shoot(Vector3 _viewDirection)
{
    weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().viewDirection = _viewDirection;
    weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().shootOrigin = shootOrigin;
}
```

One of the inputs sent by the client is the ```Mouse0``` (Left mouse button). We set this boolean to the Weapon class ````getButton```` variable and use it as the Unity's Input system.

We Update this variable in the Player class ````FixedUpdate```` method.

```c#
 weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().getButton = inputs[7];
```

For more information on shooting behaviour please see [weapons](weapons.md.md).
