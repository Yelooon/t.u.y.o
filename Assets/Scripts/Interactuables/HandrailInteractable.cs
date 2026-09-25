using System.Collections;
using UnityEngine;

public class HandrailInteractable : MonoBehaviour, IInteractable
{
    [Header("Holding")]
    [SerializeField] private float holdDuration = 2f;

    private bool isHolding = false;
    private Coroutine releaseCoroutine;

    public bool IsHolding => isHolding;

    public void Interact()
    {
        if (isHolding)
        {
            ReleaseHandrail();
        }
        else
        {
            GrabHandrail();
        }
    }

    public string GetInteractionText()
    {
        return isHolding ? "Soltar pasamanos" : "Agarrarse al pasamanos";
    }

    private void GrabHandrail()
    {
        isHolding = true;

        Debug.Log("Jugador agarrado al pasamanos.");

        // Si había una cuenta anterior, la cancelamos
        if (releaseCoroutine != null)
        {
            StopCoroutine(releaseCoroutine);
        }

        releaseCoroutine = StartCoroutine(AutoRelease());
    }

    private IEnumerator AutoRelease()
    {
        yield return new WaitForSeconds(holdDuration);

        ReleaseHandrail();

        releaseCoroutine = null;
    }

    public void ForceRelease()
    {
        if (!isHolding)
            return;

        ReleaseHandrail();
    }

    private void ReleaseHandrail()
    {
        isHolding = false;

        if (releaseCoroutine != null)
        {
            StopCoroutine(releaseCoroutine);
            releaseCoroutine = null;
        }

        Debug.Log("Jugador soltó el pasamanos.");
    }
}