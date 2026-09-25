using System.Collections;
using UnityEngine;

public class UFOConspiracyEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [SerializeField] private float conversationTime = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource conspiracyAudio;
    [SerializeField] private bool showSoundCaptions = true;

    [Header("Dialogue")]
    [SerializeField] private UFOConspiracyDialogue dialogue;

    [Header("Animator Parameters")]
    [SerializeField] private string molestarTrigger = "Molestar";
    [SerializeField] private string asentirTrigger = "Asentir";
    [SerializeField] private string negarTrigger = "Negar";

    // No roba plata: solo estorba. Puede coincidir con otras amenazas.
    public override bool IsMoneyThreat => false;

    // Se mantiene por si UFOConspiracyDialogue lo usa
    public bool EventActive => IsActive;

    private bool isTalking = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    // Se mantiene para los botones de debug
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

        if (showSoundCaptions)
            Alert("[Alguien viene hablando de ovnis...]", AlertLevel.Info);

        // Activa la transición para empezar a hablar/molestar
        if (animator != null)
            animator.SetTrigger(molestarTrigger);

        isTalking = true;

        if (conspiracyAudio != null)
            conspiracyAudio.Play();

        if (dialogue != null)
            dialogue.StartDialogue(this);

        Alert("¡Encuentra la imagen de los aliens para callarlo!", AlertLevel.Warning);

        float timer = 0f;
        while (timer < conversationTime)
        {
            timer += Time.deltaTime;
            ShowProgress("El conspiranoico no se calla", 1f - timer / conversationTime);
            yield return null;
        }

        // Se acabó el tiempo sin encontrar la respuesta
        Alert("El conspiranoico se cansó de hablar", AlertLevel.Info);
        StopTalking();
        EndEvent();
    }

    /// <summary>La llama UFOConspiracyDialogue cuando el jugador clickea la imagen correcta.</summary>
    public void CorrectAnswer()
    {
        if (!IsActive || !isTalking)
            return;

        StopAllCoroutines();

        // Ejecuta la animación de asentir (el Animator la devolverá a Idle según tus transiciones)
        if (animator != null)
            animator.SetTrigger(asentirTrigger);

        Alert("Conspiranoico: ¡¿VES?! ¡TÚ SÍ ENTIENDES!", AlertLevel.Success);

        StopTalking();
        EndEvent();
    }

    /// <summary>Llamar desde UFOConspiracyDialogue o UI cuando el jugador clickea una opción/imagen incorrecta.</summary>
    public void WrongAnswer()
    {
        if (!IsActive || !isTalking)
            return;

        // Ejecuta la animación de negar y regresa al estado de hablar
        if (animator != null)
            animator.SetTrigger(negarTrigger);

        Alert("Conspiranoico: ¡No, eso no tiene nada que ver con los aliens!", AlertLevel.Warning);
    }

    private void StopTalking()
    {
        isTalking = false;
        HideProgress();

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