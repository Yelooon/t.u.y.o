using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOConspiracyDialogue : MonoBehaviour
{
    [Header("UI General")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Image Pool")]
    [SerializeField] private List<UFOImageData> imagePool;

    [Header("Las 12 Opciones Clickeables")]
    [Tooltip("Arrastra aquí las 12 burbujas/opciones de la UI.")]
    [SerializeField] private UFOImageOption[] options = new UFOImageOption[12];

    [Header("Efecto Latido (Pulsación)")]
    [SerializeField] private bool enablePulse = true;
    [Tooltip("Velocidad del latido.")]
    [SerializeField] private float pulseSpeed = 4f;
    [Tooltip("Qué tanto se agranda/enoje la imagen (ej: 0.1 = 10% más grande).")]
    [SerializeField] private float pulseAmount = 0.12f;
    [Tooltip("Si se activa, cada burbuja late a un ritmo ligeramente desfasado para dar más caos visual.")]
    [SerializeField] private bool offsetPhases = true;

    private UFOConspiracyEvent currentEvent;
    private Vector3 originalScale = Vector3.one;
    private float[] phaseOffsets;

    private void Awake()
    {
        // Guardar la escala original de referencia
        if (options.Length > 0 && options[0] != null)
        {
            originalScale = options[0].transform.localScale;
        }

        // Crear desfasamiento de tiempo para que no latan todas exactamente al unísono
        phaseOffsets = new float[options.Length];
        for (int i = 0; i < phaseOffsets.Length; i++)
        {
            phaseOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    private void Update()
    {
        if (!enablePulse || currentEvent == null || dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        // Animar la escala de cada una de las 12 opciones en tiempo real
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null && options[i].gameObject.activeSelf)
            {
                float offset = offsetPhases ? phaseOffsets[i] : 0f;
                float wave = Mathf.Sin(Time.time * pulseSpeed + offset);
                float scaleFactor = 1f + (wave * pulseAmount);

                options[i].transform.localScale = originalScale * scaleFactor;
            }
        }
    }

    public void StartDialogue(UFOConspiracyEvent conspiracyEvent)
    {
        currentEvent = conspiracyEvent;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        SetupRound();
    }

    private void SetupRound()
    {
        if (options == null || options.Length == 0 || imagePool == null || imagePool.Count == 0)
        {
            Debug.LogWarning("Faltan componentes asignados en UFOConspiracyDialogue.");
            return;
        }

        // 1. Clasificar imágenes de la pool
        List<UFOImageData> alienImages = new List<UFOImageData>();
        List<UFOImageData> nonAlienImages = new List<UFOImageData>();

        foreach (UFOImageData image in imagePool)
        {
            if (image == null) continue;

            if (image.isAlien && !alienImages.Contains(image))
                alienImages.Add(image);
            else if (!image.isAlien && !nonAlienImages.Contains(image))
                nonAlienImages.Add(image);
        }

        if (alienImages.Count == 0 || nonAlienImages.Count == 0)
        {
            Debug.LogWarning("Necesitas al menos 1 imagen alien y 1 imagen distractor en la Pool.");
            return;
        }

        // 2. Seleccionar 1 imagen alienígena
        UFOImageData alienImage = alienImages[Random.Range(0, alienImages.Count)];

        // 3. Seleccionar 11 imágenes distractoras (se reciclan si hay menos de 11 únicas en la pool)
        List<UFOImageData> selectedImages = new List<UFOImageData> { alienImage };

        while (selectedImages.Count < options.Length)
        {
            UFOImageData randomNonAlien = nonAlienImages[Random.Range(0, nonAlienImages.Count)];
            selectedImages.Add(randomNonAlien);
        }

        // 4. Mezclar las 12 imágenes
        Shuffle(selectedImages);

        // 5. Asignar las imágenes y mostrar las 12 burbujas
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
            {
                options[i].SetImage(selectedImages[i]);
                options[i].Show();
            }
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void SelectOption(UFOImageOption option)
    {
        if (currentEvent == null)
            return;

        if (option.IsAlien())
        {
            currentEvent.CorrectAnswer();
        }
        else
        {
            currentEvent.WrongAnswer();
            // Mezcla de nuevo y reasigna las 12 imágenes al fallar
            SetupRound();
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Ocultar y restaurar el tamaño original de las 12 casillas
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
            {
                options[i].transform.localScale = originalScale;
                options[i].Hide();
            }
        }

        currentEvent = null;
    }
}