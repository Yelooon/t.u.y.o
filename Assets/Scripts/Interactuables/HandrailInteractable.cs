using UnityEngine;

public class HandrailInteractable : MonoBehaviour, IInteractable
{
    private bool isHolding = false;

    public void Interact()
    {
        isHolding = !isHolding;

        if (isHolding)
        {
            GrabHandrail();
        }
        else
        {
            ReleaseHandrail();
        }
    }

    public string GetInteractionText()
    {
        return isHolding ? "Soltar pasamanos" : "Agarrarse al pasamanos";
    }

    private void GrabHandrail()
    {
        Debug.Log("Jugador agarrado al pasamanos");
    }

    private void ReleaseHandrail()
    {
        Debug.Log("Jugador soltó el pasamanos");
    }
}