using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* Author: Marcus King
* Date Created: 10-14-2025
* Date Modified: 10-16-2025
* Summary: Robust Singleton pattern implementation
*/
//Inspiration From: https://gamedevbeginner.com/singletons-in-unity-the-right-way/
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    public abstract bool defineScenePersistence();
    private bool isScenePersistent;

    protected virtual void Awake()
    {
        isScenePersistent = defineScenePersistence();

        if (Instance == null)
        {
            Instance = this as T;
        }
        else if (Instance != this)
        {
            Debug.LogWarning(gameObject.name + " is an illegal singleton instance. Removing singleton component...");
            Destroy(gameObject);
            return; // important: stop running on the destroyed duplicate
        }

        if (isScenePersistent)
        {
            DontDestroyOnLoad(Instance.gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}