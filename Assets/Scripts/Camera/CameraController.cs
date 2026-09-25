using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public enum CameraView
    {
        Forward,
        Left,
        Right,
        Back,
        Down
    }

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Camera Points")]
    [SerializeField] private Transform forwardPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform downPoint;

    [Header("Movement")]
    [SerializeField] private float transitionSpeed = 8f;

    [Header("Edge Screen Look")]
    [SerializeField] private bool enableEdgeLook = true;
    [Range(0.01f, 0.15f)]
    [Tooltip("Porcentaje del borde de la pantalla que activa el giro (ej. 0.05 = 5%).")]
    [SerializeField] private float edgeThreshold = 0.05f;
    [Tooltip("Tiempo de espera mínimo entre giros provocados por el mouse.")]
    [SerializeField] private float edgeCooldown = 0.6f;

    [Header("Mouse Parallax / Sway")]
    [SerializeField] private float parallaxRotAmount = 2.5f;
    [Tooltip("Velocidad de respuesta del movimiento del mouse.")]
    [SerializeField] private float parallaxSmoothSpeed = 4f;

    [Header("Vertical Limit")]
    [Range(0f, 0.5f)]
    [Tooltip("Límite máximo de inclinación hacia arriba (0 = no permite mirar más arriba del horizonte exacto).")]
    [SerializeField] private float maxUpwardLook = 0.08f;

    private CameraView currentView;
    private Quaternion targetRotation;
    private Quaternion currentBaseRotation;

    private float speedMultiplier = 1f;
    private float lastEdgeTriggerTime;
    private bool isEdgeActive = false;
    private bool isLocked = false; // Estado de bloqueo

    private Vector2 currentMouseNormalized;

    public CameraView CurrentView => currentView;
    public bool IsLocked => isLocked;

    private Vector2 MousePosition
    {
        get
        {
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        }
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        SetView(CameraView.Forward, true);
    }

    private void Update()
    {
        // Si está bloqueada, ignora el giro por los bordes
        if (!isLocked)
        {
            HandleEdgeLook();
        }

        MoveCamera();
    }

    private void HandleEdgeLook()
    {
        if (!enableEdgeLook) return;

        Vector2 mousePos = MousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        bool inLeftEdge = mousePos.x <= screenWidth * edgeThreshold;
        bool inRightEdge = mousePos.x >= screenWidth * (1f - edgeThreshold);
        bool inBottomEdge = mousePos.y <= screenHeight * edgeThreshold;
        bool inTopEdge = mousePos.y >= screenHeight * (1f - edgeThreshold);

        bool inAnyEdge = inLeftEdge || inRightEdge || inBottomEdge || inTopEdge;

        if (inAnyEdge && !isEdgeActive && Time.time >= lastEdgeTriggerTime + edgeCooldown)
        {
            isEdgeActive = true;
            lastEdgeTriggerTime = Time.time;

            if (inLeftEdge)
            {
                LookLeft();
            }
            else if (inRightEdge)
            {
                LookRight();
            }
            else if (inBottomEdge && currentView != CameraView.Down)
            {
                LookDown();
            }
            else if (inTopEdge && currentView == CameraView.Down)
            {
                LookFront();
            }
        }
        else if (!inAnyEdge)
        {
            isEdgeActive = false;
        }
    }

    private void MoveCamera()
    {
        if (playerCamera == null) return;

        float currentSpeed = transitionSpeed * speedMultiplier;

        // 1. Interpolación base de la rotación hacia el objetivo
        currentBaseRotation = Quaternion.Slerp(
            currentBaseRotation,
            targetRotation,
            currentSpeed * Time.deltaTime
        );

        // 2. Paralaje del mouse (se va a 0 si la cámara está bloqueada)
        Vector2 targetMouseNormalized = Vector2.zero;

        if (!isLocked)
        {
            Vector2 mousePos = MousePosition;
            targetMouseNormalized = new Vector2(
                (mousePos.x / Screen.width) - 0.5f,
                (mousePos.y / Screen.height) - 0.5f
            );
            targetMouseNormalized.y = Mathf.Min(targetMouseNormalized.y, maxUpwardLook);
        }

        currentMouseNormalized = Vector2.Lerp(
            currentMouseNormalized,
            targetMouseNormalized,
            Time.deltaTime * parallaxSmoothSpeed
        );

        Quaternion parallaxRot = Quaternion.Euler(
            -currentMouseNormalized.y * parallaxRotAmount,
            currentMouseNormalized.x * parallaxRotAmount,
            0f
        );

        playerCamera.transform.rotation = currentBaseRotation * parallaxRot;
    }

    // Permite bloquear o desbloquear la cámara desde eventos externos
    public void SetLock(bool lockState)
    {
        isLocked = lockState;
    }

    public void LookFront()
    {
        SetView(CameraView.Forward);
    }

    public void LookLeft()
    {
        switch (currentView)
        {
            case CameraView.Forward: SetView(CameraView.Left); break;
            case CameraView.Left: SetView(CameraView.Back); break;
            case CameraView.Right: SetView(CameraView.Forward); break;
            case CameraView.Down: SetView(CameraView.Forward); break;
        }
    }

    public void LookRight()
    {
        switch (currentView)
        {
            case CameraView.Forward: SetView(CameraView.Right); break;
            case CameraView.Left: SetView(CameraView.Forward); break;
            case CameraView.Back: SetView(CameraView.Left); break;
            case CameraView.Down: SetView(CameraView.Forward); break;
        }
    }

    public void LookDown()
    {
        SetView(CameraView.Down);
    }

    public void SetView(CameraView newView, bool instant = false)
    {
        currentView = newView;
        Transform targetPoint = GetPointForView(newView);

        targetRotation = targetPoint.rotation;

        if (instant)
        {
            currentBaseRotation = targetRotation;
            playerCamera.transform.rotation = targetRotation;
        }
    }

    private Transform GetPointForView(CameraView view)
    {
        switch (view)
        {
            case CameraView.Forward: return forwardPoint;
            case CameraView.Left: return leftPoint;
            case CameraView.Right: return rightPoint;
            case CameraView.Back: return backPoint;
            case CameraView.Down: return downPoint;
            default: return forwardPoint;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.05f, multiplier);
    }
}