using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public float interactDistance = 3f; // Distancia máxima en metros para agarrar el objeto

    void Update()
    {
        // Detecta la pulsación de la tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // Lanza un rayo invisible desde la mirada de la cámara
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // Verifica si el objeto enfocado contiene el script CollectibleItem
            CollectibleItem item = hit.collider.GetComponentInParent<CollectibleItem>();

            if (item != null)
            {
                item.Collect();
            }
        }
    }
}