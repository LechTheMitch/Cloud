using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{

    [Header("Auth Panel")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private GameObject loginForm;
    [SerializeField] private GameObject registerForm;

    // Login fields
    [SerializeField] private TMPro.TMP_InputField loginUsernameInput;
    [SerializeField] private TMPro.TMP_InputField loginPasswordInput;
    [SerializeField] private TMPro.TextMeshProUGUI loginErrorText;

    // Register fields
    [SerializeField] private TMPro.TMP_InputField regUsernameInput;
    [SerializeField] private TMPro.TMP_InputField regEmailInput;
    [SerializeField] private TMPro.TMP_InputField regPasswordInput;
    [SerializeField] private TMPro.TextMeshProUGUI regErrorText;

    [Header("Logged In Display")]
    [SerializeField] private TMPro.TextMeshProUGUI welcomeText;


    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Buttons")]
    [SerializeField] private Button btnNewGame;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnLeaderboard;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnCredits;
    [SerializeField] private Button btnQuit;

    [Header("Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Leaderboard")]
    [SerializeField] private Transform entriesContent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Apartment";

    [Header("Menu Music")]
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private float musicFadeOutDuration = 1f;

    [Header("Fade")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        // wire existing buttons
        btnNewGame.onClick.AddListener(OnNewGame);
        btnContinue.onClick.AddListener(OnContinue);
        btnContinue.interactable = false; // disable until we check for a save
        btnLeaderboard.onClick.AddListener(OnLeaderboard);
        btnSettings.onClick.AddListener(OnSettings);
        btnCredits.onClick.AddListener(OnCredits);
        btnQuit.onClick.AddListener(OnQuit);

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // show auth panel if no player is logged in
        bool loggedIn = PlayerSessionManager.Instance != null &&
                        PlayerSessionManager.Instance.IsLoggedIn;

        if (!loggedIn && authPanel != null)
        {
            menuPanel.SetActive(false);
            authPanel.SetActive(true);
        }
        else
        {
            menuPanel.SetActive(true);
            if (authPanel != null) authPanel.SetActive(false);

            if (welcomeText != null &&
                PlayerSessionManager.Instance != null)
                welcomeText.text =
                    $"Welcome back, {PlayerSessionManager.Instance.CurrentUsername}";
        }

        // verify API is reachable
        if (ApiClient.Instance != null)
            ApiClient.Instance.CheckHealth(ok =>
                Debug.Log($"[MainMenu] API health: {(ok ? "OK" : "UNREACHABLE")}")
            );

        StartCoroutine(FadeIn());
    }

    public void OnLogin()
{
    string username = loginUsernameInput.text.Trim();
    string password = loginPasswordInput.text;

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        if (loginErrorText != null)
            loginErrorText.text = "Please fill in all fields.";
        return;
    }

    if (loginErrorText != null) loginErrorText.text = "Logging in...";

    ApiClient.Instance.Login(
        username, password,
        (success, msg) =>
        {
            if(success){
                PlayerSessionManager.Instance.SetSession(username);
                authPanel.SetActive(false);
                menuPanel.SetActive(true);

                if (welcomeText != null)
                    welcomeText.text = $"Welcome back, {username}";

                Debug.Log($"[MainMenu] Login successful: {username}");
            } else{
                            if (loginErrorText != null)
                loginErrorText.text = "Invalid username or password.";
            Debug.LogWarning($"[MainMenu] Login failed: {msg}");
            }
        }
    );
}

    public void OnRegister()
    {
        string username = regUsernameInput.text.Trim();
        string email = regEmailInput.text.Trim();
        string password = regPasswordInput.text;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            if (regErrorText != null)
                regErrorText.text = "Please fill in all fields.";
            return;
        }

        if (regErrorText != null) regErrorText.text = "Creating account...";

        ApiClient.Instance.Register(
            username, password,
            (success, msg) =>
            {
                if (success)
                {
                    PlayerSessionManager.Instance.SetSession(username);
                    authPanel.SetActive(false);
                    menuPanel.SetActive(true);
                    if (welcomeText != null)
                        welcomeText.text = $"Welcome, {username}";
                    Debug.Log($"[MainMenu] Registration successful: {username}");

                    ApiClient.Instance.Login(username, password, (loginSuccess, loginResponse) => {
                    if (loginSuccess) {
                    Debug.Log("[MainMenu] Login successful, token acquired.");
                    }
                    });
                }
                else
                {
                    if (regErrorText != null)
                        regErrorText.text = msg;
                    Debug.LogWarning($"[MainMenu] Register failed: {msg}");
                }
            }
        );
    }

    public void OnRegisterSwitch()
    {
        loginForm.SetActive(false);
        registerForm.SetActive(true);
    }

    public void OnLoginSwitch()
    {
        registerForm.SetActive(false);
        loginForm.SetActive(true);
    }

    // ── Button Handlers ───────────────────────────────────────
    private void OnNewGame()
    {
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.DeleteKey("PermanentDarkness");
        PlayerPrefs.Save();
        StartCoroutine(LoadGameScene());
    }

    private void OnContinue()
    {
        StartCoroutine(LoadGameScene());
    }

    private void OnLeaderboard()
    {
        menuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
        PopulateLeaderboard();
    }

    private void OnSettings()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnCredits()
    {
        menuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    private void OnQuit()
    {
        Debug.Log("[MainMenu] Quitting application.");
        Application.Quit();

        // This stops the "Play" mode in the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ── Panel Close Handlers ──────────────────────────────────
    public void OnSettingsClose()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OnLeaderboardClose()
    {
        leaderboardPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OnCreditsClose()
    {
        creditsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    // ── Audio Settings ────────────────────────────────────────
    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        Debug.Log($"[MainMenu] Music volume: {value:F2}");
    }

    private void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        Debug.Log($"[MainMenu] SFX volume: {value:F2}");
    }

    // ── Leaderboard ───────────────────────────────────────────
    private void PopulateLeaderboard()
    {
        foreach (Transform child in entriesContent)
            Destroy(child.gameObject);

        if (loadingText != null)
        {
            loadingText.text = "Loading scores...";
            loadingText.gameObject.SetActive(true);
        }

        // call the real API
        ApiClient.Instance.GetLeaderboard(
            50,
            entries => PopulateLeaderboardEntries(entries),
            error =>
            {
                Debug.LogWarning($"[MainMenu] Leaderboard error: {error}");
                if (loadingText != null)
                    loadingText.text = "Could not load scores.\nCheck your connection.";
            }
        );
    }

    private void PopulateLeaderboardEntries(List<LeaderboardEntry> entries)
    {
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        if (entries == null || entries.Count == 0)
        {
            if (loadingText != null)
            {
                loadingText.text = "No records yet.\nBe the first to finish.";
                loadingText.gameObject.SetActive(true);
            }
            return;
        }

        for (int i = 0; i < entries.Count; i++)
            SpawnEntry(i + 1, entries[i]);
    }

    private void SpawnEntry(int rank, LeaderboardEntry entry)
    {
        GameObject obj = Instantiate(entryPrefab, entriesContent, false);
        TMPro.TextMeshProUGUI[] texts =
            obj.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        if (texts.Length >= 3)
        {
            texts[0].text = $"#{rank}";
            texts[1].text = entry.username;
            texts[2].text = $"{entry.completion_time:F2} seconds";
            // texts[3].text = entry.ending != null
            //     ? entry.ending
            //     : "In Progress";

            // // color by ending
            // if (entry.ending == EndingType.Acceptance)
            //     texts[3].color = new Color(0.6f, 0.9f, 0.6f);
            // else if (entry.ending == EndingType.Denial)
            //     texts[3].color = new Color(0.8f, 0.4f, 0.4f);
            // else
            //     texts[3].color = new Color(0.7f, 0.7f, 0.7f);
        }

        Image bg = obj.GetComponent<Image>();
        if (bg != null && rank % 2 == 0)
            bg.color = new Color(0.12f, 0.10f, 0.18f, 1f);
    }

    // ── Local Save/Load ───────────────────────────────────────
    // temporary local storage — replace with API calls later
    // public void SaveRecord(GameRecord record)
    // {
    //     string json = PlayerPrefs.GetString("LeaderboardData", "{}");
    //     LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json)
    //                         ?? new LeaderboardData();

    //     var list = new List<GameRecord>(
    //         data.records ?? new GameRecord[0]
    //     );
    //     list.Add(record);

    //     data.records = list.ToArray();
    //     PlayerPrefs.SetString(
    //         "LeaderboardData",
    //         JsonUtility.ToJson(data)
    //     );
    //     PlayerPrefs.Save();

    //     Debug.Log($"[MainMenu] Record saved for {record.playerName}.");
    // }

    // private List<GameRecord> LoadLocalRecords()
    // {
    //     string json = PlayerPrefs.GetString("LeaderboardData", "");
    //     if (string.IsNullOrEmpty(json))
    //         return new List<GameRecord>();

    //     LeaderboardData data =
    //         JsonUtility.FromJson<LeaderboardData>(json);
    //     return data?.records != null
    //         ? new List<GameRecord>(data.records)
    //         : new List<GameRecord>();
    // }

    // ── Scene Transition ──────────────────────────────────────
    private IEnumerator LoadGameScene()
    {
        StartCoroutine(FadeOutMenuMusic());
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeIn()
    {
        Debug.Log("[MainMenu] FadeIn started.");

        if (fadePanel == null)
        {
            Debug.LogError("[MainMenu] fadePanel is null. " +
                           "Assign the Image component in Inspector.");
            yield break;
        }
        fadePanel.color = new Color(0, 0, 0, 1f);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.color = new Color(
                0, 0, 0,
                Mathf.Lerp(1f, 0f, elapsed / fadeDuration)
            );
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 0);
        Debug.Log("[MainMenu] FadeIn complete.");
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("[MainMenu] FadeOut started.");
        if (fadePanel == null)
        {
            Debug.LogError("[MainMenu] fadePanel is null.");
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.color = new Color(
                0, 0, 0,
                Mathf.Lerp(0f, 1f, elapsed / fadeDuration)
            );
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 1f);
        Debug.Log("[MainMenu] FadeOut complete.");
    }

    private IEnumerator FadeOutMenuMusic()
    {
        if (menuMusicSource == null) yield break;

        float startVolume = menuMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            menuMusicSource.volume = Mathf.Lerp(
                startVolume, 0f,
                elapsed / musicFadeOutDuration
            );
            yield return null;
        }

        menuMusicSource.Stop();
    }
}