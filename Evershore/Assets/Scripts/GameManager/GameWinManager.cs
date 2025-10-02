using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

public class GameWinManager : MonoBehaviour
{
    [SerializeField] EnemyLife playerLife;
    public static GameWinManager Instance { get; private set; }

    // singleton pattern persisting across scenes
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist this GameObject across scenes
    }
    void Start()
    {
        playerLife.onDeath.AddListener(OnPlayerDied);
        // subscribe to pillar win event (static UnityEvent on PillarInteractable)
        PillarInteractable.onAllPillarsActivated.AddListener(OnAllPillarsActivated);
    }

    private void OnDisable()
    {
        // unsubscribe to avoid duplicate calls or leaked listeners
        PillarInteractable.onAllPillarsActivated.RemoveListener(OnAllPillarsActivated);
    }

    // Called when pillars fire the global win events
    private void OnAllPillarsActivated()
    {
        SceneManager.LoadScene("WinScene");
    }

    void OnPlayerDied()
    {
        SceneManager.LoadScene("LoseScene");
    }

}