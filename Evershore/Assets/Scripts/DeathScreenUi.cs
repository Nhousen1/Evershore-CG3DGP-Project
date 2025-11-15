using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DeathScreenUI : MonoBehaviour, IGameEventListener
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float fadeTime = 0.2f;
    [SerializeField] private bool pauseOnDeath = true;
    [SerializeField] private GameEvent playerDeathEvent;
    [SerializeField] private bool unlockCursorOnDeath = true;

    bool isShowing;
    CursorLockMode cachedLockMode;
    bool cachedCursorVisible;
    bool cursorModified;

    void Reset()
    {
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void Awake()
    {
        HideInstant();
        restartButton.onClick.AddListener(Restart);
        mainMenuButton.onClick.AddListener(MainMenu);
    }

    void OnEnable()
    {
        if (playerDeathEvent)
        {
            playerDeathEvent.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("DeathScreenUI has no playerDeathEvent assigned.");
        }
    }

    void OnDisable()
    {
        if (playerDeathEvent)
        {
            playerDeathEvent.UnregisterListener(this);
        }
        RestoreCursor();
    }

    public void OnEventRaised()
    {
        HandleDeath();
    }

    void HandleDeath()
    {
        if (isShowing) return;
        isShowing = true;
        if (pauseOnDeath) Time.timeScale = 0f;
        if (unlockCursorOnDeath)
        {
            CacheAndUnlockCursor();
        }
        StartCoroutine(FadeIn());
        // Optionally: set selected button for keyboard/controller
        restartButton.Select();
    }

    void CacheAndUnlockCursor()
    {
        if (cursorModified) return;
        cachedLockMode = Cursor.lockState;
        cachedCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorModified = true;
    }

    void RestoreCursor()
    {
        if (!cursorModified) return;
        Cursor.lockState = cachedLockMode;
        Cursor.visible = cachedCursorVisible;
        cursorModified = false;
    }

    void LateUpdate()
    {
        if (!unlockCursorOnDeath || !isShowing)
            return;

        if (!cursorModified)
        {
            CacheAndUnlockCursor();
            return;
        }

        // Some controllers continuously re-lock the cursor each frame; force our desired state
        if (Cursor.lockState != CursorLockMode.None)
            Cursor.lockState = CursorLockMode.None;
        if (!Cursor.visible)
            Cursor.visible = true;
    }

    System.Collections.IEnumerator FadeIn()
    {
        cg.blocksRaycasts = true;
        cg.interactable = true;
        float t = 0f;
        while (t < fadeTime)
        {
            t += pauseOnDeath ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        cg.alpha = 1f;
    }

    void HideInstant()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        isShowing = false;
        RestoreCursor();
    }

    public void Restart()
    {
        RestoreCursor();
        Time.timeScale = 1f;
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    public void MainMenu()
    {
        RestoreCursor();
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene name not set on DeathScreenUI.");
            return;
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
