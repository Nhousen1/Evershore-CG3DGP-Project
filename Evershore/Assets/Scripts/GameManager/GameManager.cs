using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [SerializeField] EnemyLife playerLife;
    public static GameManager Instance { get; private set; }

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
    }

    void CheckWinCondition()
    {
        SceneManager.LoadScene("WinScene");
    }

    void OnPlayerDied()
    {
        SceneManager.LoadScene("LoseScene");
    }

}
