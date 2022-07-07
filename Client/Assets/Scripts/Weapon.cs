using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string name;
    public int ammo;
    public int maxAmmo = 200;
    public float cooldownSeconds = 4;
    public float cooldown;
    public float fireRate;

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
        
    }

    // Update is called once per frame
    void Update()
    {       
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
                   ammo--; 
                
                  
                   if(cooldown > Time.time)
                   {   
                      cooldown = Time.time + cooldownSeconds;
                   }
                
            }
        } 
             
    }

    void Shoot(){
        MuzzleFlash.Play();
        
        ClientSend.PlayerShoot(shootOrigin.forward);

        RaycastHit hit; 
        if (Physics.Raycast(shootOrigin.position, shootOrigin.forward, out hit, 250f))
        {
            Instantiate(ImpactPoint, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}
