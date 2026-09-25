using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Raycast")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactionLayer;

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;

    private IInteractable currentInteractable;

    private void Update()
    {
        DetectInteractable();

        if (currentInteractable != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            currentInteractable.Interact();
        }
    }

    private void DetectInteractable()
    {
        currentInteractable = null;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayer
        ))
        {
            IInteractable interactable =
                hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;

                ShowPrompt(interactable.GetInteractionText());
                return;
            }
        }

        HidePrompt();
    }

    private void ShowPrompt(string text)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        if (interactionText != null)
        {
            interactionText.text = text;
        }
    }

    private void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}