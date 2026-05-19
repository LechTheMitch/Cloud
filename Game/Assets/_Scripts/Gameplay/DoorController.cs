using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public string doorID;
    public bool isLocked = false;
    public bool requiresLoop = false;
    public int requiredLoop = 0;

    [Header("Transition")]
    public Transform destinationSpawnPoint;
    public Transform alternateSpawnPoint; // optional alternate spawn point for same-scene doors
    public float transitionDelay = 0.5f;

    [Header("Visual")]
    public Sprite openSprite;
    public Sprite closedSprite;
    private SpriteRenderer spriteRenderer;


    [Header("Loop 9 Ending")]
    [SerializeField] private bool isAcceptanceDoor = false;
    [SerializeField] private bool isDenialDoor = false;
    [SerializeField] private EndingTrigger endingTrigger;
    private bool isTransitioning = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    public bool CanUse()
    {
        if (isLocked) return false;
        if (requiresLoop &&
            GameManager.Instance.CurrentLoop < requiredLoop)
            return false;
        return true;
    }

    [System.Obsolete]
    public void Use()
    {
        if (!CanUse() || isTransitioning) return;

        // play door sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDoorOpen();

            
        // at loop 9 doors trigger endings instead of transitioning
        if (GameManager.Instance.CurrentLoop == 9)
        {
            if (isAcceptanceDoor && endingTrigger != null)
            {
                Debug.Log("[DoorController] Loop 9 — Acceptance ending triggered.");
                endingTrigger.TriggerAcceptance();
                return;
            }

            if (isDenialDoor && endingTrigger != null)
            {
                Debug.Log("[DoorController] Loop 9 — Denial ending triggered.");
                endingTrigger.TriggerDenial();
                return;
            }
        }
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;
        Debug.Log($"[DoorController] Using door: {doorID}");


        // placeholder for fade out
        yield return new WaitForSeconds(transitionDelay);

        if (destinationSpawnPoint != null)
        {
            // same scene — teleport directly, no scene reload needed
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Transform target = GetTargetPoint(player.transform.position);
                player.transform.position = target.position;

                Debug.Log($"[DoorController] Player teleported to: {target.position}");
            }
        }
        isTransitioning = false;
        UpdateVisual();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateVisual();
    }
    private Transform GetTargetPoint(Vector3 playerPos)
    {
        float distToA = Vector3.Distance(playerPos, destinationSpawnPoint.position);
        float distToB = Vector3.Distance(playerPos, alternateSpawnPoint.position);

        //Decide which point is closer to the player and return the opposite one
        if (distToA < distToB)
            return alternateSpawnPoint != null ? alternateSpawnPoint : destinationSpawnPoint;
        else
            return destinationSpawnPoint;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        if (isLocked && closedSprite != null)
            spriteRenderer.sprite = closedSprite;
        else if (!isLocked && openSprite != null)
            spriteRenderer.sprite = openSprite;
    }
}