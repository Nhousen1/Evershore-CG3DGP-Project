using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TypewriterDemoAllInOne : MonoBehaviour
{
    [Header("Text Styling")]
    [SerializeField] private TMP_FontAsset fontAsset;
    [Header("Demo Text (edit in Inspector)")]
    [TextArea(6, 20)]
    public string sampleText =
@"This is a sample typing scene. It types characters with a short sound.
It pauses after each sentence and waits for any key to continue.
Press any key during typing to fast-reveal the current sentence.";

    [Header("Typing Settings")]
    public float charsPerSecond = 45f;
    public float punctuationHold = 0.25f;   // for , ; :
    public float sentenceHold = 0.60f;    // small beat after . ! ?
    public bool sfxEveryChar = false;    // if false, skips whitespace etc.

    [Header("Flow")] 
    [Tooltip("Optional scene name to load automatically when all text has finished.")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio (optional)")]
    public AudioClip typeSfx;               // assign a short “tick” if you have one

    // Runtime refs
    private Canvas _canvas;
    private TextMeshProUGUI _typingText;
    private TextMeshProUGUI _continueHint;
    private AudioSource _audio;
    private bool _fastForward;

    private void Start()
    {
        BuildUIIfNeeded();
        StartCoroutine(PlayRoutine(sampleText));
    }

    private void BuildUIIfNeeded()
    {
        // Canvas
        var canvasGO = new GameObject("TypewriterCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = canvasGO.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        // Background panel (dim)
        var panelGO = new GameObject("Panel", typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImg = panelGO.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.70f); // translucent black

        // Typing text
        var typingGO = new GameObject("TypingText", typeof(TextMeshProUGUI));
        typingGO.transform.SetParent(canvasGO.transform, false);
        _typingText = typingGO.GetComponent<TextMeshProUGUI>();
        var tRect = _typingText.rectTransform;
        tRect.anchorMin = new Vector2(0f, 0f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = new Vector2(48f, 120f);  // left/bottom padding
        tRect.offsetMax = new Vector2(-48f, -160f); // right/top padding
        _typingText.alignment = TextAlignmentOptions.TopLeft;
    _typingText.textWrappingMode = TextWrappingModes.Normal;
        _typingText.richText = true;
        _typingText.fontSize = 38;
        _typingText.text = "";

        // Continue hint
        var hintGO = new GameObject("ContinueHint", typeof(TextMeshProUGUI));
        hintGO.transform.SetParent(canvasGO.transform, false);
        _continueHint = hintGO.GetComponent<TextMeshProUGUI>();
        var hRect = _continueHint.rectTransform;
        hRect.anchorMin = new Vector2(1f, 0f);
        hRect.anchorMax = new Vector2(1f, 0f);
        hRect.pivot = new Vector2(1f, 0f);
        hRect.anchoredPosition = new Vector2(-48f, 48f);
        _continueHint.alignment = TextAlignmentOptions.BottomRight;
        _continueHint.fontSize = 24;
        _continueHint.text = ">";
        _continueHint.alpha = 0f; // hidden until pause

        // Apply custom font if one is assigned in the Inspector
        if (fontAsset != null)
        {
            _typingText.font = fontAsset;
            _continueHint.font = fontAsset;
        }

        // Audio
        _audio = canvasGO.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
    }

    private IEnumerator PlayRoutine(string fullText)
    {
        _typingText.text = "";
        SetHintVisible(false);

        var sentences = SplitIntoSentences(fullText);
        if (sentences.Count == 0)
            sentences.Add(fullText);

        for (int i = 0; i < sentences.Count; i++)
        {
            yield return StartCoroutine(TypeSentence(sentences[i]));

            // Restore the natural space that was stripped by sentence splitting
            if (i < sentences.Count - 1)
            {
                var currentText = _typingText.text;
                if (!string.IsNullOrEmpty(currentText) && !char.IsWhiteSpace(currentText[currentText.Length - 1]))
                {
                    _typingText.text = currentText + " ";
                }
            }
            yield return new WaitForSeconds(sentenceHold);
            SetHintVisible(true);
            yield return StartCoroutine(WaitForAnyPress());
            SetHintVisible(false);
        }

        // All sentences complete; reset run state and auto-return to main menu if configured.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            try
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"TypewriterDemoAllInOne: Failed to load scene '{mainMenuSceneName}'. Exception: {ex.Message}", this);
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _fastForward = false;
        var sb = new StringBuilder(_typingText.text);
        float perCharDelay = 1f / Mathf.Max(1f, charsPerSecond);
        int i = 0;

        while (i < sentence.Length)
        {
            if (_fastForward)
            {
                sb.Append(sentence.Substring(i));
                _typingText.text = sb.ToString();
                break;
            }

            if (sentence[i] == '<') // instantly add TMP tags
            {
                int close = sentence.IndexOf('>', i);
                if (close < 0) close = i;
                sb.Append(sentence, i, close - i + 1);
                i = close + 1;
                _typingText.text = sb.ToString();
                continue;
            }

            char c = sentence[i];
            sb.Append(c);
            _typingText.text = sb.ToString();

            // Blip
            if (typeSfx && _audio)
            {
                if (sfxEveryChar || ShouldBlip(c))
                {
                    _audio.pitch = 1f + Random.Range(-0.04f, 0.04f);
                    _audio.PlayOneShot(typeSfx, 0.9f);
                }
            }

            if (IsMidPunctuation(c)) yield return new WaitForSeconds(punctuationHold);
            else yield return new WaitForSeconds(perCharDelay);

            if (Input.anyKeyDown) _fastForward = true;
            i++;
        }
    }

    private IEnumerator WaitForAnyPress()
    {
        // Skip a frame so a fast-forward tap doesn't auto-advance
        yield return null;
        while (!Input.anyKeyDown) yield return null;
    }

    private void SetHintVisible(bool on)
    {
        _continueHint.alpha = on ? 1f : 0f;
    }

    // Helpers
    private static bool ShouldBlip(char c)
    {
        if (char.IsWhiteSpace(c)) return false;
        return c != '<' && c != '>';
    }
    private static bool IsMidPunctuation(char c) => c == ',' || c == ';' || c == ':';

    private static List<string> SplitIntoSentences(string text)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return list;
        // Split on whitespace following ., !, or ?
        var parts = Regex.Split(text.Trim(), @"(?<=[\.!\?])\s+");
        foreach (var p in parts) if (!string.IsNullOrWhiteSpace(p)) list.Add(p);
        return list;
    }
}
