using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Animación")]
    public float rotationSpeed = 50f;

    [Header("Sonido")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float collectVolume = 1f;

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

        // PlayClipAtPoint crea su propio AudioSource temporal que sobrevive a este
        // objeto, ya que Destroy() se lo lleva antes de que un AudioSource propio
        // alcanzara a terminar de sonar
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
        }

        // Elimina el objeto de la escena
        Destroy(gameObject);
    }
}