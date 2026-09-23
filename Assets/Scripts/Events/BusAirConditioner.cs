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

    private void Start()
    {
        UpdateVentMaterial();
    }

    public void FailAC()
    {
        if (currentState == ACState.Failed)
            return;

        currentState = ACState.Failed;

        Debug.Log("❌ Aire acondicionado APAGADO.");

        UpdateVentMaterial();
    }

    public void RestoreAC()
    {
        if (currentState == ACState.Working)
            return;

        currentState = ACState.Working;

        Debug.Log("❄ Aire acondicionado ENCENDIDO.");

        UpdateVentMaterial();
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

        if (IsWorking)
        {
            airVentRenderer.material = airOnMaterial;
        }
        else
        {
            airVentRenderer.material = airOffMaterial;
        }
    }
}