using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PillarInteractable : MonoBehaviour
{
    [Header("Pillar Settings")]
    [SerializeField] private bool isActivated = false;
    
    private static int totalPillars = 0;
    private static int activatedPillars = 0;
    private static bool hasWon = false;
    // Static UnityEvent fired when all pillars are activated (same style as EnemyLife.onDeath)
    public static UnityEvent onAllPillarsActivated = new UnityEvent();

    void Start()
    {
        // Register this pillar
        totalPillars++;
        // If this pillar is already activated in the inspector, count it
        if (isActivated)
        {
            activatedPillars++;
        }
        
        // Ensure this object has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"PillarInteractable on {gameObject.name}: NO COLLIDER FOUND! Please add a collider component.");
        }

        Debug.Log($"Pillar {gameObject.name} initialized. Total pillars: {totalPillars}");

        // In case all pillars are already activated on scene load, check win condition
        CheckForAllActivated();
    }



    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER: Something entered {gameObject.name} - Object: {other.name}, Tag: '{other.tag}'");

        // Activate on any collision entry
        if (!isActivated)
        {
            Debug.Log($"Object {other.name} triggered {gameObject.name}.");
            ActivatePillar();
        }
        else
        {
            Debug.Log($"{gameObject.name} is already activated.");
        }
    }

    public void ActivatePillar()
    {
        if (isActivated || hasWon) return;

        isActivated = true;
        activatedPillars++;
        
        Debug.Log($"Pillar {gameObject.name} activated! Progress: {activatedPillars}/{totalPillars}");

        // Check if all pillars are activated
        if (activatedPillars >= totalPillars && !hasWon)
        {
            hasWon = true;
            Debug.Log("*** YOU WIN! All pillars have been triggered! ***");
            // Fire the static UnityEvent (matches EnemyLife.onDeath style)
            try { onAllPillarsActivated?.Invoke(); } catch (System.Exception e) { Debug.LogWarning($"Exception invoking onAllPillarsActivated: {e}"); }
        }
    }

    // Helper used to check and dispatch the all-activated event (used at Start too)
    private static void CheckForAllActivated()
    {
        if (hasWon) return;
        if (totalPillars > 0 && activatedPillars >= totalPillars)
        {
            hasWon = true;
            Debug.Log("*** YOU WIN! All pillars have been triggered! *** (checked)");
            try { onAllPillarsActivated?.Invoke(); } catch (System.Exception e) { Debug.LogWarning($"Exception invoking onAllPillarsActivated: {e}"); }
        }
    }


    public bool IsActivated()
    {
        return isActivated;
    }

    private void OnDestroy()
    {
        // Clean up global counts so that runtime add/remove of pillars keeps counters sane
        totalPillars = Mathf.Max(0, totalPillars - 1);
        if (isActivated)
            activatedPillars = Mathf.Max(0, activatedPillars - 1);
        // If something destroyed after win, leave hasWon true; you can call ResetAllPillars to reset
    }

    //
    public static void ResetAllPillars()
    {
        totalPillars = 0;
        activatedPillars = 0;
        hasWon = false;
        onAllPillarsActivated = new UnityEvent();
    }

    
}