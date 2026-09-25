using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Todo lo que antes iba a la consola ahora se ve aquí:
/// billetes, alertas, barras de tiempo, temperatura, progreso del viaje y pantallas finales.
/// </summary>
public class BusHUD : MonoBehaviour
{
    [Header("Sistemas")]
    [SerializeField] private BusEventManager eventManager;
    [SerializeField] private MoneyManager money;
    [SerializeField] private BusTemperatureSystem temperature;
    [SerializeField] private BusAirConditioner airConditioner;

    [Header("Billetes (3 imágenes, una por billete)")]
    [SerializeField] private UnityEngine.UI.Image[] billIcons;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Color billActiveColor = Color.white;
    [SerializeField] private Color billLostColor = new Color(1f, 1f, 1f, 0.15f);

    [Header("Alertas")]
    [SerializeField] private RectTransform alertContainer;
    [SerializeField] private HUDAlertItem alertPrefab;
    [SerializeField] private float alertDuration = 2.5f;
    [SerializeField] private int maxAlerts = 4;

    [Header("Barras de tiempo de eventos")]
    [SerializeField] private RectTransform progressContainer;
    [SerializeField] private HUDProgressItem progressPrefab;

    [Header("Temperatura")]
    [SerializeField] private GameObject temperaturePanel;
    [SerializeField] private Slider temperatura;
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private UnityEngine.UI.Image temperatureFill;
    [SerializeField] private Color hotColor = new Color(1f, 0.45f, 0.1f);
    [SerializeField] private Color coldColor = new Color(0.3f, 0.75f, 1f);

    [Header("Viaje")]
    [SerializeField] private Image tripFill; // Cambiado de Image a Slider
    [SerializeField] private Slider tripSlider;
    [SerializeField] private TMP_Text tripText;

    [Header("Pantallas finales")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Colores de alerta")]
    [SerializeField] private Color infoColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
    [SerializeField] private Color warningColor = new Color(0.95f, 0.65f, 0.1f, 0.9f);
    [SerializeField] private Color dangerColor = new Color(0.85f, 0.15f, 0.15f, 0.9f);
    [SerializeField] private Color successColor = new Color(0.2f, 0.7f, 0.3f, 0.9f);

    private readonly Dictionary<BusEventBase, HUDProgressItem> progressItems =
        new Dictionary<BusEventBase, HUDProgressItem>();

    private readonly List<HUDAlertItem> activeAlerts = new List<HUDAlertItem>();
    private readonly List<BusEventBase> subscribedEvents = new List<BusEventBase>();

    // ------------------------------------------------------------
    // Suscripciones
    // ------------------------------------------------------------

    private void Start()
    {
        if (tripSlider != null)
        {
            tripSlider.minValue = 0f;
            tripSlider.maxValue = 1f;
            tripSlider.interactable = false;
        }

        if (money == null)
            money = MoneyManager.Instance;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (temperaturePanel != null) temperaturePanel.SetActive(false);

        if (eventManager != null)
        {
            eventManager.TripCompleted += ShowVictory;
            eventManager.GameOver += ShowGameOver;

            foreach (BusEventBase evt in eventManager.RegisteredEvents)
                Subscribe(evt);
        }

        if (money != null)
            money.BillLost += OnBillLost;

        if (airConditioner != null)
            airConditioner.StateChanged += OnACChanged;

        RefreshBills();
    }

    private void OnDestroy()
    {
        if (eventManager != null)
        {
            eventManager.TripCompleted -= ShowVictory;
            eventManager.GameOver -= ShowGameOver;
        }

        foreach (BusEventBase evt in subscribedEvents)
        {
            if (evt == null) continue;
            evt.AlertRaised -= OnEventAlert;
            evt.ProgressChanged -= OnEventProgress;
            evt.ProgressHidden -= OnEventProgressHidden;
        }

        if (money != null)
            money.BillLost -= OnBillLost;

        if (airConditioner != null)
            airConditioner.StateChanged -= OnACChanged;
    }

    /// <summary>Por si agregas eventos que no están en el manager (ej. solo para debug).</summary>
    public void Subscribe(BusEventBase evt)
    {
        if (evt == null || subscribedEvents.Contains(evt))
            return;

        evt.AlertRaised += OnEventAlert;
        evt.ProgressChanged += OnEventProgress;
        evt.ProgressHidden += OnEventProgressHidden;
        subscribedEvents.Add(evt);
    }

    // ------------------------------------------------------------
    // Update: cosas que cambian cada frame
    // ------------------------------------------------------------

    private void Update()
    {
        UpdateTrip();
        UpdateTemperature();
    }

    private void UpdateTrip()
    {
        if (eventManager == null)
            return;

        // Actualización del Slider de viaje
        if (tripFill != null)
            tripFill.fillAmount = eventManager.TripProgress;

        if (tripSlider != null)
            tripSlider.value = eventManager.TripProgress;

        if (tripText != null)
            tripText.text = $"Viaje {Mathf.RoundToInt(eventManager.TripProgress * 100f)}%";
    }

    private void UpdateTemperature()
    {
        if (temperature == null || temperaturePanel == null)
            return;

        bool show = temperature.Condition != BusTemperatureSystem.TemperatureCondition.Normal ||
                    temperature.Exposure > 0f;

        temperaturePanel.SetActive(show);

        if (!show)
            return;

        // Mientras se recupera usamos la causa del efecto; si no, la condición actual
        var shownCondition = temperature.Condition != BusTemperatureSystem.TemperatureCondition.Normal
            ? temperature.Condition
            : temperature.AffectedBy;

        bool isCold = shownCondition == BusTemperatureSystem.TemperatureCondition.Cold;
        Color color = isCold ? coldColor : hotColor;

        string text;
        if (temperature.Condition == BusTemperatureSystem.TemperatureCondition.Normal)
            text = "Recuperándote...";
        else if (temperature.IsAffected)
            text = isCold ? "¡Te estás congelando!" : "¡Te estás asando!";
        else
            text = isCold ? "Hace frío: cierra la ventana" : "Hace calor: abre la ventana";

        if (temperatureText != null)
        {
            temperatureText.text = text;
            temperatureText.color = color;
        }

        // Actualización del Slider de temperatura
        if (temperatura != null)
        {
            temperatura.value = temperature.Exposure;
        }

        // Si la imagen de relleno dentro del Slider tiene un color asignable
        if (temperatureFill != null)
        {
            temperatureFill.color = color;
        }
    }

    // ------------------------------------------------------------
    // Alertas
    // ------------------------------------------------------------

    /// <summary>Cualquier script puede usar esto para mostrar un mensaje en pantalla.</summary>
    public void ShowAlert(string message, AlertLevel level)
    {
        if (alertPrefab == null || alertContainer == null)
        {
            Debug.Log($"[HUD] {message}");
            return;
        }

        activeAlerts.RemoveAll(a => a == null);

        while (activeAlerts.Count >= maxAlerts)
        {
            activeAlerts[0].Dismiss();
            activeAlerts.RemoveAt(0);
        }

        HUDAlertItem item = Instantiate(alertPrefab, alertContainer);
        item.Setup(message, GetColor(level), alertDuration);
        activeAlerts.Add(item);
    }

    private void OnEventAlert(BusEventBase evt, string message, AlertLevel level)
    {
        ShowAlert(message, level);
    }

    private void OnACChanged(BusAirConditioner.ACState state)
    {
        if (state == BusAirConditioner.ACState.Failed)
            ShowAlert("Se fue la energía: el aire se apagó", AlertLevel.Warning);
        else
            ShowAlert("Volvió la energía: el aire está prendido", AlertLevel.Info);
    }

    // ------------------------------------------------------------
    // Barras de progreso por evento
    // ------------------------------------------------------------

    private void OnEventProgress(BusEventBase evt, string label, float value)
    {
        if (progressPrefab == null || progressContainer == null)
            return;

        if (!progressItems.TryGetValue(evt, out HUDProgressItem item) || item == null)
        {
            item = Instantiate(progressPrefab, progressContainer);
            progressItems[evt] = item;
        }

        Color color = evt.IsMoneyThreat ? dangerColor : warningColor;
        item.Set(label, value, color);
    }

    private void OnEventProgressHidden(BusEventBase evt)
    {
        if (progressItems.TryGetValue(evt, out HUDProgressItem item))
        {
            if (item != null)
                Destroy(item.gameObject);

            progressItems.Remove(evt);
        }
    }

    // ------------------------------------------------------------
    // Billetes
    // ------------------------------------------------------------

    private void OnBillLost(int remaining, string reason)
    {
        ShowAlert($"-100K ({reason})", AlertLevel.Danger);

        // El billete que se acaba de perder es el índice "remaining"
        if (billIcons != null && remaining >= 0 && remaining < billIcons.Length)
            StartCoroutine(FlashBill(billIcons[remaining]));

        RefreshBills();
    }

    private void RefreshBills()
    {
        if (money == null)
            return;

        if (billIcons != null)
        {
            for (int i = 0; i < billIcons.Length; i++)
            {
                if (billIcons[i] != null)
                    billIcons[i].color = i < money.Bills ? billActiveColor : billLostColor;
            }
        }

        if (moneyText != null)
            moneyText.text = $"${money.TotalMoney:N0}";
    }

    private IEnumerator FlashBill(UnityEngine.UI.Image bill)
    {
        if (bill == null)
            yield break;

        Vector3 originalScale = bill.transform.localScale;
        float t = 0f;
        const float duration = 0.5f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;
            bill.transform.localScale = originalScale * (1f + Mathf.Sin(k * Mathf.PI) * 0.3f);
            bill.color = Color.Lerp(Color.red, billLostColor, k);
            yield return null;
        }

        bill.transform.localScale = originalScale;
        bill.color = billLostColor;
    }

    // ------------------------------------------------------------
    // Fin del juego
    // ------------------------------------------------------------

    private void ShowGameOver()
    {
        ClearProgressBars();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void ShowVictory()
    {
        ClearProgressBars();
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private void ClearProgressBars()
    {
        foreach (var pair in progressItems)
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);

        progressItems.Clear();
    }

    /// <summary>Conéctalo al botón "Reintentar" de los paneles finales.</summary>
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private Color GetColor(AlertLevel level)
    {
        switch (level)
        {
            case AlertLevel.Warning: return warningColor;
            case AlertLevel.Danger: return dangerColor;
            case AlertLevel.Success: return successColor;
            default: return infoColor;
        }
    }
}