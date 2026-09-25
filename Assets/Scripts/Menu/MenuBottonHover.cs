using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Hace que un botón crezca un poquito al pasar el mouse. Pónganlo en cada botón.
/// </summary>
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float speed = 12f;
    [SerializeField] private AudioSource hoverSound;

    private Vector3 baseScale;
    private Vector3 targetScale;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void OnDisable()
    {
        transform.localScale = baseScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, targetScale, speed * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = baseScale * hoverScale;

        if (hoverSound != null)
            hoverSound.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }
}