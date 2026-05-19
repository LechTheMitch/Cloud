using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LoopTimer_0",
                 menuName = "TheLastGaze/Loop Timer")]
public class LoopTimerData : ScriptableObject
{
    [Header("Identity")]
    public int loopIndex;

    [Header("Timer")]
    public float duration = 90f;

    [Header("Murder Sequence")]
    public bool playScream = true;
    public bool playRedFlash = true;
    public float screamDelay = 0f;
    public float flashDuration = 0.5f;
    public float holdAfterMurder = 2f;

    [Header("Override Dialogue")]
    public bool hasCustomMurderDialogue;
    public string customMurderNpcID;

    [Header("Event Triggers")]
    [Tooltip("Clue IDs that trigger immediate murder when collected")]
    public List<string> criticalClueIDs = new List<string>();

    [Tooltip("Event IDs that trigger immediate murder when fired")]
    public List<string> criticalEventIDs = new List<string>();

    [Header("Early Trigger Settings")]
    [Tooltip("Delay after critical discovery before murder fires")]
    public float reactionDelay = 1.5f
    ;
}