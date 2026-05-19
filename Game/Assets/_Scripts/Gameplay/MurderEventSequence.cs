using UnityEngine;
using System.Collections;

public class MurderEventSequence : MonoBehaviour
{
    public static MurderEventSequence Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject victimGameObject;
    [SerializeField] private GameObject murderFlashObject;

    [Header("Timing")]
    [SerializeField] private float gazeDuration = 2.5f;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float holdAfterMurder = 2f;

    private bool hasTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        hasTriggered = false;
    }

    [System.Obsolete]
    public void TriggerMurderSequence()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(MurderRoutine());
    }

    [System.Obsolete]
    private IEnumerator MurderRoutine()
    {
        Debug.Log("[MurderEvent] Murder sequence started.");

        // freeze player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.SetCanMove(false);

        // step 1 � start the gaze dialogue
        DialogueManager.Instance.StartDialogue("victim_gaze");
        // wait untel the dialogue is done
        while (DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }

        //add the scream audio
        if (MurderTimer.Instance != null && MurderTimer.Instance.screamClip != null)
        {
            AudioSource.PlayClipAtPoint(MurderTimer.Instance.screamClip, Camera.main.transform.position);
        }

        // step 2 � flash effect (red for blood)
        if (LightingManager.Instance != null)
            yield return StartCoroutine(
                LightingManager.Instance.FlashEffect(
                    new Color(0.8f, 0f, 0f, 1f),
                    flashDuration
                )
            );

        // step 3 � victim disappears
        if (victimGameObject != null)
            victimGameObject.SetActive(false);

        // step 4 � show aftermath dialogue
        DialogueManager.Instance.StartDialogue("murder_aftermath");
        while (DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }

        // step 6 trigger loop reset
        Debug.Log("[MurderEvent] Triggering loop reset.");
        TimeLoopManager.Instance.TriggerLoopReset();
    }
}