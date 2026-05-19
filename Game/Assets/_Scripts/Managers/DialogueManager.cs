    using UnityEngine;
    using System.Collections.Generic;

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [System.Serializable]
        public class NPCProfile
        {
            public string npcID;
            public string displayName;
            public Sprite portrait;
        }

        [Header("All Dialogue Lines")]
        [SerializeField] private List<DialogueLine> allDialogueLines;

        [Header("NPC Profiles")]
        [SerializeField] private List<NPCProfile> npcProfiles;

        private int currentLineIndex = 0;
        private DialogueLine activeDialogue;
        private bool isDialogueActive = false;
        private bool hasDisplayedCurrentLine = false;

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

        public void StartDialogue(string npcID)
        {
            DialogueLine dialogue = GetDialogueForNPC(npcID);

            if (dialogue == null)
            {
                Debug.LogWarning($"[DialogueManager] No dialogue found for NPC: {npcID} on loop #{GameManager.Instance.CurrentLoop}");
                return;
            }

            activeDialogue = dialogue;
            currentLineIndex = 0;
            isDialogueActive = true;
            hasDisplayedCurrentLine = false;

            Debug.Log($"[DialogueManager] Starting dialogue for {npcID} on loop #{GameManager.Instance.CurrentLoop}");
            PrintCurrentLine();
        }

        public void AdvanceDialogue()
        {
            if (!isDialogueActive || !hasDisplayedCurrentLine) return;

            currentLineIndex++;
            hasDisplayedCurrentLine = false;

            if (currentLineIndex >= activeDialogue.lines.Length)
            {
                EndDialogue();
                return;
            }

            PrintCurrentLine();
        }

        private void EndDialogue()
        {
            isDialogueActive = false;
            activeDialogue = null;
            currentLineIndex = 0;
            hasDisplayedCurrentLine = false;

            if (DialogueUI.Instance != null)
                DialogueUI.Instance.HideDialogue();

            // stop blip immediately
            if (AudioManager.Instance != null)
                AudioManager.Instance.StopDialogueBlip();

            Debug.Log("[DialogueManager] Dialogue ended.");
        }

        private void PrintCurrentLine()
        {
            if (activeDialogue == null || currentLineIndex >= activeDialogue.lines.Length)
                return;

            string line = activeDialogue.lines[currentLineIndex];
            NPCProfile profile = GetProfile(activeDialogue.npcID);

            string displayName = profile != null ? profile.displayName : activeDialogue.npcID;
            Sprite portrait = profile != null ? profile.portrait : null;

            hasDisplayedCurrentLine = true;

            if (DialogueUI.Instance != null)
                DialogueUI.Instance.ShowDialogue(displayName, line, portrait);
            else
                Debug.Log($"[NPC]: {line}");

            // play dialogue blip
            if (AudioManager.Instance != null)
                AudioManager.Instance.StartDialogueBlip();
    }

        private DialogueLine GetDialogueForNPC(string npcID)
        {
            int currentLoop = GameManager.Instance.CurrentLoop;

            // find best match — same npcID, closest loop that doesn't exceed current
            DialogueLine bestMatch = null;

            foreach (var line in allDialogueLines)
            {
                if (line.npcID != npcID) continue;
                if (line.loopIndex > currentLoop) continue;

                // check clue condition if required
                if (line.requiresClue && !ClueManager.Instance.HasClueByID(line.requiredClueID))
                    continue;

                if (bestMatch == null || line.loopIndex > bestMatch.loopIndex)
                    bestMatch = line;
            }

            return bestMatch;
        }

        public string GetCurrentLine()
        {
            if (activeDialogue == null) return "";
            if (currentLineIndex >= activeDialogue.lines.Length) return "";
            return activeDialogue.lines[currentLineIndex];
        }

        public NPCProfile GetProfile(string npcID)
        {
            return npcProfiles.Find(p => p.npcID == npcID);
        }

        public bool IsDialogueActive() => isDialogueActive;

        public bool CanAdvanceDialogue()
        {
            return isDialogueActive && hasDisplayedCurrentLine;
        }
    }