/*
 * Este script é utilizado para criar uma transição de fade in/out entre cenas.
 * Utiliza a biblioteca DOTween para animar o efeito de fade in/out de um CanvasGroup, 
 * que é atribuido à variável crossFade.
 */

using System.Collections;
using UnityEngine;
using DG.Tweening;

// #my_code
public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade;

    public override IEnumerator AnimateTransitionIn()
    {
        var tweener = crossFade.DOFade(1f, 1f);
        yield return tweener.WaitForCompletion();
    }

    public override IEnumerator AnimateTransitionOut()
    {
        var tweener = crossFade.DOFade(0f, 1f);
        yield return tweener.WaitForCompletion();
    }
}