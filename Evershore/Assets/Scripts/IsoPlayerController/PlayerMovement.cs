using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed;
    public float runSpeedMultiplier;
    public float gravity = -9.8f;
    public float jumpHeight;

    private Vector2 moveInput;
    private bool isRunning;

    [SerializeField]
    private CinemachineVirtualCamera isoCam;
    private Vector3 isoForward;
    private Vector3 isoRight;

    private CharacterController cc;

    [SerializeField]
    private bool canMove;

    private Vector3 velocity;
    public void OnMove(InputValue value)
    {
        if (canMove)
        {
            moveInput = value.Get<Vector2>();
            moveInput.Normalize();
        }
    }
    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }
    public void OnJump(InputValue value)
    {
        //jump conditions
        if (!cc.isGrounded || !canMove) { return; }
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
    public void Start()
    {
        canMove = true;
        cc = GetComponent<CharacterController>();
        //UNDER THIS SETUP, THE CAMERA SHOULD NEVER ROTATE
        if (isoCam != null) 
        {
            //Define camrea isometric coordinate system
            //Notice how right and forward are switched here to map to the keyboard and screen
            isoForward = Vector3.ProjectOnPlane(isoCam.transform.right, Vector3.up).normalized;
            isoRight = Vector3.ProjectOnPlane(isoCam.transform.forward, Vector3.up).normalized;
        }
        else
        {
            Debug.LogError("Please define virtual camrea component for player movement.");
        }
    }

    void Update()
    {
        float speed = walkSpeed * (isRunning ? runSpeedMultiplier : 1f);

        Vector3 move = isoForward * moveInput.x + isoRight * moveInput.y;
        move = new Vector3(move.x * speed * Time.deltaTime, 0, move.z * speed * Time.deltaTime);
        cc.Move(move);

        // gravity
        if (cc.isGrounded && velocity.y < 0)
        {
            // Reset downward velocity when grounded
            velocity.y = -2f;
        }

        // Apply gravity every frame
        velocity.y += gravity * Time.deltaTime;

        cc.Move(velocity * Time.deltaTime);
    }
    public void stopInputMovement()
    {
        canMove = false;
    }
    public void unstopInputMovement()
    {
        canMove = true;
    }
}
