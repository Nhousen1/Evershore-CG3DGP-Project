using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OarWeapon : Weapon
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private Transform hitPoint;
    [SerializeField] 
    private float radius = 0.6f;
    [SerializeField] 
    private LayerMask damageLayers = 0; 
    [SerializeField] 
    private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    //reccomended to use a buffer for effeciency
    private readonly Collider[] hits = new Collider[32];
    public override void DoAttack()
    {
        int count = Physics.OverlapSphereNonAlloc(hitPoint.position, radius, hits, damageLayers, triggerInteraction);

        //Apply damage once per collider hit
        for (int i = 0; i < count; i++)
        {
            var col = hits[i];
            if (!col) continue;

            var life = col.GetComponentInParent<EnemyLife>();
            if (life != null)
            {
                life.amount -= damage;
            }
        }
    }
}
