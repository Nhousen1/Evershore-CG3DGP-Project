using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Audio Fade")]
    [SerializeField] private bool fadeAudio = true;
    private float savedVolume = 1f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetAlpha(0f);
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndSwitch(sceneName));
    }

    private IEnumerator FadeAndSwitch(string sceneName)
    {
        if (fadeAudio)
            savedVolume = AudioListener.volume;

        // Fade out visuals + audio together
        yield return StartCoroutine(FadeVisualAndAudio(0f, 1f, fadeOutAudio: true));

        SceneManager.LoadScene(sceneName);
        yield return null;

        // Fade back in visuals + audio together
        yield return StartCoroutine(FadeVisualAndAudio(1f, 0f, fadeOutAudio: false));
    }

    private IEnumerator FadeVisualAndAudio(float startAlpha, float endAlpha, bool fadeOutAudio)
    {
        float timer = 0f;

        float startVol = fadeAudio ? AudioListener.volume : 1f;
        float endVol = fadeAudio ? (fadeOutAudio ? 0f : savedVolume) : 1f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));

            if (fadeAudio)
                AudioListener.volume = Mathf.Lerp(startVol, endVol, t);

            yield return null;
        }

        SetAlpha(endAlpha);
        if (fadeAudio)
            AudioListener.volume = endVol;
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}
