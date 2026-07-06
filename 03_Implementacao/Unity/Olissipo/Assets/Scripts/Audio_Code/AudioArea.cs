/*
 * Este script define uma área de áudio no jogo. Quando o jogador entra na área, o áudio 
 * associado é reproduzido e o título da área é exibido. Quando o jogador sai da área, o áudio pausa.
 */

using UnityEngine;

public class AudioArea : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private string title;

    public AudioClip Clip => clip;
    public string Title => title;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        VoiceManager.Instance.SetCurrentArea(this);
        VoiceManager.Instance.PlayVoice(clip);

        if (!string.IsNullOrEmpty(title) && AreaTitleDisplay.Instance != null)
            AreaTitleDisplay.Instance.ShowTitle(title);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        VoiceManager.Instance.PauseVoice();
        VoiceManager.Instance.ClearCurrentArea(this);
    }
}