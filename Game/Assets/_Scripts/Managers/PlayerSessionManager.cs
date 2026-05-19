using UnityEngine;

public class PlayerSessionManager : MonoBehaviour
{
    public static PlayerSessionManager Instance { get; private set; }
    public string CurrentUsername { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUsername);

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSession(string username) {
        CurrentUsername = username;
        if (GameSessionTracker.Instance != null) GameSessionTracker.Instance.PlayerName = username;
    }

    public void ApplySaveToGame(GameSaveData data) {
        if (GameManager.Instance != null) GameManager.Instance.setCurrentLoop(data.currentLoop);
    }

    public GameSaveData CreateSaveSnapshot() {
        return new GameSaveData {
            currentLoop = GameManager.Instance.CurrentLoop,
            story_state = "active", // Define based on your game logic
        };
    }
}