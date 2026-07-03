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