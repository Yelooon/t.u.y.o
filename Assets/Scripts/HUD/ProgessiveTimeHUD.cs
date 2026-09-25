using TMPro;
using UnityEngine;

/// <summary>
/// Prefab de una barra de tiempo de un evento.
/// El Image "fill" debe tener Image Type = Filled, Fill Method = Horizontal.
/// </summary>
public class HUDProgressItem : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private UnityEngine.UI.Image fill;

    public void Set(string text, float normalized, Color color)
    {
        if (label != null)
            label.text = text;

        if (fill != null)
        {
            fill.fillAmount = normalized;
            fill.color = color;
        }
    }
}