using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public AudioClip[] footStepsAudioClips;
    float footStepTimer;
    AudioSource audioSource;

    [SerializeField] Transform camTransform;
    public WeaponSwitching weaponHolder;
    [SerializeField] DamageIndicator DamageIndicator;
    [SerializeField] GameObject DamageFrom;
    Transform DamageFromTransform;
    [SerializeField] private Image foregroundImage;
    public KillFeedHandler killFeedHandler;
    public Animator canvasAnim;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward);
        }

        HealthBar();
    }

    private void FixedUpdate()
    {
        SendInputToServer();
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            weaponHolder.currentWeapon.gameObject.GetComponent<Weapon>().isWalking = true;

            if(Input.GetKey(KeyCode.LeftShift))
                weaponHolder.currentWeapon.gameObject.GetComponent<Weapon>().isRunning = true;
            else
                weaponHolder.currentWeapon.gameObject.GetComponent<Weapon>().isRunning = false;

        }
        else
        {
            weaponHolder.currentWeapon.gameObject.GetComponent<Weapon>().isWalking = false;
            weaponHolder.currentWeapon.gameObject.GetComponent<Weapon>().isRunning = false;
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

    public void KillNotification()
    {
        canvasAnim.Play("KillNofication", 1, 0f);
    }

    public void FootStepsHandler()
    {



    }

    public void HealthBar()
    {
        float healthPer = GetComponent<PlayerManager>().health / GetComponent<PlayerManager>().maxHealth;
        foregroundImage.fillAmount = healthPer;
    }

    public void showHitmarker()
    {
        canvasAnim.Play("Hitmarker", 0, 0f);

    }
}
