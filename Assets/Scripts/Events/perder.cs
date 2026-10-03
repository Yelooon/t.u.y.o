using UnityEngine;

public class perder : MonoBehaviour
{
    public GameObject panel;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(MoneyManager.Instance.perder)
        {
            panel.SetActive(true);
        }
    }
    public void menu()
    {
        SceneFader.Instance.CambiarEscena("MainMenu");
    }

}
