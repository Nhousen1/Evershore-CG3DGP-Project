using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarInteractable : MonoBehaviour
{
    [Header("Pillar Settings")]
    [SerializeField] private bool isActivated = false;
    
    private static int totalPillars = 0;
    private static int activatedPillars = 0;
    private static bool hasWon = false;

    void Start()
    {
        // Register this pillar
        totalPillars++;
        
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
    }



    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER: Something entered {gameObject.name} - Object: {other.name}, Tag: '{other.tag}'");
        
        // Check if the player entered the trigger
        bool hasPlayerTag = other.CompareTag("Player");
        bool hasPlayerMovement = other.GetComponent<PlayerMovement>() != null;
        bool isPlayer = hasPlayerTag || hasPlayerMovement;
        
        Debug.Log($"TRIGGER: Player check - HasPlayerTag: {hasPlayerTag}, HasPlayerMovement: {hasPlayerMovement}, IsPlayer: {isPlayer}");

        if (isPlayer)
        {
            Debug.Log($"Player touched {gameObject.name}!");
            
            if (!isActivated)
            {
                ActivatePillar();
            }
            else
            {
                Debug.Log($"{gameObject.name} is already activated.");
            }
        }
        else
        {
            Debug.Log($"TRIGGER: Not a player. Object: {other.name}, Tag: '{other.tag}'");
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
        }
    }


    public bool IsActivated()
    {
        return isActivated;
    }

    
}