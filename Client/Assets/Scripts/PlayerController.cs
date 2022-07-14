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
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space),
            Input.GetKey(KeyCode.LeftShift),
            Input.GetKey(KeyCode.C),
        };

        ClientSend.PlayerMovement(_inputs);
    }
}
