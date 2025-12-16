using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks windmill destructible objects and returns player to a hub scene when all are destroyed.
/// Attach to a GameObject in the level and configure the target count and hub scene name.
/// </summary>
public class WindmillObjectiveManager : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] private int requiredWindmills = 7;
    [SerializeField] private string windmillTag = "Windmill";
    [SerializeField] private bool requireMatchingTag = true;

    [Header("Completion Behaviour")]
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private float returnDelay = 2f;
    [SerializeField] private bool autoLoadHubOnComplete = true;

    [Header("Optional UI Hooks")]
    [SerializeField] private TMP_Text counterLabel;
    [SerializeField] private string counterFormat = "Windmills: {0}/{1}";

    private int destroyedCount;
    private bool isReturning;

    void OnEnable()
    {
        Destructible.OnAnyDestructibleDestroyed += HandleDestructibleDestroyed;
    }

    void OnDisable()
    {
        Destructible.OnAnyDestructibleDestroyed -= HandleDestructibleDestroyed;
    }

    void Start()
    {
        destroyedCount = 0;
        UpdateCounterLabel();
    }

    private void HandleDestructibleDestroyed(Destructible destructible)
    {
        if (isReturning)
            return;

        if (requireMatchingTag && !destructible.CompareTag(windmillTag))
            return;

        destroyedCount++;
        UpdateCounterLabel();

        if (destroyedCount >= requiredWindmills)
        {
            CompleteObjective();
        }
    }

    private void CompleteObjective()
    {
        if (isReturning) return;
        isReturning = true;

        if (GameManager.Instance)
            GameManager.Instance.OnAllPillarsActivated();

        if (autoLoadHubOnComplete && !string.IsNullOrEmpty(hubSceneName))
            StartCoroutine(ReturnToHubAfterDelay(returnDelay));
    }

    private IEnumerator ReturnToHubAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        Debug.Log($"[WindmillObjectiveManager] Returning to hub '{hubSceneName}'. SceneFader={(SceneFader.Instance ? "YES" : "NO")}", this);

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(hubSceneName);
        else
            SceneManager.LoadScene(hubSceneName);
    }

    private void UpdateCounterLabel()
    {
        if (counterLabel)
        {
            counterLabel.text = string.Format(counterFormat, destroyedCount, requiredWindmills);
        }
    }
}
