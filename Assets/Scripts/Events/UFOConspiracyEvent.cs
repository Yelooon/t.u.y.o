using System.Collections;
using UnityEngine;

public class UFOConspiracyEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private Transform conspiranoic;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform interactionPosition;

    [Header("Timing")]
    [SerializeField] private float approachTime = 3f;
    [SerializeField] private float conversationTime = 10f;
    [Tooltip("Tiempo que tarda en devolverse a su puesto al terminar.")]
    [SerializeField] private float leaveTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource conspiracyAudio;
    [SerializeField] private bool showSoundCaptions = true;

    [Header("Dialogue")]
    [SerializeField] private UFOConspiracyDialogue dialogue;

    // No roba plata: solo estorba. Puede coincidir con otras amenazas.
    public override bool IsMoneyThreat => false;

    // Se mantiene por si UFOConspiracyDialogue lo usa
    public bool EventActive => IsActive;

    private bool isTalking = false;

    private void Start()
    {
        MoveInstant(startPosition);
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

        MoveInstant(startPosition);

        if (showSoundCaptions)
            Alert("[Alguien viene hablando de ovnis...]", AlertLevel.Info);

        yield return StartCoroutine(MoveTo(interactionPosition, approachTime));

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

        // Se acabó el tiempo sin encontrar la respuesta: se va solo
        Alert("El conspiranoico se cansó de hablar", AlertLevel.Info);
        yield return StartCoroutine(StopTalkingAndLeave());
    }

    /// <summary>La llama UFOConspiracyDialogue cuando el jugador clickea la imagen correcta.</summary>
    public void CorrectAnswer()
    {
        if (!IsActive || !isTalking)
            return;

        StopAllCoroutines();

        Alert("Conspiranoico: ¡¿VES?! ¡TÚ SÍ ENTIENDES!", AlertLevel.Success);

        StartCoroutine(StopTalkingAndLeave());
    }

    private IEnumerator StopTalkingAndLeave()
    {
        StopTalking();

        yield return StartCoroutine(MoveTo(startPosition, leaveTime));

        EndEvent();
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

    private IEnumerator MoveTo(Transform target, float duration)
    {
        if (conspiranoic == null || target == null)
            yield break;

        Vector3 fromPos = conspiranoic.position;
        Quaternion fromRot = conspiranoic.rotation;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            conspiranoic.SetPositionAndRotation(
                Vector3.Lerp(fromPos, target.position, progress),
                Quaternion.Slerp(fromRot, target.rotation, progress));

            yield return null;
        }

        conspiranoic.SetPositionAndRotation(target.position, target.rotation);
    }

    private void MoveInstant(Transform target)
    {
        if (conspiranoic == null || target == null)
            return;

        conspiranoic.SetPositionAndRotation(target.position, target.rotation);
    }

    public override void CancelEvent()
    {
        StopAllCoroutines();

        if (isTalking)
            StopTalking();

        MoveInstant(startPosition);
        base.CancelEvent();
    }
}