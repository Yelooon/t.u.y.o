using UnityEngine;

public class WindowInteractable : MonoBehaviour, IInteractable
{
    private bool isOpen = false;

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
        Debug.Log("Ventana abierta");
    }

    private void CloseWindow()
    {
        Debug.Log("Ventana cerrada");
    }
}