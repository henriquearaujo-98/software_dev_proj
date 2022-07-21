/// Pay attention to the animator's animation node names
/// The length of the reload animtion needs to match the reload time manually. A good rule is setting the Speed of the animation to twice the reload time variable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Transform shootOrigin;
    public ParticleSystem MuzzleFlash;

    [SerializeField] PlayerManager owner;

    Animator anim;

    private void Awake() {
        shootOrigin = GameObject.FindGameObjectWithTag("PlayerCamera").transform;
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmoInMagazine;
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (isWalking)
            anim.SetBool("Walk", true);
        else
            anim.SetBool("Walk", false);

        if (isReloading)
            return;

        if( currAmmo<=0 && TotalAmmo>0 )
        {
           StartCoroutine(Reload());
            return;
        }

        if(Input.GetKeyDown(KeyCode.R) && currAmmo != maxAmmoInMagazine && TotalAmmo>0){
           StartCoroutine(Reload());
            return;
        }

        if(Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire && fireRate > 0)
        {                  
            nextFire = Time.time + fireRate;
            Shoot ();
        }
    }

    private void Update()
    {
        if (isReloading)
            return;

        if (fireRate == 0 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
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
                owner.showHitmarker();
            }
            else
            {
                Instantiate(ImpactPoint, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        anim.Play("Fire Hip", 0, 0f);
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

        isReloading = false;

    }

}
