using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el viaje: dispara los eventos de forma aleatoria,
/// maneja los cortes de energía del aire y decide victoria / game over.
/// </summary>
public class BusEventManager : MonoBehaviour
{
    [Serializable]
    public class EventSlot
    {
        public BusEventBase busEvent;
        public bool enabled = true;

        [Tooltip("Segundos desde que arranca el viaje hasta que este evento puede salir por primera vez.")]
        [Min(0f)] public float firstDelay = 10f;

        [Tooltip("Tiempo aleatorio de espera entre una aparición y la siguiente.")]
        [Min(1f)] public float minInterval = 20f;
        [Min(1f)] public float maxInterval = 40f;

        [Tooltip("Si no puede salir (ej: ventana cerrada), vuelve a intentar cada X segundos.")]
        [Min(0.1f)] public float retryDelay = 2f;
    }

    [Header("Referencias")]
    [SerializeField] private MoneyManager money;
    [SerializeField] private BusAirConditioner airConditioner;

    [Header("Viaje")]
    [Tooltip("Duración total del recorrido en segundos.")]
    [SerializeField] private float tripDuration = 300f;
    [SerializeField] private bool autoStart = true;

    [Header("Eventos")]
    [SerializeField] private List<EventSlot> events = new List<EventSlot>();

    [Header("Reglas")]
    [Tooltip("Cuántos eventos que roban dinero pueden estar activos al mismo tiempo.")]
    [SerializeField] private int maxSimultaneousThreats = 1;
    [Tooltip("Tiempo mínimo entre el inicio de un evento y el siguiente.")]
    [SerializeField] private float minGapBetweenEvents = 4f;

    [Header("Energía del bus / Aire acondicionado")]
    [SerializeField] private bool controlAirConditioner = true;
    [Tooltip("Tiempo que el aire funciona al inicio del viaje.")]
    [SerializeField] private float firstPowerFailDelay = 8f;
    [Tooltip("El aire funciona poco rato... (x = mín, y = máx)")]
    [SerializeField] private Vector2 acWorkingDuration = new Vector2(8f, 15f);
    [Tooltip("...y pasa dañado la mayor parte del viaje, para que la ventana esté abierta.")]
    [SerializeField] private Vector2 acFailedDuration = new Vector2(25f, 45f);

    public event Action TripStarted;
    public event Action TripCompleted;
    public event Action GameOver;

    public bool IsRunning { get; private set; }
    public float TripTime { get; private set; }
    public float TripDuration => tripDuration;
    public float TripProgress => tripDuration > 0f ? Mathf.Clamp01(TripTime / tripDuration) : 0f;

    public IEnumerable<BusEventBase> RegisteredEvents
    {
        get
        {
            foreach (EventSlot slot in events)
                if (slot.busEvent != null)
                    yield return slot.busEvent;
        }
    }

    private float lastEventStartTime = -999f;

    private void Start()
    {
        if (money == null)
            money = MoneyManager.Instance;

        if (money != null)
            money.OutOfMoney += HandleOutOfMoney;

        if (autoStart)
            StartTrip();
    }

    private void OnDestroy()
    {
        if (money != null)
            money.OutOfMoney -= HandleOutOfMoney;
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        TripTime += Time.deltaTime;

        if (TripTime >= tripDuration)
            EndTrip(true);
    }

    public void StartTrip()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        TripTime = 0f;

        foreach (EventSlot slot in events)
        {
            if (slot.busEvent != null && slot.enabled)
                StartCoroutine(SlotLoop(slot));
        }

        if (controlAirConditioner && airConditioner != null)
            StartCoroutine(PowerLoop());

        Debug.Log("Viaje iniciado.");
        TripStarted?.Invoke();
    }

    private IEnumerator SlotLoop(EventSlot slot)
    {
        yield return new WaitForSeconds(slot.firstDelay);

        while (IsRunning)
        {
            // Esperar hasta que se cumplan las condiciones
            while (IsRunning && !CanFire(slot))
                yield return new WaitForSeconds(slot.retryDelay);

            if (!IsRunning)
                yield break;

            slot.busEvent.Trigger();

            // El evento puede negarse a arrancar (ej: se cerró la ventana justo)
            if (!slot.busEvent.IsActive)
            {
                yield return new WaitForSeconds(slot.retryDelay);
                continue;
            }

            lastEventStartTime = Time.time;

            // Esperar a que termine
            while (slot.busEvent.IsActive)
                yield return null;

            float wait = UnityEngine.Random.Range(slot.minInterval, Mathf.Max(slot.minInterval, slot.maxInterval));
            yield return new WaitForSeconds(wait);
        }
    }

    private bool CanFire(EventSlot slot)
    {
        if (!slot.enabled || !slot.busEvent.CanTrigger)
            return false;

        if (Time.time - lastEventStartTime < minGapBetweenEvents)
            return false;

        if (slot.busEvent.IsMoneyThreat && CountActiveThreats() >= maxSimultaneousThreats)
            return false;

        return true;
    }

    private int CountActiveThreats()
    {
        int count = 0;

        foreach (BusEventBase evt in RegisteredEvents)
            if (evt.IsActive && evt.IsMoneyThreat)
                count++;

        return count;
    }

    private IEnumerator PowerLoop()
    {
        airConditioner.RestoreAC();

        yield return new WaitForSeconds(firstPowerFailDelay);

        while (IsRunning)
        {
            airConditioner.FailAC();
            yield return new WaitForSeconds(UnityEngine.Random.Range(acFailedDuration.x, acFailedDuration.y));

            if (!IsRunning)
                yield break;

            airConditioner.RestoreAC();
            yield return new WaitForSeconds(UnityEngine.Random.Range(acWorkingDuration.x, acWorkingDuration.y));
        }
    }

    private void HandleOutOfMoney()
    {
        EndTrip(false);
    }

    private void EndTrip(bool reachedDestination)
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        StopAllCoroutines();

        foreach (BusEventBase evt in RegisteredEvents)
            evt.CancelEvent();

        if (reachedDestination)
        {
            Debug.Log("Llegaste a tu destino con la plata.");
            TripCompleted?.Invoke();
        }
        else
        {
            Debug.Log("Te quedaste sin plata. GAME OVER.");
            GameOver?.Invoke();
        }
    }
}