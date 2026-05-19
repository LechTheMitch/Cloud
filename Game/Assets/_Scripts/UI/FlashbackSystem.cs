using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // Ensure New Input System is installed

public class FlashbackSystem : MonoBehaviour
{
    public static FlashbackSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject flashbackPanel;
    [SerializeField] private TextMeshProUGUI flashbackText;

    [Header("Settings")]
    [SerializeField] private float charDelay = 0.04f;
    [SerializeField] private float lineHoldDuration = 1.8f;
    [SerializeField] private float lineFadeOutDuration = 0.6f;
    [SerializeField] private float pauseBetweenLines = 0.3f;

    private bool skipRequested = false;

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
        if (flashbackPanel != null)
            flashbackPanel.SetActive(false);
    }

    public void TriggerFlashback(string[] lines, System.Action onComplete)
    {
        StartCoroutine(FlashbackRoutine(lines, onComplete));
    }

    [System.Obsolete]
    private IEnumerator FlashbackRoutine(string[] lines, System.Action onComplete)
    {
        Debug.Log("[FlashbackSystem] Routine started.");

        if (flashbackPanel == null)
        {
            Debug.LogError("[FlashbackSystem] flashbackPanel is not assigned.");
            yield break;
        }

        // 1. Freeze player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.SetCanMove(false);

        // 2. Initial Fade to black
        if (ScreenEffectsController.Instance != null)
            yield return StartCoroutine(ScreenEffectsController.Instance.FadeToBlack(1f));

        flashbackPanel.SetActive(true);
        flashbackText.text = "";
        flashbackText.color = new Color(1f, 1f, 1f, 0f);

        yield return new WaitForSeconds(0.5f);

        // --- MAIN LOOP ---
        foreach (string line in lines)
        {
            // --- GLOBAL SKIP CHECK ---
            // If the Enter key is pressed, we break out of the entire foreach loop
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                Debug.Log("[FlashbackSystem] Entire flashback skipped via Enter key.");
                break; 
            }

            // Reset text for the new line
            flashbackText.text = "";
            flashbackText.color = new Color(1f, 1f, 1f, 1f);

            // Typewrite the line
            foreach (char c in line)
            {
                // We check for Enter here too so the skip is responsive during typing
                if (Keyboard.current.enterKey.wasPressedThisFrame) goto EndFlashbackLoop;

                flashbackText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            // Hold line
            float timer = 0f;
            while (timer < lineHoldDuration)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame) goto EndFlashbackLoop;
                timer += Time.deltaTime;
                yield return null;
            }

            // Fade out line
            float elapsed = 0f;
            while (elapsed < lineFadeOutDuration)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame) goto EndFlashbackLoop;
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / lineFadeOutDuration);
                flashbackText.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            flashbackText.text = "";
            yield return new WaitForSeconds(pauseBetweenLines);
        }

        // Label for jumping out of nested loops
        EndFlashbackLoop: 

        // 3. Final cleanup and return to game
        flashbackText.text = ""; 
        yield return new WaitForSeconds(0.5f); // Short pause before fading back

        flashbackPanel.SetActive(false);

        if (ScreenEffectsController.Instance != null)
            yield return StartCoroutine(ScreenEffectsController.Instance.FadeFromBlack(1.5f));

        if (player != null) player.SetCanMove(true);

        onComplete?.Invoke();
        Debug.Log("[FlashbackSystem] Flashback complete.");
    }
}