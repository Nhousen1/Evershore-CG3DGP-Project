/*
 * PlayerInteraction.cs
 * 
 * Authors: Samuel Huang, Liam Housenbold, Marcus King
 * Date: October 15, 2025
 * 
 * Description:
 * Handles player interactions with NPCs and objects in the world. Detects when
 * the player is near a villager and allows interaction via the 'E' key to start
 * or advance dialogue. Automatically closes dialogue when leaving range.
 * 
 * Requirements:
 * - Attach to the Player GameObject
 * - Player must have a trigger collider for detection
 * - NPCs must have the Villager component
 */

using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // Tracks the villager currently in interaction range
    private Villager nearbyVillager;

    private void OnTriggerEnter(Collider other)
    {
        // Ignore untagged non-trigger colliders to prevent unwanted interactions
        if (!other.isTrigger && other.CompareTag("Untagged")) { /* ignore */ }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the villager we were near is leaving range
        if (other.GetComponent<Villager>() && other.GetComponent<Villager>() == nearbyVillager)
        {
            nearbyVillager = null;
            
            // Auto-close dialogue if player walks away while dialogue is open
            // Only if the DialogueUI has closeOnExitRange enabled
            if (DialogueUI.Instance && DialogueUI.Instance.IsOpen && DialogueUI.Instance.closeOnExitRange)
                DialogueUI.Instance.Close();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Continuously update which villager is nearby while in range
        // This ensures we interact with the correct villager if multiple are nearby
        var v = other.GetComponent<Villager>();
        if (v) nearbyVillager = v;
    }

    void Update()
    {
        // Press E to interact with nearby villagers or advance dialogue
        if (Input.GetKeyDown(KeyCode.E))
        {
            // If dialogue is already open, advance to next line
            if (DialogueUI.Instance && DialogueUI.Instance.IsOpen)
            {
                DialogueUI.Instance.Advance();
            }
            // If near a villager and no dialogue open, start their dialogue
            else if (nearbyVillager != null)
            {
                nearbyVillager.StartDialogue();
            }
        }
    }
}
