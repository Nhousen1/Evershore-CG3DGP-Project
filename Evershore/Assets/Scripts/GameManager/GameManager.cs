using UnityEngine;
using UnityEngine.SceneManagement;

/* Author: Liam Housenbold, Marcus King
 * Date created: 10/1/2025
 * Date last updated: 11/15/2025
 * Summary: handles all major Game Events in a scene, mainly by calling scene switches.
 */
public class GameManager : Singleton<GameManager>
{
    public override bool defineScenePersistence()
    {
        return true;
    }
    //The listener references are not used, but they are here to check if the developer actually
    //added a listener for the corresponding event.
    [Header("Events")]
    public UnityGameEventListener PillarsActivatedListener;
    public UnityGameEventListener PlayerDeathListener;

    [Header("Scenes")]
    public string winScene;
    public string loseScene;

    void Start()
    {
        if (!PillarsActivatedListener || !PlayerDeathListener) 
        { 
            Debug.LogWarning("Missing event listener components for win manager"); 
        }
    }
    //Functions implementing the game events you want to handle. These will be called by the listeners attached to the GameManager prefab.
    public void OnAllPillarsActivated()
    {
        SceneManager.LoadScene(winScene);
    }

    public void OnPlayerDied()
    {
        //SceneManager.LoadScene(loseScene);
    }
}