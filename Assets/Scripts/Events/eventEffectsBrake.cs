using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class eventEfects : MonoBehaviour
{
    [SerializeField] private GameObject signals;
    [SerializeField] private AudioSource frenado;
    private bool parado;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AlFrenar()
    {
        parado = false;
        signals.SetActive(true);
        frenado.Play();
        StartCoroutine(parpadeo());
    }

    public void AlCaer()
    {
        signals.SetActive(false);
        StopCoroutine(parpadeo());
        parado = true;
    }

    public void AlRecuperarse()
    {
        Debug.Log("El jugador se recuperó");
    }

    public IEnumerator parpadeo()
    {
        for (int i = 0; i < 20; i++)
        {
            signals.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            signals.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            if(parado)
            {
                break;
            }
        }
    }
}
