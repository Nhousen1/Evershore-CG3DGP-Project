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

    public int ThornsDamage = 5;
    public int swingsBeforeDefense = 3;   // ex) 3 swings then defend
    private int swingCounter = 2;         // counts completed swings since last defense
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
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private EnemySight sightSensor;
    private UnityEngine.AI.NavMeshAgent agent;

    private float originalSightAngle = -1f;

    private void Awake()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        swordHit.OnSwordHit.AddListener(NotifySwordHit); //detect when player is hit on charge
        if (enemyLife != null)
            enemyLife.onEnemyDamaged.AddListener(OnEnemyDamaged); // detect when enemy is damaged
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
        if (animator != null)
            animator.speed = Random.Range(0.9f, 1.1f);
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

        if (animator != null && agent != null)
            animator.speed = agent.velocity.magnitude / agent.speed;

        if (currentState == EnemyState.OutofCombat)
        {
            ResetCombatAnimation();
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
        // Animation handling
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }

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

        if (swingCounter >= swingsBeforeDefense)
        {
            swingCounter = 0;
            defenseInitialized = false;
            if (agent != null) agent.isStopped = true;

            currentState = EnemyState.DefenseStance;
            return;
        }

        var target = sightSensor.detectedObject;
        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        float distanceToPlayer = Vector3.Distance(agentPos, target.transform.position);

        // If the player is outside the charge range, start a charge
        if (distanceToPlayer > playerChargeDistance)
        {
            if (agent != null)
                agent.isStopped = true;

            chargeDirection = (target.transform.position - agentPos);
            chargeDirection.y = 0;
            chargeDirection = chargeDirection.normalized;
            chargeStartPos = agentPos;
            chargeTarget = agentPos + chargeDirection * maxChargeDistance;
            chargeTimer = 0f;
            isCharging = false;
            currentState = EnemyState.Charge;
            return;
        }

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
            StartCoroutine(TriggerWithDelay("Charge", Random.Range(0f, 0.25f)));
        }

        if (agent == null)
            agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();

        if (sightSensor == null || sightSensor.detectedObject == null)
        {
            if (agent != null)
                agent.isStopped = false;
            currentState = EnemyState.OutofCombat;
            return;
        }

        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        Vector3 targetPos = sightSensor.detectedObject.transform.position;

        if (!isCharging)
        {
            LookTo(targetPos);
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeWindup)
            {
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

        if (agent != null)
        {
            float travelled = Vector3.Distance(agent.transform.position, chargeStartPos);
            float distToPlayer = Vector3.Distance(agent.transform.position, targetPos);
            if ((!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) || travelled >= maxChargeDistance || distToPlayer <= playerSwingDistance)
            {
                isCharging = false;
                chargeTimer = 0f;
                agent.speed = originalAgentSpeed;
                agent.isStopped = false;
                currentState = EnemyState.ChasePlayer;
            }
        }
        else
        {
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

            if (enemyRenderer != null && runtimeMaterial != null)
            {
                runtimeMaterial.EnableKeyword("_EMISSION");
                isPulsing = true;
            }

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Charge");
                animator.ResetTrigger("Defense");
                StartCoroutine(TriggerWithDelay("Defense", Random.Range(0f, 0.25f)));
                animator.SetBool("isMoving", false);
            }

            defenseTimer = 0f;
            if (agent != null)
                agent.isStopped = true;

            if (enemyLife != null)
            {
                prevArmorAmount = enemyLife.armor_amount;
                enemyLife.armor_amount = prevArmorAmount * defenseArmorMultiplier;
            }
            defenseInitialized = true;
        } 

        defenseTimer += Time.deltaTime;


        if (defenseTimer >= defenseDuration)
        {
            if (enemyRenderer != null && runtimeMaterial != null)
            {
                isPulsing = false;
                runtimeMaterial.DisableKeyword("_EMISSION");
                runtimeMaterial.SetColor("_EmissionColor", Color.black);
                DynamicGI.SetEmissive(enemyRenderer, Color.black);
            }

            if (enemyLife != null)
                enemyLife.armor_amount = prevArmorAmount;

            isDefending = false;
            defenseInitialized = false;

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
            currentState = EnemyState.OutofCombat;
            return;
        }

        var target = sightSensor.detectedObject;
        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        float distanceToPlayer = Vector3.Distance(agentPos, target.transform.position);

        LookTo(target.transform.position);

        if (agent != null)
            agent.isStopped = true;

        swingTimer += Time.deltaTime;

        if (swingTimer < 0.1f)
        {
            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                StartCoroutine(TriggerWithDelay("Attack", Random.Range(0f, 0.25f)));
            }
        }

        if (swingTimer >= swingCooldown)
        {
            swingTimer = 0f;
            swingCounter++;

            if (swingCounter >= swingsBeforeDefense)
            {
                swingCounter = 0;
                defenseInitialized = false;
                if (agent != null) agent.isStopped = true;

                currentState = EnemyState.DefenseStance;
                return;
            }

            if (agent != null)
                agent.isStopped = false;

            currentState = EnemyState.ChasePlayer;
        }
    }

    public void NotifySwordHit(Collider other)
    {
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

        currentState = EnemyState.ChasePlayer;
    }

    private void OnEnemyDamaged()
    {
        // only react while in Defense Stance
        if (currentState == EnemyState.DefenseStance)
        {
            if (playerLife != null)
            {
                playerLife.Damage(ThornsDamage);
            }
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

    void LookTo(Vector3 targetPosition)
    {
        Vector3 origin = transform.position;
        Vector3 directionToPosition = targetPosition - origin;
        directionToPosition.y = 0;

        if (directionToPosition.sqrMagnitude > 0.0001f)
        {
            transform.forward = directionToPosition.normalized;
        }
    }

    private IEnumerator TriggerWithDelay(string triggerName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    private void ResetCombatAnimation()
    {
        if (animator == null) return;

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Charge");
        animator.ResetTrigger("Defense");

        animator.SetBool("isMoving", true);
        animator.SetFloat("MoveSpeed", 0f);
    }

    private void OnDestroy()
    {
        if (swordHit != null)
            swordHit.OnSwordHit.RemoveListener(NotifySwordHit);
        if (enemyLife != null)
            enemyLife.onEnemyDamaged.RemoveListener(OnEnemyDamaged); 
    }
}
