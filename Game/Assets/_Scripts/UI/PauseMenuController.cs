using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pauseMenuBackground;
    [Header("Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    

    public static bool isPaused = false;

    public static PauseMenuController Instance { get; private set; }
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

    private void Start()
    {
        // Ensure the pause menu is hidden at the start
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Load saved audio settings
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (musicSlider != null)
        {
            musicSlider.value = savedMusicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
            pauseMenuUI.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        }
    }

    public void Resume()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
        pauseMenuUI.SetActive(false);
        pauseMenuBackground.SetActive(false);
        isPaused = false;
    }

    void Pause()
    {
        GameManager.Instance.ChangeState(GameState.Paused);
        pauseMenuUI.SetActive(true);
        pauseMenuBackground.SetActive(true);
        isPaused = true;

        // Ensure cursor is visible to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        Debug.Log("Opening Settings...");
        pauseMenuBackground.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting to Main Menu...");
        isPaused = false;
        GameManager.Instance.mainMenu();
    }

    // ── Audio Settings ────────────────────────────────────────
    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        Debug.Log($"[PauseMenu] Music volume: {value:F2}");
    }

    private void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        Debug.Log($"[PauseMenu] SFX volume: {value:F2}");
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuBackground.SetActive(true);
    }
}