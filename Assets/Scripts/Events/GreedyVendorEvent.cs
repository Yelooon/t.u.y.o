using System.Collections;
using UnityEngine;

public class GreedyVendorEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private CandyQTE candyQTE;
    [SerializeField] private Transform vendor;
    [SerializeField] private Transform vendorStartPosition;
    [SerializeField] private Transform vendorInteractionPosition;
    [SerializeField] private Transform playerTransform; // Referencia al Transform del jugador
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [Tooltip("Tiempo entre el silbido y que empiece a caminar hacia ti.")]
    [SerializeField] private float warningTime = 2f;
    [SerializeField] private float approachTime = 3f;
    [SerializeField] private float rotationDuration = 0.5f; // Tiempo que tarda en rotar hacia el jugador
    [SerializeField] private float leaveTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource warningAudio;
    [SerializeField] private bool showSoundCaptions = true;

    [Header("Animator Parameters")]
    [SerializeField] private string idleTrigger = "Idle";
    [SerializeField] private string caminarTrigger = "Caminar";

    private void Awake()
    {
        if (animator == null && vendor != null)
            animator = vendor.GetComponentInChildren<Animator>();

        // Intenta encontrar la cámara principal como fallback del jugador si no está asignada
        if (playerTransform == null && Camera.main != null)
            playerTransform = Camera.main.transform;
    }

    private void Start()
    {
        MoveInstant(vendorStartPosition);
        SetIdleAnimation();
    }

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
        SetIdleAnimation();

        if (warningAudio != null)
            warningAudio.Play();

        if (showSoundCaptions)
            Alert("[Se escucha el silbido del vendedor]", AlertLevel.Info);

        yield return new WaitForSeconds(warningTime);

        // Caminar hacia la posición de interacción
        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorInteractionPosition, approachTime));

        // Pasar a Idle y rotar hacia el jugador
        SetIdleAnimation();
        yield return StartCoroutine(RotateTowards(playerTransform, rotationDuration));

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
        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorStartPosition, leaveTime));

        SetIdleAnimation();
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

    private IEnumerator RotateTowards(Transform target, float duration)
    {
        if (vendor == null || target == null)
            yield break;

        Vector3 direction = target.position - vendor.position;
        direction.y = 0f; // Mantiene la rotación en el plano horizontal (eje Y)

        if (direction == Vector3.zero)
            yield break;

        Quaternion startRotation = vendor.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            vendor.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            yield return null;
        }

        vendor.rotation = targetRotation;
    }

    private void MoveInstant(Transform target)
    {
        if (vendor == null || target == null)
            return;

        vendor.SetPositionAndRotation(target.position, target.rotation);
    }

    private void SetIdleAnimation()
    {
        if (animator == null) return;
        animator.ResetTrigger(caminarTrigger);
        animator.SetTrigger(idleTrigger);
    }

    private void SetCaminarAnimation()
    {
        if (animator == null) return;
        animator.ResetTrigger(idleTrigger);
        animator.SetTrigger(caminarTrigger);
    }

    public override void CancelEvent()
    {
        StopAllCoroutines();

        if (candyQTE != null)
            candyQTE.CancelQTE();

        MoveInstant(vendorStartPosition);
        SetIdleAnimation();

        base.CancelEvent();
    }
}