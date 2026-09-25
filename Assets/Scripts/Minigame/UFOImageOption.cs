using UnityEngine;
using UnityEngine.UI;

public class UFOImageOption : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UFOConspiracyDialogue dialogue;
    [SerializeField] private Image image;

    private UFOImageData currentImage;

    public void SetImage(UFOImageData imageData)
    {
        currentImage = imageData;

        if (image != null)
            image.sprite = imageData.image;
    }

    public void Select()
    {
        if (currentImage == null)
            return;

        dialogue.SelectOption(this);
    }

    public bool IsAlien()
    {
        return currentImage != null && currentImage.isAlien;
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