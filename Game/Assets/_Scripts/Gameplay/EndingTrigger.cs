using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndingTrigger : MonoBehaviour
{
    [Header("Choice Objects")]
    [SerializeField] private GameObject acceptanceDoor;
    [SerializeField] private GameObject denialWindow;

    [Header("Ending Settings")]
    [SerializeField] private float endingDelay = 3f;

    [System.Obsolete]
    public void TriggerAcceptance()
    {
        StartCoroutine(AcceptanceEnding());
    }

    [System.Obsolete]
    public void TriggerDenial()
    {
        StartCoroutine(DenialEnding());
    }

    [System.Obsolete]
    private IEnumerator AcceptanceEnding()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.SetCanMove(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceAll();

        if (GameSessionTracker.Instance != null)
            GameSessionTracker.Instance.EndSession("Acceptance");

        // save to API

        SaveEndingToAPI(EndingType.Acceptance);

        DialogueManager.Instance.StartDialogue("ending_acceptance");
        while(DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }

        if (ScreenEffectsController.Instance != null)
            yield return StartCoroutine(
                ScreenEffectsController.Instance.FadeToBlack(2f)
            );

        GameManager.Instance.ChangeState(GameState.GameOver);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        GameManager.Instance.mainMenu();
    }

    [System.Obsolete]
    private IEnumerator DenialEnding()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.SetCanMove(false);

        
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceAll();

        if (GameSessionTracker.Instance != null)
            GameSessionTracker.Instance.EndSession("Denial");

        // save to API
        SaveEndingToAPI(EndingType.Denial);

        DialogueManager.Instance.StartDialogue("ending_denial");
        while(DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }

        PlayerPrefs.SetInt("PermanentDarkness", 1);
        PlayerPrefs.Save();
        if (ScreenEffectsController.Instance != null)
            yield return StartCoroutine(
                ScreenEffectsController.Instance.FadeToBlack(2f)
            );

        GameManager.Instance.ChangeState(GameState.GameOver);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        GameManager.Instance.mainMenu();
    }

    private void SaveEndingToAPI(
        string ending)
    {
        if (ApiClient.Instance == null) return;
        if (PlayerSessionManager.Instance == null) return;
        if (!PlayerSessionManager.Instance.IsLoggedIn) return;

        GameSaveData finalData = PlayerSessionManager.Instance.CreateSaveSnapshot();
        finalData.ending = ending;
        finalData.is_finished = true;

        ApiClient.Instance.SaveGame(
            finalData,
            success => Debug.Log(success?
                $"[EndingTrigger] Story saved successfully — ending: {finalData.ending}"
              : $"[EndingTrigger] Story save failed."
            )
        );

        if (GameSessionTracker.Instance != null)
    {
        float finalTime = GameSessionTracker.Instance.CompletionTime;
        
        ApiClient.Instance.SubmitScore(finalTime, success => {
            if (success) 
                Debug.Log($"[Leaderboard] Score of {finalTime}s submitted successfully!");
            else 
                Debug.LogError("[Leaderboard] Failed to submit score.");
        });
    }
    }
}