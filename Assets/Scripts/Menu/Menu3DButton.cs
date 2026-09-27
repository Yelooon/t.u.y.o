using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Detecta cuando el mouse pasa sobre un objeto 3D del menú.
/// Controla: movimiento de luces, intensidad de luces, activación/fade de Texto 3D, animación de clic, audios y eventos.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Menu3DButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Configuración de Luces")]
    [Tooltip("Lista de luces que responderán al pasar el mouse por este objeto.")]
    [SerializeField] private List<Light> targetLights = new List<Light>();

    [Header("Modo Movimiento de Luz (Opcional)")]
    [SerializeField] private bool enableMovement = true;
    [Tooltip("Desplazamiento relativo desde la posición inicial de la luz.")]
    [SerializeField] private Vector3 hoverOffset = new Vector3(0f, 0.5f, -0.5f);

    [Header("Modo Encendido / Intensidad (Opcional)")]
    [SerializeField] private bool enableIntensityChange = false;
    [Tooltip("Intensidad cuando el mouse NO está encima (0 = apagada).")]
    [SerializeField] private float normalIntensity = 0f;
    [Tooltip("Intensidad cuando el mouse SÍ está encima.")]
    [SerializeField] private float hoverIntensity = 3f;

    [Header("Texto 3D en Hover (Opcional)")]
    [SerializeField] private bool enableTextHover = true;
    [Tooltip("El objeto de texto 3D que aparecerá al señalar con el mouse.")]
    [SerializeField] private GameObject targetTextObject;
    [Tooltip("Si usas el componente TextMesh clásico, desvanece suavemente el alfa en vez de apagarlo de golpe.")]
    [SerializeField] private bool fadeTextAlpha = true;

    [Header("Animación de Clic (Opcional)")]
    [Tooltip("Animator a activar al hacer clic. Si se deja vacío, buscará uno en este mismo GameObject.")]
    [SerializeField] private Animator targetAnimator;
    [Tooltip("Nombre del parámetro Trigger en el Animator.")]
    [SerializeField] private string clickTriggerName = "Play";

    [Header("Audio (Opcional)")]
    [Tooltip("AudioSource que sonará cuando el mouse pasa por encima.")]
    [SerializeField] private AudioSource hoverSound;
    [Tooltip("AudioSource que sonará al hacer clic.")]
    [SerializeField] private AudioSource Sonido;
    [Tooltip("Tiempo en segundos que sonará el audio de clic a volumen normal antes de iniciar el fade out.")]
    [SerializeField] private float soundPlayDuration = 1.0f;
    [Tooltip("Duración en segundos del desvanecimiento (fade out) de volumen hasta llegar a 0.")]
    [SerializeField] private float soundFadeDuration = 0.5f;

    [Header("Animación General")]
    [Tooltip("Velocidad de interpolación de movimiento, luz y transparencia.")]
    [SerializeField] private float lerpSpeed = 5f;

    [Header("Acción al hacer Clic")]
    [SerializeField] private UnityEvent onClickAction;

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private float currentTargetIntensity;

    private TextMesh textMeshComponent;
    private float targetTextAlpha = 0f;
    private float currentTextAlpha = 0f;

    private float originalSoundVolume = 1f;
    private Coroutine soundFadeCoroutine;

    private void Start()
    {
        currentTargetIntensity = enableIntensityChange ? normalIntensity : 0f;

        // Guarda el volumen original configurado en el AudioSource de clic
        if (Sonido != null)
        {
            originalSoundVolume = Sonido.volume;
        }

        // Si no asignaste manualmente un Animator, intenta buscar uno en este objeto
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }

        // Configuración inicial de luces
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                originalPositions.Add(light.transform.localPosition);
                targetPositions.Add(light.transform.localPosition);

                if (enableIntensityChange)
                {
                    light.intensity = normalIntensity;
                }
            }
        }

        // Configuración inicial del Texto 3D
        if (enableTextHover && targetTextObject != null)
        {
            textMeshComponent = targetTextObject.GetComponent<TextMesh>();

            if (fadeTextAlpha && textMeshComponent != null)
            {
                targetTextObject.SetActive(true);
                targetTextAlpha = 0f;
                currentTextAlpha = 0f;
                SetTextMeshAlpha(0f);
            }
            else
            {
                targetTextObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // 1. Animación de Luces
        for (int i = 0; i < targetLights.Count; i++)
        {
            Light light = targetLights[i];
            if (light == null) continue;

            if (enableMovement)
            {
                light.transform.localPosition = Vector3.Lerp(
                    light.transform.localPosition,
                    targetPositions[i],
                    Time.deltaTime * lerpSpeed
                );
            }

            if (enableIntensityChange)
            {
                light.intensity = Mathf.Lerp(
                    light.intensity,
                    currentTargetIntensity,
                    Time.deltaTime * lerpSpeed
                );
            }
        }

        // 2. Animación de Transparencia de Texto 3D
        if (enableTextHover && targetTextObject != null && fadeTextAlpha && textMeshComponent != null)
        {
            currentTextAlpha = Mathf.Lerp(currentTextAlpha, targetTextAlpha, Time.deltaTime * lerpSpeed);
            SetTextMeshAlpha(currentTextAlpha);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Reproduce el sonido al pasar el cursor
        if (hoverSound != null)
        {
            hoverSound.Play();
        }

        // Posición de luces
        if (enableMovement)
        {
            for (int i = 0; i < targetLights.Count; i++)
            {
                targetPositions[i] = originalPositions[i] + hoverOffset;
            }
        }

        // Intensidad de luces
        if (enableIntensityChange)
        {
            currentTargetIntensity = hoverIntensity;
        }

        // Texto 3D
        if (enableTextHover && targetTextObject != null)
        {
            if (fadeTextAlpha && textMeshComponent != null)
            {
                targetTextAlpha = 1f;
            }
            else
            {
                targetTextObject.SetActive(true);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Posición de luces
        if (enableMovement)
        {
            for (int i = 0; i < targetLights.Count; i++)
            {
                targetPositions[i] = originalPositions[i];
            }
        }

        // Intensidad de luces
        if (enableIntensityChange)
        {
            currentTargetIntensity = normalIntensity;
        }

        // Texto 3D
        if (enableTextHover && targetTextObject != null)
        {
            if (fadeTextAlpha && textMeshComponent != null)
            {
                targetTextAlpha = 0f;
            }
            else
            {
                targetTextObject.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Activa el Trigger de animación si el objeto tiene un Animator
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger(clickTriggerName);
        }

        // Manejo del audio con Fade Out tras 1 segundo
        if (Sonido != null)
        {
            if (soundFadeCoroutine != null)
            {
                StopCoroutine(soundFadeCoroutine);
            }
            soundFadeCoroutine = StartCoroutine(PlaySoundWithFadeOut());
        }

        // Ejecuta la acción asignada en el evento UnityEvent
        onClickAction?.Invoke();
    }

    private IEnumerator PlaySoundWithFadeOut()
    {
        // Restablece el volumen inicial y reproduce el audio
        Sonido.volume = originalSoundVolume;
        Sonido.Play();

        // Espera 1 segundo a volumen normal
        yield return new WaitForSeconds(soundPlayDuration);

        // Hace el desvanecimiento progresivo (Fade Out)
        float startVolume = Sonido.volume;
        float t = 0f;

        while (t < soundFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            Sonido.volume = Mathf.Lerp(startVolume, 0f, t / soundFadeDuration);
            yield return null;
        }

        Sonido.volume = 0f;
        Sonido.Stop();

        // Restablece el volumen original por si vuelve a hacerse clic más adelante
        Sonido.volume = originalSoundVolume;
    }

    private void SetTextMeshAlpha(float alpha)
    {
        if (textMeshComponent != null)
        {
            Color color = textMeshComponent.color;
            color.a = alpha;
            textMeshComponent.color = color;
        }
    }
}