using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Animación")]
    public float rotationSpeed = 50f;

    private bool isCollected = false; // Bandera de seguridad para evitar doble ejecución

    private void Update()
    {
        // Hace girar el objeto en el aire continuamente
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    // Método llamado por la cámara (PlayerInteraction) al presionar 'E'
    public void Collect()
    {
        // Si ya se recolectó en este frame o anteriormente, cancela la ejecución
        if (isCollected) return; 
        isCollected = true;

        // Notifica al LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddCollectible();
        }

        // Elimina el objeto de la escena
        Destroy(gameObject);
    }
}