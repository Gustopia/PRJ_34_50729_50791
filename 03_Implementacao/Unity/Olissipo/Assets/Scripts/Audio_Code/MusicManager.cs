using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;

    private Coroutine currentFadeCoroutine; // Tracks the active fade coroutine

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        AudioClip clip = musicLibrary.GetClipFromName(trackName);
        if (clip == null)
        {
            Debug.LogWarning($"[MusicManager] Track '{trackName}' não encontrada.");
            return;
        }
        if (musicSource.clip == clip && musicSource.isPlaying) return; // evita re-crossfade da mesma track

        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
    }

    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }

        currentFadeCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
    }

    private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration)
    {
        // Fade out current track
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(1, 0, percent);
            yield return null;
        }

        // Switch to new track
        musicSource.clip = nextTrack;
        musicSource.Play();

        // Fade in new track
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1, percent);
            yield return null;
        }

        // Ensure volume is exactly 1 and clear the coroutine reference
        musicSource.volume = 1;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStop(float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float percent = 0;

        // Fade out to zero
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }

        // Stop playback and reset volume
        musicSource.Stop();
        musicSource.volume = 0;
        currentFadeCoroutine = null;
    }
}