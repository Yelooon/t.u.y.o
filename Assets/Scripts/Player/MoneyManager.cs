using System;
using UnityEngine;

/// <summary>
/// Los 3 billetes de 100K = las 3 vidas del jugador.
/// Los billetes se CONSERVAN entre rondas (escenas) y solo vuelven a 3
/// cuando se empieza una partida nueva desde el menú.
/// </summary>
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [SerializeField] private int startingBills = 3;
    [SerializeField] private int billValue = 100000;

    // "static" = este valor sobrevive al cambiar de escena.
    // -1 significa "partida nueva, todavía no hay billetes guardados".
    private static int savedBills = -1;

    public int Bills { get; private set; }
    public int MaxBills => startingBills;
    public int BillValue => billValue;
    public int TotalMoney => Bills * billValue;
    public bool IsBroke => Bills <= 0;

    [HideInInspector] public bool perder = false;

    /// <summary>(billetes restantes, razón)</summary>
    public event Action<int, string> BillLost;
    public event Action OutOfMoney;

    /// <summary>
    /// Llamar al empezar una partida nueva (botón Jugar del menú).
    /// La próxima escena de juego arrancará con los billetes completos.
    /// </summary>
    public static void StartNewGame()
    {
        savedBills = -1;
    }

    private void Awake()
    {
        // La escena nueva siempre manda
        Instance = this;

        // Si venimos de otra ronda, recuperar lo que quedaba; si no, partida nueva
        Bills = savedBills >= 0 ? savedBills : startingBills;
        savedBills = Bills;

        perder = Bills <= 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void LoseBill(string reason)
    {
        if (IsBroke)
            return;

        Bills--;
        savedBills = Bills;

        Debug.Log($"Perdiste un billete por: {reason}. Quedan {Bills}.");

        BillLost?.Invoke(Bills, reason);

        if (Bills <= 0)
        {
            perder = true;
            OutOfMoney?.Invoke();
        }
    }

    /// <summary>Vuelve a los billetes completos (por si lo necesitan para pruebas).</summary>
    public void ResetMoney()
    {
        Bills = startingBills;
        savedBills = Bills;
        perder = false;
    }
}