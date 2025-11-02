using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

/**
* Author: Liam Housenbold
* Date Created: 9-30-2025
* Date Modified: 10-1-2025
* Summary: A finite state machine (FSM) for Ranged enemy AI behavior. has a out of combat patrol state, chase player state, and attack state.
* The enemy uses a NavMeshAgent for movement and pathfinding.
*/

public class EnemyRangedFSM : MonoBehaviour
{
    // Different states the enemy can be in
    public enum EnemyState { OutofCombat, ShootPlayer, ChasePlayer }
    public EnemyState currentState;
    private Animator animator;

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
        /*
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Deep-clone the controller so this enemy’s animation state machine is unique
            RuntimeAnimatorController originalController = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = Instantiate(originalController);

            // Randomize animator time offset to break global sync
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, Random.Range(0f, 1f));
        }
        */

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
        /*
        if (animator != null)
            animator.speed = Random.Range(0.9f, 1.1f);
            */
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerShootDistance);
    }

    void Update()
    {
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

/*
        if (animator != null && agent != null)
            animator.speed = agent.velocity.magnitude / agent.speed;
            */

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
                Vector3 nextTarget = patrolPoints[patrolIndex];
                UnityEngine.AI.NavMeshHit hit;
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
            Vector3 startTarget = patrolPoints[patrolIndex];
            UnityEngine.AI.NavMeshHit hit;
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
            Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            shootEffect.Play();
        }
    }

    void LookTo(Vector3 targetPosition)
    {
        Vector3 directionToPosition = Vector3.Normalize(targetPosition - transform.parent.position);
        directionToPosition.y = 0;
        transform.parent.forward = directionToPosition;
    }
    
    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
