using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenEffectsController : MonoBehaviour
{
    public static ScreenEffectsController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform glitchOverlay;
    [SerializeField] private RawImage glitchImage;
    [SerializeField] private GameObject hallucinationFlash;
    [SerializeField] private Image flashImage;
    [SerializeField] private Image flashRedOverlay;
    [SerializeField] private Image fadePanel;

    [Header("Glitch Settings")]
    [SerializeField] private float glitchBaseInterval = 8f;
    [SerializeField] private float glitchBaseDuration = 0.08f;
    [SerializeField] private float glitchBaseIntensity = 5f;

    [Header("Hallucination Settings")]
    [SerializeField] private Sprite[] hallucinationImages;
    [SerializeField] private float hallucinationInterval = 15f;
    [SerializeField] private float hallucinationDuration = 0.4f;

    private Camera mainCamera;
    private Vector3 originalCameraPos;
    private Coroutine glitchLoopRoutine;
    private Coroutine hallucinationLoopRoutine;
    private int currentLoop = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshCamera();

        // Ensure UI is initialized correctly
        if (glitchImage != null) glitchImage.color = new Color(1, 1, 1, 0);
        if (fadePanel != null) fadePanel.color = new Color(0, 0, 0, 0);
        if (hallucinationFlash != null) hallucinationFlash.SetActive(false);
    }

    private void RefreshCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
            originalCameraPos = mainCamera.transform.localPosition;
    }

    [System.Obsolete]
    public void OnLoopChanged(int loop)
    {
        currentLoop = loop;
        StopAllLoopRoutines();

        // Progressive escalation
        if (loop >= 3)
            glitchLoopRoutine = StartCoroutine(GlitchLoop());

        if (loop >= 5)
            hallucinationLoopRoutine = StartCoroutine(HallucinationLoop());

        if (loop == 9)
            TriggerLoop9Flashback();
    }

    private void StopAllLoopRoutines()
    {
        if (glitchLoopRoutine != null) StopCoroutine(glitchLoopRoutine);
        if (hallucinationLoopRoutine != null) StopCoroutine(hallucinationLoopRoutine);

        // Reset visuals to clean state
        if (mainCamera != null) mainCamera.transform.localPosition = originalCameraPos;
        if (glitchImage != null) glitchImage.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator GlitchLoop()
    {
        while (true)
        {
            float interval = Mathf.Max(1.5f, glitchBaseInterval - (currentLoop * 0.8f));
            yield return new WaitForSeconds(Random.Range(interval * 0.5f, interval * 1.5f));
            yield return PlayGlitch();
        }
    }

    private IEnumerator PlayGlitch()
    {
        float intensity = glitchBaseIntensity + (currentLoop * 2f);
        float duration = glitchBaseDuration + (currentLoop * 0.01f);

        // Use a temporary offset so we don't break the camera's follow logic
        Vector3 shakeOffset = Vector3.zero;

        for (int i = 0; i < Random.Range(2, 4 + currentLoop); i++)
        {
            if (glitchImage != null)
            {
                glitchImage.color = new Color(
                    Random.Range(0.6f, 1f),
                    0f,
                    Random.Range(0f, 0.2f),
                    Random.Range(0.06f, 0.18f)
                );
            }

            if (mainCamera != null)
            {
                shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * (intensity * 0.02f);
                mainCamera.transform.localPosition += shakeOffset;
            }

            yield return new WaitForSeconds(duration);

            // Reset the offset immediately so the camera follow script can take over
            if (mainCamera != null)
                mainCamera.transform.localPosition -= shakeOffset;
            glitchImage.color = new Color(1, 1, 1, 0);

            yield return new WaitForSeconds(duration * 0.5f);
        }
    }

    private IEnumerator HallucinationLoop()
    {
        yield return new WaitForSeconds(Random.Range(5f, hallucinationInterval));
        while (true)
        {
            yield return PlayHallucination();
            float wait = Random.Range(hallucinationInterval * 0.5f, hallucinationInterval * 1.5f);
            yield return new WaitForSeconds(wait);
        }
    }

    private IEnumerator PlayHallucination()
    {
        if (hallucinationFlash == null) yield break;

        hallucinationFlash.SetActive(true);

        if (flashImage != null && hallucinationImages.Length > 0)
        {
            flashImage.sprite = hallucinationImages[Random.Range(0, hallucinationImages.Length)];
        }

        // Fade In & Out logic is fine, but consider using a CanvasGroup for easier alpha management
        yield return StartCoroutine(LerpAlpha(0f, 0.7f, hallucinationDuration * 0.5f));
        yield return new WaitForSeconds(0.05f);
        yield return PlayGlitch();
        yield return LerpAlpha(0.7f, 0f, hallucinationDuration * 0.5f);

        hallucinationFlash.SetActive(false);
    }

    private IEnumerator LerpAlpha(float start, float end, float time)
    {
        float elapsed = 0;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(start, end, elapsed / time);
            if (flashRedOverlay != null) flashRedOverlay.color = new Color(0.7f, 0f, 0f, a);
            if (flashImage != null) flashImage.color = new Color(1, 1, 1, a * 0.7f);
            yield return null;
        }
    }

    [System.Obsolete]
    private void TriggerLoop9Flashback()
    {
        Debug.Log("[ScreenEffectsController] Triggering loop 9 flashback.");
        StopAllLoopRoutines();

        // cancel murder timer — flashback takes over at loop 9
        if (MurderTimer.Instance != null)
            MurderTimer.Instance.CancelTimer();

        if (FlashbackSystem.Instance == null)
        {
            Debug.LogError("[ScreenEffectsController] FlashbackSystem.Instance " +
                        "is null. Add FlashbackSystem GameObject to the scene.");
            return;
        }
        string[] flashbackLines = new string[]
        {
            "You loved her.",
            "That part was real.",
            "She said she needed space.",
            "You gave her the apartment next door.",
            "You thought that was enough.",
            "It wasn't.",
            "She found someone else.",
            "It was you.",
            "It was always you.",
            "What happens now is the only choice",
        };

        if (FlashbackSystem.Instance != null)
            FlashbackSystem.Instance.TriggerFlashback(
                flashbackLines,
                () => Debug.Log("[Loop9] Flashback done. Player has the choice.")
            );
    }

    // ── Fade System ────────────────────────────────────────────
    public IEnumerator FadeToBlack(float duration)
    {
        Debug.Log("[ScreenEffects] FadeToBlack started.");

        if (fadePanel == null)
        {
            Debug.LogError("[ScreenEffects] fadePanel is null. " +
                           "Assign it in the Inspector.");
            yield break;
        }

        Debug.Log($"[ScreenEffects] fadePanel active: {fadePanel.gameObject.activeSelf}");
        yield return StartCoroutine(Fade(0f, 1f, duration));
        Debug.Log("[ScreenEffects] FadeToBlack complete.");
    }

    public IEnumerator FadeFromBlack(float duration)
    {
        Debug.Log("[ScreenEffects] FadeFromBlack started.");

        if (fadePanel == null)
        {
            Debug.LogError("[ScreenEffects] fadePanel is null.");
            yield break;
        }

        yield return StartCoroutine(Fade(1f, 0f, duration));
        Debug.Log("[ScreenEffects] FadeFromBlack complete.");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0f, 0f, 0f, to);
    }

    // ── Public triggers ────────────────────────────────────────
    public void TriggerManualGlitch()
    {
        StartCoroutine(PlayGlitch());
    }

    public void TriggerManualHallucination()
    {
        StartCoroutine(PlayHallucination());
    }
}