using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : PlaceableObject
{
    
    // Vectors
    private Vector3 movementVector = Vector2.zero;
    private Vector2 lookVector = Vector2.zero;
    private Vector3 jumpVector = Vector3.zero;
    
    // Variables
    // -General
    public Camera playerCamera;
    private PhysicsEquation physicsEquation;
    private float xRotation = 0f;
    private CharacterController controller;
    private bool isGrounded;
    private float playerJumpVelocity = 0f;
    
    // Editable Variables
    // -Camera
    private float xSensitivity = Constants.X_SENSITIVITY;
    private float ySensitivity = Constants.Y_SENSITIVITY;
    
    // -Movement
    private float jumpHeight = Constants.JUMP_HEIGHT;
    private float timeToVertex = Constants.TIME_TO_VERTEX;
    private int maxJumps = Constants.MAX_JUMPS;
    private int jumpsLeft;
    private float playerSpeed = Constants.PLAYER_SPEED;

    private bool hasInstantiated = false;
    private bool isActive = true;

    private Action onHitEnemyAction;

    // Update is called once per frame
    void Update()
    {
        if (!hasInstantiated)
        {
            Instantiate();
        }

        if (isActive)
        {
            runGameLogic();
        }
        
        // Jumping Logic
        jumpVector = new Vector3(0, physicsEquation.GetCurentVelocity(Time.deltaTime), 0);
        
        if (isGrounded && jumpVector.y < 0)
        {
            physicsEquation.Stop();
            jumpsLeft = maxJumps;
            jumpVector.y = 0;
        }

        if (!isGrounded && jumpsLeft == maxJumps && !physicsEquation.isStarted)
        {
            physicsEquation.Start(0);
            physicsEquation.velocity = 0;
        }

        controller.Move(transform.TransformDirection(jumpVector) * Time.deltaTime);
        physicsEquation.UpdateLastTime(Time.deltaTime);
    }

    private void Instantiate()
    {
        jumpsLeft = maxJumps;
        physicsEquation = new PhysicsEquation(jumpHeight, timeToVertex, 0);
        controller = GetComponent<CharacterController>();
        playerJumpVelocity = (float) physicsEquation.velocity;
            
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(GetXSize(), GetYSize(), GetZSize());
        
        boxCollider.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        hasInstantiated = true;
    }

    private void runGameLogic()
    {
        isGrounded = controller.isGrounded;
        
        // Movement of player
        controller.Move(transform.TransformDirection(movementVector) * Time.deltaTime * playerSpeed);
        
        // Camera movement
        xRotation -= (lookVector.y * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (lookVector.x * Time.deltaTime) * xSensitivity);
    }

    private void Jump()
    {
        if (jumpsLeft >= 1)
        {
            physicsEquation.Start(transform.position.y);
            physicsEquation.velocity = playerJumpVelocity;
            jumpsLeft--;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 temp = context.ReadValue<Vector2>();
        movementVector = new Vector3(temp.x, 0, temp.y);

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookVector = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            Jump();
        }
    }

    public void SetIsActive(bool isActive)
    {
        this.isActive = isActive;

        if (!isActive && controller != null)
        {
            controller.Move(Vector3.zero * Time.deltaTime * playerSpeed);
        }
    }

    public void SetPosition(Vector3 position)
    {
        if (isActive)
        {
            transform.position = position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && onHitEnemyAction != null)
        {
            onHitEnemyAction();
        }
    }

    public void SetOnHitEnemyAction(Action action)
    {
        onHitEnemyAction = action;
    }
}
