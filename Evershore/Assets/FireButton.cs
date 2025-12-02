using UnityEngine;

public class FireButton : MonoBehaviour
{
    public int fireIndex;                  // 0–4
    public AudioSource audioSource;        // assigned per fire
    public SimonPuzzleManager puzzle;

    private bool playerNear = false;

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            audioSource.Play();
            puzzle.PlayerSelect(fireIndex);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            puzzle.ShowPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            puzzle.ShowPrompt(false);
        }
    }
}
