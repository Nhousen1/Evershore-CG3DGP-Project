using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

/**
* Author: Liam Housenbold
* Date Created: 9-25-2025
* Date Modified: 10-15-2025
* Summary: A finite state machine (FSM) for melee enemy AI behavior. has a out of combat patrol state, chase player state, charge attack state, sword swing attack state, and defense stance state.
* The enemy uses a NavMeshAgent for movement and pathfinding.
*/

public class EnemyFSM : MonoBehaviour
{
    // Different states the enemy can be in
    public enum EnemyState { OutofCombat, SwingSword, Charge, DefenseStance, ChasePlayer }
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
    public float playerSwingDistance;
    public float chaseStopDistance;
    public float playerChargeDistance;


    // Charge settings
    [Header("Enemy Charge Move Settings")]
    public float chargeWindup = 0.5f; // seconds to aim before charging
    public float chargeSpeed = 8f; // movement speed while charging
    public float maxChargeDistance = 8f; // maximum distance to travel during charge
    private float chargeTimer = 0f;
    private bool isCharging = false;
    private Vector3 chargeDirection = Vector3.zero;
    private Vector3 chargeStartPos = Vector3.zero;
    private Vector3 chargeTarget = Vector3.zero;
    private float originalAgentSpeed = 0f;

    // Defense params
    [Header("Enemy Defense Stance Settings")]
    public float defenseDuration;
    private bool isDefending = false;
    private float defenseTimer = 0f;
    private bool defenseInitialized = false;
    public int swingsBeforeDefense = 3;   // ex) 3 swings then defend
    private int swingCounter = 0;         // counts completed swings since last defense
    public float defenseArmorMultiplier = 2.0f; // modify armor while defending
    private float prevArmorAmount = 0f;
    // Material Settings
    private SkinnedMeshRenderer enemyRenderer;
    private Material runtimeMaterial;
    // Pulse effect parameters
    private float pulseSpeed = 15.0f; // how fast the glow pulses
    private float pulseIntensity = 4.0f; // maximum emission brightness
    private bool isPulsing = false;


    [Header("Enemy Sword Swing Settings")]
    // Sword Swing Params
    public float swingCooldown = 1f;
    private float swingTimer = 0f;


    [Header("Refrences")]
    [SerializeField] private SwordHit swordHit;
    [SerializeField] private EnemyLife enemyLife;
    [SerializeField] private EnemySight sightSensor;
    private UnityEngine.AI.NavMeshAgent agent;

    private float originalSightAngle = -1f;

    private void Awake()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        swordHit.OnSwordHit.AddListener(NotifySwordHit); //detect when player is hit on charge
        animator = GetComponent<Animator>();

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
        // Run AFTER Unity finishes prefab instantiation
        enemyRenderer = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (enemyRenderer != null)
        {
            // Create a unique material instance just for this enemy
            runtimeMaterial = Instantiate(enemyRenderer.sharedMaterial);
            enemyRenderer.material = runtimeMaterial;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerChargeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerSwingDistance);

        // Sword hitbox visualization
        if (swordHit != null)
        {
            BoxCollider box = swordHit.GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }

    void Update()
    {
        // Prevent other states from interrupting defense
        if (isDefending)
        {
            DefenseStance();
            return; // skip everything else until defense ends
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

        if (currentState == EnemyState.OutofCombat)
        {
            OutofCombat();
        }
        else if (currentState == EnemyState.ChasePlayer)
        {
            ChasePlayer();
        }
        else if (currentState == EnemyState.Charge)
        {
            Charge();
        }
        else if (currentState == EnemyState.DefenseStance)
        {
            DefenseStance();
        }
        else if (currentState == EnemyState.SwingSword)
        {
            SwingSword();
        }

        // Handle pulsing red glow in Defense Stance
        if (isPulsing && runtimeMaterial != null)
        {
            // Pulse intensity using a sine wave
            float emission = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // oscillates 0–1
            Color pulseColor = Color.red * (emission * pulseIntensity);
            runtimeMaterial.SetColor("_EmissionColor", pulseColor);
            DynamicGI.SetEmissive(enemyRenderer, pulseColor);
        }
    }

    void OutofCombat()
    {
        if (animator != null)
            animator.SetBool("isMoving", true);

        // If a player appears, break patrol and chase
        if (sightSensor != null && sightSensor.detectedObject != null)
        {
            currentState = EnemyState.ChasePlayer;
            return;
        }

        // start moving toward the current patrol point
        float destDiff = float.PositiveInfinity;
        if (agent.hasPath)
            destDiff = Vector3.Distance(agent.destination, patrolPoints[patrolIndex]);

        if (!agent.hasPath || destDiff > 0.1f)
        {
            // if the current path is invalid, reset it and try a reachable nearby point
            if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
            {
                agent.ResetPath();
            }

            // Try to sample a nearby valid NavMesh position in case the exact patrol point is off-mesh
            UnityEngine.AI.NavMeshHit hit;
            Vector3 targetPoint = patrolPoints[patrolIndex];
            if (UnityEngine.AI.NavMesh.SamplePosition(patrolPoints[patrolIndex], out hit, patrolRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                targetPoint = hit.position;
            }

            SetNavDestination(targetPoint);
            // reset timer when we start moving toward a point
            patrolTimer = 0f;
        }

        // use NavMeshAgent's remainingDistance for arrival check
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + patrolPointTolerance)
        {
            // arrived
            if (!agent.isStopped)
                agent.isStopped = true;

            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolPauseTime)
            {
                patrolTimer = 0f;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.isStopped = false;
                // set destination to the next point
                SetNavDestination(patrolPoints[patrolIndex]);
            }
        }
    }

    void ChasePlayer()
    {
        if (animator != null)
            animator.SetBool("isMoving", true);

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

        // If the player is outside the charge range, start a charge
        if (distanceToPlayer > playerChargeDistance)
        {
            // prepare charge: windup/aim then set an agent destination for the charge
            if (agent != null)
                agent.isStopped = true;

            // compute direction from agent position toward player's current position
            chargeDirection = (target.transform.position - agentPos);
            chargeDirection.y = 0;
            chargeDirection = chargeDirection.normalized;
            chargeStartPos = agentPos;
            // compute a target point maxChargeDistance away along the direction
            chargeTarget = agentPos + chargeDirection * maxChargeDistance;
            chargeTimer = 0f;
            isCharging = false;
            currentState = EnemyState.Charge;
            return;
        }

        // otherwise continue chasing using NavMeshAgent, but maintain a minimum chase distance
        if (agent != null)
        {
            // if we're further than the stop distance, move toward the player
            if (distanceToPlayer > chaseStopDistance)
            {
                agent.isStopped = false;
                SetNavDestination(target.transform.position);
            }
            else
            {
                // close enough: stop moving and face the player
                agent.isStopped = true;
                LookTo(target.transform.position);
            }
        }

        if (distanceToPlayer <= playerSwingDistance)
        {
            currentState = EnemyState.SwingSword;
        }
    }

    void Charge()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Charge");
            animator.SetTrigger("Charge");
        }

        if (agent == null)
            agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();

        if (sightSensor == null || sightSensor.detectedObject == null)
        {
            // no target, abort charge
            if (agent != null)
                agent.isStopped = false;
            currentState = EnemyState.OutofCombat;
            return;
        }

        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        Vector3 targetPos = sightSensor.detectedObject.transform.position;

        if (!isCharging)
        {
            // Aim at the player during windup
            LookTo(targetPos);
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeWindup)
            {
                // start charging: set agent destination to the sampled chargeTarget and increase speed
                isCharging = true;
                chargeTimer = 0f;
                chargeStartPos = agentPos;
                if (agent != null)
                {
                    originalAgentSpeed = agent.speed;
                    agent.speed = chargeSpeed;
                    SetNavDestination(chargeTarget);
                }
            }
            return;
        }

        // While charging we let the NavMeshAgent follow the previously set destination
        if (agent != null)
        {
            // check arrival or exceeding max distance
            float travelled = Vector3.Distance(agent.transform.position, chargeStartPos);
            float distToPlayer = Vector3.Distance(agent.transform.position, targetPos);
            if ((!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) || travelled >= maxChargeDistance || distToPlayer <= playerSwingDistance)
            {
                // stop charging and resume chase
                isCharging = false;
                chargeTimer = 0f;
                agent.speed = originalAgentSpeed;
                agent.isStopped = false;
                currentState = EnemyState.ChasePlayer;
            }
        }
        else
        {
            // no agent: fallback to previous behavior
            Vector3 move = chargeDirection * chargeSpeed * Time.deltaTime;
            transform.parent.position += move;
            float travelled = Vector3.Distance(transform.parent.position, chargeStartPos);
            float distToPlayer = Vector3.Distance(transform.parent.position, targetPos);
            if (travelled >= maxChargeDistance || distToPlayer <= playerSwingDistance)
            {
                isCharging = false;
                chargeTimer = 0f;
                currentState = EnemyState.ChasePlayer;
            }
        }
    }

    void DefenseStance()
    {
        if (!defenseInitialized)
        {
            isDefending = true;

            // Start pulsing emission 
            if (enemyRenderer != null && runtimeMaterial != null)
            {
                runtimeMaterial.EnableKeyword("_EMISSION");
                isPulsing = true; // enable pulsing in Update()
            }

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Charge");
                animator.ResetTrigger("Defense");
                animator.SetTrigger("Defense");
                animator.SetBool("isMoving", false);
            }

            defenseTimer = 0f;
            // stop movement
            if (agent != null)
            {
                agent.isStopped = true;
            }
            // temporarily increase armor if EnemyLife is available
            if (enemyLife != null)
            {
                prevArmorAmount = enemyLife.armor_amount;
                enemyLife.armor_amount = prevArmorAmount * defenseArmorMultiplier;
            }
            defenseInitialized = true;
        }

        // count down
        defenseTimer += Time.deltaTime;

        // When the timer reaches defenseDuration, transition into SingSword
        if (defenseTimer >= defenseDuration)
        {
            // Stop pulsing and clear emission 
            if (enemyRenderer != null && runtimeMaterial != null)
            {
                isPulsing = false;
                runtimeMaterial.DisableKeyword("_EMISSION");
                runtimeMaterial.SetColor("_EmissionColor", Color.black);
                DynamicGI.SetEmissive(enemyRenderer, Color.black);
            }

            // Restore armor and flags
            if (enemyLife != null)
                enemyLife.armor_amount = prevArmorAmount;

            isDefending = false;
            defenseInitialized = false;

            // Reset animator triggers so swing restarts cleanly
            if (animator != null)
            {
                animator.ResetTrigger("Defense");
                animator.ResetTrigger("Attack");
                animator.SetBool("isMoving", false);
            }

            currentState = EnemyState.ChasePlayer;
        }
    }

    void SwingSword()
    {
        if (sightSensor == null || sightSensor.detectedObject == null)
        {
            // No target, return to patrol
            currentState = EnemyState.OutofCombat;
            return;
        }

        var target = sightSensor.detectedObject;
        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        float distanceToPlayer = Vector3.Distance(agentPos, target.transform.position);

        // Face the player
        LookTo(target.transform.position);

        // -------------------------
        // Attack animation and timing
        // -------------------------

        // Make sure NavMeshAgent is stopped while swinging
        if (agent != null)
            agent.isStopped = true;

        // Increment swing timer
        swingTimer += Time.deltaTime;

        // If just starting the swing
        if (swingTimer < 0.1f)
        {
            // Trigger the attack animation once
            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
            }
        }

        // Wait for the attack animation duration before resuming
        if (swingTimer >= swingCooldown)
        {
            // Reset swing timer and go back to chase
            swingTimer = 0f;

            // Count this swing
            swingCounter++;

            // If we've hit the threshold, go to Defense instead of Chase
            if (swingCounter >= swingsBeforeDefense)
            {
                swingCounter = 0;          // reset for next cycle
                defenseInitialized = false;
                if (agent != null) agent.isStopped = true;

                currentState = EnemyState.DefenseStance;
                return; // exit early to avoid switching to Chase
            }

            if (agent != null)
                agent.isStopped = false;

            currentState = EnemyState.ChasePlayer;
        }
    }

    public void NotifySwordHit(Collider other)
    {
        // Force stop the charge: clear flags and immediately halt the NavMeshAgent
        isCharging = false;
        chargeTimer = 0f;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.Warp(agent.transform.position);
            if (originalAgentSpeed > 0f)
            {
                agent.speed = originalAgentSpeed;
            }
        }

        // return to chasing state so AI can resume normal behavior
        currentState = EnemyState.ChasePlayer;
    }


    // Helper: set destination using NavMesh.SamplePosition fallback this helped a lot with buggy path setting
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

    void LookTo(Vector3 targetPosition)
    {
        Vector3 origin = transform.position;
        // Compute direction from enemy to target
        Vector3 directionToPosition = targetPosition - origin;
        directionToPosition.y = 0; // keep rotation flat on the ground

        // If direction has nonzero length, apply rotation
        if (directionToPosition.sqrMagnitude > 0.0001f)
        {
            transform.forward = directionToPosition.normalized;
        }
    }


    private void OnDestroy()
    {
        if (swordHit != null)
            swordHit.OnSwordHit.RemoveListener(NotifySwordHit);
    }
}