using UnityEngine;

public enum LoopEventTriggerType
{
    OnLoad,       // fires automatically when the loop scene starts
    OnEvent       // fires when ProgressionSystem.TriggerEvent(id) is called
}

public enum LoopEventActionType
{
    ActivateObject,     // enable a GameObject
    DeactivateObject,   // disable a GameObject
    ChangeNPCState,     // tell an NPC to enter a new behaviour state
    PlayAmbientSound,   // trigger an ambient audio clip
    ShowScreenEffect    // trigger a visual effect (flash, glitch)
}

[CreateAssetMenu(fileName = "NewLoopEvent", menuName = "TheLastGaze/Loop Event")]
public class LoopEvent : ScriptableObject
{
    [Header("Identity")]
    public string eventID;

    [Header("Trigger")]
    public LoopEventTriggerType triggerType;
    public int requiredLoop;

    [Header("Action")]
    public LoopEventActionType actionType;

    [Header("Target (set based on action type)")]
    public string targetObjectName;  // name of GameObject to find in scene
    public string targetNPCState;    // npc state string if action is ChangeNPCState
    public AudioClip ambientClip;    // audio clip if action is PlayAmbientSound

    [Header("Condition (optional)")]
    public bool requiresClue;
    public string requiredClueID;
}