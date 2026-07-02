using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;

    [SerializeField]
    private AudioMixerGroup sfxGroup;

    [Header("Loops / Ambient")]
    [SerializeField] private AudioMixerGroup loopGroup;
    [SerializeField] private float defaultLoopFade = 1f;

    private readonly Dictionary<string, AudioSource> activeLoops = new();
    private readonly Dictionary<string, Coroutine> activeFades = new();


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

    public void PlaySound3D(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject go = new GameObject("OneShot3D");
        go.transform.position = position;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f;
        src.outputAudioMixerGroup = sfxGroup;
        src.Play();
        Destroy(go, clip.length);
    }

    public void PlaySound3D(string soundName, Vector3 position)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), position);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));
    }

    public void StopAll2D()
    {
        sfx2DSource.Stop();
    }

    // Loops de sons ambiente
    public void PlayLoop(string soundName, float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultLoopFade;

        if (activeLoops.TryGetValue(soundName, out var existing))
        {
            if (activeFades.TryGetValue(soundName, out var c) && c != null)
                StopCoroutine(c);
            StartFade(soundName, existing, existing.volume, 1f, fadeDuration, false);
            return;
        }

        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] Loop '{soundName}' não encontrado.");
            return;
        }

        var go = new GameObject($"Loop_{soundName}");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.volume = 0f;
        src.spatialBlend = 0f;
        src.outputAudioMixerGroup = loopGroup != null ? loopGroup : sfxGroup;
        src.Play();

        activeLoops[soundName] = src;
        StartFade(soundName, src, 0f, 1f, fadeDuration, false);
    }

    public void StopLoop(string soundName, float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultLoopFade;
        if (!activeLoops.TryGetValue(soundName, out var src)) return;

        StartFade(soundName, src, src.volume, 0f, fadeDuration, true);
    }

    public void StopAllLoops(float fadeDuration = -1f)
    {
        var keys = new List<string>(activeLoops.Keys);
        foreach (var k in keys) StopLoop(k, fadeDuration);
    }

    // Para loops fora da lista, arranca os novos, mantém os que coincidem
    public void SyncLoops(IEnumerable<string> wanted, float fadeDuration = -1f)
    {
        var wantedSet = new HashSet<string>(wanted ?? System.Array.Empty<string>());

        var toStop = new List<string>();
        foreach (var key in activeLoops.Keys)
            if (!wantedSet.Contains(key)) toStop.Add(key);
        foreach (var k in toStop) StopLoop(k, fadeDuration);

        foreach (var name in wantedSet)
            if (!activeLoops.ContainsKey(name)) PlayLoop(name, fadeDuration);
    }

    private void StartFade(string key, AudioSource src, float from, float to,
                           float duration, bool destroyOnEnd)
    {
        if (activeFades.TryGetValue(key, out var existing) && existing != null)
            StopCoroutine(existing);

        var co = StartCoroutine(FadeRoutine(key, src, from, to, duration, destroyOnEnd));
        activeFades[key] = co;
    }

    private IEnumerator FadeRoutine(string key, AudioSource src, float from, float to,
                                    float duration, bool destroyOnEnd)
    {
        if (duration <= 0f)
        {
            if (src != null) src.volume = to;
        }
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                if (src == null) yield break;
                src.volume = Mathf.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
            if (src != null) src.volume = to;
        }

        activeFades.Remove(key);

        if (destroyOnEnd)
        {
            activeLoops.Remove(key);
            if (src != null) Destroy(src.gameObject);
        }
    }
}