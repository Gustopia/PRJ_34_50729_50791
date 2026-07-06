/*
 * Este script é responsável por gerir a música e os sons ambientes de uma cena.
 * Utiliza o MusicManager para tocar a música da cena e o SoundManager para sincronizar os loops de sons ambientes.
 */

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