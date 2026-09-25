using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal: botón Jugar y botón Salir, con fundido a negro opcional.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego")]
    [Tooltip("Nombre EXACTO de la escena del bus (como aparece en Build Settings).")]
    [SerializeField] private string gameSceneName = "BusScene";

    [Header("Fundido (opcional)")]
    [Tooltip("Un Image negro a pantalla completa con CanvasGroup. Si está vacío, cambia de escena de una.")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource clickSound;

    private bool isLoading = false;

    private void Start()
    {
        // Por si venimos de un juego pausado o con el cursor bloqueado
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fadePanel != null)
            StartCoroutine(Fade(1f, 0f));
    }

    // Conectar al OnClick del botón Jugar
    public void Play()
    {
        if (isLoading)
            return;

        isLoading = true;
        PlayClick();
        StartCoroutine(LoadGame());
    }

    // Conectar al OnClick del botón Salir
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

        // Si quedó transparente, que no bloquee los clics a los botones
        fadePanel.blocksRaycasts = to > 0f;
    }

    private void PlayClick()
    {
        if (clickSound != null)
            clickSound.Play();
    }
}