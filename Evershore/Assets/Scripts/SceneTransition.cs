using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private bool requirePlayerTag = true;
    
    private bool hasTriggered = false;

    void Start()
    {

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("SceneTransition: No collider found. Please add a collider and set it as trigger.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"SceneTransition: Something entered the trigger - {other.name} with tag '{other.tag}'");
        
        // Check if the colliding object is the player
        if (hasTriggered) 
        {
            Debug.Log("SceneTransition: Already triggered, ignoring.");
            return;
        }
        
        bool isPlayer = false;
        if (requirePlayerTag)
        {
            isPlayer = other.CompareTag("Player");
            Debug.Log($"SceneTransition: Checking Player tag - Result: {isPlayer}");
        }
        else
        {
            // Check if it has PlayerMovement component as alternative
            isPlayer = other.GetComponent<PlayerMovement>() != null;
            Debug.Log($"SceneTransition: Checking PlayerMovement component - Result: {isPlayer}");
        }
        
        if (isPlayer)
        {
            Debug.Log("SceneTransition: Player detected! Starting scene transition...");
            hasTriggered = true;
            StartCoroutine(TransitionToScene());
        }
        else
        {
            Debug.Log($"SceneTransition: Not a player. Object: {other.name}, Tag: {other.tag}");
        }
    }
    
    private IEnumerator TransitionToScene()
    {
        Debug.Log($"SceneTransition: Starting transition to '{targetSceneName}' in {transitionDelay} seconds...");
        
        yield return new WaitForSeconds(transitionDelay);
        
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"SceneTransition: Loading scene '{targetSceneName}'");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneTransition: Target scene name is not set!");
        }
    }
    
    // Method to manually trigger the transition (can be called from other scripts)
    public void TriggerTransition()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(TransitionToScene());
        }
    }
}
