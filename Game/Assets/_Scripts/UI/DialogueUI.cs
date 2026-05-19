using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject continuePrompt;

    [Header("Typewriter Settings")]
    [SerializeField] private float typewriterSpeed = 0.04f;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

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
        dialoguePanel.SetActive(false);
        continuePrompt.SetActive(false);
    }

    public void ShowDialogue(string npcName, string line, Sprite portrait = null)
    {
        dialoguePanel.SetActive(true);
        continuePrompt.SetActive(false);

        nameText.text = npcName;

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.gameObject.SetActive(portrait != null);
        }

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(line));
    }

    private IEnumerator TypewriterRoutine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        continuePrompt.SetActive(false);

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        continuePrompt.SetActive(true);
    }

    // called by GameManager when Space is pressed
    public void OnAdvanceInput()
    {
        if (isTyping)
        {
            // player skipped — stop blip immediately then show full line
            if (AudioManager.Instance != null)
                AudioManager.Instance.StopDialogueBlip();
            // first press skips typewriter and shows full line
            SkipTypewriter();
        }
        else
        {
            // second press advances to next line
            DialogueManager.Instance.AdvanceDialogue();
        }
    }

    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // get full current line from DialogueManager
        dialogueText.text = DialogueManager.Instance.GetCurrentLine();
        isTyping = false;
        continuePrompt.SetActive(true);
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        continuePrompt.SetActive(false);

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // stop blip regardless of how dialogue was closed
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopDialogueBlip();

        isTyping = false;
    }
}