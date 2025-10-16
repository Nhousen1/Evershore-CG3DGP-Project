using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Villager nearbyVillager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.CompareTag("Untagged")) { /* ignore */ }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Villager>() && other.GetComponent<Villager>() == nearbyVillager)
        {
            nearbyVillager = null;
            // OPTIONAL: auto-close if leaving range while dialogue is open
            if (DialogueUI.Instance && DialogueUI.Instance.IsOpen && DialogueUI.Instance.closeOnExitRange)
                DialogueUI.Instance.Close();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // This pattern supports any villager collider marked as Trigger
        var v = other.GetComponent<Villager>();
        if (v) nearbyVillager = v;
    }

    void Update()
    {
        // Press E to open/advance only if in range
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueUI.Instance && DialogueUI.Instance.IsOpen)
            {
                DialogueUI.Instance.Advance();
            }
            else if (nearbyVillager != null)
            {
                nearbyVillager.StartDialogue();
            }
        }
    }
}
