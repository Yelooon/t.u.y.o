using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class UFOImageOption : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UFOConspiracyDialogue dialogue;

    private Image image;
    private Button button;
    private UFOImageData currentImage;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        if (dialogue == null)
            dialogue = GetComponentInParent<UFOConspiracyDialogue>();

        // Conecta automáticamente el clic con el método Select()
        if (button != null)
        {
            button.onClick.RemoveListener(Select);
            button.onClick.AddListener(Select);
        }
    }

    public void SetImage(UFOImageData imageData)
    {
        currentImage = imageData;

        if (image == null)
        {
            Debug.LogError($"{gameObject.name} no tiene un componente Image asignado.");
            return;
        }

        if (imageData == null)
        {
            Debug.LogError($"{gameObject.name} recibió UFOImageData NULL.");
            return;
        }

        if (imageData.image == null)
        {
            Debug.LogError($"{imageData.name} NO tiene un Sprite asignado.");
            return;
        }

        image.sprite = imageData.image;
        image.preserveAspect = true;
    }

    public void Select()
    {
        if (currentImage == null)
        {
            Debug.LogWarning($"{gameObject.name} no tiene datos de imagen cargados al hacer clic.");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogError($"{gameObject.name} no pudo comunicarse con UFOConspiracyDialogue.");
            return;
        }

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