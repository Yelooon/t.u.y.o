using System.Collections;
using UnityEngine;

public class UFOConspiracyEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cameraController;

    [Header("Audio")]
    [SerializeField] private AudioSource conspiracyAudio;
    [SerializeField] private bool showSoundCaptions = true;

    [Header("Dialogue")]
    [SerializeField] private UFOConspiracyDialogue dialogue;

    [Header("Animator Parameters")]
    [SerializeField] private string molestarTrigger = "Molestar";
    [SerializeField] private string asentirTrigger = "Asentir";
    [SerializeField] private string negarTrigger = "Negar";

    public override bool IsMoneyThreat => false;
    public bool EventActive => IsActive;

    private bool isTalking = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
    }

    public void StartConspiracyEvent() => Trigger();

    public override void Trigger()
    {
        if (!CanTrigger)
            return;

        StartCoroutine(ConspiracySequence());
    }

    private IEnumerator ConspiracySequence()
    {
        BeginEvent();

        // Bloquea la cámara mirando al frente
        if (cameraController != null)
        {
            cameraController.LookFront();
            cameraController.SetLock(true);
        }

        if (showSoundCaptions)
            Alert("[Alguien viene hablando de ovnis...]", AlertLevel.Info);

        if (animator != null)
            animator.SetTrigger(molestarTrigger);

        isTalking = true;

        if (conspiracyAudio != null)
            conspiracyAudio.Play();

        if (dialogue != null)
            dialogue.StartDialogue(this);

        Alert("¡Encuentra la imagen de los aliens para callarlo!", AlertLevel.Warning);

        // Bucle infinito: Mantiene el evento activo sin límite de tiempo
        while (isTalking)
        {
            yield return null;
        }
    }

    /// <summary>Llamada por UFOConspiracyDialogue cuando se acierta la imagen.</summary>
    public void CorrectAnswer()
    {
        if (!IsActive || !isTalking)
            return;

        StopAllCoroutines();

        if (animator != null)
            animator.SetTrigger(asentirTrigger);

        Alert("Conspiranoico: ¡¿VES?! ¡TÚ SÍ ENTIENDES!", AlertLevel.Success);

        StopTalking();
        EndEvent();
    }

    /// <summary>Llamada por UFOConspiracyDialogue cuando se falla la imagen.</summary>
    public void WrongAnswer()
    {
        if (!IsActive || !isTalking)
            return;

        if (animator != null)
            animator.SetTrigger(negarTrigger);

        Alert("Conspiranoico: ¡No, eso no tiene nada que ver con los aliens!", AlertLevel.Warning);
    }

    private void StopTalking()
    {
        isTalking = false;
        HideProgress();

        // Desbloquea la cámara al terminar
        if (cameraController != null)
        {
            cameraController.SetLock(false);
        }

        if (conspiracyAudio != null)
            conspiracyAudio.Stop();

        if (dialogue != null)
            dialogue.EndDialogue();
    }

    public override void CancelEvent()
    {
        StopAllCoroutines();

        if (isTalking)
            StopTalking();

        base.CancelEvent();
    }
}