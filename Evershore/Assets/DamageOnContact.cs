using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    public float damageAmount;
    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        
        PlayerLife life = other.GetComponent<PlayerLife>();

        if (life != null)
        {
            Debug.Log("Hit projectile");
            life.Damage(damageAmount);
        }
    }
}

