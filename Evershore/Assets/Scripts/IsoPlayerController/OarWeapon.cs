using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/6/2025
 * Summary: placeholder test melee weapon item which damages everything within range once per attack.
 */
public class OarWeapon : Weapon
{
    [Header("Gameplay Variables")]
    [SerializeField]
    private float damage;
    [Header("Collision Info")]
    [SerializeField]
    private Transform hitPoint;
    [SerializeField] 
    private float radius = 0.6f;
    [SerializeField] 
    private LayerMask damageLayers = 0; 
    [SerializeField] 
    private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    //Detect colliders (or triggers) in range of weapon on the first frame of the attack cycle. This is the most simple form of melee
    public override void DoAttack()
    {
        Collider[] hits = new Collider[32];
        hits = Physics.OverlapSphere(hitPoint.position, radius, damageLayers, triggerInteraction);

        for (int i = 0; i < hits.Length; i++)
        {
            var collider = hits[i];
            if (!collider) continue;

            //This assumes the life script and the collider script are on the same GameObject
            var life = collider.GetComponent<EnemyLife>();
            if (life != null)
            {
                life.amount -= damage;
            }
        }
    }
}
