using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central authority for checking end-game conditions and loading the proper ending.
/// Place this in the hub scene and wire references in the Inspector.
/// </summary>
public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Puzzle Completion Flags")]
    [Tooltip("Set true when the wind/windmill puzzle is completed.")]
    public bool windPuzzleCompleted;

    [Tooltip("Set true when the flame/Simon puzzle is completed.")]
    public bool flamePuzzleCompleted;

    [Tooltip("Set true when all required enemies have been defeated.")]
    public bool enemiesCleared;

    [Header("Ending Scene Names")] 
    [Tooltip("Scene to load for the neutral 'Lull' ending.")]
    public string lullEndingScene = "LullEnding";

    [Tooltip("Scene to load if villagers are alerted and aid the ritual.")]
    public string bindingEndingScene = "BindingEnding";

    [Tooltip("Scene to load if player leaves on their own, sacrificing themselves.")]
    public string sacrificeEndingScene = "SacrificeEnding";

    [Header("Hub Feedback")] 
    [Tooltip("Optional message shown when player tries to leave before completing rituals.")]
    [TextArea]
    public string incompleteRitualsMessage = "The restless sea pushes you back.";

    [Tooltip("Optional on-screen label used to show incomplete-rituals feedback.")]
    [SerializeField] private TMPro.TMP_Text feedbackLabel;

    [Header("Ending Choice UI")]
    [Tooltip("Optional UI controller that shows buttons for the final choice.")]
    [SerializeField] private EndingChoiceUI endingChoiceUI;

    /// <summary>
    /// Returns true if both wind and flame puzzles have been completed.
    /// </summary>
    public bool AllRitualsCompleted => windPuzzleCompleted && flamePuzzleCompleted;

    /// <summary>
    /// Called when the player interacts with the boat for the first time.
    /// This decides whether they can leave, and if so which default ending is used.
    /// </summary>
    public void OnBoatTriggered()
    {
        // Rituals incomplete: block departure and show feedback.
        if (!AllRitualsCompleted)
        {
            ShowIncompleteRitualsFeedback();
            return;
        }

        // Rituals done. Now check whether enemies were dealt with.
        if (!enemiesCleared)
        {
            // Player finished puzzles but ignored the threat; use Lull ending.
            LoadEndingSceneSafe(lullEndingScene);
            return;
        }

        // Rituals and enemies both cleared: allow a branching choice handled via dialogue / UI.
        // Here we simply signal that both conditions are met; hook this into your
        // dialogue system to present choices, then call OnVillagersAlertedChoice
        // or OnLeaveOnOwnChoice based on the player's response.
        ShowEndChoices();
    }

    /// <summary>
    /// Call this when the player chooses the dialogue option to alert villagers.
    /// </summary>
    public void OnVillagersAlertedChoice()
    {
        LoadEndingSceneSafe(bindingEndingScene);
    }

    /// <summary>
    /// Call this when the player chooses to leave alone.
    /// </summary>
    public void OnLeaveOnOwnChoice()
    {
        LoadEndingSceneSafe(sacrificeEndingScene);
    }

    void ShowIncompleteRitualsFeedback()
    {
        var text = string.IsNullOrWhiteSpace(incompleteRitualsMessage)
            ? "The restless sea pushes you back."
            : incompleteRitualsMessage;

        if (feedbackLabel)
        {
            feedbackLabel.text = text;
            feedbackLabel.gameObject.SetActive(true);
        }

        Debug.Log(text, this);
    }

    /// <summary>
    /// Hides the incomplete-rituals feedback message from the UI, if any.
    /// Call this when the player leaves the boat trigger.
    /// </summary>
    public void HideIncompleteRitualsFeedback()
    {
        if (feedbackLabel)
        {
            feedbackLabel.gameObject.SetActive(false);
        }
    }

    void ShowEndChoices()
    {
        if (endingChoiceUI)
        {
            endingChoiceUI.Show(OnVillagersAlertedChoice, OnLeaveOnOwnChoice);
        }
        else
        {
            Debug.Log("End conditions met but no EndingChoiceUI assigned; defaulting to Lull ending.", this);
            LoadEndingSceneSafe(lullEndingScene);
        }
    }

    void LoadEndingSceneSafe(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("GameEndManager: Ending scene name is empty; cannot load ending.", this);
            return;
        }

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"GameEndManager: Failed to load ending scene '{sceneName}'. Exception: {ex.Message}", this);
        }
    }
}
