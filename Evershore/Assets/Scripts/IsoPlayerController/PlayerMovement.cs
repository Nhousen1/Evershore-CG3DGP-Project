using Cinemachine;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/20/2025
 * Summary: handles player movement inputs, sets animator controller vars, and defines isometric coordinate system.
 */
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float runSpeedMultiplier;
    public float gravity = -9.8f;
    public float jumpHeight;

    private Vector2 moveInput;
    private bool isRunning;

    [Header("Camera Configuration")]
    [SerializeField]
    private CinemachineVirtualCamera isoCam;
    private Vector3 isoForward;
    private Vector3 isoRight;

    private CharacterController cc;

    [Header("Input Flags")]
    [SerializeField]
    private bool canMove;

    [Header("Animator and particles")]
    [SerializeField]
    private Animator animator;//TODO: should the animator really be handled here?
    [SerializeField]
    private ParticleSystem dustTrail;
    [Header("Audio")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField, Tooltip("Multiplier applied to the walk interval while running (lower = faster).")] private float runStepIntervalMultiplier = 0.65f;

    private Vector3 velocity;
    private float footstepTimer;
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
            //Define camera isometric coordinate system
            //Notice how right and forward are switched here to map to the keyboard and screen
            isoForward = Vector3.ProjectOnPlane(isoCam.transform.right, Vector3.up).normalized;
            isoRight = Vector3.ProjectOnPlane(isoCam.transform.forward, Vector3.up).normalized;
        }
        else
        {
            Debug.LogError("Please define virtual camera component for player movement.");
        }
    }

    void Update()
    {
        float speed = walkSpeed * (isRunning ? runSpeedMultiplier : 1f);

        Vector3 move = isoForward * moveInput.x + isoRight * moveInput.y;
        Vector3 moveAdjusted = new Vector3(move.x * speed * Time.deltaTime, 0, move.z * speed * Time.deltaTime);
        cc.Move(moveAdjusted);

        //Apply a downward velocity to keep the player grounded (resets to this value when grounded), and then apply gravity
        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        //The players feet need to always look like they are moving in the desired direciton, but the players head should always face the aiming direction.
        Vector2 moveDirection = moveInput;
        float lookAngle = GetComponent<PointAim>().target.transform.eulerAngles.y;

        Vector2 move2d = new Vector2(move.x, move.z);
        Vector2 forwardRef = new Vector2(Vector3.forward.x, Vector3.forward.z);
        float moveAngle = Mathf.Repeat(Vector2.SignedAngle(move2d, forwardRef), 360f);

        float animatorBlendAngle = Mathf.Repeat(360 - (lookAngle - moveAngle), 360f); //need the animator to visually "counteract" the transform rotation.

        animator.SetFloat("MoveBlendAngle", animatorBlendAngle);
        animator.SetBool("IsRunning", (moveInput.magnitude > 0)); //TODO handle walking

        //dustTrail.Play();
        if (moveInput.magnitude > 0)
        {
            var emission = dustTrail.emission;
            emission.rateOverTime = 5 * (isRunning ? runSpeedMultiplier : 1f);
            if (!dustTrail.isEmitting)
            {
                dustTrail.Play();
            }
        }
        else if (dustTrail.isPlaying) 
        {
            dustTrail.Stop();
        }

        HandleFootsteps(Time.deltaTime);
    }
    public void stopInputMovement()
    {
        //Potentially useful in the future for cutscenes, knockback, or anything that freezes player
        canMove = false;
        if (dustTrail.isEmitting)
        {
            dustTrail.Stop();
        }
    }
    public void unstopInputMovement()
    {
        canMove = true;
    }

    private void HandleFootsteps(float deltaTime)
    {
        if (!footstepSource || !footstepClip)
        {
            return;
        }

        bool shouldStep = canMove && cc.isGrounded && moveInput.magnitude > 0.1f;
        if (!shouldStep)
        {
            footstepTimer = 0f;
            return;
        }

        float interval = Mathf.Max(0.05f, walkStepInterval);
        if (isRunning)
        {
            interval *= Mathf.Clamp(runStepIntervalMultiplier, 0.1f, 1f);
        }

        footstepTimer -= deltaTime;
        if (footstepTimer <= 0f)
        {
            footstepSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            footstepSource.PlayOneShot(footstepClip);
            footstepTimer = interval;
        }
    }
}
