using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    //private CharacterController controller;
    [SerializeField] Transform shootOrigin;
    [SerializeField] Transform orientation;
    
    //public float gravity = -9.81f;
    public float currentMoveSpeed = 5f;
    //public float sprintSpeed = 8.5f;
    public float originalMoveSpeed = 5f;
    public float jumpSpeed = 5f;
    private float yVelocity = 0;
    public float health;
    public float maxHealth = 100f;
    
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public Vector2 _inputDirection;
    
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public float crouchScale = 0.1f;
    private Vector3 originalScale;
    public bool crouching;
    
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;
    
    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    public float slideTimer;

    public float slideYScale;
    public bool sliding;
    public bool[] inputs;
    
    [Header("Wall Running")]
    public float groundDrag;
    public float wallRunSpeed;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;
    
    public MovementState state;

    private Rigidbody rb;
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
        //controller = this.GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        //gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        walkSpeed *= Time.fixedDeltaTime;
        currentMoveSpeed *= Time.fixedDeltaTime;
        //originalMoveSpeed *= Time.fixedDeltaTime;
        //moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        jumpForce *= Time.fixedDeltaTime;
        //jumpCooldown *= Time.fixedDeltaTime;
        airMultiplier *= Time.deltaTime;
        //sprintSpeed *= Time.fixedDeltaTime;
        //crouchSpeed *= Time.fixedDeltaTime;
        //maxSlideTime *= Time.fixedDeltaTime;

        originalScale = transform.localScale;
        readyToJump = true;
        
        startYScale = transform.localScale.y;

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
        
        
        
        grounded = Physics.Raycast(transform.position, Vector3.down, 1.2f, whatIsGround);

        InputController();
        StateHandler();
        SpeedControl();
        
        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void InputController()
    {
        _inputDirection = Vector2.zero;
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
        if (inputs[4] && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        } 
        if (inputs[5] && !inputs[6])
        {
            Sprint(true);
            state = MovementState.sprinting;
        }
        else
        {
            Sprint(false);
        }
        if (inputs[6])
        {
            Debug.Log("I crouch");
            PlayerCrouch(true);
            crouching = true;
            //State Change
            state = MovementState.crouching;
            //currentMoveSpeed = crouchSpeed;
        }
        else
        {
            PlayerCrouch(false);
            crouching = false;
        }
        if (inputs[6] && inputs[5])
        {
            //StartSlide(true);
        }
        else
        {
            //StartSlide(false);
        }
            
        //PlayerCrouch(inputs[6]);
        
        MovePlayer(_inputDirection);
        
        if (sliding)
            SlidingMovement(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= currentMoveSpeed;
        
        
        _moveDirection.y = yVelocity;

        //controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        ServerSend.PlayerScale(this);
    }
    
    private void MovePlayer(Vector2 _inputDirection)
    {
        
        // calculate movement direction
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        //moveDirection = orientation.forward * _inputDirection.x + orientation.right * _inputDirection.y;;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(_moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if(grounded)
            rb.AddForce(_moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(_moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
        
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        ServerSend.PlayerScale(this);
    }
    
    private void Jump()
    {
        exitingSlope = true;
        
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce * 5, ForceMode.Impulse);
    }
    
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    private void PlayerCrouch(bool _input)
    {
        if (_input)
        {
            Debug.Log("Yee");
            transform.localScale = new Vector3(originalScale.x, crouchScale, originalScale.z);
            //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
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

            transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            slideTimer = maxSlideTime;
        }
        else
        {
            sliding = false;
            if (!crouching)
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }

    }
    
    private void SlidingMovement(Vector2 _input)
    {
        //Vector3 inputDirection = orientation.forward * _input.x + orientation.right * _input.y;

        // sliding normal
        //if(gameObject.GetComponent<CharacterController>().velocity.y > -0.1f)
        //{
        //    gameObject.GetComponent<CharacterController>().Move(inputDirection.normalized * slideForce);
        //
        //    slideTimer -= Time.fixedDeltaTime;
        //}

        //if (slideTimer <= 0)
        //    StopSlide();
        //---------------------------
        
        Vector3 inputDirection = orientation.forward * _input.x + orientation.right * _input.y;
        // sliding normal
        if(!OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
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
            //currentMoveSpeed = sprintSpeed; //TODO Switch with a variable and add it to the state system
            moveSpeed = sprintSpeed;
        }
        else
        {
            //currentMoveSpeed = originalMoveSpeed;
            moveSpeed = originalMoveSpeed;
        }
    }
    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
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
            //currentMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if(grounded && inputs[5] && !inputs[6])
        {
            state = MovementState.sprinting;
            //currentMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
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
            //TODO
            
            health = 0f;
            //controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f); //Respawn position
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        //TODO
        
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        //controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

}
