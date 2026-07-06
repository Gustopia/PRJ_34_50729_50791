/*
 * Este script controla a visibilidade dos elementos UI da área no jogo. 
 * Verifica se há uma área atual definida no VoiceManager e, com base nisso, ativa ou desativa
 * os elementos, permitindo que a UI da área seja exibida apenas quando uma área está ativa.
 */

using UnityEngine;

public class AreaUIPanel : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private void Update()
    {
        bool hasArea = VoiceManager.Instance != null
                       && VoiceManager.Instance.CurrentArea != null;
        if (content.activeSelf != hasArea)
            content.SetActive(hasArea);
    }
}