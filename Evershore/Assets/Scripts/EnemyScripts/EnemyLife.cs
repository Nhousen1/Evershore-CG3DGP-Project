using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
* Author: Liam Housenbold
* Date Created: 9-25-2025
* Date Modified: 10-15-2025
* Summary: Manages the health and armor of an enemy character. When health reaches zero, it triggers a death event and destroys the enemy GameObject.
*/
public class EnemyLife : MonoBehaviour
{
    [Header("EnemyLife Settings")]
    public static float max_amount;
    public float amount;
    public float armor_amount;
    public UnityEvent onEnemyDeath = new UnityEvent();

    [SerializeField] private FloatingEnemyHpBar floatingHpBar;

    void Awake()
    {
        floatingHpBar = GetComponentInChildren<FloatingEnemyHpBar>();
        max_amount = amount;
    }


    void Update()
    {
        floatingHpBar.UpdateHpBar(amount, max_amount);
        if (amount <= 0)
        {
            onEnemyDeath.Invoke();
            Destroy(gameObject);
        }
    }
}
