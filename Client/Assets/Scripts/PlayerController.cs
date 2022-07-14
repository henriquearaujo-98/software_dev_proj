using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] Transform camTransform;
    

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
