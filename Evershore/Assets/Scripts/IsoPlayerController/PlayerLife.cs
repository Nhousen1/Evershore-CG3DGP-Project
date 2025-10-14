using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLife : MonoBehaviour
{
    [Header("Player Life Settings")]
    public float amount;
    public float armor_amount;
    [Header("Events")]
    public GameEvent onPlayerDeath;
    void Update()
    {
        if (amount <= 0)
        {
            onPlayerDeath.Raise();
            Destroy(gameObject);
        }
    }
}
