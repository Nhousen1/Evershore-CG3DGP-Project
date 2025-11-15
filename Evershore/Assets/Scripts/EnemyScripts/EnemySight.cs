using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* Author: Liam Housenbold
* Date Created: 9-25-2025
* Date Modified: 11-14-2025
* Summary: Manages the enemy's sight detection, allowing it to detect objects within a certain distance and angle, considering obstacles.
*/
public class EnemySight : MonoBehaviour
{
    [Header("EnemySight Settings")]
    public float distance;
    public float angle;
    public LayerMask objectsLayers;
    public LayerMask obstaclesLayers;
    public Collider detectedObject;

    [Header("Sprint Detection Settings")]

    public PlayerMovement player; 
    public float sprintMultiplier = 1.8f;   // how much bigger the radius becomes
    public float sprintCheckInterval = 0.1f;
    private float baseDistance;
    private float sprintCheckTimer = 0f;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distance);

        Vector3 rightDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDirection * distance);

        Vector3 leftDirection = Quaternion.Euler(0, -angle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDirection * distance);

        if (detectedObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(detectedObject.bounds.center, 0.2f);
        }
    }

    private void Start()
    {
        baseDistance = distance;
    }
    private void Update()
    {
        // Adjust detection radius based on sprint
        sprintCheckTimer += Time.deltaTime;
        if (sprintCheckTimer >= sprintCheckInterval)
        {
            sprintCheckTimer = 0f;

            if (player != null && player.isRunning)
                distance = baseDistance * sprintMultiplier;
            else
                distance = baseDistance;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, objectsLayers);

        detectedObject = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];

            Vector3 directionToController = Vector3.Normalize(collider.bounds.center - transform.position);

            float angleToCollider = Vector3.Angle(transform.forward, directionToController);

            if (angleToCollider < angle)
            {
                if (!Physics.Linecast(transform.position, collider.bounds.center, out RaycastHit hit, obstaclesLayers))
                {
                    Debug.DrawLine(transform.position, collider.bounds.center, Color.green);
                    detectedObject = collider;
                    break;
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                }
            }
        }
    }
}
