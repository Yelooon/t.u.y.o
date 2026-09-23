using System.Collections;
using UnityEngine;

public class PickpocketEvent : MonoBehaviour
{
    [Header("Pickpocket")]
    [SerializeField] private Transform pickpocket;

    [Header("Positions")]
    [SerializeField] private Transform position1;
    [SerializeField] private Transform position2;
    [SerializeField] private Transform position3;

    [Header("Timing")]
    [SerializeField] private float timeBetweenMoves = 5f;
    [SerializeField] private float watchDuration = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource movementAudio;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;

    private int currentPosition = 1;
    private bool eventActive = false;

    private void Start()
    {
        // Inicializar físicamente al Pickpocket
        MovePickpocket(position1);
    }

    // ESTE es el método que llamas desde el botón
    public void StartPickpocket()
    {
        if (eventActive)
            return;

        eventActive = true;

        StartCoroutine(PickpocketSequence());
    }

    private IEnumerator PickpocketSequence()
    {
        Debug.Log("🧍 Pickpocket comenzó en posición 1.");

        // Esperar antes de moverse a posición 2
        yield return new WaitForSeconds(timeBetweenMoves);

        MoveToPosition(2);

        // Esperar antes de moverse a posición 3
        yield return new WaitForSeconds(timeBetweenMoves);

        MoveToPosition(3);

        // Comienza la fase de peligro
        yield return StartCoroutine(CheckIfPlayerWatches());
    }

    private void MoveToPosition(int newPosition)
    {
        currentPosition = newPosition;

        switch (newPosition)
        {
            case 1:
                MovePickpocket(position1);
                break;

            case 2:
                MovePickpocket(position2);
                break;

            case 3:
                MovePickpocket(position3);
                break;
        }

        PlayMovementSound();

        Debug.Log("🧍 Pickpocket → posición " + currentPosition);
    }

    private void MovePickpocket(Transform target)
    {
        if (pickpocket == null || target == null)
            return;

        pickpocket.position = target.position;
        pickpocket.rotation = target.rotation;
    }

    private IEnumerator CheckIfPlayerWatches()
    {
        float watchTimer = 0f;
        float dangerTimer = 0f;

        Debug.Log("⚠️ Pickpocket está en posición 3.");

        while (true)
        {
            dangerTimer += Time.deltaTime;

            // Está mirando hacia atrás
            if (cameraController.CurrentView ==
                CameraController.CameraView.Back)
            {
                watchTimer += Time.deltaTime;

                Debug.Log(
                    $"👀 Vigilando Pickpocket: {watchTimer:F1}/{watchDuration:F1}"
                );

                // Lo miró durante 5 segundos
                if (watchTimer >= watchDuration)
                {
                    Debug.Log("✓ Pickpocket descubierto.");

                    ResetPickpocket();

                    cameraController.LookFront();

                    yield break;
                }
            }
            else
            {
                // Dejó de mirarlo
                watchTimer = 0f;
            }

            // Pasó demasiado tiempo sin detenerlo
            if (dangerTimer >= timeBetweenMoves)
            {
                Debug.Log("💸 ROBO 100K");

                ResetPickpocket();

                cameraController.LookFront();

                yield break;
            }

            yield return null;
        }
    }

    private void PlayMovementSound()
    {
        if (movementAudio != null)
        {
            movementAudio.Play();
        }
    }

    private void ResetPickpocket()
    {
        currentPosition = 1;

        MovePickpocket(position1);

        eventActive = false;

        Debug.Log("↩ Pickpocket regresó a posición 1.");
    }
}