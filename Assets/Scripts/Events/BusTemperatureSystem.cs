using System;
using UnityEngine;
using UnityEngine.Events;

public class BusTemperatureSystem : MonoBehaviour
{
    public enum TemperatureCondition
    {
        Normal,
        Hot,    // Aire dañado + ventana cerrada
        Cold    // Aire funcionando + ventana abierta
    }

    [Header("References")]
    [SerializeField] private BusAirConditioner airConditioner;
    [SerializeField] private WindowInteractable window;

    [Header("Temperature Threshold")]
    [Tooltip("Segundos en mala condición hasta que el jugador queda afectado.")]
    [SerializeField] private float temperatureThreshold = 3f;
    [Tooltip("Qué tan rápido se recupera comparado con lo que tarda en afectarse (1 = igual).")]
    [SerializeField] private float recoverySpeed = 1.5f;

    [Header("Efectos (conecta aquí cámara lenta, temblor, mareo, etc.)")]
    public UnityEvent onPlayerOverheat;
    public UnityEvent onPlayerFreeze;
    public UnityEvent onPlayerRecovered;

    public TemperatureCondition Condition { get; private set; } = TemperatureCondition.Normal;

    /// <summary>0 = bien, 1 = afectado. El HUD lo usa para la barra.</summary>
    public float Exposure { get; private set; }

    public bool IsAffected { get; private set; }

    /// <summary>La condición que causó el efecto actual (sigue siendo válida mientras se recupera).</summary>
    public TemperatureCondition AffectedBy { get; private set; } = TemperatureCondition.Normal;

    public event Action<TemperatureCondition> ConditionChanged;
    public event Action<TemperatureCondition> PlayerAffected;
    public event Action PlayerRecovered;

    private void Update()
    {
        if (window == null || airConditioner == null)
            return;

        TemperatureCondition newCondition = GetCondition();

        if (newCondition != Condition)
        {
            // Pasó directo de calor a frío (o al revés): empezar de cero
            if (newCondition != TemperatureCondition.Normal &&
                Condition != TemperatureCondition.Normal)
            {
                Exposure = 0f;
                SetAffected(false, TemperatureCondition.Normal);
            }

            Condition = newCondition;
            ConditionChanged?.Invoke(Condition);
        }

        float rate = Time.deltaTime / Mathf.Max(0.01f, temperatureThreshold);

        if (Condition != TemperatureCondition.Normal)
        {
            Exposure = Mathf.Clamp01(Exposure + rate);

            if (Exposure >= 1f && !IsAffected)
                SetAffected(true, Condition);
        }
        else
        {
            Exposure = Mathf.Clamp01(Exposure - rate * recoverySpeed);

            if (Exposure <= 0f && IsAffected)
                SetAffected(false, TemperatureCondition.Normal);
        }
    }

    private TemperatureCondition GetCondition()
    {
        if (window.IsOpen && airConditioner.IsWorking)
            return TemperatureCondition.Cold;

        if (!window.IsOpen && !airConditioner.IsWorking)
            return TemperatureCondition.Hot;

        return TemperatureCondition.Normal;
    }

    private void SetAffected(bool affected, TemperatureCondition cause)
    {
        if (IsAffected == affected)
            return;

        IsAffected = affected;

        if (affected)
        {
            AffectedBy = cause;

            if (cause == TemperatureCondition.Cold)
            {
                Debug.Log("PLAYER SE CONGELA");
                onPlayerFreeze?.Invoke();
            }
            else
            {
                Debug.Log("PLAYER SE ASA");
                onPlayerOverheat?.Invoke();
            }

            PlayerAffected?.Invoke(cause);
        }
        else
        {
            AffectedBy = TemperatureCondition.Normal;
            Debug.Log("Temperatura normal. Jugador recuperado.");
            onPlayerRecovered?.Invoke();
            PlayerRecovered?.Invoke();
        }
    }
}