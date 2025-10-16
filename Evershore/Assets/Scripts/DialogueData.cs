using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    [TextArea(2, 6)] public string[] lines;
    public Sprite portrait;
}
