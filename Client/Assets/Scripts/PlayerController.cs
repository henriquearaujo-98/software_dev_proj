using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public AudioClip walkingSound;

    public AudioSource audioSource;

    [SerializeField] Transform camTransform;
    [SerializeField] WeaponSwitching weaponHolder;
    [SerializeField] DamageIndicator DamageIndicator;
    [SerializeField] GameObject DamageFrom;
    Transform DamageFromTransform;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward);
            Debug.Log(camTransform.forward);
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            weaponHolder.weapons[weaponHolder.selectedWeapon].gameObject.GetComponent<Weapon>().isWalking = true;
            StartCoroutine(WalkingSound());
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
            Input.GetKey(KeyCode.R),            // 8

            
        };

        ClientSend.PlayerMovement(_inputs);
    }


    public void RegisterDamageIndicator(Vector3 _from)
    {

        GameObject temp = Instantiate(DamageFrom, _from, Quaternion.identity);
        DamageFromTransform = temp.transform;

        DI_System.CreateIndicator(DamageFromTransform);

    }

    IEnumerator WalkingSound()
    {
        yield return new WaitForSecondsRealtime(2f);
        audioSource.PlayOneShot(walkingSound);

    }
}
