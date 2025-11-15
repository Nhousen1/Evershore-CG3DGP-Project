using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Generic loader that takes a scene name
    public void LoadSceneByName(string sceneName)
    {
        // Make sure timescale is normal in case you paused in-game
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}
