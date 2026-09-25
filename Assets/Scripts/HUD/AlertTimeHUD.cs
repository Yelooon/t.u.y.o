using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Prefab de una alerta (un "toast"): aparece, espera y se desvanece.
/// Prefab sugerido: Image (fondo) + CanvasGroup + hijo TextMeshProUGUI.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HUDAlertItem : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private UnityEngine.UI.Image background;
    [SerializeField] private float fadeTime = 0.25f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void Setup(string message, Color color, float duration)
    {
        if (label != null)
            label.text = message;

        if (background != null)
            background.color = color;
        else if (label != null)
            label.color = color;

        StartCoroutine(Life(duration));
    }

    /// <summary>Hace que la alerta desaparezca ya (cuando hay demasiadas).</summary>
    public void Dismiss()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator Life(float duration)
    {
        yield return Fade(0f, 1f);
        yield return new WaitForSecondsRealtime(duration);
        yield return FadeOutAndDestroy();
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return Fade(canvasGroup.alpha, 0f);
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}