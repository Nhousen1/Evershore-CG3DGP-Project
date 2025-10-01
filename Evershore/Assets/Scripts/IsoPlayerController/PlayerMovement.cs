using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float runSpeed;
    public float walkSpeed;

    private Vector2 moveInput;
    [SerializeField]

    private CinemachineVirtualCamera isoCam;
    private Vector3 isoForward;
    private Vector3 isoRight;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    public void Start()
    {
        //UNDER THIS SETUP, THE CAMREA SHOULD NEVER ROTATE
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

    // Update is called once per frame
    void Update()
    {
        Vector3 move = isoForward * moveInput.x + isoRight * moveInput.y;

        transform.Translate(move * walkSpeed * Time.deltaTime);
    }
}
