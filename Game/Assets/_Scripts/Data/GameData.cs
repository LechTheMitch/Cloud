using System;

[Serializable]
public class GameRecord
{
    public string playerName;
    public float completionTime;
    public int loopsTaken;
    public string endingAchieved;  // "Acceptance" or "Denial"
    public string formattedTime;

    public GameRecord(
        string name,
        float time,
        int loops,
        string ending)
    {
        playerName    = name;
        completionTime = time;
        loopsTaken    = loops;
        endingAchieved = ending;
        formattedTime = FormatTime(time);
    }

    private string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        int ms   = (int)((seconds * 100) % 100);
        return $"{mins:00}:{secs:00}.{ms:00}";
    }
}

[Serializable]
public class LeaderboardData
{
    public GameRecord[] records;
}