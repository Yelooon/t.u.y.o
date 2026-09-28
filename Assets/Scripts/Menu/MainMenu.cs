using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Requerido para el Nuevo Input System

/// <summary>
/// Gestor principal del menú: escena, transiciones, paneles (Tutorial/Salir/Jugar),
/// secuencia de logos iniciales (Splash) y historieta/cómic introductorio.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego")]
    [Tooltip("Nombre EXACTO de la escena del juego en Build Settings.")]
    [SerializeField] private string gameSceneName = "Pruebas";

    [Header("Secuencia Intro / Logos (Solo primera vez)")]
    [Tooltip("Panel UI donde se mostrarán los logos/pantallas de inicio.")]
    [SerializeField] private GameObject introPanel;
    [Tooltip("Componente Image donde se renderiza cada logo de la intro.")]
    [SerializeField] private Image introDisplayImage;
    [Tooltip("Arreglo de imágenes/sprites que se mostrarán una por una al arrancar.")]
    [SerializeField] private Sprite[] introSprites;
    [Tooltip("Duración en segundos que se muestra cada imagen en pantalla.")]
    [SerializeField] private float introDisplayDuration = 2f;

    [Header("Interfaz UI")]
    [Tooltip("Panel del tutorial que se activará/desactivará.")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Sistema de Cómic / Intro")]
    [Tooltip("Panel principal donde se muestra el cómic.")]
    [SerializeField] private GameObject comicPanel;
    [Tooltip("Componente Image de UI donde se cargará cada viñeta.")]
    [SerializeField] private Image comicDisplayImage;
    [Tooltip("Lista de las imágenes/sprites del cómic en orden.")]
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

    // Persiste durante la sesión del ejecutable
    private static bool hasShownIntro = false;

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

        if (introPanel != null)
            introPanel.SetActive(false);

        // Comprobamos si es la primera vez que se entra al menú en esta sesión de juego
        if (!hasShownIntro && introPanel != null && introDisplayImage != null && introSprites != null && introSprites.Length > 0)
        {
            hasShownIntro = true; // Guardamos que ya se mostró
            StartCoroutine(PlayIntroSplashSequence());
        }
        else
        {
            // Si ya se mostró antes o no hay imágenes, entramos directamente al menú
            if (fadePanel != null)
                StartCoroutine(FadeFromBlack());
        }
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

    // --- SECUENCIA DE INTRO / LOGOS INICIALES ---

    private IEnumerator PlayIntroSplashSequence()
    {
        introPanel.SetActive(true);

        // Aseguramos pantalla en negro al iniciar
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 1f;
        }

        for (int i = 0; i < introSprites.Length; i++)
        {
            introDisplayImage.sprite = introSprites[i];

            // Revelar la imagen actual
            yield return FadeFromBlack();

            // Tiempo visible en pantalla (2 segundos por defecto)
            yield return new WaitForSeconds(introDisplayDuration);

            // Ocultar la imagen con fundido a negro
            yield return FadeToBlack();
            yield return new WaitForSeconds(0.1f);
        }

        // Cerramos el panel de la intro
        introPanel.SetActive(false);

        // Revelamos el Menú Principal
        yield return FadeFromBlack();
    }

    // --- BOTONES PRINCIPALES ---

    public void Play()
    {
        if (isLoading) return;

        isLoading = true;
        PlayClick();

        if (comicPanel != null && comicDisplayImage != null && comicSprites != null && comicSprites.Length > 0)
        {
            StartCoroutine(StartComicSequence());
        }
        else
        {
            StartCoroutine(LoadGameWithDelay());
        }
    }

    // --- SECUENCIA DEL CÓMIC ---

    private IEnumerator StartComicSequence()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(playDelayDuration);
        yield return FadeToBlack();

        currentComicIndex = 0;
        comicDisplayImage.sprite = comicSprites[currentComicIndex];
        comicPanel.SetActive(true);

        if (skipPrompt != null)
            skipPrompt.SetActive(true);

        yield return FadeFromBlack();

        isComicActive = true;
        isTransitioning = false;
    }

    private IEnumerator NextComicSlide()
    {
        isTransitioning = true;
        PlayClick();

        currentComicIndex++;

        if (currentComicIndex >= comicSprites.Length)
        {
            yield return EndComicAndLoadGame();
        }
        else
        {
            yield return FadeToBlack();
            yield return new WaitForSeconds(0.15f);

            comicDisplayImage.sprite = comicSprites[currentComicIndex];

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