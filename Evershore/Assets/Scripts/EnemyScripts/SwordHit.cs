using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwordHit : MonoBehaviour
{
    public float damage;
    public UnityEvent<Collider> OnSwordHit;

    void OnTriggerEnter(Collider other)
    {
        //TODO: need to change to work with player
        EnemyLife life = other.GetComponent<EnemyLife>();

        if (life != null)
        {
            life.amount -= (damage - life.armor_amount);
        }

        // invoke the UnityEvent so other listeners can react
        if (OnSwordHit != null)
        {
            OnSwordHit.Invoke(other);
        }
    }

    
}
