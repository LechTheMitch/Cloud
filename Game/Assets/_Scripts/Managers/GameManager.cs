using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    LoopResetting,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState CurrentState { get; private set; }

    [Header("Loop Info")]
    public int CurrentLoop { get; private set; } = 0;
    public float LoopStartTime { get; private set; }

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
        ChangeState(GameState.Playing);
        if (GameSessionTracker.Instance != null)
        GameSessionTracker.Instance.StartSession();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] State changed to: {newState}");

        switch (newState)
        {
            case GameState.Playing:
                LoopStartTime = Time.time;
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.LoopResetting:
                Time.timeScale = 1f;
                TriggerLoopReset();
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                Debug.Log("[GameManager] Game Over.");
                break;
        }
    }

    private void TriggerLoopReset()
    {
        CurrentLoop++;
        Debug.Log($"[GameManager] Loop reset. Now entering loop #{CurrentLoop}");
    }

    public bool IsPlaying() => CurrentState == GameState.Playing;

    [System.Obsolete]
    private void Update()
    {
        // Debug input for testing loop resets
        if (Keyboard.current.rKey.wasPressedThisFrame)
            TimeLoopManager.Instance.TriggerLoopReset();

        // Debug input for testing state changes
        if (Keyboard.current.pKey.wasPressedThisFrame)
            ChangeState(GameState.Paused);

        // Debug input for testing clue collection
        // if (Keyboard.current.cKey.wasPressedThisFrame)
        // {
        //     ClueData testClue = Resources.Load<ClueData>("Clues/Clue_WomanGaze");
        //     if (testClue != null)
        //         ClueManager.Instance.CollectClue(testClue);
        //     else
        //         Debug.LogWarning("[Test] Clue asset not found. Check the path.");
        // }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance.IsDialogueActive())
                DialogueUI.Instance.OnAdvanceInput();
        }
        
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ClueUI.Instance.ToggleJournal();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PauseMenuController.Instance.TogglePause();
        }

        // Debug input for testing dialogue
        // press E to start dialogue, press Space to advance to next line
        // if (Keyboard.current.eKey.wasPressedThisFrame)
        // {
        //     if (!DialogueManager.Instance.IsDialogueActive())
        //         DialogueManager.Instance.StartDialogue("neighbour");
        // }
    }

    public void mainMenu()
    {
        // Reset TimeScale so UI and animations work
        Time.timeScale = 1f;

        // Explicitly set the state so it's no longer "GameOver"
        CurrentState = GameState.MainMenu;

        // Optional: Clear the darkness flag if you want the normal menu back
        if (PlayerPrefs.HasKey("PermanentDarkness"))
        {
            PlayerPrefs.DeleteKey("PermanentDarkness");
            PlayerPrefs.Save();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceAll();

        SceneManager.LoadScene("MainMenu");
    }

    public void setCurrentLoop(int loop)
    {
        CurrentLoop = loop;
    }
    
}