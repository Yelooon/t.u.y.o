using UnityEngine;

public class perder : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        // Asegura que el Game Over arranque escondido
        panel.SetActive(false);
    }

    void Update()
    {
        if (MoneyManager.Instance != null && MoneyManager.Instance.IsBroke)
        {
            panel.SetActive(true);
        }
    }

    public void menu()
    {
        SceneFader.Instance.CambiarEscena("MainMenu");
    }
}