using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Author: Liam Housenbold, Marcus King
 * Date created: 10/22/2025
 * Date last updated: 10/30/2025
 * Summary: handles player health events
 */
public class PlayerLife : MonoBehaviour
{
    [Header("Player Life Settings")]
    public float amount;
    public float amountMax;
    public float armor_amount;
    [Header("Events")]
    public GameEvent onPlayerDeath;
    private void Start()
    {
        if (PlayerHealthBar.Instance == null)
        {
            Debug.LogWarning("Player life in scene without healthbar, consider adding prefab.");
        }
        else
        {
            PlayerHealthBar.Instance.ChangeValue(amount, amountMax);
        }
    }
    //Never input negative values for these events
    public void Damage(float damage)
    {
        Mathf.Abs(damage);
        amount -= damage;

        PlayerHealthBar.Instance.ChangeValue(amount);

        if (amount <= 0)
        {
            onPlayerDeath.Raise();
            Destroy(gameObject);
        }
    }
    public void Heal(float add)
    {
        if (amount + add >= amountMax)
        {
            return;
        }
        Mathf.Abs(add);
        amount += add;

        PlayerHealthBar.Instance.ChangeValue(amount);
    }
}
