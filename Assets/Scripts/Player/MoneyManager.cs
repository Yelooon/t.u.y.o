using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Los 3 billetes de 100K = las 3 vidas del jugador.
/// </summary>
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [SerializeField] private int startingBills = 3;
    [SerializeField] private int billValue = 100000;

    public int Bills { get; private set; }
    public int MaxBills => startingBills;
    public int BillValue => billValue;
    public int TotalMoney => Bills * billValue;
    public bool IsBroke => Bills <= 0;

    /// <summary>(billetes restantes, razón)</summary>
    public event Action<int, string> BillLost;
    public event Action OutOfMoney;
    public bool perder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetState(); // Reinicia billetes y bandera de derrota al cargar la escena
    }

    private void OnDestroy()
    {
        // Limpia la instancia estática cuando la escena se descarga
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Restablece la salud/dinero y el estado de derrota del jugador.
    /// </summary>
    public void ResetState()
    {
        Bills = startingBills;
        perder = false;
    }

    public void LoseBill(string reason)
    {
        if (IsBroke)
            return;

        Bills--;

        Debug.Log($"Perdiste un billete por: {reason}. Quedan {Bills}.");

        BillLost?.Invoke(Bills, reason);

        if (Bills <= 0)
        {
            perder = true;
            OutOfMoney?.Invoke();
        }
    }
}