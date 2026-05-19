using UnityEngine;

[CreateAssetMenu(fileName = "NewClue", menuName = "TheLastGaze/Clue Data")]
public class ClueData : ScriptableObject
{
    [Header("Identity")]
    public string clueID;
    public string clueName;

    [Header("Content")]
    [TextArea(3, 6)]
    public string description;
    public Sprite icon;

    [Header("Progression")]
    public int discoveredOnLoop;
    public bool isCritical;

    [Header("Foreshadowing")]
    [TextArea(2, 4)]
    public string hintText;
}