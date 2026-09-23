using System.Collections;
using UnityEngine;

public class BusBrakeEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandrailInteractable handrail;

    [Header("Event Settings")]
    [SerializeField] private float warningDuration = 1.5f;

    private bool eventActive = false;

    public void TriggerBrake()
    {
        if (eventActive)
            return;

        StartCoroutine(BrakeSequence());
    }

    private IEnumerator BrakeSequence()
    {
        eventActive = true;

        Debug.Log("⚠ FRENazo inminente");

        yield return new WaitForSeconds(warningDuration);

        Debug.Log("🚌 ¡FRENazo!");

        if (handrail != null && handrail.IsHolding)
        {
            Debug.Log("Jugador estaba agarrado.");

            handrail.ForceRelease();

            Debug.Log("Jugador se soltó por el frenazo.");
        }
        else
        {
            Debug.Log("Jugador no estaba agarrado. Se cae.");

            PlayerFall();
        }

        eventActive = false;
    }

    private void PlayerFall()
    {
        Debug.Log("Jugador cae y queda temporalmente afectado.");
    }
}