using UnityEngine;

public class GameSessionTracker : MonoBehaviour
{
    public static GameSessionTracker Instance { get; private set; }

    public float SessionStartTime { get; private set; }
    public float CompletionTime   { get; private set; }
    public string PlayerName      { get; set; } = "Unknown";
    public string EndingAchieved  { get; private set; }

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

    public void StartSession()
    {
        SessionStartTime = Time.time;
        Debug.Log("[GameSessionTracker] Session started.");
    }

    public void EndSession(string ending)
    {
        CompletionTime = Time.time - SessionStartTime;
        EndingAchieved = ending;

        Debug.Log($"[GameSessionTracker] Session ended. " +
                  $"Time: {CompletionTime:F1}s " +
                  $"Loops: {GameManager.Instance.CurrentLoop} " +
                  $"Ending: {ending}");
    }

    public GameRecord BuildRecord()
    {
        return new GameRecord(
            PlayerName,
            CompletionTime,
            GameManager.Instance.CurrentLoop,
            EndingAchieved
        );
    }
}