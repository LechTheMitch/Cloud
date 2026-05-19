using UnityEngine;
using System.Collections.Generic;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance { get; private set; }

    private List<ClueData> collectedClues = new List<ClueData>();

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

    [System.Obsolete]
    public void CollectClue(ClueData clue)
    {
        if (HasClue(clue))
        {
            Debug.Log($"[ClueManager] Clue already collected: {clue.clueName}");
            return;
        }

        clue.discoveredOnLoop = GameManager.Instance.CurrentLoop;
        collectedClues.Add(clue);
        Debug.Log($"[ClueManager] Clue collected: {clue.clueName} on loop #{clue.discoveredOnLoop}");

        // notify murder timer — may trigger early murder
    if (MurderTimer.Instance != null)
        MurderTimer.Instance.NotifyDiscovery("clue", clue.clueID);
    }

    public bool HasClue(ClueData clue)
    {
        return collectedClues.Contains(clue);
    }

    public bool HasClueByID(string id)
    {
        return collectedClues.Exists(c => c.clueID == id);
    }

    public List<ClueData> GetAllClues()
    {
        return collectedClues;
    }

    public void DebugPrintAllClues()
    {
        if (collectedClues.Count == 0)
        {
            Debug.Log("[ClueManager] No clues collected yet.");
            return;
        }

        Debug.Log($"[ClueManager] Total clues collected: {collectedClues.Count}");
        foreach (var clue in collectedClues)
        {
            Debug.Log($"  - [{clue.clueID}] {clue.clueName} (loop #{clue.discoveredOnLoop}) | Critical: {clue.isCritical}");
        }
    }
}