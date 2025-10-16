/*
 * DialogueUI.cs
 * 
 * Authors: Samuel Huang, Liam Housenbold, Marcus King
 * Date: October 15, 2025
 * 
 * Description:
 * Manages the dialogue UI panel and displays dialogue from DialogueData assets.
 * Handles showing/hiding the panel, advancing through dialogue lines, and 
 * updating UI elements (name, text, portrait).
 * 
 * Usage:
 * Access via DialogueUI.Instance.Show(dialogueData) to display dialogue.
 * Call Advance() to progress to the next line or Close() to end dialogue.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    // Singleton instance for global access from any script
    public static DialogueUI Instance { get; private set; }

    [Header("Panel & Widgets")]
    [SerializeField] GameObject dialoguePanel;     // Main dialogue UI panel
    [SerializeField] TMP_Text nameText;            // Displays speaker name
    [SerializeField] TMP_Text bodyText;            // Displays dialogue text
    [SerializeField] Image portraitImage;          // Displays speaker portrait

    [Header("Behavior")]
    // Whether the player script should auto-close dialogue when leaving interaction range
    public bool closeOnExitRange = true;

    // Tracks the current dialogue being displayed
    private DialogueData current;
    // Tracks which line of dialogue we're currently showing
    private int lineIndex;

    // Quick check if dialogue panel is currently visible
    public bool IsOpen => dialoguePanel && dialoguePanel.activeSelf;

    void Awake()
    {
        // Singleton pattern - ensures only one DialogueUI exists
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        // Start with dialogue panel hidden
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    // Opens dialogue panel and displays the first line from the provided DialogueData
    public void Show(DialogueData data)
    {
        // Safety check - don't show empty or null dialogue
        if (data == null || data.lines == null || data.lines.Length == 0) return;

        current = data;
        lineIndex = 0;

        // Update speaker name
        if (nameText) nameText.text = current.speakerName;
        
        // Update portrait - hide if no portrait is set
        if (portraitImage)
        {
            portraitImage.enabled = current.portrait != null;
            portraitImage.sprite = current.portrait;
        }

        // Show panel and display first line
        dialoguePanel.SetActive(true);
        RenderLine();
    }

    // Advances to the next dialogue line or closes if finished
    public void Advance()
    {
        if (!IsOpen) return;

        lineIndex++;
        
        // If we've reached the end of the dialogue, close the panel
        if (current == null || lineIndex >= current.lines.Length)
        {
            Close();
            return;
        }
        
        // Otherwise show the next line
        RenderLine();
    }

    // Updates the body text to show the current dialogue line
    private void RenderLine()
    {
        if (bodyText) bodyText.text = current.lines[lineIndex];
    }

    // Closes dialogue panel and resets state
    public void Close()
    {
        dialoguePanel.SetActive(false);
        current = null;
        lineIndex = 0;
    }
}
