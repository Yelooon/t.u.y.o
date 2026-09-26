using UnityEngine;

public class CityMover : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 15f;          // Velocidad a la que se mueve la ciudad
    public float destroyXPoint = -100f; // Posición Z en la que el objeto se destruirá

    void Update()
    {
        // Mueve la sección hacia atrás en el espacio global
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        // Si la sección sobrepasa el límite establecido, se destruye para liberar memoria
        if (transform.position.x >= destroyXPoint)
        {
            Destroy(gameObject);
        }
    }
}