/*
 * DialogueData.cs
 * 
 * Authors: Samuel Huang, Liam Housenbold, Marcus King
 * Date: October 15, 2025
 * 
 * Description:
 * ScriptableObject for storing NPC dialogue data. Allows designers to create
 * dialogue assets in the editor without modifying code.
 * 
 * Usage:
 * Right-Click in Project → Create → Dialogue → Dialogue Data
 */

using UnityEngine;

// Adds a Create menu option for making new dialogue assets in the editor
[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    // Name of the character speaking
    public string speakerName;
    
    // Array of dialogue lines - TextArea makes multi-line editing easier in Inspector
    [TextArea(2, 6)] 
    public string[] lines;
    
    // Character portrait displayed during dialogue (optional)
    public Sprite portrait;
}
