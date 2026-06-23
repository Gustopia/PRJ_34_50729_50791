using UnityEngine;

public class ChangeScene : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneName;

    public void Interact()
    {
        LevelManager.Instance.LoadScene(sceneName, "CrossFade");
    }
}