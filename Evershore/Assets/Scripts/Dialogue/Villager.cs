/*
 * Villager.cs
 * 
 * Authors: Samuel Huang, Liam Housenbold, Marcus King
 * Date: October 15, 2025
 * 
 * Description:
 * Component for NPC villagers that can be interacted with to show dialogue.
 * Each villager has their own DialogueData asset that defines what they say.
 * 
 * Usage:
 * - Attach to NPC GameObjects
 * - Assign a DialogueData asset to the dialogue field in the Inspector
 * - Add a trigger collider for PlayerInteraction to detect
 */

using UnityEngine;

public class Villager : MonoBehaviour
{
    // DialogueData asset containing this villager's conversation
    // Assigned in the Inspector for each unique NPC
    public DialogueData dialogue;

    // Called by PlayerInteraction when player presses 'E' near this villager
    // Displays the dialogue through the DialogueUI singleton
    public void StartDialogue()
    {
        // ?. null check - only shows dialogue if DialogueUI exists
        DialogueUI.Instance?.Show(dialogue);
    }
}
