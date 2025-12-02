using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

/**
* Author: Liam Housenbold
* Date Created: 9-30-2025
* Date Modified: 12-1-2025
* Summary: A finite state machine (FSM) for Ranged enemy AI behavior. has a out of combat patrol state, chase player state, and attack state.
* The enemy uses a NavMeshAgent for movement and pathfinding.
*/

public class EnemyRangedFSM : MonoBehaviour
{
    // Different states the enemy can be in
    public enum EnemyState { OutofCombat, ShootPlayer, ChasePlayer }
    public EnemyState currentState;
    
    // Animator & movement → animation glue
    [Header("Animation")]
    [SerializeField] private Animator animator;          // assigned in inspector or auto-found
    private float baseAnimatorSpeed = 1.0f;              // randomized per enemy
    
    [Header("Out of Combat Patrol Settings")]
    // Simple patrol (two-point) settings — computed from the enemy's starting position
    public float patrolRadius = 4f;
    public float patrolPointTolerance = 0.5f;
    public float patrolPauseTime = 1.5f;
    private Vector3[] patrolPoints = new Vector3[2];
    private int patrolIndex = 0;
    private float patrolTimer = 0f;
    private Vector3 initialPosition;

    [Header("Enemy Trigger Distance Settings")]
    // distance settings
    public float playerShootDistance;
    public float chaseStopDistance;

    // Material Settings
    private SkinnedMeshRenderer enemyRenderer;
    private Material runtimeMaterial;

    [Header("Shoot Settings")]
    public float lastShootTime;
    public float fireRate;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip; 

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepClips; 
    [SerializeField] private float footstepInterval = 0.45f;
    private float footstepTimer = 0f;
    private int footstepIndex = 0;   

    [Header("Refrences")]
    public GameObject projectilePrefab;
    [SerializeField] private EnemyLife enemyLife;
    [SerializeField] private EnemySight sightSensor;
    private UnityEngine.AI.NavMeshAgent agent;
    [SerializeField] private Transform shootPoint;
    public ParticleSystem shootEffect;

    private float originalSightAngle = -1f;

    private void Awake()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();

        // --- Animator setup (similar pattern as melee FSM) ---
        // Try serialized reference first. If not set, auto-find.
        if (animator == null)
        {
            // 1) Check this object & its children
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            // 2) Check parent & its children (to catch Skeleton sibling case)
            if (animator == null && transform.parent != null)
                animator = transform.parent.GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("EnemyRangedFSM: Animator not found! Make sure to assign the Skeleton's Animator.", this);
        }
        else
        {
            // Deep-clone the controller so this enemy’s animation state machine is unique
            RuntimeAnimatorController originalController = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = Instantiate(originalController);

            // Randomize animator time offset to break global sync
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, Random.Range(0f, 1f));
        }
        // ------------------------------------------------------

        // record starting position and initialize patrol points
        initialPosition = transform.position;
        // Use the enemy's local right direction for patrol points
        Vector3 right = transform.right;
        // Generate two patrol points offset by patrolRadius to the left and right
        patrolPoints[0] = initialPosition + right * patrolRadius;
        patrolPoints[1] = initialPosition - right * patrolRadius;
        currentState = EnemyState.OutofCombat;
    }

    private void Start()
    {
        // Run after Unity finishes prefab instantiation
        enemyRenderer = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (enemyRenderer != null)
        {
            // Create a unique material instance just for this enemy
            runtimeMaterial = Instantiate(enemyRenderer.sharedMaterial);
            enemyRenderer.material = runtimeMaterial;
        }

        // Randomize base animator speed slightly per enemy (like melee FSM)
        if (animator != null)
        {
            baseAnimatorSpeed = Random.Range(0.9f, 1.1f);
            animator.speed = baseAnimatorSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerShootDistance);
    }

    void Update()
    {
        // --- FOOTSTEP HANDLING ---
        if (animator != null && agent != null)
        {
            bool moving = !agent.isStopped && agent.velocity.magnitude > 0.15f;

            if (moving)
            {
                footstepInterval = Mathf.Lerp(1.05f, 0.55f, agent.velocity.magnitude / agent.speed);
                footstepTimer += Time.deltaTime;

                if (footstepTimer >= footstepInterval)
                {
                    footstepTimer = 0f;

                    if (audioSource != null && footstepClips != null && footstepClips.Length > 0)
                    {
                        // pick clip in sequence
                        AudioClip clip = footstepClips[footstepIndex];

                        // play it quietly, with slight pitch variation
                        audioSource.pitch = Random.Range(0.9f, 1.1f);
                        float vol = Random.Range(0.05f, 0.10f);
                        audioSource.PlayOneShot(clip, vol);

                        // move to next clip (looping)
                        footstepIndex = (footstepIndex + 1) % footstepClips.Length;
                    }
                }
            }
            else
            {
                // reset timer + clip cycle when stopping
                footstepTimer = 0f;
                footstepIndex = 0;
            }
        }

        if (sightSensor != null)
        {
            // Cache the original angle if not already stored
            if (originalSightAngle < 0f)
                originalSightAngle = sightSensor.angle;

            // Expand vision to 360° while not patrolling
            if (currentState != EnemyState.OutofCombat)
                sightSensor.angle = 360f;
            else
                sightSensor.angle = originalSightAngle;
        }

        // Drive Animator locomotion parameters globally from NavMeshAgent
        if (animator != null && agent != null)
        {
            float normalizedSpeed = 0f;
            if (agent.speed > 0.01f)
                normalizedSpeed = agent.velocity.magnitude / agent.speed; // 0 = idle, 1 ≈ full speed

            animator.SetFloat("MoveSpeed", normalizedSpeed);
            bool isMoving = !agent.isStopped && normalizedSpeed > 0.05f;
            animator.SetBool("isMoving", isMoving);
        }

        if (currentState == EnemyState.OutofCombat)
        {
            OutofCombat();
        }
        else if (currentState == EnemyState.ChasePlayer)
        {
            ChasePlayer();
        }
        else if (currentState == EnemyState.ShootPlayer)
        {
            ShootPlayer();
        }
    }

    void OutofCombat()
    {
        /*
        // Animation handling
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
        */

        // Player detection
        if (sightSensor != null && sightSensor.detectedObject != null)
        {
            currentState = EnemyState.ChasePlayer;
            return;
        }

        // If close enough to current patrol point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + patrolPointTolerance)
        {
            patrolTimer += Time.deltaTime;
            agent.isStopped = true;

            // wait at patrol point
            if (patrolTimer >= patrolPauseTime)
            {
                // choose next patrol point
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                patrolTimer = 0f;

                // find a valid NavMesh point
                UnityEngine.AI.NavMeshHit hit;
                Vector3 nextTarget = patrolPoints[patrolIndex];
                if (UnityEngine.AI.NavMesh.SamplePosition(nextTarget, out hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
                    nextTarget = hit.position;

                // resume movement to next patrol point
                agent.isStopped = false;
                agent.SetDestination(nextTarget);
            }
        }
        else if (!agent.hasPath)
        {
            // Agent has no destination at all
            UnityEngine.AI.NavMeshHit hit;
            Vector3 startTarget = patrolPoints[patrolIndex];
            if (UnityEngine.AI.NavMesh.SamplePosition(startTarget, out hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
                startTarget = hit.position;

            agent.isStopped = false;
            agent.SetDestination(startTarget);
        }
    }

    void ChasePlayer()
    {
        /*
        if (animator != null)
            animator.SetBool("isMoving", true);
        */

        if (agent == null)
            agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();

        // if player out of sight, return to patrol
        if (sightSensor == null || sightSensor.detectedObject == null)
        {
            currentState = EnemyState.OutofCombat;
            return;
        }

        var target = sightSensor.detectedObject;
        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        float distanceToPlayer = Vector3.Distance(agentPos, target.transform.position);
        LookTo(target.transform.position);

        // otherwise continue chasing using NavMeshAgent
        if (agent != null)
        {
            if (distanceToPlayer > chaseStopDistance)
            {
                agent.isStopped = false;
                SetNavDestination(target.transform.position);
            }
            else
            {
                agent.isStopped = true;
                LookTo(target.transform.position);
            }
        }

        if (distanceToPlayer <= playerShootDistance)
        {
            currentState = EnemyState.ShootPlayer;
        }

    }

    void SetNavDestination(Vector3 desiredPoint)
    {
        if (agent == null)
        {
            agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
            if (agent == null)
                return;
        }

        UnityEngine.AI.NavMeshHit hit;
        Vector3 targetPoint = desiredPoint;
        if (UnityEngine.AI.NavMesh.SamplePosition(desiredPoint, out hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            targetPoint = hit.position;
        }

        agent.isStopped = false;
        agent.SetDestination(targetPoint);
    }

    void ShootPlayer()
    {
        agent.isStopped = true;

        if (sightSensor.detectedObject == null)
        {
            currentState = EnemyState.OutofCombat;
            return;
        }

        LookTo(sightSensor.detectedObject.transform.position);
        Shoot();

        float distanceToPlayer = Vector3.Distance(transform.position, sightSensor.detectedObject.transform.position);

        if (distanceToPlayer > playerShootDistance * 1.1f)
        {
            currentState = EnemyState.ChasePlayer;
        }
    }

    void Shoot()
    {
        var timeSinceLastShoot = Time.time - lastShootTime;
        if (timeSinceLastShoot > fireRate)
        {
            lastShootTime = Time.time;

            // --- Trigger attack animation when actually firing ---
            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
            }
            // -----------------------------------------------------

            // Delay projectile spawn by 0.5 seconds
            StartCoroutine(DelayedProjectile());
        }
    }

    void LookTo(Vector3 targetPosition)
    {
        Vector3 directionToPosition = Vector3.Normalize(targetPosition - transform.parent.position);
        directionToPosition.y = 0;
        transform.parent.forward = directionToPosition;
    }

    private IEnumerator DelayedProjectile()
    {
        yield return new WaitForSeconds(0.5f);

        Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        shootEffect.Play();
        // play shoot sound here
        if (audioSource != null && shootClip != null)
        {
            audioSource.pitch = Random.Range(0.97f, 1.03f);
            audioSource.volume = 0.3f;
            audioSource.PlayOneShot(shootClip);
        }
    }
    
    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}

