using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Configuración del Nivel")]
    public int totalRequired = 5;      // Cantidad de ítems necesarios
    private int currentCollected = 0;   // Contador actual

    public int CurrentCollected => currentCollected;

    private GameObject exitPoint;

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
            return;
        }

        // La salida empieza desactivada; se activa recién al completar el nivel.
        // Se busca aquí (antes de que nada más corra) porque GameObject.Find no
        // encuentra objetos ya desactivados
        exitPoint = GameObject.Find("Exit");
        if (exitPoint != null) exitPoint.SetActive(false);
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

        if (exitPoint != null) exitPoint.SetActive(true);
    }
}