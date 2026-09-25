using System;
using UnityEngine;

public enum AlertLevel
{
    Info,
    Warning,
    Danger,
    Success
}

/// <summary>
/// Clase base para TODOS los eventos del bus (frenazo, raponazo, ladrón,
/// y los que faltan: conspiranoico, vendedor...).
/// Los eventos ya no hablan con la consola ni con la UI directamente:
/// lanzan "señales" (C# events) y el HUD las escucha.
/// </summary>
public abstract class BusEventBase : MonoBehaviour
{
    [Header("Info del evento")]
    [Tooltip("Nombre que se usa en la UI y en los logs. Si está vacío se usa el nombre de la clase.")]
    [SerializeField] private string displayName = "";

    public string DisplayName =>
        string.IsNullOrEmpty(displayName) ? GetType().Name : displayName;

    /// <summary>¿Este evento puede quitar dinero? El manager limita cuántas amenazas corren a la vez.</summary>
    public virtual bool IsMoneyThreat => true;

    public bool IsActive { get; private set; }

    /// <summary>Condiciones para poder arrancar. Los hijos pueden añadir más (ej: ventana abierta).</summary>
    public virtual bool CanTrigger => !IsActive && isActiveAndEnabled;

    // ---- Señales que escucha el HUD ----
    public event Action<BusEventBase> Started;
    public event Action<BusEventBase> Ended;
    public event Action<BusEventBase, string, AlertLevel> AlertRaised;
    public event Action<BusEventBase, string, float> ProgressChanged;
    public event Action<BusEventBase> ProgressHidden;

    /// <summary>Arranca el evento. Lo llama el EventManager o un botón de debug.</summary>
    public abstract void Trigger();

    /// <summary>Corta el evento en seco (fin del viaje / game over).</summary>
    public virtual void CancelEvent()
    {
        StopAllCoroutines();
        EndEvent();
    }

    // ---- Helpers para los hijos ----

    protected void BeginEvent()
    {
        IsActive = true;
        Started?.Invoke(this);
    }

    protected void EndEvent()
    {
        if (!IsActive)
            return;

        IsActive = false;
        HideProgress();
        Ended?.Invoke(this);
    }

    protected void Alert(string message, AlertLevel level = AlertLevel.Info)
    {
        Debug.Log($"[{DisplayName}] {message}");
        AlertRaised?.Invoke(this, message, level);
    }

    /// <param name="normalized">Valor de 0 a 1 para la barra.</param>
    protected void ShowProgress(string label, float normalized)
    {
        ProgressChanged?.Invoke(this, label, Mathf.Clamp01(normalized));
    }

    protected void HideProgress()
    {
        ProgressHidden?.Invoke(this);
    }

    protected void StealBill(string reason)
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.LoseBill(reason);
        else
            Debug.LogWarning("No hay MoneyManager en la escena.");
    }
}