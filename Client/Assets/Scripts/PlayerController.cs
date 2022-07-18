using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] Transform camTransform;
    [SerializeField] WeaponSwitching weaponHolder;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward);
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            weaponHolder.weapons[weaponHolder.selectedWeapon].gameObject.GetComponent<Weapon>().isWalking = true;
        }
        else
        {
            weaponHolder.weapons[weaponHolder.selectedWeapon].gameObject.GetComponent<Weapon>().isWalking = false;
        }
    }

    private void SendInputToServer()
    { 

        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),            // 0
            Input.GetKey(KeyCode.S),            // 1
            Input.GetKey(KeyCode.A),            // 2
            Input.GetKey(KeyCode.D),            // 3
            Input.GetKey(KeyCode.Space),        // 4
            Input.GetKey(KeyCode.LeftShift),    // 5
            Input.GetKey(KeyCode.C),            // 6
            Input.GetKey(KeyCode.Mouse0),       // 7 
        };

        ClientSend.PlayerMovement(_inputs);
    }
}
