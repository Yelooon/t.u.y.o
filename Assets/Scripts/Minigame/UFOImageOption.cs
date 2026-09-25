using UnityEngine;
using UnityEngine.UI;

public class UFOImageOption : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UFOConspiracyDialogue dialogue;

    private Image image;
    private UFOImageData currentImage;

    private void Awake()
    {
        image = GetComponent<Image>();

        Debug.Log(
            gameObject.name +
            " → UFOImageOption Awake. Image = " +
            (image != null)
        );
    }

    public void SetImage(UFOImageData imageData)
    {
        currentImage = imageData;

        if (image == null)
        {
            Debug.LogError(
                gameObject.name + " no tiene Image."
            );

            return;
        }

        if (imageData == null)
        {
            Debug.LogError(
                gameObject.name + " recibió UFOImageData NULL."
            );

            return;
        }

        if (imageData.image == null)
        {
            Debug.LogError(
                imageData.name + " NO tiene Sprite asignado."
            );

            return;
        }

        Debug.Log(
            gameObject.name +
            " recibió Sprite: " +
            imageData.image.name
        );

        image.sprite = imageData.image;
        image.preserveAspect = true;
    }

    public void Select()
    {
        if (currentImage == null)
            return;

        dialogue.SelectOption(this);
    }

    public bool IsAlien()
    {
        return currentImage != null &&
               currentImage.isAlien;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}