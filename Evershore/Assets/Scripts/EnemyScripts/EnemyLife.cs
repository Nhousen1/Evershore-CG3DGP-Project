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

    private float current_amount;
    public float armor_amount;
    public UnityEvent onEnemyDeath = new UnityEvent();

    public UnityEvent onEnemyDamaged = new UnityEvent();

    [SerializeField] private FloatingEnemyHpBar floatingHpBar;
    private GameManager gameManager;
    public ParticleSystem deathBloodEffect;

    void Awake()
    {
        gameManager = GameManager.Instance;
        floatingHpBar = GetComponentInChildren<FloatingEnemyHpBar>();
        max_amount = amount;
        current_amount = amount;

    }


    void Update()
    {
        floatingHpBar.UpdateHpBar(amount, max_amount);
        if (current_amount > amount)
        {
            onEnemyDamaged.Invoke();
            current_amount = amount;
        }
        if (amount <= 0)
        {
            onEnemyDeath.Invoke();
            if (deathBloodEffect != null)
            {
                ParticleSystem ps = Instantiate(
                    deathBloodEffect,
                    transform.position,
                    transform.rotation
                );
                ps.Play();
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            if (gameManager != null)
            {
                gameManager.enemiesKilled++;
                Debug.Log("Enemy killed. Total: " + gameManager.enemiesKilled);
            }
            Destroy(gameObject);
        }
    }
}
