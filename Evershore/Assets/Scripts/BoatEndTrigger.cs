using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoatEndTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (GameEndManager.Instance)
        {
            GameEndManager.Instance.OnBoatTriggered();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (GameEndManager.Instance)
        {
            GameEndManager.Instance.HideIncompleteRitualsFeedback();
        }
    }
}
