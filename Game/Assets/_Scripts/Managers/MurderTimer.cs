using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MurderTimer : MonoBehaviour
{
    public static MurderTimer Instance { get; private set; }

    [Header("Loop Timer Data")]
    [SerializeField] private List<LoopTimerData> loopTimers;

    [Header("Murder SFX")]
    [SerializeField] public AudioClip screamClip;

    private Coroutine timerCoroutine;
    private bool hasFired = false;
    private LoopTimerData currentData;
    private float timerStartTime;
    private bool acceptingTriggers = false;
    private float gracePeriod = 3f; // seconds before event triggers are live

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
        UnityEngine.SceneManagement.SceneManager
            .sceneLoaded += OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager
            .sceneLoaded -= OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnSceneLoaded(
    UnityEngine.SceneManagement.Scene scene,
    UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name != "LastGaze") return;

        hasFired = false;
        acceptingTriggers = false;
        StopAllCoroutines();

        int loop = GameManager.Instance.CurrentLoop;
        currentData = GetTimerData(loop);

        if (currentData == null || currentData.duration <= 0) return;

        timerStartTime = Time.time;
        timerCoroutine = StartCoroutine(MurderTimerRoutine(currentData));

        // start accepting triggers after grace period
        StartCoroutine(EnableTriggersAfterDelay());

        Debug.Log($"[MurderTimer] Loop {loop} started " +
                  $"— {currentData.duration}s " +
                  $"— triggers live in {gracePeriod}s");
    }

    private IEnumerator EnableTriggersAfterDelay()
    {
        yield return new WaitForSeconds(gracePeriod);
        acceptingTriggers = true;
        Debug.Log("[MurderTimer] Event triggers now active.");
    }

    // ── Timer Routine ─────────────────────────────────────────
    [System.Obsolete]
    private IEnumerator MurderTimerRoutine(LoopTimerData data)
    {
        Debug.Log($"[MurderTimer] Waiting {data.duration}s...");
        yield return new WaitForSeconds(data.duration);
        Debug.Log("[MurderTimer] Timer expired — checking dialogue.");

        if (DialogueManager.Instance.IsDialogueActive())
        {
            Debug.Log("[MurderTimer] Dialogue active — waiting...");
            yield return new WaitUntil(
                () => !DialogueManager.Instance.IsDialogueActive()
            );
            yield return new WaitForSeconds(0.5f);
        }

        if (hasFired)
        {
            Debug.Log("[MurderTimer] Already fired — skipping.");
            yield break;
        }

        hasFired = true;
        Debug.Log("[MurderTimer] Firing murder sequence.");
        yield return StartCoroutine(FireMurderSequence(data, false));
    }

    // ── Event Trigger — called externally ─────────────────────
    // source: "clue" or "event"
    // id: the clueID or eventID that triggered this
    [System.Obsolete]
    public void NotifyDiscovery(string source, string id)
    {
        if (hasFired) return;
        if (currentData == null) return;

        // block triggers during scene load grace period
        if (!acceptingTriggers)
        {
            Debug.Log($"[MurderTimer] Trigger blocked during grace period: " +
                      $"{source} — {id}");
            return;
        }

        bool shouldTrigger = false;

        if (source == "clue" &&
            currentData.criticalClueIDs.Contains(id))
        {
            shouldTrigger = true;
            Debug.Log($"[MurderTimer] Critical clue: {id} " +
                      $"— early trigger.");
        }
        else if (source == "event" &&
                 currentData.criticalEventIDs.Contains(id))
        {
            shouldTrigger = true;
            Debug.Log($"[MurderTimer] Critical event: {id} " +
                      $"— early trigger.");
        }

        if (!shouldTrigger) return;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        hasFired = true;

        float elapsed = Time.time - timerStartTime;
        Debug.Log($"[MurderTimer] Early trigger at {elapsed:F1}s");

        StartCoroutine(EarlyMurderRoutine(currentData));
    }

    [System.Obsolete]
    private IEnumerator EarlyMurderRoutine(LoopTimerData data)
    {
        // reaction delay — brief moment before murder fires
        // player just found something, then BOOM
        yield return new WaitForSeconds(data.reactionDelay);

        // wait for dialogue if active
        if (DialogueManager.Instance.IsDialogueActive())
        {
            yield return new WaitUntil(
                () => !DialogueManager.Instance.IsDialogueActive()
            );
        }

        yield return StartCoroutine(FireMurderSequence(data, true));
    }

    // ── Core Murder Sequence ──────────────────────────────────
    // isEarlyTrigger: true = player caused this by discovering something
    [System.Obsolete]
    private IEnumerator FireMurderSequence(
        LoopTimerData data,
        bool isEarlyTrigger)
    {
        Debug.Log($"[MurderTimer] Murder firing — " +
                  $"loop #{GameManager.Instance.CurrentLoop} " +
                  $"early: {isEarlyTrigger}");

        // freeze player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.SetCanMove(false);

        // early trigger gets a extra glitch burst first
        // — the world reacts to the discovery
        if (isEarlyTrigger &&
            ScreenEffectsController.Instance != null)
        {
            ScreenEffectsController.Instance.TriggerManualGlitch();
            yield return new WaitForSeconds(0.3f);
            ScreenEffectsController.Instance.TriggerManualGlitch();
            yield return new WaitForSeconds(0.2f);
        }

        // scream delay
        if (data.screamDelay > 0)
            yield return new WaitForSeconds(data.screamDelay);

        // scream
        if (data.playScream && screamClip != null)
            AudioSource.PlayClipAtPoint(
                screamClip,
                Camera.main.transform.position
            );

        // silence all other audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceAll();

        // red flash
        if (data.playRedFlash && LightingManager.Instance != null)
            yield return StartCoroutine(
                LightingManager.Instance.FlashEffect(
                    new Color(0.8f, 0f, 0f, 1f),
                    data.flashDuration
                )
            );

        // glitch burst
        if (ScreenEffectsController.Instance != null)
            ScreenEffectsController.Instance.TriggerManualGlitch();

        // custom murder dialogue
        if (data.hasCustomMurderDialogue &&
            !string.IsNullOrEmpty(data.customMurderNpcID))
        {
            DialogueManager.Instance
                .StartDialogue(data.customMurderNpcID);

            yield return new WaitUntil(
                () => !DialogueManager.Instance.IsDialogueActive()
            );
        }

        yield return new WaitForSeconds(data.holdAfterMurder);

        TimeLoopManager.Instance.TriggerLoopReset();
    }

    // ── Cancel (loop 9) ───────────────────────────────────────
    public void CancelTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        hasFired = false;
        Debug.Log("[MurderTimer] Timer cancelled.");
    }

    private LoopTimerData GetTimerData(int loop)
    {
        return loopTimers.Find(t => t.loopIndex == loop);
    }
}