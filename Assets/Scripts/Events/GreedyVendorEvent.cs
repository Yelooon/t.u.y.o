using System.Collections;
using UnityEngine;

public class GreedyVendorEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private CandyQTE candyQTE;
    [SerializeField] private Transform vendor;
    [SerializeField] private Transform vendorStartPosition;
    [SerializeField] private Transform vendorInteractionPosition;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [Tooltip("Tiempo entre el silbido y que empiece a caminar hacia ti.")]
    [SerializeField] private float warningTime = 2f;
    [SerializeField] private float approachTime = 3f;
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private float leaveTime = 2f;

    [Header("Rotation Fix")]
    [Tooltip("Ajusta si el modelo mira hacia atrás (180 para girarlo de frente, 90 o -90 si mira de lado).")]
    [SerializeField] private float yRotationOffset = 180f;

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

        // 1. Girar en Idle hacia el punto de interacción
        yield return StartCoroutine(RotateTowardsPosition(vendorInteractionPosition.position, rotationDuration));

        // 2. Una vez mirando hacia el waypoint, empieza a caminar en línea recta
        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorInteractionPosition, approachTime));

        // 3. Al llegar al destino, se detiene (Idle) y gira a mirar al jugador
        SetIdleAnimation();
        yield return StartCoroutine(RotateTowardsPosition(playerTransform.position, rotationDuration));

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
        // 1. Girar en Idle hacia la posición inicial
        SetIdleAnimation();
        yield return StartCoroutine(RotateTowardsPosition(vendorStartPosition.position, rotationDuration));

        // 2. Caminar en línea recta de regreso
        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorStartPosition, leaveTime));

        // 3. Al llegar al punto inicial, se detiene y gira a la rotación de reposo original
        SetIdleAnimation();
        Quaternion originalTargetRotation = vendorStartPosition.rotation * Quaternion.Euler(0f, yRotationOffset, 0f);
        yield return StartCoroutine(RotateToRotation(originalTargetRotation, rotationDuration));

        EndEvent();
    }

    private IEnumerator MoveTo(Transform target, float duration)
    {
        if (vendor == null || target == null)
            yield break;

        Vector3 fromPos = vendor.position;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Desplaza la posición sin alterar la rotación lograda previamente
            vendor.position = Vector3.Lerp(fromPos, target.position, progress);

            yield return null;
        }

        vendor.position = target.position;
    }

    private IEnumerator RotateTowardsPosition(Vector3 targetPosition, float duration)
    {
        if (vendor == null)
            yield break;

        Vector3 direction = targetPosition - vendor.position;
        direction.y = 0f; // Mantener plano horizontal

        if (direction == Vector3.zero)
            yield break;

        // Calcula la mirada al punto objetivo sumando el offset fix de la malla (-Z a +Z)
        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, yRotationOffset, 0f);
        yield return StartCoroutine(RotateToRotation(targetRotation, duration));
    }

    private IEnumerator RotateToRotation(Quaternion targetRotation, float duration)
    {
        if (vendor == null)
            yield break;

        Quaternion startRotation = vendor.rotation;

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

        vendor.position = target.position;
        vendor.rotation = target.rotation * Quaternion.Euler(0f, yRotationOffset, 0f);
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