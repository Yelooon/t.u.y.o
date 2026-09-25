using System;
using UnityEngine;

public class BusAirConditioner : MonoBehaviour
{
    public enum ACState
    {
        Working,
        Failed
    }

    [Header("State")]
    [SerializeField] private ACState currentState = ACState.Working;

    [Header("References")]
    [SerializeField] private Renderer airVentRenderer;

    [Header("Vent Materials")]
    [SerializeField] private Material airOnMaterial;
    [SerializeField] private Material airOffMaterial;

    public ACState CurrentState => currentState;
    public bool IsWorking => currentState == ACState.Working;

    /// <summary>El HUD escucha esto para avisar "se fue / volvió la energía".</summary>
    public event Action<ACState> StateChanged;

    private void Start()
    {
        UpdateVentMaterial();
    }

    public void FailAC()
    {
        if (currentState == ACState.Failed)
            return;

        currentState = ACState.Failed;
        Debug.Log("Aire acondicionado APAGADO.");

        UpdateVentMaterial();
        StateChanged?.Invoke(currentState);
    }

    public void RestoreAC()
    {
        if (currentState == ACState.Working)
            return;

        currentState = ACState.Working;
        Debug.Log("Aire acondicionado ENCENDIDO.");

        UpdateVentMaterial();
        StateChanged?.Invoke(currentState);
    }

    public void ToggleAC()
    {
        if (IsWorking)
            FailAC();
        else
            RestoreAC();
    }

    private void UpdateVentMaterial()
    {
        if (airVentRenderer == null)
            return;

        airVentRenderer.material = IsWorking ? airOnMaterial : airOffMaterial;
    }
}