using System.Collections;
using UnityEngine;

public class WindowInteractable : MonoBehaviour, IInteractable
{
    private bool isOpen = false;

    [Header("Positions")]
    public Transform PosOpen;
    public Transform PosClosed;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.5f;

    private Coroutine moveCoroutine;

    public bool IsOpen => isOpen;

    public void Interact()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            OpenWindow();
        }
        else
        {
            CloseWindow();
        }
    }

    public string GetInteractionText()
    {
        return isOpen ? "Cerrar ventana" : "Abrir ventana";
    }

    private void OpenWindow()
    {
        Debug.Log("🪟 Ventana ABIERTA");
        AnimateTo(PosOpen);
    }

    private void CloseWindow()
    {
        Debug.Log("🪟 Ventana CERRADA");
        AnimateTo(PosClosed);
    }

    private void AnimateTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Posición de la ventana no asignada.");
            return;
        }

        // Si la ventana ya se estaba moviendo, detiene la animación previa para iniciar la nueva de forma fluida
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Transición suave al inicio y al final (Easing)
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.localPosition = Vector3.Lerp(startPos, target.localPosition, smoothProgress);
            transform.localRotation = Quaternion.Slerp(startRot, target.localRotation, smoothProgress);

            yield return null;
        }

        transform.localPosition = target.localPosition;
        transform.localRotation = target.localRotation;
    }
}