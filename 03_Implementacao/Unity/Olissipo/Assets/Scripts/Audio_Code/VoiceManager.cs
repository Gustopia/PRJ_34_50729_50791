using System.Collections.Generic;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance;

    [SerializeField] private AudioSource voiceSource;

    private AudioClip currentClip;
    private bool isPaused;
    private AudioArea currentArea;

    private class ClipState { public bool completed; public float lastTime; }
    private readonly Dictionary<AudioClip, ClipState> clipStates = new();

    public bool IsPaused => isPaused;
    public AudioArea CurrentArea => currentArea;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (currentClip == null || isPaused || voiceSource.isPlaying) return;
        var state = GetState(currentClip);
        state.completed = true;
        state.lastTime = 0f;
        currentClip = null;
    }

    private ClipState GetState(AudioClip clip)
    {
        if (!clipStates.TryGetValue(clip, out var s))
        {
            s = new ClipState();
            clipStates[clip] = s;
        }
        return s;
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;
        var state = GetState(clip);
        if (state.completed) return;

        if (currentClip == clip && isPaused)
        {
            voiceSource.UnPause();
            isPaused = false;
            return;
        }
        if (currentClip == clip && voiceSource.isPlaying) return;

        if (currentClip != null && voiceSource.isPlaying)
        {
            var prev = GetState(currentClip);
            prev.lastTime = voiceSource.time;
        }

        currentClip = clip;
        voiceSource.clip = clip;
        voiceSource.time = Mathf.Clamp(state.lastTime, 0f, Mathf.Max(0f, clip.length - 0.05f));
        voiceSource.Play();
        isPaused = false;
    }

    public void PauseVoice()
    {
        if (currentClip == null || !voiceSource.isPlaying) return;
        GetState(currentClip).lastTime = voiceSource.time;
        voiceSource.Pause();
        isPaused = true;
    }

    public void TogglePause()
    {
        if (currentClip == null) return;
        if (isPaused)
        {
            voiceSource.UnPause();
            isPaused = false;
        }
        else if (voiceSource.isPlaying)
        {
            GetState(currentClip).lastTime = voiceSource.time;
            voiceSource.Pause();
            isPaused = true;
        }
    }

    public void ReplayFromStart(AudioClip clip)
    {
        if (clip == null) return;
        var state = GetState(clip);
        state.completed = false;
        state.lastTime = 0f;

        currentClip = clip;
        voiceSource.clip = clip;
        voiceSource.time = 0f;
        voiceSource.Play();
        isPaused = false;
    }

    public void ReplayCurrentArea()
    {
        if (currentArea != null) ReplayFromStart(currentArea.Clip);
    }

    public void SetCurrentArea(AudioArea area) => currentArea = area;
    public void ClearCurrentArea(AudioArea area)
    {
        if (currentArea == area) currentArea = null;
    }

    public void StopVoice()
    {
        voiceSource.Stop();
        currentClip = null;
        isPaused = false;
    }
}