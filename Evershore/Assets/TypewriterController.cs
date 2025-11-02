using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public class TypewriterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI typingText;
    [SerializeField] private GameObject continueHint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSfx;

    [Header("Typing Settings")]
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float punctuationHold = 0.25f;
    [SerializeField] private float sentenceHold = 0.60f;
    [SerializeField] private bool playSfxOnEveryChar = false;

    private bool _isTyping;
    private bool _fastForwardRequested;

    public void PlayText(string fullText)
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine(fullText));
    }

    private IEnumerator PlayRoutine(string fullText)
    {
        if (typingText == null) yield break;

        typingText.text = string.Empty;
        if (continueHint) continueHint.SetActive(false);

        var sentences = SplitIntoSentences(fullText);

        for (int s = 0; s < sentences.Count; s++)
        {
            yield return StartCoroutine(TypeSentence(sentences[s]));
            yield return new WaitForSeconds(sentenceHold);
            if (continueHint) continueHint.SetActive(true);
            yield return StartCoroutine(WaitForAnyPress());
            if (continueHint) continueHint.SetActive(false);
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        _fastForwardRequested = false;

        var builder = new StringBuilder(typingText.text);
        float delay = 1f / Mathf.Max(1f, charsPerSecond);
        int i = 0;

        while (i < sentence.Length)
        {
            if (_fastForwardRequested)
            {
                builder.Append(sentence.Substring(i));
                typingText.text = builder.ToString();
                break;
            }

            if (sentence[i] == '<')
            {
                int close = sentence.IndexOf('>', i);
                if (close < 0) close = i;
                builder.Append(sentence, i, close - i + 1);
                i = close + 1;
                typingText.text = builder.ToString();
                continue;
            }

            char c = sentence[i];
            builder.Append(c);
            typingText.text = builder.ToString();

            if (typeSfx && audioSource)
            {
                if (playSfxOnEveryChar || ShouldBlip(c))
                {
                    audioSource.pitch = 1f + Random.Range(-0.04f, 0.04f);
                    audioSource.PlayOneShot(typeSfx, 0.9f);
                }
            }

            if (IsMidPunctuation(c))
                yield return new WaitForSeconds(punctuationHold);
            else
                yield return new WaitForSeconds(delay);

            if (Input.anyKeyDown)
                _fastForwardRequested = true;

            i++;
        }

        _isTyping = false;
        yield break;
    }

    private IEnumerator WaitForAnyPress()
    {
        yield return null;
        while (!Input.anyKeyDown) yield return null;
    }

    private static bool ShouldBlip(char c)
    {
        if (char.IsWhiteSpace(c)) return false;
        if (c == '<' || c == '>') return false;
        return true;
    }

    private static bool IsMidPunctuation(char c)
    {
        return c == ',' || c == ';' || c == ':';
    }

    private static System.Collections.Generic.List<string> SplitIntoSentences(string text)
    {
        var list = new System.Collections.Generic.List<string>();
        var pattern = @"(?<=[\.!\?])\s+";
        foreach (var piece in Regex.Split(text.Trim(), pattern))
            if (!string.IsNullOrWhiteSpace(piece)) list.Add(piece);
        if (list.Count == 0 && !string.IsNullOrWhiteSpace(text)) list.Add(text);
        return list;
    }

    [TextArea(4, 12)]
    public string demoText;

    [ContextMenu("Play Demo Text")]
    private void PlayDemo()
    {
        if (!string.IsNullOrEmpty(demoText))
            PlayText(demoText);
    }
}
