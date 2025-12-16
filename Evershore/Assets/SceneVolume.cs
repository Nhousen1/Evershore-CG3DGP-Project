using UnityEngine;
using System.Collections;

public class SceneVolume : MonoBehaviour
{
    [Header("Scene Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float sceneVolume = 0.6f;

    [SerializeField] private float muteDuration = 1f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private float previousVolume;

    void OnEnable()
    {
        previousVolume = AudioListener.volume;

        // Start muted
        AudioListener.volume = 0f;

        StartCoroutine(FadeInAfterDelay());
    }

    void OnDisable()
    {
        // Restore previous volume when scene unloads
        AudioListener.volume = previousVolume;
    }

    private IEnumerator FadeInAfterDelay()
    {
        // Fully muted at scene start
        yield return new WaitForSecondsRealtime(muteDuration);

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;
            AudioListener.volume = Mathf.Lerp(0f, sceneVolume, timer / fadeInDuration);
            yield return null;
        }

        AudioListener.volume = sceneVolume;
    }
}