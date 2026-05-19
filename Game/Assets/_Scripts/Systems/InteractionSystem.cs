using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    private Interactable currentInteractable;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.IsPlaying()) return;
        if (DialogueManager.Instance.IsDialogueActive()) return;

        DetectInteractable();
        HandleInput();
    }

    private void DetectInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRadius,
            interactableLayer
        );

        Interactable closest = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable == null) continue;
            if (!interactable.CanInteract()) continue;

            float distance = Vector2.Distance(
                transform.position,
                hit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        // switched to a new interactable
        if (closest != currentInteractable)
        {
            currentInteractable?.HidePrompt();
            currentInteractable = closest;
            currentInteractable?.ShowPrompt();
        }
    }

    private void HandleInput()
    {
        
        // block all interaction during active dialogue
        if (DialogueManager.Instance.IsDialogueActive()) return;

        if (Keyboard.current.eKey.wasPressedThisFrame ||
            (Gamepad.current != null &&
             Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            if (currentInteractable != null)
                currentInteractable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}