using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
* Author: Liam Housenbold
* Date Created: 9-25-2025
* Date Modified: 10-15-2025
* Summary: Handles sword hit detection and applies damage to player characters upon collision. 
* It also triggers an event when a hit occurs (this is used in EnemyFSM to manage certain attack behavior).
*/
public class SwordHit : MonoBehaviour
{
    [Header("Sword Hit Settings")]
    public float damage;
    public UnityEvent<Collider> OnSwordHit;

    void OnTriggerEnter(Collider other)
    {
        //TODO: need to change to work with player
        PlayerLife life = other.GetComponent<PlayerLife>();

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
