using System;
using System.Collections.Generic;

// --- Auth Models ---
[Serializable]
public class UserRegisterRequest {
    public string username;
    public string password;
    public bool is_admin = false;
}

[Serializable]
public class TokenResponse {
    public string access_token;
    public string token_type;
}

// --- Save Models ---
[Serializable]
public class SaveRequest {
    public string save_data; // The serialized GameSaveData goes here
}

[Serializable]
public class GameSaveData {
    public int currentLoop;
    public string story_state;
    public string ending;
    public bool is_finished;
}

// --- Leaderboard Models ---
[Serializable]
public class ScoreSubmission
{
    public float completion_time;
}

[Serializable]
public class LeaderboardEntry
{
    public string username;
    public float completion_time;
}

// Helper wrapper because JsonUtility cannot deserialize top-level arrays directly
[Serializable]
public class LeaderboardRoot
{
    public List<LeaderboardEntry> entries;
}

// --- Constants ---
public static class StoryState {
    public const string Looping = "looping";
    public const string GoToApartment = "go_to_apartment";
    public const string ConfessToPolice = "confess_to_police";
}

public static class EndingType {
    public const string Acceptance = "acceptance";
    public const string Denial = "denial";
    public const string None = "none";
}