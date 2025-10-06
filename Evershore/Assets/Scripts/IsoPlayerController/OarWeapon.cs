using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OarWeapon : Weapon
{
    [Header("Gameplay Varibles")]
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

    public override void DoAttack()
    {
        Collider[] hits = new Collider[32];
        hits = Physics.OverlapSphere(hitPoint.position, radius, damageLayers, triggerInteraction);

        //Apply damage once per collider hit
        for (int i = 0; i < hits.Length; i++)
        {
            var collider = hits[i];
            if (!collider) continue;

            var life = collider.GetComponentInParent<EnemyLife>();
            if (life != null)
            {
                life.amount -= damage;
            }
        }
    }
}
