using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyLife : MonoBehaviour
{
    [Header("EnemyLife Settings")]
    public float amount;
    public float armor_amount;
    public UnityEvent onEnemyDeath = new UnityEvent();
    void Update()
    {
        if (amount <= 0)
        {
            onEnemyDeath.Invoke();
            Destroy(gameObject);
        }
    }
}
