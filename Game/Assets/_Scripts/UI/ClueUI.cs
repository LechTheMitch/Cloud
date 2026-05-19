using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class ClueUI : MonoBehaviour
{
    public static ClueUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject journalPanel;

    [Header("Clue List")]
    [SerializeField] private Transform clueListContent;
    [SerializeField] private GameObject clueButtonPrefab;

    [Header("Clue Detail")]
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private TextMeshProUGUI detailHint;
    [SerializeField] private Image detailIcon;

    private bool isOpen = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();

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
        journalPanel.SetActive(false);
        ClearDetail();
    }

    public void ToggleJournal()
    {
        if (isOpen) 
            CloseJournal();
        else 
            OpenJournal();
    }

    public void OpenJournal()
    {
        if (DialogueManager.Instance.IsDialogueActive()) return;

        isOpen = true;
        journalPanel.SetActive(true);
        GameManager.Instance.ChangeState(GameState.Paused);
        PopulateClueList();

        Debug.Log("[ClueUI] Journal opened.");
    }

    public void CloseJournal()
    {
        isOpen = false;
        journalPanel.SetActive(false);
        GameManager.Instance.ChangeState(GameState.Playing);
        ClearDetail();

        Debug.Log("[ClueUI] Journal closed.");
    }

    private void PopulateClueList()
    {
        // clear existing buttons
        foreach (var btn in spawnedButtons)
            Destroy(btn);
        spawnedButtons.Clear();

        List<ClueData> clues = ClueManager.Instance.GetAllClues();

        if (clues.Count == 0)
        {
            // show empty state message
            GameObject empty = new GameObject("EmptyMessage");
            empty.transform.SetParent(clueListContent, false);
            TextMeshProUGUI msg = empty.AddComponent<TextMeshProUGUI>();
            msg.text = "No memories recovered yet.";
            msg.fontSize = 14;
            msg.color = new Color(0.6f, 0.6f, 0.7f, 1f);
            msg.alignment = TextAlignmentOptions.Center;
            spawnedButtons.Add(empty);
            return;
        }

        foreach (ClueData clue in clues)
        {
            GameObject btnObj = Instantiate(
                clueButtonPrefab,
                clueListContent
            );

            // set button name text
            TextMeshProUGUI btnText =
                btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = clue.clueName;

            // set button icon
            Image[] images = btnObj.GetComponentsInChildren<Image>();
            if (images.Length > 1 && clue.icon != null)
                images[1].sprite = clue.icon;

            // highlight critical clues
            Image btnImage = btnObj.GetComponent<Image>();
            if (btnImage != null && clue.isCritical)
                btnImage.color = new Color(0.3f, 0.15f, 0.4f, 1f);

            // wire click to show detail
            ClueData captured = clue;
            btnObj.GetComponent<Button>().onClick.AddListener(
                () => ShowClueDetail(captured)
            );

            spawnedButtons.Add(btnObj);
        }

        // auto select first clue
        if (clues.Count > 0)
            ShowClueDetail(clues[0]);
    }

    private void ShowClueDetail(ClueData clue)
    {
        if (detailName != null)
            detailName.text = clue.clueName;

        if (detailDescription != null)
            detailDescription.text = clue.description;

        if (detailHint != null)
        {
            detailHint.text = clue.isCritical
                ? $"\"{clue.hintText}\""
                : clue.hintText;
        }

        if (detailIcon != null)
        {
            detailIcon.sprite = clue.icon;
            detailIcon.gameObject.SetActive(clue.icon != null);
        }

        Debug.Log($"[ClueUI] Showing detail for: {clue.clueName}");
    }

    private void ClearDetail()
    {
        if (detailName != null)        detailName.text = "";
        if (detailDescription != null) detailDescription.text = "";
        if (detailHint != null)        detailHint.text = "";
        if (detailIcon != null)        detailIcon.gameObject.SetActive(false);
    }
}