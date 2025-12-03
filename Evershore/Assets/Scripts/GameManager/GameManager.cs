using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/* Author: Liam Housenbold, Marcus King
 * Date created: 10/1/2025
 * Date last updated: 12/3/2025
 * Summary: Central hub for ritual/enemy completion events and ending selection via UnityGameEventListeners.
 */
public class GameManager : Singleton<GameManager>
{
    public override bool defineScenePersistence()
    {
        return true;
    }

    [Header("Events")]
    public UnityGameEventListener PillarsActivatedListener;
    public UnityGameEventListener FlamePuzzleCompletedListener;
    public UnityGameEventListener PlayerDeathListener;

    private bool windRitualCompleted;
    private bool flameRitualCompleted;

    [Header("Ending Scene Names")]
    [SerializeField] private string lullEndingScene = "LullEnding";
    [SerializeField] private string bindingEndingScene = "BindingEnding";
    [SerializeField] private string sacrificeEndingScene = "SacrificeEnding";

    [Header("Hub Feedback")]
    [TextArea]
    [SerializeField] private string incompleteRitualsMessage = "The restless sea pushes you back.";
    [SerializeField] private TMP_Text feedbackLabel;

    [Header("Ending Choice UI")]
    [SerializeField] private EndingChoiceUI endingChoiceUI;

    [Header("Fallback UI (Optional)")]
    [Tooltip("When true the manager will spawn basic UI overlays if scene references are missing.")]
    [SerializeField] private bool createFallbackUI = true;

    [Header("Debug/Tracking")]
    public int enemiesKilled = 0; // optional external tracking

    // Runtime-created UI references when inspector fields are not assigned.
    private Canvas runtimeCanvas;
    private TMP_Text runtimeFeedbackLabel;
    private GameObject runtimeChoicePanel;
    private Button runtimeBindingButton;
    private Button runtimeSacrificeButton;

    public bool AllRitualsCompleted => windRitualCompleted && flameRitualCompleted;

    void Start()
    {
        if (!PillarsActivatedListener || !FlamePuzzleCompletedListener)
        {
            Debug.LogWarning("GameManager: Missing critical UnityGameEventListeners (pillars/flame).");
        }
        if (!PlayerDeathListener)
        {
            Debug.LogWarning("GameManager: Player death listener not assigned (optional).");
        }
    }

    #region Puzzle/Event Hooks

    public void OnAllPillarsActivated()
    {
        windRitualCompleted = true;
        Debug.Log("GameManager: Wind ritual completed.");
    }

    public void OnFlamePuzzleCompleted()
    {
        flameRitualCompleted = true;
        Debug.Log("GameManager: Flame ritual completed.");
    }

    public void OnPlayerDied()
    {
        enemiesKilled = 0;
        Debug.Log("GameManager: Player died; enemiesKilled reset to 0.");
    }

    #endregion

    #region Ending Flow

    public void OnBoatTriggered()
    {
        Debug.Log($"[GM] Boat triggered. AllRitualsCompleted={AllRitualsCompleted}, enemiesKilled={enemiesKilled}", this);

        if (!AllRitualsCompleted)
        {
            Debug.Log("[GM] → Rituals incomplete, showing feedback.", this);
            ShowIncompleteRitualsFeedback();
            return;
        }

        if (enemiesKilled <= 0)
        {
            Debug.Log("[GM] → enemiesKilled <= 0, loading lull ending.", this);
            LoadEndingSceneSafe(lullEndingScene);
            return;
        }

        Debug.Log("[GM] → Showing end choices.", this);
        ShowEndChoices();
    }

    public void OnVillagersAlertedChoice()
    {
        endingChoiceUI?.Hide();
        HideRuntimeChoiceUI();
        LoadEndingSceneSafe(bindingEndingScene);
    }

    public void OnLeaveOnOwnChoice()
    {
        endingChoiceUI?.Hide();
        HideRuntimeChoiceUI();
        LoadEndingSceneSafe(sacrificeEndingScene);
    }

    public void HideIncompleteRitualsFeedback()
    {
        if (feedbackLabel)
        {
            feedbackLabel.gameObject.SetActive(false);
        }

        if (runtimeFeedbackLabel)
        {
            runtimeFeedbackLabel.gameObject.SetActive(false);
        }
    }

    void ShowIncompleteRitualsFeedback()
    {
        var text = string.IsNullOrWhiteSpace(incompleteRitualsMessage)
            ? "The restless sea pushes you back."
            : incompleteRitualsMessage;

        var targetLabel = ResolveFeedbackLabel();
        if (targetLabel)
        {
            targetLabel.text = text;
            targetLabel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager: No feedback label assigned; showing message only via console log.", this);
        }

        Debug.Log(text, this);
    }

    void ShowEndChoices()
    {
        HideIncompleteRitualsFeedback();

        if (endingChoiceUI)
        {
            endingChoiceUI.Show(OnVillagersAlertedChoice, OnLeaveOnOwnChoice);
            return;
        }

        if (TryShowFallbackChoices())
        {
            return;
        }

        Debug.LogWarning("GameManager: No EndingChoiceUI assigned and fallback disabled; defaulting to binding ending.", this);
        LoadEndingSceneSafe(bindingEndingScene);
    }

    void LoadEndingSceneSafe(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("GameManager: Ending scene name is empty; cannot load ending.", this);
            return;
        }

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"GameManager: Failed to load ending scene '{sceneName}'. Exception: {ex.Message}", this);
        }
    }

    #endregion

    TMP_Text ResolveFeedbackLabel()
    {
        if (feedbackLabel)
        {
            return feedbackLabel;
        }

        if (!createFallbackUI)
        {
            return null;
        }

        if (!runtimeFeedbackLabel)
        {
            var canvas = EnsureRuntimeCanvas();
            if (!canvas)
            {
                return null;
            }

            runtimeFeedbackLabel = BuildRuntimeFeedbackLabel(canvas);
        }

        return runtimeFeedbackLabel;
    }

    Canvas EnsureRuntimeCanvas()
    {
        if (runtimeCanvas)
        {
            return runtimeCanvas;
        }

        if (!createFallbackUI)
        {
            return null;
        }

        var canvasGO = new GameObject("GameManagerRuntimeCanvas", typeof(RectTransform));
        runtimeCanvas = canvasGO.AddComponent<Canvas>();
        runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runtimeCanvas.sortingOrder = 5000;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        return runtimeCanvas;
    }

    TMP_Text BuildRuntimeFeedbackLabel(Canvas parentCanvas)
    {
        var go = new GameObject("IncompleteRitualsFallbackText", typeof(RectTransform));
        go.transform.SetParent(parentCanvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.18f);
        rect.anchorMax = rect.anchorMin;
        rect.sizeDelta = new Vector2(720f, 96f);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 32f;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.text = string.Empty;
        text.gameObject.SetActive(false);

        return text;
    }

    bool TryShowFallbackChoices()
    {
        if (!createFallbackUI)
        {
            return false;
        }

        if (!runtimeChoicePanel)
        {
            var canvas = EnsureRuntimeCanvas();
            if (!canvas)
            {
                return false;
            }

            CreateRuntimeChoiceUI(canvas);
        }

        if (!runtimeChoicePanel || runtimeBindingButton == null || runtimeSacrificeButton == null)
        {
            return false;
        }

        runtimeChoicePanel.SetActive(true);

        runtimeBindingButton.onClick.RemoveAllListeners();
        runtimeBindingButton.onClick.AddListener(OnVillagersAlertedChoice);

        runtimeSacrificeButton.onClick.RemoveAllListeners();
        runtimeSacrificeButton.onClick.AddListener(OnLeaveOnOwnChoice);

        return true;
    }

    void CreateRuntimeChoiceUI(Canvas parentCanvas)
    {
        runtimeChoicePanel = new GameObject("EndingChoiceFallbackPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        runtimeChoicePanel.transform.SetParent(parentCanvas.transform, false);

        var rect = runtimeChoicePanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = rect.anchorMin;
        rect.sizeDelta = new Vector2(520f, 260f);
        rect.anchoredPosition = Vector2.zero;

        var bg = runtimeChoicePanel.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.78f);

        var header = new GameObject("ChoiceHeader", typeof(RectTransform));
        header.transform.SetParent(runtimeChoicePanel.transform, false);
        var headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(480f, 70f);
        headerRect.anchoredPosition = new Vector2(0f, -30f);

        var headerText = header.AddComponent<TextMeshProUGUI>();
        headerText.text = "The sea calms. Choose your fate.";
        headerText.fontSize = 28f;
        headerText.color = Color.white;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.enableWordWrapping = true;

        runtimeBindingButton = CreateRuntimeChoiceButton("AlertVillagersButton", "Alert Villagers", new Vector2(0f, -110f));
        runtimeSacrificeButton = CreateRuntimeChoiceButton("LeaveAloneButton", "Leave Alone", new Vector2(0f, -190f));

        runtimeChoicePanel.SetActive(false);
    }

    Button CreateRuntimeChoiceButton(string name, string label, Vector2 anchoredPosition)
    {
        var buttonGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(runtimeChoicePanel.transform, false);

        var rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 60f);
        rect.anchoredPosition = anchoredPosition;

        var image = buttonGO.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.15f);

        var button = buttonGO.GetComponent<Button>();
        button.targetGraphic = image;

        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(buttonGO.transform, false);
        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var text = labelGO.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 26f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        return button;
    }

    void HideRuntimeChoiceUI()
    {
        if (runtimeChoicePanel)
        {
            runtimeChoicePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Debug hotkeys for quickly testing endings
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnAllPillarsActivated();
            Debug.Log("Debug: Forced wind ritual complete via key 1.", this);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnFlamePuzzleCompleted();
            Debug.Log("Debug: Forced flame ritual complete via key 2.", this);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            enemiesKilled++;
            Debug.Log($"Debug: Forced enemy kill via key 3. enemiesKilled={enemiesKilled}", this);
        }
    }
}