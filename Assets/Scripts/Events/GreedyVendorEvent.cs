using System.Collections;
using UnityEngine;

public class GreedyVendorEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private CandyQTE candyQTE;
    [SerializeField] private Transform vendor;
    [SerializeField] private Transform vendorStartPosition;
    [SerializeField] private Transform vendorInteractionPosition;

    [Header("Timing")]
    [Tooltip("Tiempo entre el silbido y que empiece a caminar hacia ti.")]
    [SerializeField] private float warningTime = 2f;
    [SerializeField] private float approachTime = 3f;
    [SerializeField] private float leaveTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource warningAudio;
    [SerializeField] private bool showSoundCaptions = true;


    private void Start()
    {
        MoveInstant(vendorStartPosition);
    }

    // Se mantiene para los botones de debug
    public void StartVendorEvent() => Trigger();

    public override void Trigger()
    {
        if (!CanTrigger)
            return;

        StartCoroutine(VendorSequence());
    }

    private IEnumerator VendorSequence()
    {
        BeginEvent();

        MoveInstant(vendorStartPosition);

        if (warningAudio != null)
            warningAudio.Play();

        if (showSoundCaptions)
            Alert("[Se escucha el silbido del vendedor]", AlertLevel.Info);

        yield return new WaitForSeconds(warningTime);

        yield return StartCoroutine(MoveTo(vendorInteractionPosition, approachTime));

        // Aquí pueden activar los modelos de dulces en las manos
        Alert("¡El vendedor te llenó las manos de dulces!", AlertLevel.Warning);

        yield return new WaitForSeconds(1f);
        Alert("Vendedor: Si lo sostienes más de 5 segundos...", AlertLevel.Info);

        yield return new WaitForSeconds(1f);
        Alert("Vendedor: ...ya debes pagármelo.", AlertLevel.Danger);

        StartCandyQTE();
    }

    private void StartCandyQTE()
    {
        if (candyQTE == null)
        {
            Debug.LogWarning("CandyQTE no está asignado.");
            StartCoroutine(LeaveSequence());
            return;
        }

        candyQTE.StartQTE(OnQTESuccess, OnQTEFailed);
    }

    private void OnQTESuccess()
    {
        // Si el viaje terminó mientras el QTE corría, ignorar
        if (!IsActive)
            return;

        Alert("Le devolviste todos los dulces", AlertLevel.Success);
        StartCoroutine(LeaveSequence());
    }

    private void OnQTEFailed()
    {
        if (!IsActive)
            return;

        Alert("Vendedor: Entonces me los tienes que pagar", AlertLevel.Danger);
        StealBill("Vendedor avaricioso");
        StartCoroutine(LeaveSequence());
    }

    private IEnumerator LeaveSequence()
    {
        yield return StartCoroutine(MoveTo(vendorStartPosition, leaveTime));
        EndEvent();
    }

    private IEnumerator MoveTo(Transform target, float duration)
    {
        if (vendor == null || target == null)
            yield break;

        Vector3 fromPos = vendor.position;
        Quaternion fromRot = vendor.rotation;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            vendor.SetPositionAndRotation(
                Vector3.Lerp(fromPos, target.position, progress),
                Quaternion.Slerp(fromRot, target.rotation, progress));

            yield return null;
        }

        vendor.SetPositionAndRotation(target.position, target.rotation);
    }

    private void MoveInstant(Transform target)
    {
        if (vendor == null || target == null)
            return;

        vendor.SetPositionAndRotation(target.position, target.rotation);
    }

    public override void CancelEvent()
    {
        StopAllCoroutines();

        // Esconder la UI del minijuego si estaba en pantalla
        if (candyQTE != null)
            candyQTE.CancelQTE();

        MoveInstant(vendorStartPosition);
        base.CancelEvent();
    }
}