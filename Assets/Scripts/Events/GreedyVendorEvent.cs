using System.Collections;
using UnityEngine;

public class GreedyVendorEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CandyQTE candyQTE;
    [SerializeField] private Transform vendor;
    [SerializeField] private Transform vendorStartPosition;
    [SerializeField] private Transform vendorInteractionPosition;

    [Header("Timing")]
    [SerializeField] private float approachTime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource warningAudio;

    private bool eventActive = false;

    public void StartVendorEvent()
    {
        if (eventActive)
            return;

        StartCoroutine(VendorSequence());
    }

    private IEnumerator VendorSequence()
    {
        eventActive = true;

        Debug.Log("📢 Se escucha al vendedor...");

        if (warningAudio != null)
            warningAudio.Play();

        // Colocar al vendedor en su posición inicial
        if (vendor != null && vendorStartPosition != null)
        {
            vendor.position = vendorStartPosition.position;
            vendor.rotation = vendorStartPosition.rotation;
        }

        // El vendedor se acerca
        yield return StartCoroutine(MoveVendorToPlayer());

        Debug.Log("🧍 El vendedor se acercó.");

        VendorGivesCandy();

        yield return new WaitForSeconds(1f);

        Debug.Log("🍬 Vendedor: Si lo sostienes más de 5 segundos...");

        yield return new WaitForSeconds(1f);

        Debug.Log("🍬 Vendedor: ...ya debes pagármelo.");

        StartCandyQTE();
    }

    private IEnumerator MoveVendorToPlayer()
    {
        if (vendor == null || vendorInteractionPosition == null)
            yield break;

        Vector3 startPosition = vendor.position;
        Quaternion startRotation = vendor.rotation;

        float timer = 0f;

        while (timer < approachTime)
        {
            timer += Time.deltaTime;

            float progress = timer / approachTime;

            vendor.position = Vector3.Lerp(
                startPosition,
                vendorInteractionPosition.position,
                progress
            );

            vendor.rotation = Quaternion.Slerp(
                startRotation,
                vendorInteractionPosition.rotation,
                progress
            );

            yield return null;
        }

        vendor.position = vendorInteractionPosition.position;
        vendor.rotation = vendorInteractionPosition.rotation;
    }

    private void VendorGivesCandy()
    {
        Debug.Log("🍬🍬🍬 El vendedor llenó las manos del jugador de dulces.");
    }

    private void StartCandyQTE()
    {
        if (candyQTE == null)
        {
            Debug.LogWarning("CandyQTE no está asignado.");
            return;
        }

        candyQTE.StartQTE(
            OnQTESuccess,
            OnQTEFailed
        );
    }

    private void OnQTESuccess()
    {
        Debug.Log("🍬✓ Jugador devolvió todos los dulces.");
        Debug.Log("🧍 El vendedor se lleva sus dulces y se marcha.");

        EndVendorEvent();
    }

    private void OnQTEFailed()
    {
        Debug.Log("🍬✗ El jugador no pudo devolver los dulces.");
        Debug.Log("💸 Vendedor: Entonces me los tienes que pagar.");
        Debug.Log("💸 ROBO/PAGO: 100K");

        EndVendorEvent();
    }

    private void EndVendorEvent()
    {
        eventActive = false;
    }
}