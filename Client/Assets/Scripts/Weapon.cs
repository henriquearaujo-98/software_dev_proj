using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string name;
    public int maxAmmoInMagazine;
    public int currAmmo;
    public int TotalAmmo;
    public float cooldownSeconds = 4;
    public float cooldown;
    public float fireRate;

    public float reloadTime = 1f;
    public bool isReloading = false ;


    public float nextFire;
    public ParticleSystem ImpactPoint; 

    private Transform shootOrigin;
    public ParticleSystem MuzzleFlash;

    private void Awake() {
        shootOrigin = GameObject.FindGameObjectWithTag("PlayerCamera").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmoInMagazine;
    }

    // Update is called once per frame
    void Update()
    {    
        if (isReloading)
            return;


        if( currAmmo<=0 && TotalAmmo>0 )
        {
           StartCoroutine(Reload());
            return;

        }

        if(fireRate == 0 && Input.GetKeyDown(KeyCode.Mouse0))
        {  
            Shoot ();
            
        }
        else
        {
            if(Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire && fireRate > 0)
            {
                   
                   nextFire = Time.time + fireRate;
                   Shoot ();
                   
                
                  
                   if(cooldown > Time.time)
                   {   
                      cooldown = Time.time + cooldownSeconds; 
                   }
                
            }
        } 
             
    }

    void Shoot(){

        if (TotalAmmo<=0 && currAmmo<=0)
            return; ///add clicking sound 

        MuzzleFlash.Play();

        currAmmo--;
        
        ClientSend.PlayerShoot(shootOrigin.forward);

        RaycastHit hit; 
        if (Physics.Raycast(shootOrigin.position, shootOrigin.forward, out hit, 250f))
        {
            Instantiate(ImpactPoint, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    IEnumerator Reload()
    {

        isReloading = true;

        Debug.Log("reloading ..");

        yield return new WaitForSeconds(reloadTime);

        currAmmo = maxAmmoInMagazine;

        TotalAmmo = TotalAmmo - maxAmmoInMagazine + (currAmmo);

        isReloading = false;
    }
}
