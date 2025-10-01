using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed;
    public float runSpeedMultiplier;

    private Vector2 moveInput;
    private bool isRunning;

    [SerializeField]
    private CinemachineVirtualCamera isoCam;
    private Vector3 isoForward;
    private Vector3 isoRight;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        moveInput.Normalize();
    }
    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }
    public void Start()
    {
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
        Vector3 move = isoForward * moveInput.x + isoRight * moveInput.y;
        float speed = walkSpeed * (isRunning ? runSpeedMultiplier : 1f);
        transform.Translate(move * speed * Time.deltaTime, Space.World);
    }
}
