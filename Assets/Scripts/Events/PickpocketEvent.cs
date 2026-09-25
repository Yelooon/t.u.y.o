using System.Collections;
using UnityEngine;

public class PickpocketEvent : BusEventBase
{
    [Header("Pickpocket")]
    [SerializeField] private Transform pickpocket;

    [Header("Positions")]
    [SerializeField] private Transform position1;
    [SerializeField] private Transform position2;
    [SerializeField] private Transform position3;

    [Header("Timing")]
    [SerializeField] private float timeBetweenMoves = 5f;
    [Tooltip("Tiempo que tienes estando él en la posición 3 antes de que te robe.")]
    [SerializeField] private float stealDelay = 5f;
    [Tooltip("Segundos seguidos que hay que mirarlo para que se devuelva.")]
    [SerializeField] private float watchDuration = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource movementAudio;
    [Tooltip("Muestra subtítulos de los pasos. Desactívalo si quieres que sea solo por audio.")]
    [SerializeField] private bool showSoundCaptions = true;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;

    private int currentPosition = 1;

    private void Start()
    {
        MovePickpocket(position1);
    }

    // Se mantiene para los botones de debug
    public void StartPickpocket() => Trigger();

    public override void Trigger()
    {
        if (!CanTrigger)
            return;

        StartCoroutine(PickpocketSequence());
    }

    private IEnumerator PickpocketSequence()
    {
        BeginEvent();

        yield return new WaitForSeconds(timeBetweenMoves);
        MoveToPosition(2);

        if (showSoundCaptions)
            Alert("[Pasos detrás de ti]", AlertLevel.Info);

        yield return new WaitForSeconds(timeBetweenMoves);
        MoveToPosition(3);

        if (showSoundCaptions)
            Alert("[Pasos MUY cerca de ti]", AlertLevel.Warning);

        yield return StartCoroutine(CheckIfPlayerWatches());
    }

    private IEnumerator CheckIfPlayerWatches()
    {
        float watchTimer = 0f;
        float dangerTimer = 0f;

        while (true)
        {
            dangerTimer += Time.deltaTime;

            bool isWatching = cameraController != null &&
                cameraController.CurrentView == CameraController.CameraView.Back;

            if (isWatching)
            {
                watchTimer += Time.deltaTime;
                ShowProgress("Vigilando al ladrón", watchTimer / watchDuration);

                if (watchTimer >= watchDuration)
                {
                    Alert("El ladrón se echó para atrás", AlertLevel.Success);
                    FinishPickpocket();
                    yield break;
                }
            }
            else
            {
                watchTimer = 0f;
                HideProgress();
            }

            if (dangerTimer >= stealDelay)
            {
                Alert("¡Te bolsiquearon!", AlertLevel.Danger);
                StealBill("Ladrón silencioso");
                FinishPickpocket();
                yield break;
            }

            yield return null;
        }
    }

    private void MoveToPosition(int newPosition)
    {
        currentPosition = newPosition;

        switch (newPosition)
        {
            case 1: MovePickpocket(position1); break;
            case 2: MovePickpocket(position2); break;
            case 3: MovePickpocket(position3); break;
        }

        if (movementAudio != null)
            movementAudio.Play();

        Debug.Log("Pickpocket -> posición " + currentPosition);
    }

    private void MovePickpocket(Transform target)
    {
        if (pickpocket == null || target == null)
            return;

        pickpocket.SetPositionAndRotation(target.position, target.rotation);
    }

    private void FinishPickpocket()
    {
        currentPosition = 1;
        MovePickpocket(position1);

        if (cameraController != null)
            cameraController.LookFront();

        EndEvent();
    }

    public override void CancelEvent()
    {
        StopAllCoroutines();
        currentPosition = 1;
        MovePickpocket(position1);
        base.CancelEvent();
    }
}