using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Global")]
    [SerializeField] Player owner;
    public int id;
    public Transform shootOrigin;
    public int damage;

    [Header("Ammunition")]
    public int maxAmmoInMagazine;
    public int currAmmo;
    public int TotalAmmo;



    [Header("Fire Rate")]
    public float fireRate;
    public float reloadTime = 1f;
    private float nextFire;
    

    [Header("Reload")]
    public bool canShoot;
    

    [Header("States")]
    bool automaticControl = true;
    public bool isReloading = false;
    public bool isRunning = false;




    [Header("Networking")]
    public bool getButton; // for automatic fire
    public Vector3 viewDirection;

    


    

    

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmoInMagazine;
    }

    private void Update()
    {
        if (isReloading)
            return;

        if (canShoot == false)
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

        if (isRunning)
            return;

        currAmmo--;
        

       if (Physics.Raycast(shootOrigin.position, viewDirection, out RaycastHit _hit, 25f, LayerMask.GetMask("Hitbox")))
        {

            HitboxHandler HH = _hit.collider.GetComponent<HitboxHandler>();

            if (HH.playerScript.id == owner.id)
                return;

            float damageMultiplier = HH.damageMultiplier;

            float other_health = HH.playerScript.TakeDamage(damage * damageMultiplier, owner, this);
            
            if (other_health <= 0 && HH.type == Hitbox.Head)
                ServerSend.KillNotification(owner);

            Debug.Log(HH.name);
            
        }

        Debug.DrawRay(shootOrigin.position, viewDirection, Color.red, 2f);

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
