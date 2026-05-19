using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI loopText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateHUD();
        // time never changes — always 7:12 AM
        if (timeText != null)
            timeText.text = "7:12 AM";
    }

    public void UpdateHUD()
    {
        if (loopText != null)
            loopText.text = $"LOOP {GameManager.Instance.CurrentLoop}";
    }
}