using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AreaActionButton : MonoBehaviour
{
    private enum Action { Replay, TogglePause }

    [SerializeField] private Action action;
    [SerializeField] private KeyCode key = KeyCode.None;

    [SerializeField] private TMPro.TMP_Text label;
    [SerializeField] private string pauseText = "[P] - PAUSE";
    [SerializeField] private string resumeText = "[P] - PLAY";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Execute);
    }

    private void Update()
    {
        if (key != KeyCode.None
            && Input.GetKeyDown(key)
            && VoiceManager.Instance.CurrentArea != null)
        {
            Execute();
        }

        if (action == Action.TogglePause && label != null)
            label.text = VoiceManager.Instance.IsPaused ? resumeText : pauseText;
    }

    private void Execute()
    {
        switch (action)
        {
            case Action.Replay: VoiceManager.Instance.ReplayCurrentArea(); break;
            case Action.TogglePause: VoiceManager.Instance.TogglePause(); break;
        }
    }
}