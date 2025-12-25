using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(LineRenderer))]
public class bulletTrail : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Gradient originalGradient;
    [HideInInspector]
    public ObjectPool<bulletTrail> pool; //Injected

    [Header("Effect Settings")]
    [SerializeField]
    private float fadeTime = 5;
    private float currentFadeTime = 0;

    // Start is called before the first frame update
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        originalGradient = lineRenderer.colorGradient;
        if(fadeTime <= 0)
        {
            Debug.LogError("Fadetime for bullet trail must be greater than 0");
        }
    }
    public void Play(Vector3 start, Vector3 end)
    {
        StopAllCoroutines();
        currentFadeTime = fadeTime;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lineRenderer.colorGradient = originalGradient;

        StartCoroutine(fadeTrail());
    }
    IEnumerator fadeTrail()
    {
       while(currentFadeTime > 0) 
       { 
            currentFadeTime -= Time.deltaTime;

            Gradient g = lineRenderer.colorGradient;

            GradientAlphaKey[] alphaKeys = g.alphaKeys;

            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = originalGradient.alphaKeys[i].alpha * currentFadeTime / fadeTime;
            }

            g.alphaKeys = alphaKeys;
            lineRenderer.colorGradient = g;

            yield return null;
       }
       pool.Release(this);
    }
}
