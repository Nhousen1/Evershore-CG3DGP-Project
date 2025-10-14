using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLife : MonoBehaviour
{
    [Header("Player Life Settings")]
    [SerializeField] float amount;
    [SerializeField] float armor_amount;
    public UnityEvent onPlayerDeath = new UnityEvent();
    void Update()
    {
        if (amount <= 0)
        {
            onPlayerDeath.Invoke();
            Destroy(gameObject);
        }
    }
}
