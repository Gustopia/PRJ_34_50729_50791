using System.Collections;
using TMPro;
using UnityEngine;

public class AreaTitleDisplay : MonoBehaviour
{
    public static AreaTitleDisplay Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float visibleDuration = 3f;
    [SerializeField] private float fadeOutDuration = 1f;

    private Coroutine routine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        canvasGroup.alpha = 0f;
    }

    public void ShowTitle(string title)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(title));
    }

    private IEnumerator ShowRoutine(string title)
    {
        titleText.text = title;
        yield return Fade(canvasGroup.alpha, 1f, fadeInDuration);
        yield return new WaitForSeconds(visibleDuration);
        yield return Fade(1f, 0f, fadeOutDuration);
        routine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f) { canvasGroup.alpha = to; yield break; }
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}