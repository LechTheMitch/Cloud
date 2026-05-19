public static class ApiConfig
{
    public const string BaseURL = "http://16.171.161.234:8000"; // Update if your IP changed

    // ── Auth ──────────────────────────────────────────────────
    public const string Register      = BaseURL + "/auth/register";
    public const string Login         = BaseURL + "/auth/login";
    public const string GameSave      = BaseURL + "/game/save";
    public const string Leaderboard   = BaseURL + "/leaderboard/";
    public const string SubmitScore   = BaseURL + "/leaderboard/submit";
    public const string HealthCheck   = BaseURL + "/";
}