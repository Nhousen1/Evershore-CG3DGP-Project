using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SimonPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public int sequenceLength = 5;
    public float flashDuration = 0.5f;
    public float delayBetweenFlashes = 0.3f;

    [Header("References")]
    [SerializeField] private ParticleSystem pillarParticles;
    [SerializeField] private bool hideFlameUntilPuzzleStarts = true;
    public TMP_Text interactPrompt;
    [Tooltip("Map each firewood Destructible to its Simon index (0..N-1)")]
    [SerializeField] private FireInput[] fireInputs;
    [SerializeField] private bool allowDamageDrivenInput = true;

    [Header("Colors & Sounds")]
    [Tooltip("Index 0 = Blue, 1 = Pink, 2 = Green, 3 = Orange, 4 = Purple")]
    [SerializeField] private Color[] colors;
    [SerializeField] private AudioClip[] sequenceClips;
    [SerializeField] private AudioSource sequenceAudioSource;
    [SerializeField] private AudioClip lighterClip;
    [SerializeField] private AudioSource lighterAudioSource;
    [Header("Result Sounds")]
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioSource successAudioSource;
    [SerializeField] private AudioClip failureClip;
    [SerializeField] private AudioSource failureAudioSource;
    [SerializeField] private AudioSource fallbackOutcomeAudioSource;

    [Header("Progression")]
    [SerializeField] private int sequencesRequired = 3;
    [SerializeField] private string hubSceneName = "IslandHub 1";
    [Header("Game End Integration")]
    [Tooltip("Optional GameEvent raised when the flame puzzle fully completes.")]
    [SerializeField] private GameEvent flameCompleteEvent;
    [Header("Debugging")]
    [SerializeField] private bool logPlayerChoices = false;

    private readonly List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private bool showingSequence = false;
    private bool puzzleComplete = false;
    private bool puzzleStarted = false;
    private bool awaitingPlayerInput = false;
    private Coroutine sequenceRoutine;
    private ParticleSystem.MinMaxGradient defaultPillarColor;
    private bool hasDefaultColor = false;
    private int completedSequenceCount = 0;
    private readonly Queue<int> pendingInputs = new Queue<int>();
    private readonly Dictionary<Destructible, int> destructibleToIndex = new Dictionary<Destructible, int>();

    [Header("Triggering")]
    [SerializeField] private bool startOnPillarAttack = true;
    [SerializeField] private Destructible triggerPillar;

    //---------------------------------------------------------
    // INITIALIZATION
    //---------------------------------------------------------
    void Awake()
    {
        ResolvePillarParticles();
        BuildFireInputMap();
        CacheDefaultColor();
        if (hideFlameUntilPuzzleStarts)
        {
            EnableDefaultFlame(false);
        }
    }

    void OnValidate()
    {
        ResolvePillarParticles();
    }

    void ResolvePillarParticles()
    {
        if (!pillarParticles && triggerPillar)
        {
            pillarParticles = triggerPillar.GetComponentInChildren<ParticleSystem>(true);
        }
    }

    //---------------------------------------------------------
    // FLAME CONTROL
    //---------------------------------------------------------
    void EnsureFlameReady()
    {
        if (!pillarParticles)
            return;
        if (!pillarParticles.gameObject.activeSelf)
        {
            EnableDefaultFlame(true);
            ResetPillarParticleColor();
        }
    }

    void EnableDefaultFlame(bool enable)
    {
        if (!pillarParticles)
            return;
        var go = pillarParticles.gameObject;
        if (go.activeSelf == enable)
            return;

        go.SetActive(enable);
        pillarParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (enable)
        {
            pillarParticles.Play();
        }
    }

    void SetPillarParticleColor(Color color)
    {
        if (!pillarParticles)
            return;
        var main = pillarParticles.main;
        main.startColor = ForceOpaque(color);
    }

    void ResetPillarParticleColor()
    {
        if (!pillarParticles || !hasDefaultColor)
            return;
        var main = pillarParticles.main;
        main.startColor = defaultPillarColor;
    }

    void CacheDefaultColor()
    {
        if (!pillarParticles || hasDefaultColor)
            return;
        var main = pillarParticles.main;
        defaultPillarColor = main.startColor;
        hasDefaultColor = true;
    }

    Color ForceOpaque(Color c)
    {
        return new Color(c.r, c.g, c.b, 1f);
    }

    //---------------------------------------------------------
    // START PUZZLE (called from trigger or from UI)
    //---------------------------------------------------------
    public void StartPuzzle()
    {
        if (puzzleComplete)
            return;

        if (!puzzleStarted)
        {
            puzzleStarted = true;
        }

        EnsureFlameReady();
        RestartSequencePlayback();
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            BuildFireInputMap();
        }
        Destructible.OnAnyDestructibleDamaged += HandleDestructibleDamage;
    }

    void OnDisable()
    {
        Destructible.OnAnyDestructibleDamaged -= HandleDestructibleDamage;
    }

    private void HandleDestructibleDamage(Destructible destructible)
    {
        if (!destructible)
            return;

        // 1) First see if this hit came from one of the color fires
        if (TryHandleFireDamage(destructible))
            return;

        // 2) Otherwise treat it as a hit on the central pillar that controls playback
        Debug.Log($"Simon puzzle pillar damage event from {destructible.name}", this);

        if (!startOnPillarAttack || puzzleComplete)
            return;

        if (triggerPillar && destructible != triggerPillar)
            return;

        if (!puzzleStarted)
        {
            StartPuzzle();
        }
        else
        {
            RestartSequencePlayback();
        }
    }

    //---------------------------------------------------------
    // PLAY THE SIMON SAYS SEQUENCE
    //---------------------------------------------------------
    IEnumerator PlaySequence()
    {
        showingSequence = true;
        CancelPlayerTurn();
        playerIndex = 0;
        EnsureFlameReady();

        if (!EnsureSequence())
        {
            showingSequence = false;
            sequenceRoutine = null;
            EnableDefaultFlame(false);
            yield break;
        }

        // Play flashes one by one
        for (int i = 0; i < sequence.Count; i++)
        {
            int index = sequence[i];

            // Flash ON
            PlayLighterSound();
            SetPillarParticleColor(colors[index]);
            PlaySound(index);

            yield return new WaitForSeconds(flashDuration);

            // Flash OFF
            ResetPillarParticleColor();

            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        showingSequence = false;
        sequenceRoutine = null;
        EnableDefaultFlame(false);
        PreparePlayerTurn();
    }

    //---------------------------------------------------------
    // PLAYER CLICKS/INTERACTS WITH A FIRE
    //---------------------------------------------------------
    public void PlayerSelect(int fireIndex)
    {
        if (showingSequence || puzzleComplete) return;

        if (!awaitingPlayerInput)
        {
            Debug.LogWarning("SimonPuzzleManager: Ignoring player input because the puzzle is not ready for input.", this);
            return;
        }

        if (sequence.Count == 0)
        {
            Debug.LogWarning("SimonPuzzleManager: Ignoring player input because no sequence is ready yet.", this);
            return;
        }

        if (fireIndex < 0 || fireIndex >= colors.Length)
        {
            Debug.LogWarning($"SimonPuzzleManager: Fire index {fireIndex} is invalid for available colors.", this);
            HandlePlayerMistake();
            return;
        }

        if (pendingInputs.Count == 0)
        {
            Debug.LogWarning("SimonPuzzleManager: Pending input queue empty while player is allowed to respond. Resetting state.", this);
            HandlePlayerMistake();
            return;
        }

        int expected = pendingInputs.Peek();
        if (logPlayerChoices)
        {
            Debug.Log($"SimonPuzzleManager: Player selected {fireIndex}, expected {expected} (step {playerIndex + 1}/{sequence.Count}).", this);
        }

        // correct input
        if (fireIndex == expected)
        {
            pendingInputs.Dequeue();
            playerIndex++;

            if (logPlayerChoices)
            {
                Debug.Log($"SimonPuzzleManager: Sequence correct for {playerIndex}/{sequence.Count} steps.", this);
            }

            // entire sequence matched
            if (pendingInputs.Count == 0)
            {
                HandleSequenceCompleted();
            }
        }
        else
        {
            // WRONG — restart same sequence
            HandlePlayerMistake();
        }
    }

    bool EnsureSequence()
    {
        if (sequenceLength <= 0)
        {
            sequenceLength = Mathf.Max(1, sequenceLength);
        }

        // Ensure colors array has at least 5 slots in the fixed order we expect.
        EnsureColorArray();

        if (sequenceClips == null || sequenceClips.Length < colors.Length)
        {
            Debug.LogWarning("SimonPuzzleManager: sequenceClips missing or shorter than colors; sequence audio may fail.", this);
        }

        if (colors.Length == 1 && sequenceLength > 1)
        {
            Debug.LogWarning("SimonPuzzleManager: only one color provided; sequence will repeat the same color every step.", this);
        }

        if (sequence.Count == sequenceLength)
            return true;

        sequence.Clear();
        int lastIndex = -1;
        for (int i = 0; i < sequenceLength; i++)
        {
            int nextIndex = GetRandomColorIndex(lastIndex);
            sequence.Add(nextIndex);
            lastIndex = nextIndex;
        }

        if (logPlayerChoices)
        {
            Debug.Log($"SimonPuzzleManager: Generated sequence {FormatSequence(sequence)}", this);
        }

        return true;
    }

    void PlaySound(int index)
    {
        if (sequenceClips == null || sequenceClips.Length == 0) return;
        if (index < 0 || index >= sequenceClips.Length) return;

        var clip = sequenceClips[index];
        if (!clip)
            return;

        if (sequenceAudioSource)
        {
            sequenceAudioSource.Stop();
            sequenceAudioSource.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    void PlayLighterSound()
    {
        if (!lighterClip)
            return;

        if (lighterAudioSource)
        {
            lighterAudioSource.Stop();
            lighterAudioSource.PlayOneShot(lighterClip);
        }
        else if (sequenceAudioSource)
        {
            sequenceAudioSource.PlayOneShot(lighterClip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(lighterClip, transform.position);
        }
    }

    void HandleSequenceCompleted()
    {
        awaitingPlayerInput = false;
        completedSequenceCount++;
        int required = Mathf.Max(1, sequencesRequired);
        PlayOutcomeSound(successClip, successAudioSource);

        if (completedSequenceCount >= required)
        {
            puzzleComplete = true;
            Debug.Log("SIMON PUZZLE COMPLETE - returning to hub");

            if (flameCompleteEvent != null)
            {
                try
                {
                    flameCompleteEvent.Raise();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SimonPuzzleManager: Exception raising flameCompleteEvent: {e}");
                }
            }

            if (GameManager.Instance)
            {
                GameManager.Instance.OnFlamePuzzleCompleted();
            }

            EnsureFlameReady();
            SetPillarParticleColor(Color.green);
            StartCoroutine(ReturnToHubAfterDelay(0.5f));
            return;
        }

        // Prepare next round with a brand new sequence
        sequence.Clear();
        playerIndex = 0;
        RestartSequencePlayback();
    }

    void LoadHubScene()
    {
        if (string.IsNullOrWhiteSpace(hubSceneName))
        {
            Debug.LogWarning("SimonPuzzleManager: hubSceneName is empty, cannot return to hub.", this);
            return;
        }

        try
        {
            Debug.Log($"SimonPuzzleManager: Returning to hub '{hubSceneName}' (using SceneFader if available)", this);

            if (SceneFader.Instance != null)
                SceneFader.Instance.FadeToScene(hubSceneName);
            else
                SceneManager.LoadScene(hubSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SimonPuzzleManager: Failed to load scene '{hubSceneName}'. Exception: {ex.Message}", this);
        }
    }


    int GetRandomColorIndex(int lastIndex)
    {
        // We always treat indices as 0..(colors.Length-1). If colors is
        // not yet initialised, EnsureColorArray will be called before
        // sequence generation.
        if (colors == null || colors.Length == 0)
            return 0;

        if (colors.Length == 1)
            return 0;

        int candidate;
        do
        {
            candidate = Random.Range(0, colors.Length);
        } while (candidate == lastIndex);

        return candidate;
    }

    /// <summary>
    /// Ensures the colors array exists and that the first five slots follow
    /// the agreed convention: 0=Blue,1=Pink,2=Green,3=Orange,4=Purple.
    /// Any existing user-assigned colors are preserved where possible.
    /// </summary>
    void EnsureColorArray()
    {
        const int required = 5;

        if (colors == null || colors.Length < required)
        {
            var old = colors;
            colors = new Color[required];

            // Copy any existing values into the new array
            if (old != null)
            {
                int copy = Mathf.Min(old.Length, required);
                for (int i = 0; i < copy; i++)
                {
                    colors[i] = old[i];
                }
            }
        }

        // If specific slots are still default (0,0,0,0), assign sensible
        // defaults for the canonical palette the puzzle expects.
        if (IsUninitializedColor(colors[0])) colors[0] = Color.blue;                 // 0 = Blue
        if (IsUninitializedColor(colors[1])) colors[1] = new Color(1.0f, 0.4f, 0.8f); // 1 = Pink
        if (IsUninitializedColor(colors[2])) colors[2] = Color.green;                // 2 = Green
        if (IsUninitializedColor(colors[3])) colors[3] = new Color(1.0f, 0.55f, 0.0f);// 3 = Orange
        if (IsUninitializedColor(colors[4])) colors[4] = new Color(0.6f, 0.0f, 1.0f); // 4 = Purple
    }

    bool IsUninitializedColor(Color c)
    {
        return Mathf.Approximately(c.r, 0f)
               && Mathf.Approximately(c.g, 0f)
               && Mathf.Approximately(c.b, 0f)
               && Mathf.Approximately(c.a, 0f);
    }

        void PlayOutcomeSound(AudioClip clip, AudioSource preferredSource)
        {
            if (preferredSource)
            {
                preferredSource.Stop();
                if (clip)
                {
                    preferredSource.PlayOneShot(clip);
                }
                else if (preferredSource.clip)
                {
                    preferredSource.Play();
                }
                else
                {
                    Debug.LogWarning($"SimonPuzzleManager: No clip assigned for {preferredSource.name}", this);
                }
                return;
            }

            AudioClip resolvedClip = clip;
            if (!resolvedClip)
            {
                Debug.LogWarning("SimonPuzzleManager: Outcome sound requested but no clip provided.", this);
                return;
            }

            if (fallbackOutcomeAudioSource)
            {
                fallbackOutcomeAudioSource.Stop();
                fallbackOutcomeAudioSource.PlayOneShot(resolvedClip);
                return;
            }

            AudioSource.PlayClipAtPoint(resolvedClip, transform.position);
        }

    void RestartSequencePlayback()
    {
            CancelPlayerTurn();
        EnsureFlameReady();

        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
            showingSequence = false;
        }

        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    void PreparePlayerTurn()
    {
        pendingInputs.Clear();
        foreach (int step in sequence)
        {
            pendingInputs.Enqueue(step);
        }

        playerIndex = 0;
        awaitingPlayerInput = pendingInputs.Count > 0;

        if (awaitingPlayerInput && logPlayerChoices)
        {
            Debug.Log($"SimonPuzzleManager: Player turn ready. Sequence length {pendingInputs.Count}. Pending {FormatQueue(pendingInputs)}", this);
        }
    }

    void CancelPlayerTurn()
    {
        awaitingPlayerInput = false;
        pendingInputs.Clear();
        playerIndex = 0;
    }

    void HandlePlayerMistake()
    {
        PlayOutcomeSound(failureClip, failureAudioSource);
        CancelPlayerTurn();
        sequence.Clear();
        if (logPlayerChoices)
        {
            Debug.Log("SimonPuzzleManager: Failure detected. Clearing sequence and starting a new one.", this);
        }
        RestartSequencePlayback();
    }

    bool TryHandleFireDamage(Destructible destructible)
    {
        if (!allowDamageDrivenInput)
            return false;

        if (destructibleToIndex.TryGetValue(destructible, out int index))
        {
            if (logPlayerChoices)
            {
                Debug.Log($"SimonPuzzleManager: Damage detected on fire '{destructible.name}' mapped to index {index}.", this);
            }
            PlayerSelect(index);
            return true;
        }

        return false;
    }

    [System.Serializable]
    struct FireInput
    {
        public Destructible destructible;
        public int fireIndex;
    }

    void BuildFireInputMap()
    {
        destructibleToIndex.Clear();

        if (fireInputs != null)
        {
            foreach (var entry in fireInputs)
            {
                if (!entry.destructible) continue;
                if (destructibleToIndex.ContainsKey(entry.destructible)) continue;
                destructibleToIndex.Add(entry.destructible, Mathf.Max(0, entry.fireIndex));
            }
        }

        var fireButtons = FindObjectsOfType<FireButton>(true);
        foreach (var button in fireButtons)
        {
            if (!button) continue;
            if (button.puzzle != this) continue;

            var destructible = button.GetComponent<Destructible>()
                               ?? button.GetComponentInParent<Destructible>()
                               ?? button.GetComponentInChildren<Destructible>();

            if (!destructible) continue;

            if (destructibleToIndex.ContainsKey(destructible))
                continue;

            int idx = Mathf.Max(0, button.fireIndex);
            destructibleToIndex.Add(destructible, idx);
        }

        if (logPlayerChoices)
        {
            Debug.Log($"SimonPuzzleManager: Fire mapping ready with {destructibleToIndex.Count} entries.", this);
        }
    }


    string FormatSequence(IEnumerable<int> seq)
    {
        return string.Join(",", seq);
    }

    string FormatQueue(Queue<int> queue)
    {
        return string.Join(",", queue.ToArray());
    }

    //---------------------------------------------------------
    // FIRE BUTTONS CALL THIS
    //---------------------------------------------------------
    public void ShowPrompt(bool state)
    {
        if (!interactPrompt)
        {
            Debug.LogWarning("SimonPuzzleManager: interactPrompt reference missing", this);
            return;
        }
        interactPrompt.gameObject.SetActive(state);
    }

    private IEnumerator ReturnToHubAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(hubSceneName);
        else
            SceneManager.LoadScene(hubSceneName);
    }
}


