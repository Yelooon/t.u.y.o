using System.Collections;
using UnityEngine;

public class MotorcycleRobberyEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WindowInteractable window;

    [Header("Event Timing")]
    [SerializeField] private float reactionTime = 3f;

    [Header("Visual")]
    [SerializeField] private GameObject strangerFace;

    [Header("Audio")]
    [SerializeField] private AudioSource motorcycleAudio;

    private bool eventActive = false;

    public bool EventActive => eventActive;

    public void TriggerRobbery()
    {
        if (eventActive)
            return;

        // El evento solamente puede comenzar
        // si la ventana está abierta.
        if (window == null || !window.IsOpen)
        {
            Debug.Log("Raponazo cancelado: la ventana está cerrada.");
            return;
        }

        StartCoroutine(RobberySequence());
    }

    private IEnumerator RobberySequence()
    {
        eventActive = true;

        Debug.Log("🏍️ Se escucha una moto...");

        PlayMotorcycleSound();

        yield return new WaitForSeconds(1f);

        // Comprobar nuevamente porque el jugador
        // pudo haber cerrado la ventana mientras tanto.
        if (!window.IsOpen)
        {
            EndRobbery();
            yield break;
        }

        ShowStranger();

        Debug.Log("👤 ¡Un extraño apareció en la ventana!");
        Debug.Log("⚠️ ¡CIERRA LA VENTANA!");

        float timer = 0f;

        while (timer < reactionTime)
        {
            // El jugador cerró la ventana
            if (!window.IsOpen)
            {
                Debug.Log("✓ El jugador cerró la ventana a tiempo.");
                EndRobbery();
                yield break;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        // Si llegamos aquí, nunca cerró la ventana
        Debug.Log("💸 ¡RAPONAZO!");

        StealMoney();

        EndRobbery();
    }

    private void PlayMotorcycleSound()
    {
        if (motorcycleAudio != null)
        {
            motorcycleAudio.Play();
        }
    }

    private void ShowStranger()
    {
        if (strangerFace != null)
        {
            strangerFace.SetActive(true);
        }
    }

    private void HideStranger()
    {
        if (strangerFace != null)
        {
            strangerFace.SetActive(false);
        }
    }

    private void StealMoney()
    {
        Debug.Log("💸 El jugador perdió dinero.");
        
        // Aquí conectaremos posteriormente
        // el sistema real de dinero.
    }

    private void EndRobbery()
    {
        HideStranger();

        eventActive = false;
    }
}