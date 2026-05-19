using UnityEngine;
using System.Collections.Generic;

public class ProgressionSystem : MonoBehaviour
{
    public static ProgressionSystem Instance { get; private set; }

    [Header("All Loop Events")]
    [SerializeField] private List<LoopEvent> allLoopEvents;

    private HashSet<string> firedEventIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [System.Obsolete]
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // clear per-loop fired events, then apply all OnLoad events
        firedEventIDs.Clear();
        ApplyOnLoadEvents();
    }

    [System.Obsolete]
    private void ApplyOnLoadEvents()
    {
        int currentLoop = GameManager.Instance.CurrentLoop;
        Debug.Log($"[ProgressionSystem] ApplyOnLoadEvents called. Loop: {currentLoop}");

        foreach (var loopEvent in allLoopEvents)
        {
            Debug.Log($"[ProgressionSystem] Checking event: {loopEvent.eventID} " +
                  $"requiredLoop: {loopEvent.requiredLoop} " +
                  $"triggerType: {loopEvent.triggerType}");
            if (loopEvent.triggerType != LoopEventTriggerType.OnLoad) continue;
            if (loopEvent.requiredLoop > currentLoop) continue;
            if (loopEvent.requiresClue &&
                !ClueManager.Instance.HasClueByID(loopEvent.requiredClueID)) continue;

            ExecuteEvent(loopEvent);
        }
    }

    // call this from anywhere to fire a named event mid-loop
    [System.Obsolete]
    public void TriggerEvent(string eventID)
    {
        if (firedEventIDs.Contains(eventID))
        {
            Debug.Log($"[ProgressionSystem] Event already fired this loop: {eventID}");
            return;
        }

        int currentLoop = GameManager.Instance.CurrentLoop;

        foreach (var loopEvent in allLoopEvents)
        {
            if (loopEvent.eventID != eventID) continue;
            if (loopEvent.triggerType != LoopEventTriggerType.OnEvent) continue;
            if (loopEvent.requiredLoop > currentLoop) continue;
            if (loopEvent.requiresClue &&
                !ClueManager.Instance.HasClueByID(loopEvent.requiredClueID)) continue;

            ExecuteEvent(loopEvent);
            firedEventIDs.Add(eventID);
            return;
        }

        Debug.LogWarning($"[ProgressionSystem] No matching event found for ID: {eventID}");
    }

    [System.Obsolete]
    private void ExecuteEvent(LoopEvent loopEvent)
    {
        Debug.Log($"[ProgressionSystem] Executing: {loopEvent.eventID}");

        // only notify murder timer for OnEvent triggers
        // OnLoad events fire at scene start and should not trigger murder
        if (loopEvent.triggerType == LoopEventTriggerType.OnEvent)
        {
            if (MurderTimer.Instance != null)
                MurderTimer.Instance.NotifyDiscovery(
                    "event", loopEvent.eventID
                );
        }

        switch (loopEvent.actionType)
        {
            case LoopEventActionType.ActivateObject:
                SetObjectActive(loopEvent.targetObjectName, true);
                break;
            case LoopEventActionType.DeactivateObject:
                SetObjectActive(loopEvent.targetObjectName, false);
                break;
            case LoopEventActionType.ChangeNPCState:
                ChangeNPCState(
                    loopEvent.targetObjectName,
                    loopEvent.targetNPCState
                );
                break;
            case LoopEventActionType.PlayAmbientSound:
                PlayAmbientSound(loopEvent.ambientClip);
                break;
            case LoopEventActionType.ShowScreenEffect:
                Debug.Log("[ProgressionSystem] Screen effect placeholder.");
                break;
        }
    }

    private void SetObjectActive(string objectName, bool active)
    {
        // GameObject.Find() can't find disabled objects
        // so we search all objects including inactive ones
        GameObject target = FindInactiveObject(objectName);

        if (target == null)
        {
            Debug.LogWarning($"[ProgressionSystem] GameObject not found: {objectName}");
            return;
        }

        target.SetActive(active);
        Debug.Log($"[ProgressionSystem] {objectName} set active: {active}");
    }

    private GameObject FindInactiveObject(string objectName)
    {
        // find all transforms including inactive ones
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform t in allTransforms)
        {
            // skip prefabs and assets, only look at scene objects
            if (t.hideFlags != HideFlags.None) continue;
            if (t.name == objectName) return t.gameObject;
        }

        return null;
    }

    private void ChangeNPCState(string npcName, string state)
    {
        GameObject npcObj = GameObject.Find(npcName);
        if (npcObj == null)
        {
            Debug.LogWarning($"[ProgressionSystem] NPC not found: {npcName}");
            return;
        }

        NPCBehavior npc = npcObj.GetComponent<NPCBehavior>();
        if (npc == null)
        {
            Debug.LogWarning($"[ProgressionSystem] NPCBehavior not found on: {npcName}");
            return;
        }

        npc.SetState(state);
    }

    private void PlayAmbientSound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[ProgressionSystem] No AudioClip assigned to event.");
            return;
        }
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        Debug.Log($"[ProgressionSystem] Playing ambient sound: {clip.name}");
    }
}