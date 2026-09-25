using System.Collections;
using UnityEngine;

public class UFOConspiracyEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform conspiranoic;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform interactionPosition;

    [Header("Timing")]
    [SerializeField] private float approachTime = 3f;
    [SerializeField] private float conversationTime = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource conspiracyAudio;

    [Header("Dialogue")]
    [SerializeField] private UFOConspiracyDialogue dialogue;

    private bool eventActive = false;

    public bool EventActive => eventActive;

    public void StartConspiracyEvent()
    {
        if (eventActive)
            return;

        StartCoroutine(ConspiracySequence());
    }

    private IEnumerator ConspiracySequence()
    {
        eventActive = true;

        Debug.Log("👽 El conspiranoico se acerca...");

        MoveToStartPosition();

        yield return StartCoroutine(MoveToPlayer());

        Debug.Log("🗣️ El conspiranoico comenzó a hablar.");

        if (conspiracyAudio != null)
            conspiracyAudio.Play();

        if (dialogue != null)
            dialogue.StartDialogue(this);

        yield return new WaitForSeconds(conversationTime);

        if (eventActive)
        {
            Debug.Log("🗣️ El conspiranoico sigue hablando.");

            EndEvent();
        }
    }

    private IEnumerator MoveToPlayer()
    {
        if (conspiranoic == null || interactionPosition == null)
            yield break;

        Vector3 startPos = conspiranoic.position;
        Quaternion startRot = conspiranoic.rotation;

        float timer = 0f;

        while (timer < approachTime)
        {
            timer += Time.deltaTime;

            float progress = timer / approachTime;

            conspiranoic.position = Vector3.Lerp(
                startPos,
                interactionPosition.position,
                progress
            );

            conspiranoic.rotation = Quaternion.Slerp(
                startRot,
                interactionPosition.rotation,
                progress
            );

            yield return null;
        }

        conspiranoic.position = interactionPosition.position;
        conspiranoic.rotation = interactionPosition.rotation;
    }

    private void MoveToStartPosition()
    {
        if (conspiranoic == null || startPosition == null)
            return;

        conspiranoic.position = startPosition.position;
        conspiranoic.rotation = startPosition.rotation;
    }

    public void CorrectAnswer()
    {
        if (!eventActive)
            return;

        Debug.Log("👽✓ ¡El jugador encontró la conspiración alienígena!");

        Debug.Log("🗣️ Conspiranoico: ¡¿VES?! ¡TÚ SÍ ENTIENDES!");

        EndEvent();
    }

    private void EndEvent()
    {
        eventActive = false;

        if (conspiracyAudio != null)
            conspiracyAudio.Stop();

        if (dialogue != null)
            dialogue.EndDialogue();

        Debug.Log("🗣️ El conspiranoico dejó de hablar.");
    }
}