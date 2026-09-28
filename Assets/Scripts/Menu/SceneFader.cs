using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private float duracion = 1f;
    [SerializeField] private Color color = Color.black;
    [Tooltip("Hacer fundido de entrada al abrir la primera escena.")]
    [SerializeField] private bool fundidoAlIniciar = true;

    private CanvasGroup grupo;
    private bool enTransicion;
    public string sceneName;
    public bool EnTransicion => enTransicion;
    private void Awake()
    {

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CrearPantalla();

        if (fundidoAlIniciar)
        {
            grupo.alpha = 1f;
            StartCoroutine(Fundir(1f, 0f));
        }
        else
        {
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
        }
    }


    public void CambiarEscena(string nombreEscena)
    {
        if (enTransicion) return;
        StartCoroutine(Transicion(nombreEscena));
    }

    public void ReiniciarEscena() => CambiarEscena(SceneManager.GetActiveScene().name);


    public Coroutine FundidoSalida() => StartCoroutine(Fundir(0f, 1f));

    public Coroutine FundidoEntrada() => StartCoroutine(Fundir(1f, 0f));

    private IEnumerator Transicion(string nombreEscena)
    {
        enTransicion = true;

        yield return Fundir(0f, duracion);

        AsyncOperation carga = SceneManager.LoadSceneAsync(nombreEscena);
        while (!carga.isDone)
            yield return null;

        yield return Fundir(duracion, 0f);

        enTransicion = false;
    }

    private IEnumerator Fundir(float desde, float hasta)
    {
        grupo.blocksRaycasts = true;

        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime; 
            grupo.alpha = Mathf.Lerp(desde, hasta, Mathf.SmoothStep(0f, 1f, t / duracion));
            yield return null;
        }

        grupo.alpha = hasta;
        grupo.blocksRaycasts = hasta > 0.5f;
    }

    private void CrearPantalla()
    {

        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasGO.AddComponent<GraphicRaycaster>();

        grupo = canvasGO.AddComponent<CanvasGroup>();

        var imagenGO = new GameObject("Fade");
        imagenGO.transform.SetParent(canvasGO.transform, false);

        var imagen = imagenGO.AddComponent<Image>();
        imagen.color = color;

        RectTransform rt = imagen.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            CambiarEscena(sceneName);
    }
}