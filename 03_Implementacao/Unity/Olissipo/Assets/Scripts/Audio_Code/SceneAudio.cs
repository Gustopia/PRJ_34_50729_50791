using UnityEngine;

public class SceneAudio : MonoBehaviour
{
    [Tooltip("Nome da track de música (deixar vazio para parar a música).")]
    [SerializeField] private string musicTrack;

    [Tooltip("Loops ambientes que devem estar a tocar nesta cena.")]
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