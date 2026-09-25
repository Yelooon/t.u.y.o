using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class CandyQTE : MonoBehaviour
{
    [Header("QTE Settings")]
    [SerializeField] private float timeLimit = 5f;
    [SerializeField] private int sequenceLength = 4;

    [Header("UI")]
    [SerializeField] private GameObject qtePanel;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Slider timerBar;

    private Key[] possibleKeys =
    {
        Key.W,
        Key.A,
        Key.S,
        Key.D
    };

    private Key[] currentSequence;

    private int currentIndex;
    private float currentTimer;

    private bool qteActive = false;

    private Action onCompleted;
    private Action onFailed;

    public bool IsActive => qteActive;

    private void Update()
    {
        if (!qteActive)
            return;

        currentTimer -= Time.deltaTime;

        UpdateTimerBar();

        if (currentTimer <= 0f)
        {
            FailQTE();
            return;
        }

        CheckInput();
    }

    public void StartQTE(Action completedCallback, Action failedCallback)
    {
        if (qteActive)
            return;

        onCompleted = completedCallback;
        onFailed = failedCallback;

        GenerateSequence();

        currentIndex = 0;
        currentTimer = timeLimit;
        qteActive = true;

        if (qtePanel != null)
            qtePanel.SetActive(true);

        if (timerBar != null)
        {
            timerBar.maxValue = timeLimit;
            timerBar.value = timeLimit;
        }

        UpdateKeyUI();

        Debug.Log("🍬 QTE iniciado.");
    }

    private void GenerateSequence()
    {
        currentSequence = new Key[sequenceLength];

        for (int i = 0; i < sequenceLength; i++)
        {
            currentSequence[i] =
                possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
        }
    }

    private void CheckInput()
    {
        if (Keyboard.current == null)
            return;

        Key requiredKey = currentSequence[currentIndex];

        if (Keyboard.current[requiredKey].wasPressedThisFrame)
        {
            Debug.Log("✓ Tecla correcta: " + requiredKey);

            currentIndex++;

            if (currentIndex >= currentSequence.Length)
            {
                CompleteQTE();
                return;
            }

            UpdateKeyUI();
        }
    }

    private void UpdateKeyUI()
    {
        if (keyText == null)
            return;

        keyText.text = FormatKey(currentSequence[currentIndex]);
    }

    private void UpdateTimerBar()
    {
        if (timerBar == null)
            return;

        timerBar.value = currentTimer;
    }

    private string FormatKey(Key key)
    {
        switch (key)
        {
            case Key.W:
                return "W";
            case Key.A:
                return "A";
            case Key.S:
                return "S";
            case Key.D:
                return "D";
            default:
                return key.ToString();
        }
    }

    private void CompleteQTE()
    {
        qteActive = false;

        if (qtePanel != null)
            qtePanel.SetActive(false);

        Debug.Log("✓ QTE completado.");

        onCompleted?.Invoke();

        ClearCallbacks();
    }

    private void FailQTE()
    {
        qteActive = false;

        if (qtePanel != null)
            qtePanel.SetActive(false);

        Debug.Log("✗ QTE fallado.");

        onFailed?.Invoke();

        ClearCallbacks();
    }

    private void ClearCallbacks()
    {
        onCompleted = null;
        onFailed = null;
    }
}