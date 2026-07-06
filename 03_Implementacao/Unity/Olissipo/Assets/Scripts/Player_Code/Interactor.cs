/*
 * Este script implementa a interface IInteractable e o comportamento de interação do jogador,
 * tornando visivel um prompt de interação quando o jogador olha para um objeto interagível.
 */

using TMPro;
using UnityEngine;

// #my_code
public interface IInteractable
{
    void Interact();
    string GetPrompt() => "[E] - Open Door"; // Texto default; objetos podem fazer override
}

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource; // Player cam
    public float InteractRange = 3f;

    [Header("UI")]
    public GameObject InteractPrompt;
    public TMP_Text PromptText;

    private IInteractable _currentTarget;

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            SetPromptVisible(false);
            return;
        }

        _currentTarget = null;

        Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
        if (Physics.Raycast(r, out RaycastHit hit, InteractRange))
        {
            hit.collider.gameObject.TryGetComponent(out _currentTarget);
        }

        // Toggle UI
        bool hasTarget = _currentTarget != null;
        SetPromptVisible(hasTarget);
        if (hasTarget && PromptText != null)
            PromptText.text = _currentTarget.GetPrompt();

        // Interact
        if (hasTarget && Input.GetKeyDown(KeyCode.E))
            _currentTarget.Interact();
    }

    private void SetPromptVisible(bool visible)
    {
        if (InteractPrompt != null && InteractPrompt.activeSelf != visible)
            InteractPrompt.SetActive(visible);
    }
}