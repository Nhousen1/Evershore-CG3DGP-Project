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
    Debug.Log($"[BoatEndTrigger] GM.Instance={(GameManager.Instance ? GameManager.Instance.GetInstanceID().ToString() : "NULL")}");

    if (!other.CompareTag(playerTag))
    {
        Debug.Log($"[BoatEndTrigger] Ignored because tag != {playerTag} (was {other.tag})");
        return;
    }

    Debug.Log($"[BoatEndTrigger] Player detected. GameManager.Instance={(GameManager.Instance ? "OK" : "NULL")}");
    if (GameManager.Instance)
    {
        GameManager.Instance.OnBoatTriggered();
    }
}


    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (GameManager.Instance)
        {
            GameManager.Instance.HideIncompleteRitualsFeedback();
        }
    }
}
