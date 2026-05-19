using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimeLoopManager : MonoBehaviour
{
    public static TimeLoopManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float resetDelay = 0.5f;
    [SerializeField] private string gameSceneName = "LastGaze";
    [SerializeField] private float fadeOutDuration = 1.2f;
    [SerializeField] private float fadeInDuration = 1.5f;

    private bool isResetting = false;

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
    public void TriggerLoopReset()
    {
        if (isResetting) return;
        isResetting = true;

        Debug.Log("[TimeLoopManager] Loop reset triggered.");
        GameManager.Instance.ChangeState(GameState.LoopResetting);

        // sync loop count to API
        SyncLoopToAPI();

        StartCoroutine(ResetRoutine());
    }

    private void SyncLoopToAPI()
    {
        if (ApiClient.Instance == null) return;
        if (PlayerSessionManager.Instance == null) return;
        if (!PlayerSessionManager.Instance.IsLoggedIn) return;

        GameSaveData currentSave = PlayerSessionManager.Instance.CreateSaveSnapshot();

        ApiClient.Instance.SaveGame(
            currentSave,
            success => Debug.Log(success? 
                $"[TimeLoopManager] Loop synced to API successfully: Loop #{currentSave.currentLoop}"
              : $"[TimeLoopManager] Loop sync failed."
            )
        );
    }

    [System.Obsolete]
    private IEnumerator ResetRoutine()
    {
        Debug.Log("[TimeLoopManager] ResetRoutine started.");
        // freeze player immediately
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.SetCanMove(false);

        if (ScreenEffectsController.Instance == null)
        {
            Debug.LogError("[TimeLoopManager] ScreenEffectsController.Instance " +
                           "is null — fade will be skipped.");
            yield return new WaitForSeconds(fadeOutDuration);
        }
        else
        {
            Debug.Log("[TimeLoopManager] Starting FadeToBlack.");
            yield return StartCoroutine(
                ScreenEffectsController.Instance.FadeToBlack(fadeOutDuration)
            );
            Debug.Log("[TimeLoopManager] FadeToBlack complete.");
        }

        // brief hold on black
        yield return new WaitForSeconds(resetDelay);

        isResetting = false;
        Debug.Log($"[TimeLoopManager] Reloading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName); ;
    }

    [System.Obsolete]
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [System.Obsolete]
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            Debug.Log($"[TimeLoopManager] Scene loaded. " +
                      $"Loop #{GameManager.Instance.CurrentLoop}");

            if (GameManager.Instance.CurrentState == GameState.LoopResetting)
            {
                GameManager.Instance.ChangeState(GameState.Playing);

                if (LightingManager.Instance != null)
                    LightingManager.Instance.OnLoopChanged();

                if (HUDController.Instance != null)
                    HUDController.Instance.UpdateHUD();

                if (ScreenEffectsController.Instance != null)
                    ScreenEffectsController.Instance.OnLoopChanged(
                        GameManager.Instance.CurrentLoop
                    );

                if (AudioManager.Instance != null)
                    AudioManager.Instance.OnLoopChanged(
                        GameManager.Instance.CurrentLoop
                    );

                // fade in after scene loads
                StartCoroutine(FadeInAfterLoad());
            }
        }
    }
    private IEnumerator FadeInAfterLoad()
    {
        Debug.Log("[TimeLoopManager] FadeInAfterLoad started.");
        // small delay so scene has fully initialised
        yield return new WaitForSeconds(0.1f);

        if (ScreenEffectsController.Instance == null)
        {
            Debug.LogError("[TimeLoopManager] ScreenEffectsController null " +
                           "during fade in.");
            yield break;
        }

        Debug.Log("[TimeLoopManager] Starting FadeFromBlack.");
        if (ScreenEffectsController.Instance != null)
        {
            yield return StartCoroutine(
                ScreenEffectsController.Instance.FadeFromBlack(fadeInDuration)
            );
        }
        Debug.Log("[TimeLoopManager] FadeFromBlack complete.");

        Debug.Log("[TimeLoopManager] Fade in complete. Loop running.");
    }
}