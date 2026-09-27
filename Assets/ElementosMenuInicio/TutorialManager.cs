using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la animación de cierre diferida y la navegación por subpaneles dentro del Tutorial.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Animación de Cierre")]
    [Tooltip("El Animator adjunto a este panel de tutorial.")]
    [SerializeField] private Animator tutorialAnimator;

    [Tooltip("Nombre del Trigger configurado en el Animator para la salida.")]
    [SerializeField] private string closeTriggerName = "Cerrar";

    [Tooltip("Tiempo en segundos que dura la animación antes de desactivar el objeto.")]
    [SerializeField] private float closeDelay = 0.5f;

    [Header("Navegación de Paneles")]
    [Tooltip("El contenedor de la pantalla principal/lista de opciones del tutorial.")]
    [SerializeField] private GameObject mainTutorialView;

    [Tooltip("Lista de todos los subpaneles existentes (Controles, Reglas, Consejos, etc.).")]
    [SerializeField] private List<GameObject> subPanels = new List<GameObject>();

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioSource clickSound;

    private bool isClosing = false;

    private void OnEnable()
    {
        // Cada vez que el tutorial se activa, resetea a la pantalla principal
        ResetTutorialView();
        isClosing = false;
    }

    /// <summary>
    /// Conectar al botón principal 'X' o 'Cerrar' del Tutorial.
    /// </summary>
    public void CloseTutorial()
    {
        if (isClosing) return;

        PlayClickSound();
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        isClosing = true;

        // 1. Disparar el Trigger de la animación "Cerrar"
        if (tutorialAnimator != null)
        {
            tutorialAnimator.SetTrigger(closeTriggerName);
        }

        // 2. Esperar el tiempo exacto de la animación (independiente de Time.timeScale)
        yield return new WaitForSecondsRealtime(closeDelay);

        // 3. Ocultar el objeto
        gameObject.SetActive(false);
        isClosing = false;
    }

    /// <summary>
    /// Abre un subpanel en particular y oculta la pantalla principal del tutorial.
    /// </summary>
    public void OpenSubPanel(GameObject targetSubPanel)
    {
        PlayClickSound();

        // Ocultar la pantalla principal
        if (mainTutorialView != null)
            mainTutorialView.SetActive(false);

        // Desactivar todos los subpaneles y encender solo el seleccionado
        foreach (GameObject panel in subPanels)
        {
            if (panel != null)
                panel.SetActive(panel == targetSubPanel);
        }
    }

    /// <summary>
    /// Conectar al botón 'Volver' o 'Atrás' ubicado dentro de cada subpanel.
    /// </summary>
    public void BackToMainTutorial()
    {
        PlayClickSound();
        ResetTutorialView();
    }

    private void ResetTutorialView()
    {
        // Desactivar todos los subpaneles
        foreach (GameObject panel in subPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // Mostrar solo la vista principal del tutorial
        if (mainTutorialView != null)
            mainTutorialView.SetActive(true);
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
            clickSound.Play();
    }
}