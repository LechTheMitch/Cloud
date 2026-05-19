using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource blipSource;

    [Header("Music — one track, pitch shifts per loop")]
    [SerializeField] private AudioClip mainTheme;

    [Header("Ambience")]
    [SerializeField] private AudioClip apartmentAmbience;

    [Header("SFX")]
    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip cluePickupSFX;
    [SerializeField] private AudioClip dialogueBlipSFX;
    [SerializeField] private AudioClip heartbeatSFX;
    [SerializeField] private List<AudioClip> footstepSFX;

    [Header("Music Evolution Settings")]
    [SerializeField] private float basePitch = 1f;
    [SerializeField] private float pitchReductionPerLoop = 0.03f;
    [SerializeField] private float baseVolume = 0.8f;
    [SerializeField] private float volumeReductionPerLoop = 0.02f;
    [SerializeField] private float heartbeatStartLoop = 6;
    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("Footstep Settings")]
    [SerializeField] private float footstepInterval = 0.35f;

    private Coroutine musicFadeCoroutine;
    private Coroutine footstepCoroutine;
    private Coroutine footstepFadeCoroutine;
    private bool isWalking = false;
    private int footstepIndex = 0;

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
        ApplyPlayerPrefsVolume();
        StartAmbience();
        PlayMusic(0);
    }

    // ── Volume from PlayerPrefs ───────────────────────────────
    private void ApplyPlayerPrefsVolume()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void SetMusicVolume(float value)
    {
        // convert 0-1 slider to decibels
        float db = value > 0.001f
            ? Mathf.Log10(value) * 20f
            : -80f;
        audioMixer.SetFloat("MusicVolume", db);
    }

    public void SetSFXVolume(float value)
    {
        float db = value > 0.001f
            ? Mathf.Log10(value) * 20f
            : -80f;
        audioMixer.SetFloat("SFXVolume", db);
        audioMixer.SetFloat("AmbienceVolume", db);
    }

    // ── Music System ──────────────────────────────────────────
    public void PlayMusic(int loop)
    {
        if (mainTheme == null) return;

        // pitch gets lower each loop — feels heavier
        float pitch = Mathf.Max(
            0.6f,
            basePitch - (loop * pitchReductionPerLoop)
        );

        // volume drops slightly each loop
        float volume = Mathf.Max(
            0.3f,
            baseVolume - (loop * volumeReductionPerLoop)
        );

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(
            CrossfadeMusic(mainTheme, pitch, volume)
        );

        // start heartbeat at high loops
        if (loop >= heartbeatStartLoop)
            StartHeartbeat(loop);
        else
            StopHeartbeat();

        Debug.Log($"[AudioManager] Music loop {loop} " +
                  $"— pitch: {pitch:F2} volume: {volume:F2}");
    }

    private IEnumerator CrossfadeMusic(
        AudioClip clip,
        float targetPitch,
        float targetVolume)
    {
        // fade out current music
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(
                    startVolume, 0f,
                    elapsed / (musicFadeDuration * 0.5f)
                );
                yield return null;
            }
        }

        // switch clip and settings
        musicSource.clip = clip;
        musicSource.pitch = targetPitch;
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        // fade in new version
        float fadeElapsed = 0f;
        while (fadeElapsed < musicFadeDuration * 0.5f)
        {
            fadeElapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(
                0f, targetVolume,
                fadeElapsed / (musicFadeDuration * 0.5f)
            );
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // ── Ambience ──────────────────────────────────────────────
    private void StartAmbience()
    {
        if (apartmentAmbience == null) return;

        ambienceSource.clip = apartmentAmbience;
        ambienceSource.loop = true;
        ambienceSource.volume = 0.3f;
        ambienceSource.Play();
    }

    // ── Heartbeat ─────────────────────────────────────────────
    private void StartHeartbeat(int loop)
    {
        if (heartbeatSFX == null || heartbeatSource == null) return;

        // heartbeat gets faster at higher loops
        float pitch = 1f + ((loop - heartbeatStartLoop) * 0.1f);
        heartbeatSource.clip = heartbeatSFX;
        heartbeatSource.pitch = Mathf.Min(pitch, 1.8f);
        heartbeatSource.loop = true;
        heartbeatSource.volume = 0.4f;

        if (!heartbeatSource.isPlaying)
            heartbeatSource.Play();
    }

    private void StopHeartbeat()
    {
        if (heartbeatSource != null && heartbeatSource.isPlaying)
            heartbeatSource.Stop();
    }

    // ── SFX ───────────────────────────────────────────────────
    public void PlayDoorOpen()
    {
        PlaySFX(doorOpenSFX);
    }

    public void PlayCluePickup()
    {
        PlaySFX(cluePickupSFX, 0.8f);
    }

    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
    // ── Dialogue Blip ─────────────────────────────────────────────
    public void StartDialogueBlip()
    {
        if (dialogueBlipSFX == null || blipSource == null) return;

        // stop any existing blip first — prevents stacking
        blipSource.Stop();

        blipSource.clip = dialogueBlipSFX;
        blipSource.loop = true;
        blipSource.Play();
    }

    public void StopDialogueBlip()
    {
        if (blipSource == null) return;
        blipSource.Stop();
    }

    // ── Footsteps ─────────────────────────────────────────────────
    public void StartFootsteps()
    {
        if (isWalking) return;
        isWalking = true;

        // cancel fade if player starts moving again mid-fade
        if (footstepFadeCoroutine != null)
        {
            StopCoroutine(footstepFadeCoroutine);
            footstepFadeCoroutine = null;

            // restore full volume immediately
            if (footstepSource != null)
                footstepSource.volume = 0.5f;
        }

        if (footstepSource == null) return;
        if (footstepSFX == null || footstepSFX.Count == 0) return;

        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }

        footstepCoroutine = StartCoroutine(FootstepRoutine());
    }

    public void StopFootsteps()
    {
        if (!isWalking) return;
        isWalking = false;

        // stop the step coroutine immediately
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }

        // cancel any existing fade before starting a new one
        if (footstepFadeCoroutine != null)
        {
            StopCoroutine(footstepFadeCoroutine);
            footstepFadeCoroutine = null;
        }

        // fade out instead of hard stop
        if (footstepSource != null && footstepSource.isPlaying)
            footstepFadeCoroutine = StartCoroutine(FadeOutFootsteps());
        else if (footstepSource != null)
            footstepSource.Stop();
    }

    private IEnumerator FadeOutFootsteps()
    {
        float startVolume = footstepSource.volume;
        float fadeDuration = 0.25f;  // short fade — feels natural not sluggish
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            footstepSource.volume = Mathf.Lerp(
                startVolume, 0f,
                elapsed / fadeDuration
            );
            yield return null;
        }

        footstepSource.Stop();

        // restore original volume for next time
        footstepSource.volume = startVolume;
        footstepFadeCoroutine = null;
    }

    private IEnumerator FootstepRoutine()
    {
        while (isWalking)
        {
            if (footstepSFX != null && footstepSFX.Count > 0)
            {
                AudioClip step = footstepSFX[footstepIndex % footstepSFX.Count];
                footstepIndex++;

                if (step != null && footstepSource != null)
                {
                    footstepSource.Stop();       // stop previous before starting new
                    footstepSource.clip = step;
                    footstepSource.loop = false;
                    footstepSource.Play();

                    // wait exactly as long as the clip plays
                    // then add the gap between steps
                    yield return new WaitForSeconds(
                        step.length + footstepInterval
                    );
                }
                else
                {
                    yield return new WaitForSeconds(footstepInterval);
                }
            }
            else
            {
                yield return new WaitForSeconds(footstepInterval);
            }
        }

        if (footstepSource != null)
            footstepSource.Stop();
    }

    // ── Loop Change ───────────────────────────────────────────
    public void OnLoopChanged(int loop)
    {
        PlayMusic(loop);
        Debug.Log($"[AudioManager] Loop changed to {loop} " +
                  $"— audio updated.");
    }

    // ── Silence everything ────────────────────────────────────
    public void SilenceAll()
    {
        if (musicSource != null) musicSource.Stop();
        if (ambienceSource != null) ambienceSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        StopHeartbeat();
        StopFootsteps();
    }
}