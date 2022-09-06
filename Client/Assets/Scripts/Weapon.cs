/// Pay attention to the animator's animation node names
/// The length of the reload animtion needs to match the reload time manually. A good rule is setting the Speed of the animation to twice the reload time variable

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Audio;

public class Weapon : MonoBehaviour
{
    public int id;
    public int maxAmmoInMagazine;
    public int currAmmo;
    public int TotalAmmo;
    public float fireRate;
    public float reloadTime = 1f;
    public bool isReloading = false ;
    public bool isWalking = true;

    public float nextFire;

    public ParticleSystem ImpactPoint;

    public Text ammoUI;

    public AudioClip shootingSound;
    public AudioClip reloadingSound;
    public AudioClip hitmarkerSound;

    public AudioSource audioSource;

    private Transform shootOrigin;
    public ParticleSystem MuzzleFlash;

    [SerializeField] PlayerManager owner;

    public Animator anim;

    private Vector3 originalPosition;
    public Vector3 aimPosition;
    private bool isAiming;
    Camera playerCam;
    public float ADSSpeed = 8f;
    float initialZoom;
    public float aimZoom = 45;
    public GameObject crosshair;

    private void Awake() {
        shootOrigin = GameObject.FindGameObjectWithTag("PlayerCamera").transform;
        playerCam = shootOrigin.gameObject.GetComponent<Camera>();
        initialZoom = playerCam.fieldOfView;
    }

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmoInMagazine;   
        originalPosition = transform.localPosition;
    }

    private void AimDownSights()
    {
        if(Input.GetKey(KeyCode.F) && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * ADSSpeed);
            isAiming = true;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, aimZoom, Time.deltaTime * ADSSpeed);
            crosshair.SetActive(false);
            Debug.Log("aiming");
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * ADSSpeed);
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView,initialZoom, Time.deltaTime * ADSSpeed);
            crosshair.SetActive(true);
            isAiming = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       

        if (isReloading)
            return;

        if( currAmmo<=0 && TotalAmmo>0 )
        {
           StartCoroutine(Reload());
            return;
        }

        

        
    }

    private void Update()
    {
        if (isReloading)
        {
            anim.Play("Reload w bullets");
            return;
        }
            

        if (isWalking)
            anim.SetBool("Walk", true);
        else
            anim.SetBool("Walk", false);

        if (fireRate == 0 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
            
        }

        if (Input.GetKeyDown(KeyCode.R) && currAmmo != maxAmmoInMagazine && TotalAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire && fireRate > 0)
        {
            nextFire = Time.time + fireRate;
            Shoot();
        }
        anim.SetBool("Aim", isAiming);
        AimDownSights();

        UpdateAmmoUI();
        
    }

    void Shoot(){

        if (TotalAmmo<=0 && currAmmo<=0)
            return; ///add clicking sound 

        MuzzleFlash.Play();

        currAmmo--;
        

        RaycastHit hit; 
        if (Physics.Raycast(shootOrigin.position, shootOrigin.forward, out hit, 250f))
        {
            if (hit.collider.tag == "Enemy")
            {
                if(owner.pc)
                    owner.pc.showHitmarker();
                    audioSource.PlayOneShot(hitmarkerSound);
            }
            else
            {
                Instantiate(ImpactPoint, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        anim.Play("Fire Hip", 0, 0f);
        audioSource.PlayOneShot(shootingSound);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        anim.SetBool("Reload", true);

        if (currAmmo <= 0)
            anim.Play("Reload No Bullets", 0, 0f);
        else
            anim.Play("Reload With Bullets", 0, 0f);


        Debug.Log("reloading ..");

        yield return new WaitForSeconds(reloadTime);

        if (TotalAmmo >= maxAmmoInMagazine){
            TotalAmmo = TotalAmmo - maxAmmoInMagazine + currAmmo;

            currAmmo = maxAmmoInMagazine;
        }else{
            currAmmo = TotalAmmo;
            TotalAmmo = 0;
        }

        anim.SetBool("Reload", false);
        audioSource.PlayOneShot(reloadingSound);

        isReloading = false;

    }

    public void UpdateAmmoUI()
    {
        ammoUI.text = currAmmo + "/" + TotalAmmo;
    }

}
