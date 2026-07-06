/*
 * Este script define uma classe abstrata chamada que contém 2 métodos abstratos, 
 * AnimateTransitionIn e AnimateTransitionOut, que devem ser implementados por subclasses 
 * para definir as animações específicas de transição de entrada e saída de uma cena. 
 */

using System.Collections;
using UnityEngine;

// #my_code
public abstract class SceneTransition : MonoBehaviour
{
    public abstract IEnumerator AnimateTransitionIn();
    public abstract IEnumerator AnimateTransitionOut();
}