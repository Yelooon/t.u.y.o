using UnityEngine;
using UnityEngine.InputSystem; // Importante: añade esta línea

public class MenuCameraTilt : MonoBehaviour
{
    [Header("Configuración de Inclinación")]
    [Tooltip("Ángulo máximo en grados que se moverá la cámara.")]
    [SerializeField] private float maxTiltAngle = 5f;

    [Tooltip("Velocidad de suavizado del movimiento.")]
    [SerializeField] private float smoothSpeed = 5f;

    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        // Verificar si existe un mouse conectado
        if (Mouse.current == null) return;

        // Leer la posición del mouse con el nuevo Input System
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Normalizar la posición del mouse de -0.5 a 0.5
        float mouseX = (mousePos.x / Screen.width) - 0.5f;
        float mouseY = (mousePos.y / Screen.height) - 0.5f;

        // Calcular la rotación objetivo
        Quaternion targetTilt = Quaternion.Euler(-mouseY * maxTiltAngle, mouseX * maxTiltAngle, 0f);
        Quaternion targetRotation = initialRotation * targetTilt;

        // Aplicar suavizado
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}