using UnityEngine;

public class CitySpawner : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject cityPrefab;       // Tu prefab de la sección de ciudad

    [Header("Configuración de Spawn")]
    public Vector3 spawnPosition = new Vector3(0, 0, 100); // Punto donde aparecerá la nueva sección
    public float spawnInterval = 6.66f;  // Intervalo de tiempo para instanciar la siguiente parte

    private float timer;

    void Start()
    {
        // Genera la primera sección al iniciar la escena
        SpawnCitySection();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnCitySection();
            timer = 0f;
        }
    }

    void SpawnCitySection()
    {
        Instantiate(cityPrefab, spawnPosition, Quaternion.identity);
    }
}