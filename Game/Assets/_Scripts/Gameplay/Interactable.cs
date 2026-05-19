using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Settings")]
    public string interactableName;
    [TextArea(2, 4)]
    public string promptText;
    public bool isOneTimeUse;

    [Header("Loop Condition")]
    public bool hasLoopRequirement;
    public int requiredLoop;
    public bool requiresExactLoop; // if true, player must be on the exact loop to interact

    [Header("Loop 9 Prompt Override")]
    [SerializeField] private bool hasPromptOverride;
    [SerializeField] private int promptOverrideLoop;
    [SerializeField] private string overridePromptText;

    [Header("Dialogue")]
    public bool startsDialogue;
    public string npcID;

    [Header("Clue")]
    public bool collectsClue;
    public ClueData clueToCollect;

    [Header("Events")]
    public UnityEvent onInteract;

    private bool hasBeenUsed = false;

    private void OnEnable()
    {
        hasBeenUsed = false;
    }

    public bool CanInteract()
    {
        if (isOneTimeUse && hasBeenUsed) return false;

        if (hasLoopRequirement)
        {
            if (requiresExactLoop &&
                GameManager.Instance.CurrentLoop != requiredLoop)
                return false;

            if (!requiresExactLoop &&
                GameManager.Instance.CurrentLoop < requiredLoop)
                return false;
        }

        return true;
    }

    [System.Obsolete]
    public void Interact()
    {
        if (!CanInteract()) return;

        Debug.Log($"[Interactable] Player interacted with: {interactableName}");

        // handle dialogue directly — no UnityEvent needed
        if (startsDialogue && !string.IsNullOrEmpty(npcID))
        {
            if (!DialogueManager.Instance.IsDialogueActive())
                DialogueManager.Instance.StartDialogue(npcID);
        }

        // handle clue collection directly
        if (collectsClue && clueToCollect != null)
        {
            ClueManager.Instance.CollectClue(clueToCollect);
            // play clue pickup sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCluePickup();
        }
        // fire any additional custom events
        onInteract?.Invoke();

        if (isOneTimeUse) hasBeenUsed = true;
    }

    public void ShowPrompt()
    {
        if (InteractionPromptUI.Instance == null) return;

        // use override prompt at specified loop
        if (hasPromptOverride &&
            GameManager.Instance.CurrentLoop == promptOverrideLoop)
        {
            InteractionPromptUI.Instance.ShowPrompt(overridePromptText);
            return;
        }

        InteractionPromptUI.Instance.ShowPrompt(promptText);
    }

    public void HidePrompt()
    {
        if (InteractionPromptUI.Instance != null)
            InteractionPromptUI.Instance.HidePrompt();
    }
}