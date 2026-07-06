/*
 * Este script move a câmera para a posição de um objeto
 * especificado (cameraPosition) a cada frame.
 */

using UnityEngine;

// #my_code
public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    private void LateUpdate()
    {
        transform.position = cameraPosition.position;
    }
}