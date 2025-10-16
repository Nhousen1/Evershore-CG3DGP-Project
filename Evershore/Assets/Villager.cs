using UnityEngine;

public class Villager : MonoBehaviour
{
    public DialogueData dialogue;

    // optional: if you want auto-close when leaving range, PlayerInteraction can call CloseOnExit
    public void StartDialogue()
    {
        DialogueUI.Instance?.Show(dialogue);
    }
}
