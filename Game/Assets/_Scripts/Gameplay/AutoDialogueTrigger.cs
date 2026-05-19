using UnityEngine;

public class AutoDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string npcID;

    [Header("Loop Condition")]
    [SerializeField] private bool hasLoopRequirement;
    [SerializeField] private int requiredLoop;
    [SerializeField] private bool exactLoopOnly;

    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        // reset each loop so it can trigger again
        hasTriggered = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered && triggerOnce) return;
        if (DialogueManager.Instance.IsDialogueActive()) return;
        if (!GameManager.Instance.IsPlaying()) return;
        if (!MeetsLoopCondition()) return;

        hasTriggered = true;
        DialogueManager.Instance.StartDialogue(npcID);

        Debug.Log($"[AutoDialogueTrigger] Triggered dialogue: {npcID}");
    }

    private bool MeetsLoopCondition()
    {
        if (!hasLoopRequirement) return true;

        int loop = GameManager.Instance.CurrentLoop;

        if (exactLoopOnly) return loop == requiredLoop;
        return loop >= requiredLoop;
    }
}