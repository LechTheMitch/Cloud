using UnityEngine;

public enum NPCState
{
    Idle,
    Suspicious,
    Fearful,
    Hostile,
    Unaware,
    Dead
}

public class NPCBehavior : MonoBehaviour
{
    [Header("Identity")]
    public string npcID;

    [Header("State")]
    public NPCState currentState = NPCState.Idle;

    [Header("Loop Behaviour")]
    [SerializeField] private int firstReactionLoop = 3;

    private void Start()
    {
        ApplyLoopState();
    }

    // called by ProgressionSystem when a ChangeNPCState event fires
    public void SetState(string stateName)
    {
        if (System.Enum.TryParse(stateName, out NPCState parsed))
        {
            currentState = parsed;
            Debug.Log($"[NPCBehavior] {npcID} state set to: {currentState}");
            OnStateChanged();
        }
        else
        {
            Debug.LogWarning($"[NPCBehavior] Unknown state: {stateName} on {npcID}");
        }
    }

    // applies a default state based on loop count at scene load
    private void ApplyLoopState()
    {
        int loop = GameManager.Instance.CurrentLoop;

        if (loop == 0)
            currentState = NPCState.Unaware;
        else if (loop >= firstReactionLoop && loop < 7)
            currentState = NPCState.Suspicious;
        else if (loop >= 7)
            currentState = NPCState.Fearful;

        Debug.Log($"[NPCBehavior] {npcID} initialised with state: " +
                  $"{currentState} on loop #{loop}");
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        // hook movement, animation, and dialogue reaction here
        // as those systems are built in later sprints
        switch (currentState)
        {
            case NPCState.Idle:
                Debug.Log($"[NPCBehavior] {npcID} is idle.");
                break;
            case NPCState.Suspicious:
                Debug.Log($"[NPCBehavior] {npcID} is acting suspicious.");
                break;
            case NPCState.Fearful:
                Debug.Log($"[NPCBehavior] {npcID} is fearful of the player.");
                break;
            case NPCState.Hostile:
                Debug.Log($"[NPCBehavior] {npcID} is hostile.");
                break;
            case NPCState.Unaware:
                Debug.Log($"[NPCBehavior] {npcID} is unaware of events.");
                break;
            case NPCState.Dead:
                Debug.Log($"[NPCBehavior] {npcID} is dead.");
                gameObject.SetActive(false);
                break;
        }
    }
}