using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("Panel & Widgets")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Image portraitImage;

    [Header("Behavior")]
    public bool closeOnExitRange = true; // player script calls Close() when leaving range if true

    private DialogueData current;
    private int lineIndex;

    public bool IsOpen => dialoguePanel && dialoguePanel.activeSelf;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    public void Show(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0) return;

        current = data;
        lineIndex = 0;

        if (nameText) nameText.text = current.speakerName;
        if (portraitImage)
        {
            portraitImage.enabled = current.portrait != null;
            portraitImage.sprite = current.portrait;
        }

        dialoguePanel.SetActive(true);
        RenderLine();
    }

    public void Advance()
    {
        if (!IsOpen) return;

        lineIndex++;
        if (current == null || lineIndex >= current.lines.Length)
        {
            Close();
            return;
        }
        RenderLine();
    }

    private void RenderLine()
    {
        if (bodyText) bodyText.text = current.lines[lineIndex];
    }

    public void Close()
    {
        dialoguePanel.SetActive(false);
        current = null;
        lineIndex = 0;
    }
}
