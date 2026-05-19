using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject promptPanel;
    [SerializeField] private Image keyIcon;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Key Sprites")]
    [SerializeField] private Sprite eKeySprite;
    [SerializeField] private Sprite spaceKeySprite;

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
        promptPanel.SetActive(false);
    }

    public void ShowPrompt(string text, bool useSpaceKey = false)
    {
        promptPanel.SetActive(true);
        promptText.text = text;
        keyIcon.sprite = useSpaceKey ? spaceKeySprite : eKeySprite;
    }

    public void HidePrompt()
    {
        promptPanel.SetActive(false);
    }
}