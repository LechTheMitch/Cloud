using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }
    private string _authToken;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(string user, string pass, Action<bool, string> callback) {
        var body = new UserRegisterRequest { username = user, password = pass };
        StartCoroutine(PostRequest(ApiConfig.Register, JsonUtility.ToJson(body), callback));
    }

    public void Login(string user, string pass, Action<bool, string> callback) {
        // The API uses a standard Form-Post for login (OAuth2 style)
        WWWForm form = new WWWForm();
        form.AddField("username", user);
        form.AddField("password", pass);

        StartCoroutine(PostFormRequest(ApiConfig.Login, form, (success, response) => {
            if (success) {
                var tokenData = JsonUtility.FromJson<TokenResponse>(response);
                _authToken = tokenData.access_token;
            }
            callback(success, response);
        }));
    }

    public void SaveGame(GameSaveData data, Action<bool> callback) {
        var wrapper = new SaveRequest { save_data = JsonUtility.ToJson(data) };
        StartCoroutine(PostRequest(ApiConfig.GameSave, JsonUtility.ToJson(wrapper), (s, r) => callback(s), true));
    }

    public void LoadGame(Action<GameSaveData> onSuccess, Action<string> onError) {
        StartCoroutine(GetRequest(ApiConfig.GameSave, (success, response) => {
            if (success) {
                // API returns a raw string. We strip quotes if the backend returns "{\"loops\":1}"
                string cleanJson = response.Trim('"').Replace("\\\"", "\"");
                var data = JsonUtility.FromJson<GameSaveData>(cleanJson);
                onSuccess?.Invoke(data);
            } else {
                onError?.Invoke(response);
            }
        }, true));
    }

    public void SubmitScore(float time, Action<bool> callback)
    {
        var body = new ScoreSubmission { completion_time = time };
        string json = JsonUtility.ToJson(body);

        StartCoroutine(PostRequest(ApiConfig.SubmitScore, json, (success, response) =>
        {
            if (success) Debug.Log("Score submitted successfully!");
            callback?.Invoke(success);
        }, useAuth: true));
    }

    public void GetLeaderboard(int limit, Action<List<LeaderboardEntry>> onSuccess, Action<string> onError)
    {
        // Append the limit as a query parameter
        string url = $"{ApiConfig.Leaderboard}?limit={limit}";

        StartCoroutine(GetRequest(url, (success, response) =>
        {
            if (success)
            {
                // Trick to handle the raw JSON array from the API
                string wrappedJson = "{\"entries\":" + response + "}";
                LeaderboardRoot root = JsonUtility.FromJson<LeaderboardRoot>(wrappedJson);
                onSuccess?.Invoke(root.entries);
            }
            else
            {
                onError?.Invoke(response);
            }
        }, useAuth: false)); // Usually leaderboard is public, no auth needed
    }

    public void CheckHealth(Action<bool> callback) {
        StartCoroutine(GetRequest(ApiConfig.HealthCheck, (success, response) => callback(success)));
    }

    // --- Internal Helpers ---

    private IEnumerator PostRequest(string url, string json, Action<bool, string> callback, bool useAuth = false) {
        using (UnityWebRequest req = new UnityWebRequest(url, "POST")) {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (useAuth) req.SetRequestHeader("Authorization", "Bearer " + _authToken);
            yield return req.SendWebRequest();
            callback(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
        }
    }

    private IEnumerator PostFormRequest(string url, WWWForm form, Action<bool, string> callback) {
        using (UnityWebRequest req = UnityWebRequest.Post(url, form)) {
            yield return req.SendWebRequest();
            callback(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
        }
    }

    private IEnumerator GetRequest(string url, Action<bool, string> callback, bool useAuth = false) {
        using (UnityWebRequest req = UnityWebRequest.Get(url)) {
            if (useAuth) req.SetRequestHeader("Authorization", "Bearer " + _authToken);
            yield return req.SendWebRequest();
            callback(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
        }
    }
}