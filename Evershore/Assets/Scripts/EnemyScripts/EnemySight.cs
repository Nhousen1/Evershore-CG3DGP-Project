using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* Author: Liam Housenbold
* Date Created: 9-25-2025
* Date Modified: 10-15-2025
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
    private void Update()
    {
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
