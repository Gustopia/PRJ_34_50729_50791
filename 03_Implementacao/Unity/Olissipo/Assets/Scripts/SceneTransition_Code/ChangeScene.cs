/* 
 * Este script permite a mudança de cena, utilizando o método LoadScene da 
 * classe LevelManager, quando o jogador interage com um objeto específico.
 */

using UnityEngine;

// #my_code
public class ChangeScene : MonoBehaviour, IInteractable
{
    [SerializeField] private string sceneName;

    public void Interact()
    {
        LevelManager.Instance.LoadScene(sceneName, "CrossFade");
    }
}