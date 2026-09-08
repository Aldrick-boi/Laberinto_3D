using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Configuración del Nivel")]
    public int totalRequired = 5;      // Cantidad de ítems necesarios
    private int currentCollected = 0;   // Contador actual

    private void Awake()
    {
        // Patrón Singleton sencillo para poder llamarlo desde cualquier script
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCollectible()
    {
        currentCollected++;
        Debug.Log($"Objetos recolectados: {currentCollected} / {totalRequired}");

        if (currentCollected >= totalRequired)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        Debug.Log("¡NIVEL COMPLETADO! Has recogido todos los objetos.");
        // Aquí podrás activar la puerta de salida, cambiar de escena, etc.
    }
}