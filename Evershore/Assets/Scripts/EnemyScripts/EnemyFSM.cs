using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { OutofCombat, SwingSword, ChargeAttack, ParryStance, ParrySpin, ChasePlayer }

    public EnemyState currentState;

    public EnemySight sightSensor;
    // sight angle handling: cache original and use an expanded angle while in combat
    public float combatSightAngle = 180f;
    private float originalSightAngle = -1f;
    private bool sightAngleCached = false;

    // Simple patrol (two-point) settings — computed from the enemy's starting position
    public float patrolRadius = 4f;
    public float patrolPointTolerance = 0.5f;
    public float patrolPauseTime = 1.5f;
    private Vector3[] patrolPoints = new Vector3[2];
    private int patrolIndex = 0;
    private float patrolTimer = 0f;
    private Vector3 initialPosition;


    // distance settings
    public float playerSwingDistance;
    public float playerParryDistance;
    public float chaseStopDistance = 1.5f;



    // Charge attack settings
    public float playerChargeDistance; // min distance to player to trigger charge
    public float chargeWindup = 0.5f; // seconds to aim before charging
    public float chargeSpeed = 8f; // movement speed while charging
    public float maxChargeDistance = 8f; // maximum distance to travel during charge
    private float chargeTimer = 0f;
    private bool isCharging = false;
    private Vector3 chargeDirection = Vector3.zero;
    private Vector3 chargeStartPos = Vector3.zero;
    private Vector3 chargeTarget = Vector3.zero;
    private float originalAgentSpeed = 0f;

    // material settings/params
    public Renderer[] targetRenderers;
    public Material standardMaterial;
    public Material chargeMaterial;
    public Material parryMaterial;
    // Transform to rotate/translate during spin and movement (defaults to agent root)
    public Transform spinRoot;
  
    // Parry params
    public float parryDuration = 1.0f;
    public float parrySpinDuration = 0.8f;
    private float parryTimer = 0f;

    private bool parryInitialized = false;
    // Parry spin runtime fields (used when in ParrySpin state)
    private float parrySpinTimer = 0f;
    private Quaternion parryStartRot;
    private Quaternion parryEndRot;

    private bool prevAgentUpdateRotation = true;
    // parry cooldown: minimum seconds between parry activations
    public float parryCooldown = 5f;
    private float lastParryTime = 0f;

    [SerializeField] private SwordHit swordHit;
    [SerializeField] private EnemyLife enemyLife;
    // parry armor modifier
    public float parryArmorMultiplier = 2.0f; // double armor while parrying by default
    private float prevArmorAmount = 0f;

    private UnityEngine.AI.NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        swordHit.OnSwordHit.AddListener(NotifySwordHit); //stop charge on sword hit event listener

        // default spinRoot to the agent/root transform so visuals rotate with movement
        if (spinRoot == null)
            spinRoot = (agent != null) ? agent.transform : transform.parent;

    // record starting position and initialize patrol points relative to the spin root
    initialPosition = (spinRoot != null) ? spinRoot.position : transform.position;
    Vector3 right = (spinRoot != null) ? spinRoot.right : transform.right;
        patrolPoints[0] = initialPosition + right * patrolRadius;
        patrolPoints[1] = initialPosition - right * patrolRadius;
        // ensure we start out patrolling
        currentState = EnemyState.OutofCombat;

        // cache original sight angle if a sensor is assigned in the inspector
        if (sightSensor != null)
        {
            originalSightAngle = sightSensor.angle;
            sightAngleCached = true;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerParryDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerChargeDistance);
    }

    void Update()
    {
        // changing in and out of combat sightAngle
        if (!sightAngleCached && sightSensor != null)
        {
            originalSightAngle = sightSensor.angle;
            sightAngleCached = true;
        }

        if (sightSensor != null && sightAngleCached)
        {
            if (currentState == EnemyState.OutofCombat)
                sightSensor.angle = originalSightAngle;
            else
                sightSensor.angle = combatSightAngle;
        }
        if (currentState == EnemyState.OutofCombat)
        {
            OutofCombat();
        }
        else if (currentState == EnemyState.ChasePlayer)
        {
            ChasePlayer();
        }
        else if (currentState == EnemyState.ChargeAttack)
        {
            ChargeAttack();
        }
        else if (currentState == EnemyState.ParryStance)
        {
            ParryStance();
        }
        else if (currentState == EnemyState.ParrySpin)
        {
            ParrySpin();
        }
    }

    void OutofCombat()
    {
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
        if (agent == null)
            agent = GetComponentInParent<UnityEngine.AI.NavMeshAgent>();

        if (sightSensor == null || sightSensor.detectedObject == null)
        {
            currentState = EnemyState.OutofCombat;
            return;
        }

        var target = sightSensor.detectedObject;
        Vector3 agentPos = agent != null ? agent.transform.position : transform.position;
        float distanceToPlayer = Vector3.Distance(agentPos, target.transform.position);

        // If the player is close enough and cooldown has elapsed, enter parry stance
        if (distanceToPlayer <= playerParryDistance && Time.time >= lastParryTime + parryCooldown)
        {
            if (agent != null)
                agent.isStopped = true;
            parryInitialized = false;
                lastParryTime = Time.time; // Set lastParryTime when entering parry stance
            currentState = EnemyState.ParryStance;
            return;
        }

        // If the player is outside the charge range, start a charge attack
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
            currentState = EnemyState.ChargeAttack;
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
            //currentState = EnemyState.SwingSword;
        }
    }

    void ChargeAttack()
    {
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
                // apply charge material visuals
                ApplyMaterial(chargeMaterial);
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
                    // restore visuals
                    ApplyMaterial(standardMaterial);
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
                ApplyMaterial(standardMaterial);
                currentState = EnemyState.ChasePlayer;
            }
        }
    }

    void ParryStance()
    {
        if (!parryInitialized)
        {
            parryTimer = 0f;
            // stop movement
            if (agent != null)
            {
                agent.isStopped = true;
            }
            // set parry visual
            ApplyMaterial(parryMaterial);
            // temporarily increase armor if EnemyLife is available
            if (enemyLife != null)
            {
                prevArmorAmount = enemyLife.armor_amount;
                enemyLife.armor_amount = prevArmorAmount * parryArmorMultiplier;
            }
            parryInitialized = true;
        }

        // count down
            parryTimer += Time.deltaTime;

        // When the timer reaches parryDuration, transition into the ParrySpin state
        if (parryTimer >= parryDuration)
        {
            // prepare spin runtime data
            parrySpinTimer = 0f;
            parryStartRot = (spinRoot != null) ? spinRoot.rotation : transform.rotation;
            parryEndRot = parryStartRot * Quaternion.Euler(0f, 360f, 0f);

            // disable NavMeshAgent automatic rotation so it doesn't override our manual spin
            if (agent != null)
            {
                prevAgentUpdateRotation = agent.updateRotation;
                agent.updateRotation = false;
            }

            currentState = EnemyState.ParrySpin;
        }
    }
    void ParrySpin()
    {
        // restore armor after parry
        if (enemyLife != null)
            enemyLife.armor_amount = prevArmorAmount;
            
        if (agent != null)
            agent.isStopped = true;

        parrySpinTimer += Time.deltaTime;

        // how fast to spin (degrees per second)
        float spinSpeed = 720f; // 2 full rotations per second

        // Rotate the spinRoot around its up axis
        if (spinRoot != null)
            spinRoot.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        else
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // end spin after duration
        if (parrySpinTimer >= parrySpinDuration)
        {
            ApplyMaterial(standardMaterial);

            if (agent != null)
            {
                agent.isStopped = false;
                agent.updateRotation = prevAgentUpdateRotation;
            }
            parryInitialized = false;
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
        // restore visuals if we changed them
        ApplyMaterial(standardMaterial);
    }
    void ApplyMaterial(Material mat)
    {
        if (targetRenderers == null || mat == null)
            return;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            var r = targetRenderers[i];
            if (r == null) continue;
            var mats = r.materials;
            for (int m = 0; m < mats.Length; m++)
                mats[m] = mat;
            r.materials = mats;
        }
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
        Vector3 origin = (spinRoot != null) ? spinRoot.position : transform.parent.position;
        Vector3 directionToPosition = Vector3.Normalize(targetPosition - origin);
        directionToPosition.y = 0;
        if (spinRoot != null)
            spinRoot.forward = directionToPosition;
        else
            transform.parent.forward = directionToPosition;
    }
    
    private void OnDestroy()
    {
        if (swordHit != null)
            swordHit.OnSwordHit.RemoveListener(NotifySwordHit);
        if (sightSensor != null && sightAngleCached)
            sightSensor.angle = originalSightAngle;
    }
}