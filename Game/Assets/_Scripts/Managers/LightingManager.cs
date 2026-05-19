using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }

    [Header("Light Names — must match GameObject names exactly")]
    [SerializeField] private string globalLightName = "GlobalLight";
    [SerializeField] private string playerLightName = "Light_Player";
    [SerializeField] private List<string> roomLightNames = new List<string>
    {
        "Light_ProtagMain",
        "Light_ProtagBedroom",
        "Light_NeighbourMain",
        "Light_Hallway",
        "Light_Exterior"
    };

    [Header("Darkness Progression")]
    [SerializeField] private float baseDarkness = 0.15f;
    [SerializeField] private float darknessPerLoop = 0.015f;
    [SerializeField] private float minimumDarkness = 0.05f;

    [Header("Flicker Settings")]
    [SerializeField] private int flickerStartLoop = 5;
    [SerializeField] private float flickerMinIntensity = 0.3f;
    [SerializeField] private float flickerMaxIntensity = 1.0f;
    [SerializeField] private float flickerSpeed = 0.08f;

    // runtime references — resolved fresh after every scene load
    private Light2D globalLight;
    private Light2D playerLight;
    private List<Light2D> roomLights = new List<Light2D>();
    private List<float> originalRoomIntensities = new List<float>();

    private Coroutine flickerCoroutine;
    private bool isFlickering = false;

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
        if (PlayerPrefs.GetInt("PermanentDarkness", 0) == 1)
        {
            baseDarkness = 0f;
            darknessPerLoop = 0f;
            minimumDarkness = 0f;
            Debug.Log("[LightingManager] Permanent darkness active.");
        }

        ResolveLightReferences();
        ApplyLoopLighting();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // re-find all light references every time the scene loads
        ResolveLightReferences();
        ApplyLoopLighting();
    }

    private void ResolveLightReferences()
    {
        // find global light by name
        GameObject globalObj = GameObject.Find(globalLightName);
        globalLight = globalObj != null
            ? globalObj.GetComponent<Light2D>()
            : null;

        if (globalLight == null)
            Debug.LogWarning($"[LightingManager] GlobalLight not found: {globalLightName}");

        // find player light by name
        GameObject playerObj = GameObject.Find(playerLightName);
        playerLight = playerObj != null
            ? playerObj.GetComponent<Light2D>()
            : null;

        // find all room lights by name
        roomLights.Clear();
        originalRoomIntensities.Clear();

        foreach (string lightName in roomLightNames)
        {
            GameObject obj = GameObject.Find(lightName);
            if (obj != null)
            {
                Light2D light = obj.GetComponent<Light2D>();
                if (light != null)
                {
                    roomLights.Add(light);
                    originalRoomIntensities.Add(light.intensity);
                }
                else
                {
                    Debug.LogWarning($"[LightingManager] No Light2D on: {lightName}");
                }
            }
            else
            {
                Debug.LogWarning($"[LightingManager] Light GameObject not found: {lightName}");
            }
        }

        Debug.Log($"[LightingManager] Resolved {roomLights.Count} room lights.");
    }

    public void ApplyLoopLighting()
    {
        int loop = GameManager.Instance.CurrentLoop;
        ApplyDarkness(loop);

        if (loop >= flickerStartLoop)
            StartFlickering();
        else
            StopFlickering();
    }

    private void ApplyDarkness(int loop)
    {
        float newIntensity = Mathf.Max(
            minimumDarkness,
            baseDarkness - (loop * darknessPerLoop)
        );

        if (globalLight != null)
            globalLight.intensity = newIntensity;

        for (int i = 0; i < roomLights.Count; i++)
        {
            if (roomLights[i] == null) continue;
            float dimmed = Mathf.Max(
                0.1f,
                originalRoomIntensities[i] - (loop * 0.05f)
            );
            roomLights[i].intensity = dimmed;
        }

        Debug.Log($"[LightingManager] Loop #{loop} — " +
                  $"Global intensity: {newIntensity:F2}");
    }

    private void StartFlickering()
    {
        if (isFlickering) return;
        isFlickering = true;
        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    private void StopFlickering()
    {
        if (!isFlickering) return;
        isFlickering = false;

        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);

        ApplyDarkness(GameManager.Instance.CurrentLoop);
    }

    private IEnumerator FlickerRoutine()
    {
        while (isFlickering)
        {
            foreach (var light in roomLights)
            {
                if (light == null) continue;
                light.intensity = Random.Range(
                    flickerMinIntensity,
                    flickerMaxIntensity
                );
            }

            float wait = Random.Range(
                flickerSpeed * 0.5f,
                flickerSpeed * 3f
            );
            yield return new WaitForSeconds(wait);
        }
    }

    public IEnumerator FlashEffect(Color flashColor, float duration)
    {
        if (globalLight == null) yield break;

        Color originalColor = globalLight.color;
        float originalIntensity = globalLight.intensity;

        globalLight.color = flashColor;
        globalLight.intensity = 1.5f;

        yield return new WaitForSeconds(duration);

        globalLight.color = originalColor;
        globalLight.intensity = originalIntensity;
    }

    public void OnLoopChanged()
    {
        StopFlickering();
        ApplyLoopLighting();
    }
}