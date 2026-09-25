using System.Collections.Generic;
using UnityEngine;

public class UFOConspiracyDialogue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Image Pool")]
    [SerializeField] private List<UFOImageData> imagePool;

    [Header("Options")]
    [SerializeField] private UFOImageOption[] options;

    private UFOConspiracyEvent currentEvent;

    public void StartDialogue(UFOConspiracyEvent conspiracyEvent)
    {
        currentEvent = conspiracyEvent;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        GenerateOptions();
    }

    private void GenerateOptions()
    {
        if (options.Length == 0 || imagePool.Count == 0)
            return;

        List<UFOImageData> alienImages = new List<UFOImageData>();
        List<UFOImageData> nonAlienImages = new List<UFOImageData>();

        foreach (UFOImageData image in imagePool)
        {
            // Ignorar huecos vacíos y repetidos en la lista
            if (image == null ||
                alienImages.Contains(image) ||
                nonAlienImages.Contains(image))
                continue;

            if (image.isAlien)
                alienImages.Add(image);
            else
                nonAlienImages.Add(image);
        }

        // Necesitamos al menos una imagen alien
        if (alienImages.Count == 0)
        {
            Debug.LogWarning("No hay imágenes alien en la pool.");
            return;
        }

        // Necesitamos suficientes distractores
        if (nonAlienImages.Count < options.Length - 1)
        {
            Debug.LogWarning("No hay suficientes imágenes distractoras.");
            return;
        }

        // Elegir una imagen alien
        UFOImageData alienImage =
            alienImages[Random.Range(0, alienImages.Count)];

        List<UFOImageData> selectedImages =
            new List<UFOImageData>();

        selectedImages.Add(alienImage);

        // Elegir distractores
        while (selectedImages.Count < options.Length)
        {
            UFOImageData randomImage =
                nonAlienImages[Random.Range(0, nonAlienImages.Count)];

            if (!selectedImages.Contains(randomImage))
            {
                selectedImages.Add(randomImage);
            }
        }

        // Mezclar posiciones
        Shuffle(selectedImages);

        // Asignar imágenes a las opciones
        for (int i = 0; i < options.Length; i++)
        {
            options[i].SetImage(selectedImages[i]);
            options[i].Show();
        }
    }

    private void Shuffle(List<UFOImageData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            UFOImageData temp = list[i];
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
            Debug.Log("¡Encontraste la imagen relacionada con aliens!");

            currentEvent.CorrectAnswer();
        }
        else
        {
            Debug.Log("¡Eso no tiene nada que ver con aliens!");

            // Podemos generar otra combinación
            GenerateOptions();
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        foreach (UFOImageOption option in options)
        {
            option.Hide();
        }

        currentEvent = null;
    }
}