using System.Collections;
using UnityEngine;

public class MotorcycleRobberyEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private WindowInteractable window;

    [Header("Event Timing")]
    [Tooltip("Tiempo desde que suena la moto hasta que aparece el extraño.")]
    [SerializeField] private float approachTime = 1f;
    [SerializeField] private float reactionTime = 3f;

    [Header("Visual")]
    [SerializeField] private GameObject strangerFace;

    [Header("Audio")]
    [SerializeField] private AudioSource motorcycleAudio;
    [Tooltip("Muestra subtítulos de los sonidos (accesibilidad). Desactívalo si quieres que sea solo por audio.")]
    [SerializeField] private bool showSoundCaptions = true;

    // Solo puede ocurrir con la ventana abierta
    public override bool CanTrigger =>
        base.CanTrigger && window != null && window.IsOpen;

    // Se mantiene para los botones de debug
    public void TriggerRobbery() => Trigger();

    public override void Trigger()
    {
        if (!CanTrigger)
        {
            Debug.Log("Raponazo cancelado: la ventana está cerrada o ya hay uno activo.");
            return;
        }

        StartCoroutine(RobberySequence());
    }

    private IEnumerator RobberySequence()
    {
        BeginEvent();

        if (motorcycleAudio != null)
            motorcycleAudio.Play();

        if (showSoundCaptions)
            Alert("[Se escucha una moto acercándose]", AlertLevel.Info);

        yield return new WaitForSeconds(approachTime);

        // El jugador pudo cerrar la ventana mientras tanto
        if (!window.IsOpen)
        {
            FinishRobbery();
            yield break;
        }

        ShowStranger();
        Alert("¡CIERRA LA VENTANA!", AlertLevel.Danger);

        float timer = 0f;
        while (timer < reactionTime)
        {
            if (!window.IsOpen)
            {
                Alert("¡Cerraste la ventana a tiempo!", AlertLevel.Success);
                FinishRobbery();
                yield break;
            }

            timer += Time.deltaTime;
            ShowProgress("¡Cierra la ventana!", 1f - timer / reactionTime);
            yield return null;
        }

        Alert("¡RAPONAZO!", AlertLevel.Danger);
        StealBill("Raponazo en moto");

        FinishRobbery();
    }

    private void ShowStranger()
    {
        if (strangerFace != null)
            strangerFace.SetActive(true);
    }

    private void HideStranger()
    {
        if (strangerFace != null)
            strangerFace.SetActive(false);
    }

    private void FinishRobbery()
    {
        HideStranger();
        EndEvent();
    }

    public override void CancelEvent()
    {
        HideStranger();
        base.CancelEvent();
    }
}