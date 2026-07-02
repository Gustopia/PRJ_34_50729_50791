using UnityEngine;

public class SceneAudio : MonoBehaviour
{
    [Tooltip("Nome da música")]
    [SerializeField] private string musicTrack;

    [Tooltip("Loops de sons ambientes da cena")]
    [SerializeField] private string[] ambientLoops;

    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        if (MusicManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(musicTrack))
                MusicManager.Instance.PlayMusic(musicTrack, fadeDuration);
            else
                MusicManager.Instance.StopMusic(fadeDuration);
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.SyncLoops(ambientLoops, fadeDuration);
    }
}