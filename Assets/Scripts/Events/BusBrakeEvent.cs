using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// OJO: el archivo DEBE llamarse BusBrakeEvent.cs (antes era BusBreakEvent.cs)
public class BusBrakeEvent : BusEventBase
{
    [Header("References")]
    [SerializeField] private HandrailInteractable handrail;

    [Header("Event Settings")]
    [SerializeField] private float warningDuration = 1.5f;
    [Tooltip("Cuánto dura el mareo si el jugador se cae.")]
    [SerializeField] private float fallEffectDuration = 4f;

    [Header("Efectos (cámara lenta, mareo, sonido distorsionado...)")]
    public UnityEvent onBrakeHit;
    public UnityEvent onPlayerFell;
    public UnityEvent onPlayerRecovered;

    // El frenazo no roba plata: puede coincidir con otras amenazas
    public override bool IsMoneyThreat => false;

    public bool PlayerIsStunned { get; private set; }

    // Se mantiene para los botones de debug
    public void TriggerBrake() => Trigger();

    public override void Trigger()
    {
        if (!CanTrigger)
            return;

        StartCoroutine(BrakeSequence());
    }

    private IEnumerator BrakeSequence()
    {
        onBrakeHit?.Invoke();
        BeginEvent();
        Alert("¡El MIO va a frenar! Mantén CTRL para agarrarte", AlertLevel.Warning);

        float timer = 0f;
        while (timer < warningDuration)
        {
            timer += Time.deltaTime;
            ShowProgress("Frenazo...", 1f - timer / warningDuration);
            yield return null;
        }

        HideProgress();
        

        if (handrail != null && handrail.IsHolding)
        {
            handrail.ForceRelease();
            Alert("¡Aguantaste el frenazo!", AlertLevel.Success);
        }
        else
        {
            Alert("¡Te caíste! Estás mareado", AlertLevel.Danger);
            yield return StartCoroutine(FallSequence());
        }

        EndEvent();
    }

    private IEnumerator FallSequence()
    {
        PlayerIsStunned = true;
        onPlayerFell?.Invoke();

        float timer = 0f;
        while (timer < fallEffectDuration)
        {
            timer += Time.deltaTime;
            ShowProgress("Mareado", 1f - timer / fallEffectDuration);
            yield return null;
        }

        PlayerIsStunned = false;
        onPlayerRecovered?.Invoke();
    }

    public override void CancelEvent()
    {
        if (PlayerIsStunned)
        {
            PlayerIsStunned = false;
            onPlayerRecovered?.Invoke();
        }

        base.CancelEvent();
    }
}