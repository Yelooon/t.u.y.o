using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Requerido para el Nuevo Input System

/// <summary>
/// Gestor principal del menú: escena, transiciones, paneles (Tutorial/Salir/Jugar)
/// y secuencia de historieta/cómic introductorio.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego")]
    [Tooltip("Nombre EXACTO de la escena del juego en Build Settings.")]
    [SerializeField] private string gameSceneName = "BusScene";

    [Header("Interfaz UI")]
    [Tooltip("Panel del tutorial que se activará/desactivará.")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Sistema de Cómic / Intro")]
    [Tooltip("Panel principal donde se muestra el cómic.")]
    [SerializeField] private GameObject comicPanel;
    [Tooltip("Componente Image de UI donde se cargará cada viñeta.")]
    [SerializeField] private Image comicDisplayImage;
    [Tooltip("Lista de las 4 imágenes/sprites del cómic en orden.")]
    [SerializeField] private Sprite[] comicSprites;
    [Tooltip("Objeto o texto opcional que dice 'Presiona ESC para omitir'.")]
    [SerializeField] private GameObject skipPrompt;
    [Tooltip("Tiempo de espera en segundos tras presionar Play antes de iniciar el cómic.")]
    [SerializeField] private float playDelayDuration = 3f;

    [Header("Fundido (opcional)")]
    [Tooltip("Imagen negra con CanvasGroup.")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource clickSound;

    private bool isLoading = false;
    private bool isComicActive = false;
    private bool isTransitioning = false;
    private int currentComicIndex = 0;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (comicPanel != null)
            comicPanel.SetActive(false);

        if (skipPrompt != null)
            skipPrompt.SetActive(false);

        if (fadePanel != null)
            StartCoroutine(FadeFromBlack());
    }

    private void Update()
    {
        // Solo escuchamos teclado/clic si el cómic está activo y no hay una transición en curso
        if (!isComicActive || isTransitioning) return;

        // Omitir cómic con la tecla ESC (Nuevo Input System)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StartCoroutine(SkipComic());
            return;
        }

        // Avanzar viñeta con Clic Izquierdo o Barra Espaciadora (Nuevo Input System)
        bool leftClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (leftClick || spacePressed)
        {
            StartCoroutine(NextComicSlide());
        }
    }

    // Botón / Objeto: Jugar
    public void Play()
    {
        if (isLoading) return;

        isLoading = true;
        PlayClick();

        // Si hay un cómic configurado con imágenes, iniciamos la secuencia con delay
        if (comicPanel != null && comicDisplayImage != null && comicSprites != null && comicSprites.Length > 0)
        {
            StartCoroutine(StartComicSequence());
        }
        else
        {
            // Si no hay imágenes de cómic, va directo al juego con el delay
            StartCoroutine(LoadGameWithDelay());
        }
    }

    // --- SECUENCIA DEL CÓMIC ---

    private IEnumerator StartComicSequence()
    {
        isTransitioning = true;

        // Espera de 3 segundos antes de iniciar el fundido/transición al cómic
        yield return new WaitForSeconds(playDelayDuration);

        // Fade Out (a negro)
        yield return FadeToBlack();

        // Preparamos la primera imagen y activamos el panel
        currentComicIndex = 0;
        comicDisplayImage.sprite = comicSprites[currentComicIndex];
        comicPanel.SetActive(true);

        if (skipPrompt != null)
            skipPrompt.SetActive(true);

        // Fade In (revelar la primera viñeta)
        yield return FadeFromBlack();

        isComicActive = true;
        isTransitioning = false;
    }

    private IEnumerator NextComicSlide()
    {
        isTransitioning = true;
        PlayClick();

        currentComicIndex++;

        // Si ya pasamos la última imagen, salimos al juego
        if (currentComicIndex >= comicSprites.Length)
        {
            yield return EndComicAndLoadGame();
        }
        else
        {
            // 1. Fade Out: Se oscurece la pantalla por completo
            yield return FadeToBlack();

            // 2. Breve pausa en negro para suavizar la transición
            yield return new WaitForSeconds(0.15f);

            // 3. Cambiamos la imagen
            comicDisplayImage.sprite = comicSprites[currentComicIndex];

            // 4. Fade In: Reaparece la vista con la nueva viñeta
            yield return FadeFromBlack();

            isTransitioning = false;
        }
    }

    private IEnumerator SkipComic()
    {
        isTransitioning = true;
        PlayClick();
        yield return EndComicAndLoadGame();
    }

    private IEnumerator EndComicAndLoadGame()
    {
        isComicActive = false;

        // Fade a negro final
        yield return FadeToBlack();

        if (skipPrompt != null)
            skipPrompt.SetActive(false);

        if (comicPanel != null)
            comicPanel.SetActive(false);

        SceneManager.LoadScene(gameSceneName);
    }

    // --- MÉTODOS DE UI DE APOYO ---

    public void OpenTutorial()
    {
        PlayClick();
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        PlayClick();
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    public void Quit()
    {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator LoadGameWithDelay()
    {
        yield return new WaitForSeconds(playDelayDuration);

        if (fadePanel != null)
        {
            yield return FadeToBlack();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // --- MÉTODOS DE FUNDIDO (FADE IN / FADE OUT) ---

    private IEnumerator FadeToBlack()
    {
        yield return Fade(0f, 1f);
    }

    private IEnumerator FadeFromBlack()
    {
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadePanel == null) yield break;

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