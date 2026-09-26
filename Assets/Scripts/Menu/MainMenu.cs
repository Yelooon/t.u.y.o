using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor principal del menú: escena, transiciones y paneles (Tutorial/Salir/Jugar).
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego")]
    [Tooltip("Nombre EXACTO de la escena del juego en Build Settings.")]
    [SerializeField] private string gameSceneName = "BusScene";

    [Header("Interfaz UI")]
    [Tooltip("Panel del tutorial que se activará/desactivará.")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Fundido (opcional)")]
    [Tooltip("Image negro con CanvasGroup.")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource clickSound;

    private bool isLoading = false;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (fadePanel != null)
            StartCoroutine(Fade(1f, 0f));
    }

    // Botón / Objeto: Jugar
    public void Play()
    {
        if (isLoading) return;

        isLoading = true;
        PlayClick();
        StartCoroutine(LoadGame());
    }

    // Botón / Objeto: Abrir Tutorial
    public void OpenTutorial()
    {
        PlayClick();
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    // Botón: Cerrar Tutorial
    public void CloseTutorial()
    {
        PlayClick();
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    // Botón / Objeto: Salir
    public void Quit()
    {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator LoadGame()
    {

        if (fadePanel != null)
            yield return new WaitForSeconds(2f);
            yield return Fade(0f, 1f);

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = to;
        fadePanel.blocksRaycasts = to > 0f;
    }

    public void PlayClick()
    {
        if (clickSound != null)
            clickSound.Play();
    }
}