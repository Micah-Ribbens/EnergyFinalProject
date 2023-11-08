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
    private float xSensitivity = 60f;
    private float ySensitivity = 60f;
    
    // -Movement
    private float jumpHeight = 4f;
    private float timeToVertex = .6f;
    private int maxJumps = 2;
    private int jumpsLeft;
    private float playerSpeed = 10f;

    private bool k = false;

    // Update is called once per frame
    void Update()
    {
        if (!k)
        {
            jumpsLeft = maxJumps;
            physicsEquation = new PhysicsEquation(jumpHeight, timeToVertex, 0);
            controller = GetComponent<CharacterController>();
            playerJumpVelocity = (float) physicsEquation.velocity;
            
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(GetXSize(), GetYSize(), GetZSize());
        
            // Debug.Log("y " + GetYSize());
            // Debug.Log("x " + GetXSize());
            // Debug.Log("z " + GetZSize());
        
            boxCollider.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            k = true;
        }
        isGrounded = controller.isGrounded;
        
        // Movement of player
        controller.Move(transform.TransformDirection(movementVector) * Time.deltaTime * playerSpeed);
        
        // Camera movement
        xRotation -= (lookVector.y * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (lookVector.x * Time.deltaTime) * xSensitivity);
        
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

    public void Jump()
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
}
