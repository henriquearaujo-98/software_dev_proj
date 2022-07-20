using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    private CharacterController controller;
    [SerializeField] Transform shootOrigin;
    [SerializeField] Transform orientation;
    
    public float gravity = -9.81f;
    public float currentMoveSpeed = 5f;
    public float sprintSpeed = 8.5f;
    public float originalMoveSpeed = 5f;
    public float jumpSpeed = 5f;
    private float yVelocity = 0;
    public float health;
    public float maxHealth = 100f;
    
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public float crouchScale = 0.1f;
    private Vector3 originalScale;
    
    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    public bool sliding;

    public bool[] inputs;
    
    [Header("Wall Running")]
    public float groundDrag;
    public float wallRunSpeed;
    
    public MovementState state;
    public enum MovementState
    {
        walking,
        wallrunning,
        sprinting,
        crouching,
        air
    }

    public bool wallrunning;

    private void Start()
    {
        controller = this.GetComponent<CharacterController>();

        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        currentMoveSpeed *= Time.fixedDeltaTime;
        originalMoveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        crouchSpeed *= Time.fixedDeltaTime;
        maxSlideTime *= Time.fixedDeltaTime;

        originalScale = transform.localScale;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[7];
    }

    public void FixedUpdate()
    {
        if (health < 1)
            return;
    
        InputController();
        StateHandler();
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
        if (inputs[5] && !inputs[6])
        {
            Sprint(true);
        }
        else
        {
            Sprint(false);
        }
        if (inputs[6] && !inputs[5])
        {
            PlayerCrouch(true);
            //State Change
            state = MovementState.crouching;
            currentMoveSpeed = crouchSpeed;
        }
        else
        {
            PlayerCrouch(false);
        }
        if (inputs[6] && inputs[5])
        {
            StartSlide(true);
        }
        else
        {
            StartSlide(false);
        }
            
        //PlayerCrouch(inputs[6]);

        Move(_inputDirection);
        
        if (sliding)
            SlidingMovement(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= currentMoveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }

            if (wallrunning)
            {
                
            }
        }
        yVelocity += gravity;
        
        _moveDirection.y = yVelocity;

        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        ServerSend.PlayerScale(this);
    }

    private void PlayerCrouch(bool _input)
    {
        if (_input)
        {
            transform.localScale = new Vector3(originalScale.x, crouchScale, originalScale.z);
        }
        else
        {
            transform.localScale = originalScale;
        }
        //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
    
    #region Sliding
    private void StartSlide(bool _slide)
    {
        if (_slide)
        {
            sliding = true;

            transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
            gameObject.GetComponent<CharacterController>().Move(Vector3.down * 5f);

            slideTimer = maxSlideTime;
        }
        else
        {
            sliding = false;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

    }
    
    private void SlidingMovement(Vector2 _input)
    {
        Vector3 inputDirection = orientation.forward * _input.x + orientation.right * _input.y;

        // sliding normal
        if(gameObject.GetComponent<CharacterController>().velocity.y > -0.1f)
        {
            gameObject.GetComponent<CharacterController>().Move(inputDirection.normalized * slideForce);

            slideTimer -= Time.fixedDeltaTime;
        }

        if (slideTimer <= 0)
            StopSlide();
    }
    
    private void StopSlide()
    {
        sliding = false;

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
    #endregion
    private void Sprint(bool sprint)
    {
        if (sprint)
        {
            currentMoveSpeed = sprintSpeed; //TODO Switch with a variable and add it to the state system
        }
        else
        {
            currentMoveSpeed = originalMoveSpeed;
        }
    }
    
    private void StateHandler()
    {
        //Mode - Wall Running
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            currentMoveSpeed = wallRunSpeed;
        }
        // Mode - Crouching
        if (inputs[6] && !inputs[5])
        {
            state = MovementState.crouching;
            currentMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if(controller.isGrounded && inputs[5] && !inputs[6])
        {
            state = MovementState.sprinting;
            currentMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (controller.isGrounded)
        {
            state = MovementState.walking;
            currentMoveSpeed = originalMoveSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {

        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                
                _hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f); //Respawn position
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

}
