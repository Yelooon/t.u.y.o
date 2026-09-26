using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Valores por defecto")]
    [SerializeField] private float duracion = 0.4f;
    [Tooltip("Cuánto se desplaza la cámara (en unidades).")]
    [SerializeField] private float intensidad = 0.15f;
    [Tooltip("Inclinación máxima (en grados) al temblar.")]
    [SerializeField] private float rotacionMax = 2f;
    [Tooltip("Qué tan rápido vibra.")]
    [SerializeField] private float frecuencia = 25f;

    [Tooltip("Cómo se apaga el temblor en el tiempo (1 = fuerte, 0 = nada).")]
    [SerializeField] private AnimationCurve atenuacion = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Tooltip("Actívalo si usas cámara lenta: el temblor ignora Time.timeScale.")]
    [SerializeField] private bool ignorarTimeScale = true;

    private Vector3 posOriginal;
    private Quaternion rotOriginal;
    private Coroutine rutina;
    private float semilla;

    private void Awake()
    {
        posOriginal = transform.localPosition;
        rotOriginal = transform.localRotation;
        semilla = Random.value * 100f;
    }


    public void Temblar() => Temblar(duracion, intensidad);

    public void TemblarFuerte() => Temblar(duracion * 1.5f, intensidad * 2.5f);

    public void TemblarDurante(float segundos) => Temblar(segundos, intensidad);

    public void Temblar(float duracionTemblor, float intensidadTemblor)
    {
        if (rutina != null)
            StopCoroutine(rutina);

        rutina = StartCoroutine(RutinaTemblor(duracionTemblor, intensidadTemblor));
    }

    public void Detener()
    {
        if (rutina != null)
            StopCoroutine(rutina);

        Restaurar();
    }

    private IEnumerator RutinaTemblor(float duracionTemblor, float intensidadTemblor)
    {
        float t = 0f;

        while (t < duracionTemblor)
        {
            float dt = ignorarTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float tiempo = ignorarTimeScale ? Time.unscaledTime : Time.time;
            t += dt;

            float fuerza = atenuacion.Evaluate(t / duracionTemblor);
            float ruidoT = tiempo * frecuencia;

            float x = Mathf.PerlinNoise(semilla, ruidoT) * 2f - 1f;
            float y = Mathf.PerlinNoise(semilla + 10f, ruidoT) * 2f - 1f;
            float giro = Mathf.PerlinNoise(semilla + 20f, ruidoT) * 2f - 1f;

            transform.localPosition = posOriginal + new Vector3(x, y, 0f) * intensidadTemblor * fuerza;
            transform.localRotation = rotOriginal * Quaternion.Euler(0f, 0f, giro * rotacionMax * fuerza);

            yield return null;
        }

        Restaurar();
    }

    private void Restaurar()
    {
        transform.localPosition = posOriginal;
        transform.localRotation = rotOriginal;
        rutina = null;
    }

    private void OnDisable()
    {
        Detener();
    }
}
