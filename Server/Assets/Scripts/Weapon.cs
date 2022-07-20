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
    public bool isReloading = false;
    public bool fire = false;
    public int damage;

    public float nextFire;
    public ParticleSystem ImpactPoint; 

    public Transform shootOrigin;
    public ParticleSystem MuzzleFlash;

    public bool getButton; // for automatic fire
    public Vector3 viewDirection;

    bool automaticControl = true;


    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmoInMagazine;
    }

    private void Update()
    {
        if (isReloading)
            return;

        if (currAmmo <= 0 && TotalAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (fireRate == 0 && getButton)
        {
            if (automaticControl)
            {
                automaticControl = false;
                Shoot();
            }
            
        }

        if(getButton == false)
        {
            automaticControl = true;
        }

        else if (getButton && Time.fixedTime > nextFire && fireRate > 0)
        {
            nextFire = Time.fixedTime + fireRate;
            Shoot();
        }
    }

    void Shoot(){

        if (TotalAmmo<=0 && currAmmo<=0)
            return; ///add clicking sound 

        currAmmo--;
        

       if (Physics.Raycast(shootOrigin.position, viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                
                _hit.collider.GetComponent<Player>().TakeDamage(damage, transform.position);
            }
        }

        Debug.DrawRay(shootOrigin.position, viewDirection, Color.green, 2f);

    }

    public IEnumerator Reload()
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
