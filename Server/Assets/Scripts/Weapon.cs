using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string name;
    public int maxAmmoInMagazine;
    public int currAmmo;
    public int TotalAmmo;
    public float fireRate;
    public float reloadTime = 1f;
    public bool isReloading = false ;
    public bool fire = false;
    public int damage;

    public float nextFire;
    public ParticleSystem ImpactPoint; 

    private Transform shootOrigin;
    public ParticleSystem MuzzleFlash;

    private void Awake() {
        shootOrigin = GameObject.FindGameObjectWithTag("ShootOrigin").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmoInMagazine;
    }

    // Update is called once per frame
    public void SetShooting(Vector3 _viewDirection)
    {    
        if (isReloading)
            return;

        if( currAmmo<=0 && TotalAmmo>0 )
        {
           StartCoroutine(Reload());
            return;
        }

        if(fireRate == 0)
        {  
            Shoot (_viewDirection);           
        }
        
        else if(Time.time > nextFire && fireRate > 0)
        {                  
            nextFire = Time.time + fireRate;
            Shoot (_viewDirection);
        }
        
             
    }

    void Shoot(Vector3 _viewDirection){

        if (TotalAmmo<=0 && currAmmo<=0)
            return; ///add clicking sound 

        currAmmo--;
        

        RaycastHit hit; 
       if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                
                _hit.collider.GetComponent<Player>().TakeDamage(damage, transform.position);
            }
        }

        Debug.Log("Shooting");
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("reloading ..");

        yield return new WaitForSeconds(reloadTime);

        if (TotalAmmo >= maxAmmoInMagazine){
            TotalAmmo = TotalAmmo - maxAmmoInMagazine + currAmmo;

            currAmmo = maxAmmoInMagazine;
        }else{
            currAmmo = TotalAmmo;
            TotalAmmo = 0;
        }

        isReloading = false;
    }
}
