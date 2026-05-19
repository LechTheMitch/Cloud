using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueLine", menuName = "TheLastGaze/Dialogue Line")]
public class DialogueLine : ScriptableObject
{
    [Header("Identity")]
    public string npcID;
    public int loopIndex;

    [Header("Content")]
    [TextArea(3, 6)]
    public string[] lines;

    [Header("Conditions")]
    public bool requiresClue;
    public string requiredClueID;
}