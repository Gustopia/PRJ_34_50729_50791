/*
 * Este script é usado para definir a superfície de passos do jogador. 
 * Contém uma variável pública "soundName" que armazena o nome do som que será reproduzido quando o 
 * jogador caminhar sobre esta superfície. O valor padrão é "FootstepWater", 
 * mas pode ser alterado no Inspector do Unity para qualquer outro som desejado.
 */

using UnityEngine;

public class FootstepSurface : MonoBehaviour
{
    [Tooltip("groupID da SoundLibrary que toca quando o jogador caminha sobre esta superfície.")]
    public string soundName = "FootstepWater";
}