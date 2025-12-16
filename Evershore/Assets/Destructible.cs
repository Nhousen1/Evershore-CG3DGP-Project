using System;
using UnityEngine;

/// <summary>
/// Attach to any prop (e.g., windmill) to allow player attacks to damage and destroy it.
/// Weapons call <see cref="ApplyDamage"/> when they detect this component.
/// </summary>
public class Destructible : MonoBehaviour
{
    public static event Action<Destructible> OnAnyDestructibleDestroyed;
    public static event Action<Destructible> OnAnyDestructibleDamaged;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool allowDestruction = true;
    [SerializeField] private bool destroyGameObject = true;
    [SerializeField] private float destroyDelay = 1.5f;

    [Header("Feedback")]
    [SerializeField] private GameObject destructionVfx;
    [SerializeField] private GameObject fireworkPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] [Range(0f, 1f)] private float hitVolume = 0.1f;
    [SerializeField] private Renderer[] renderersToDisable;
    [SerializeField] private Collider[] collidersToDisable;

    private float currentHealth;
    private bool isDestroyed;

    public event Action<Destructible> OnDestroyed;

    void Awake()
    {
        currentHealth = Mathf.Max(1f, maxHealth);
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (renderersToDisable == null || renderersToDisable.Length == 0)
        {
            renderersToDisable = GetComponentsInChildren<Renderer>();
        }
        if (collidersToDisable == null || collidersToDisable.Length == 0)
        {
            collidersToDisable = GetComponentsInChildren<Collider>();
        }
    }

    /// <summary>
    /// Called by player weapons to damage this prop.
    /// </summary>
    public void ApplyDamage(float amount)
    {
        if (isDestroyed) return;

        currentHealth -= Mathf.Abs(amount);
        PlayHitFeedback();
        OnAnyDestructibleDamaged?.Invoke(this);
        if (currentHealth <= 0f)
        {
            if (allowDestruction)
            {
                HandleDestruction();
            }
            else
            {
                currentHealth = Mathf.Max(1f, maxHealth);
            }
        }
    }

    private void PlayHitFeedback()
    {
        if (!hitSfx) return;

        if (audioSource)
        {
            audioSource.PlayOneShot(hitSfx, hitVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(hitSfx, transform.position, hitVolume);
        }
    }

    private void HandleDestruction()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Disable collisions/renderers so the player can move through the destroyed prop
        foreach (var col in collidersToDisable)
        {
            if (col) col.enabled = false;
        }
        foreach (var rend in renderersToDisable)
        {
            if (rend) rend.enabled = false;
        }

        if (destructionVfx)
        {
            Instantiate(destructionVfx, transform.position, transform.rotation);
        }

        if (fireworkPrefab)
        {
            var fireworkInstance = Instantiate(fireworkPrefab, transform.position, Quaternion.identity);
            var particleSystems = fireworkInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }

        if (destroyGameObject)
        {
            Destroy(gameObject, destroyDelay);
        }

        OnDestroyed?.Invoke(this);
        OnAnyDestructibleDestroyed?.Invoke(this);
    }
}
