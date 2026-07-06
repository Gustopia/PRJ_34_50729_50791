/*
 * Este script gere a reprodução de música no jogo, permitindo crossfade entre faixas e controle de volume.
 * Permite iniciar, parar e alternar entre diferentes faixas de música com efeitos de fade in/out.
 * Também garante que apenas uma instância do MusicManager exista durante a execução do jogo.
 */

using System.Collections;
using UnityEngine;

// #my_code
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;

    private Coroutine currentFadeCoroutine; // Coroutine de fade ativa

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
        if (musicSource.clip == clip && musicSource.isPlaying) return; // Evitar re-crossfade da mesma track

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
        // Fade out do track atual
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(1, 0, percent);
            yield return null;
        }

        // Mudar para novo track
        musicSource.clip = nextTrack;
        musicSource.Play();

        // Fade in do novo track
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1, percent);
            yield return null;
        }

        // Garantir que o volume está exatamente em 1 e limpar a referência da coroutine
        musicSource.volume = 1;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStop(float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float percent = 0;

        // Fade out para zero
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }

        // Parar reprodução e redefinir volume
        musicSource.Stop();
        musicSource.volume = 0;
        currentFadeCoroutine = null;
    }
}