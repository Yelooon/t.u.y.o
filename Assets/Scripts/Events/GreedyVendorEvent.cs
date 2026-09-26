using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreedyVendorEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private CandyQTE candyQTE;
    [SerializeField] private CameraController cameraController;   // Referencia a tu script CameraController
    [SerializeField] private Transform vendor;
    [SerializeField] private Transform vendorStartPosition;
    [SerializeField] private Transform vendorInteractionPosition;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Candy Settings")]
    [SerializeField] private GameObject[] candyObjects;            // Los 4 objetos de dulces
    [SerializeField] private Transform vendorBoxTransform;          // La caja/bandeja del vendedor
    [SerializeField] private Transform[] vendorBoxCandyPoints;      // 4 EmptyObjects en la caja del vendedor
    [SerializeField] private Transform[] playerFaceCandyPoints;     // 4 EmptyObjects hijos de la Cámara/Jugador
    [SerializeField] private float candyMoveDuration = 0.4f;       // Duración de la animación de movimiento

    [Header("Candy Drop Settings (On Fail)")]
    [SerializeField] private float dropDistance = 0.25f;           // Desplazamiento hacia abajo al quedar retenidos

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

    private int activeCandiesInFace = 0;

    private void Awake()
    {
        if (animator == null && vendor != null)
            animator = vendor.GetComponentInChildren<Animator>();

        if (playerTransform == null && Camera.main != null)
            playerTransform = Camera.main.transform;

        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
    }

    private void Start()
    {
        MoveInstant(vendorStartPosition);
        SetIdleAnimation();
        ResetCandiesToVendorBox();
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
        ResetCandiesToVendorBox();

        if (warningAudio != null)
            warningAudio.Play();

        if (showSoundCaptions)
            Alert("[Se escucha el silbido del vendedor]", AlertLevel.Info);

        yield return new WaitForSeconds(warningTime);

        // 1. Girar hacia la posición de interacción
        yield return StartCoroutine(RotateTowardsPosition(vendorInteractionPosition.position, rotationDuration));

        // 2. Caminar hacia la Posición 2 (Interacción)
        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorInteractionPosition, approachTime));

        // 3. Al llegar a la Posición 2: Detenerse, mirar al jugador y CAMBIAR CÁMARA A LEFT + BLOQUEAR
        SetIdleAnimation();
        yield return StartCoroutine(RotateTowardsPosition(playerTransform.position, rotationDuration));

        LockCameraToLeftView();

        Alert("¡El vendedor te llenó las manos de dulces!", AlertLevel.Warning);

        yield return StartCoroutine(AnimateCandiesToPlayer());

        yield return new WaitForSeconds(0.5f);
        Alert("Vendedor: Si lo sostienes más de 5 segundos...", AlertLevel.Info);

        yield return new WaitForSeconds(0.8f);
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

        candyQTE.StartQTE(OnQTESuccess, OnQTEFailed, ReturnOneCandy);
    }

    /// <summary>
    /// Devuelve 1 dulce con ANIMACIÓN a la caja del vendedor por acierto individual.
    /// </summary>
    public void ReturnOneCandy()
    {
        if (activeCandiesInFace <= 0)
            return;

        activeCandiesInFace--;
        int candyIndex = activeCandiesInFace;

        if (candyIndex < candyObjects.Length && candyIndex < vendorBoxCandyPoints.Length)
        {
            Transform boxParent = vendorBoxTransform != null ? vendorBoxTransform : vendorBoxCandyPoints[candyIndex];

            StartCoroutine(MoveCandy(
                candyObjects[candyIndex].transform,
                vendorBoxCandyPoints[candyIndex],
                boxParent,
                candyMoveDuration
            ));
        }
    }

    private void TeleportCandyToBox(int candyIndex)
    {
        if (candyIndex < 0 || candyIndex >= candyObjects.Length) return;

        GameObject candy = candyObjects[candyIndex];
        if (candy == null) return;

        Transform targetPoint = (candyIndex < vendorBoxCandyPoints.Length) ? vendorBoxCandyPoints[candyIndex] : null;
        Transform targetParent = vendorBoxTransform != null ? vendorBoxTransform : targetPoint;

        if (targetParent != null)
        {
            candy.transform.SetParent(targetParent);
        }

        if (targetPoint != null)
        {
            candy.transform.position = targetPoint.position;
            candy.transform.rotation = targetPoint.rotation;
        }
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

        StartCoroutine(FailedLeaveSequence());
    }

    private IEnumerator FailedLeaveSequence()
    {
        // Desbloqueamos la cámara para que el jugador vuelva a mover la vista si lo desea
        UnlockPlayerCamera();

        // 1. Desplazar ligeramente hacia abajo los dulces retenidos
        if (activeCandiesInFace > 0)
        {
            for (int i = 0; i < activeCandiesInFace; i++)
            {
                if (candyObjects[i] != null)
                {
                    StartCoroutine(DropCandySlightly(candyObjects[i].transform, dropDistance, candyMoveDuration));
                }
            }
            yield return new WaitForSeconds(candyMoveDuration);
        }

        // 2. Girar en Idle hacia la posición inicial
        SetIdleAnimation();
        yield return StartCoroutine(RotateTowardsPosition(vendorStartPosition.position, rotationDuration));

        // 3. Caminar de regreso: al llegar al 50% del camino, teletransportar los dulces restantes
        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorStartPosition, leaveTime, onHalfway: TeleportAllRemainingCandies));

        // 4. Detenerse y restaurar la rotación final
        SetIdleAnimation();
        Quaternion originalTargetRotation = vendorStartPosition.rotation * Quaternion.Euler(0f, yRotationOffset, 0f);
        yield return StartCoroutine(RotateToRotation(originalTargetRotation, rotationDuration));

        EndEvent();
    }

    private void TeleportAllRemainingCandies()
    {
        while (activeCandiesInFace > 0)
        {
            activeCandiesInFace--;
            TeleportCandyToBox(activeCandiesInFace);
        }
    }

    private IEnumerator LeaveSequence()
    {
        // Desbloquear la cámara al terminar exitosamente el evento
        UnlockPlayerCamera();

        SetIdleAnimation();
        yield return StartCoroutine(RotateTowardsPosition(vendorStartPosition.position, rotationDuration));

        SetCaminarAnimation();
        yield return StartCoroutine(MoveTo(vendorStartPosition, leaveTime));

        SetIdleAnimation();
        Quaternion originalTargetRotation = vendorStartPosition.rotation * Quaternion.Euler(0f, yRotationOffset, 0f);
        yield return StartCoroutine(RotateToRotation(originalTargetRotation, rotationDuration));

        EndEvent();
    }

    #region Camera Locking Logic

    private void LockCameraToLeftView()
    {
        if (cameraController == null) return;

        cameraController.SetView(CameraController.CameraView.Left);
        cameraController.SetLock(true);
    }

    private void UnlockPlayerCamera()
    {
        if (cameraController == null) return;

        cameraController.SetLock(false);
    }

    #endregion

    #region Candy Animations & Hierarchy

    private void ResetCandiesToVendorBox()
    {
        activeCandiesInFace = 0;
        for (int i = 0; i < candyObjects.Length; i++)
        {
            if (candyObjects[i] == null) continue;

            Transform defaultParent = vendorBoxTransform != null ? vendorBoxTransform : (i < vendorBoxCandyPoints.Length ? vendorBoxCandyPoints[i] : null);

            if (defaultParent != null)
            {
                candyObjects[i].transform.SetParent(defaultParent);
            }

            if (i < vendorBoxCandyPoints.Length && vendorBoxCandyPoints[i] != null)
            {
                candyObjects[i].transform.position = vendorBoxCandyPoints[i].position;
                candyObjects[i].transform.rotation = vendorBoxCandyPoints[i].rotation;
            }
        }
    }

    private IEnumerator AnimateCandiesToPlayer()
    {
        activeCandiesInFace = Mathf.Min(candyObjects.Length, playerFaceCandyPoints.Length);

        for (int i = 0; i < activeCandiesInFace; i++)
        {
            if (candyObjects[i] != null && playerFaceCandyPoints[i] != null)
            {
                StartCoroutine(MoveCandy(
                    candyObjects[i].transform,
                    playerFaceCandyPoints[i],
                    playerFaceCandyPoints[i],
                    candyMoveDuration
                ));
            }
        }

        yield return new WaitForSeconds(candyMoveDuration);
    }

    private IEnumerator MoveCandy(Transform candy, Transform targetPoint, Transform targetParent, float duration)
    {
        if (candy == null || targetPoint == null)
            yield break;

        if (targetParent != null)
        {
            candy.SetParent(targetParent, true);
        }

        Vector3 startLocalPos = candy.localPosition;
        Quaternion startLocalRot = candy.localRotation;

        Vector3 targetLocalPos = targetParent != null ? targetParent.InverseTransformPoint(targetPoint.position) : targetPoint.position;
        Quaternion targetLocalRot = targetParent != null ? Quaternion.Inverse(targetParent.rotation) * targetPoint.rotation : targetPoint.rotation;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);

            candy.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);
            candy.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, t);

            yield return null;
        }

        candy.localPosition = targetLocalPos;
        candy.localRotation = targetLocalRot;
    }

    private IEnumerator DropCandySlightly(Transform candy, float distance, float duration)
    {
        if (candy == null) yield break;

        Vector3 startLocalPos = candy.localPosition;
        Vector3 targetLocalPos = startLocalPos + (Vector3.down * distance);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            candy.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);
            yield return null;
        }

        candy.localPosition = targetLocalPos;
    }

    #endregion

    #region Movement Helpers

    private IEnumerator MoveTo(Transform target, float duration, System.Action onHalfway = null)
    {
        if (vendor == null || target == null)
            yield break;

        Vector3 fromPos = vendor.position;
        float timer = 0f;
        bool triggeredHalfway = false;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (!triggeredHalfway && progress >= 0.5f)
            {
                triggeredHalfway = true;
                onHalfway?.Invoke();
            }

            vendor.position = Vector3.Lerp(fromPos, target.position, progress);
            yield return null;
        }

        if (!triggeredHalfway)
        {
            onHalfway?.Invoke();
        }

        vendor.position = target.position;
    }

    private IEnumerator RotateTowardsPosition(Vector3 targetPosition, float duration)
    {
        if (vendor == null)
            yield break;

        Vector3 direction = targetPosition - vendor.position;
        direction.y = 0f;

        if (direction == Vector3.zero)
            yield break;

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

    #endregion

    public override void CancelEvent()
    {
        StopAllCoroutines();

        UnlockPlayerCamera();

        if (candyQTE != null)
            candyQTE.CancelQTE();

        ResetCandiesToVendorBox();
        MoveInstant(vendorStartPosition);
        SetIdleAnimation();

        base.CancelEvent();
    }
}