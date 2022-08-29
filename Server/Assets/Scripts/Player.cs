using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    private CharacterController controller;
    [SerializeField] Transform shootOrigin;

    public float gravity = -9.81f;
    float moveSpeed = 5f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 3f;
    public float jumpSpeed = 5f;
    private float yVelocity = 0;
    public float health;
    public float maxHealth = 100f;
    bool isCrouching = false;


    public bool[] inputs;

    public WeaponSwitching weaponSwitching;

    [Header("Player stats")]
    public int deaths;
    public int kills;


    [SerializeField] Animator anim;

    private void Start()
    {
        controller = this.GetComponent<CharacterController>();
        

        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        walkSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        crouchSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = username == "" ? "Player"+id : _username;
        health = maxHealth;

        deaths = 0;
        kills = 0;
        inputs = new bool[9];

    }

    public void FixedUpdate()
    {
        if (health < 1)
            return;

        AnimationHandler();

        weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().getButton = inputs[7];


        if(inputs[8]){
            StartCoroutine(weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().Reload());
        }

        
        InputController();


        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        ServerSend.PlayerInputs(this, inputs);

    }

    void AnimationHandler()
    {
        anim.SetBool("Forward", inputs[0]);

        anim.SetBool("Backwards", inputs[1]);


        anim.SetBool("Left", inputs[2]);

        anim.SetBool("Right", inputs[3]);

        if (inputs[0] || inputs[1] || inputs[2] || inputs[3])
        {
            if (inputs[5])
            {
                anim.SetBool("Run", true);
                weaponSwitching.currentWeapon.GetComponent<Weapon>().isRunning = true;
            }
            else
            {
                anim.SetBool("Run", false);
                weaponSwitching.currentWeapon.GetComponent<Weapon>().isRunning = false;
            }   
        }

        if (inputs[7] && isCrouching == false)
            anim.Play("Shoot", 0, 0f);

        //anim.SetBool("Grounded", controller.isGrounded);
    }

    private void InputController()
    {
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        CrouchToggle(inputs[6]);

        SprintToggle(inputs[5]);

        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        
    }

    void SprintToggle(bool _keydown)
    {
        if (isCrouching)
            return;

        if (_keydown)
        {
            moveSpeed = sprintSpeed;
            
        }
        else
        {
            moveSpeed = walkSpeed;
        }

        weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().canShoot = !_keydown;
    }

    void CrouchToggle(bool _keydown)
    {
        isCrouching = _keydown;
        if (_keydown)
        {
            anim.SetBool("Crouch", true);
            moveSpeed = crouchSpeed;
        }
        else
        {
            anim.SetBool("Crouch", false);
            moveSpeed = walkSpeed;
        }
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().viewDirection = _viewDirection;
        weaponSwitching.currentWeapon.gameObject.GetComponent<Weapon>().shootOrigin = shootOrigin;
    }

    /// <summary>
    /// Used to take away health, based on weapon damage
    /// </summary>
    /// <param name="_damage"></param>
    /// <param name="_fromPlayer"></param>
    /// <param name="weapon"></param>
    /// <returns>Player health after taking damage</returns>
    public float TakeDamage(float _damage, Player _fromPlayer, Weapon weapon)
    {
        if (health <= 0f)
        {
            return health;
        }

        health -= _damage;
        if (health <= 0f)
        {
            Die();
            KillFeed(_fromPlayer, weapon);
        }

        ServerSend.PlayerHealth(this);
        ServerSend.DamageIndicator(this, _fromPlayer);

        return health;

    }

    private void KillFeed(Player _fromPlayer, Weapon _weapon)
    {
        Debug.Log($"{_fromPlayer.username} [{_weapon.name}] {this.username}");
        _fromPlayer.kills++;
        this.deaths++;
        ServerSend.KillFeed(_fromPlayer, _weapon, this);
    }

    private void Die()
    {
        health = 0f;
        controller.enabled = false;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0.5f);

        transform.position = NetworkManager.instance.spawnSystem.GetNewSpawnPosition(); //Respawn position
        ServerSend.PlayerPosition(this);
        

        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

}
