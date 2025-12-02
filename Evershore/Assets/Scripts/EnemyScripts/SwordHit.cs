using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
* Author: Liam Housenbold
* Date Created: 9-25-2025
* Date Modified: 12-1-2025
* Summary: Handles sword hit detection and applies damage to player characters upon collision. 
* It also triggers an event when a hit occurs (this is used in EnemyFSM to manage certain attack behavior).
*/
public class SwordHit : MonoBehaviour
{
    [Header("Sword Hit Settings")]
    public float damage;
    public UnityEvent<Collider> OnSwordHit;
    
    [Header("Damage Window")]

    // Ensure we only deal damage once per swing window
    private bool hasDealtDamageThisSwing = true;

    public void ResetDamageWindow()
    {
        hasDealtDamageThisSwing = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Only deal damage once per swing
        if (hasDealtDamageThisSwing)
            return;

        //TODO: need to change to work with player
        PlayerLife life = other.GetComponent<PlayerLife>();

        if (life != null)
        {
            life.Damage(damage);
            hasDealtDamageThisSwing = true;
        }

        // invoke the UnityEvent so other listeners can react
        if (OnSwordHit != null)
        {
            OnSwordHit.Invoke(other);
        }
    }


}
