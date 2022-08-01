using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class footsteps : MonoBehaviour
{
    public AudioSource footstepsSound;

    public void Update() 
    {
       if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.S)) || (Input.GetKey(KeyCode.A)))
       {
           footstepsSound.volume = Random.Range(0.8f, 1);
           footstepsSound.pitch = Random.Range(0.8f, 1.1f);

           footstepsSound.enabled = true;
       }
       else
       {
           footstepsSound.enabled = false;
       } 
    }
}
