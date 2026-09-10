using UnityEngine;
using UnityEngine.SceneManagement;

// Al tocar este trigger, el jugador pasa a la escena indicada en sceneToLoad.
// Necesita un Collider marcado como "Is Trigger" en el mismo GameObject.
public class SceneTrigger : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar (tiene que estar agregada en Build Settings)")]
    public string sceneToLoad;

    [Tooltip("Coleccionables mínimos que debe llevar el jugador para poder pasar. 0 = sin restricción")]
    public int minCollectiblesRequired = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (minCollectiblesRequired > 0)
        {
            int current = LevelManager.Instance != null ? LevelManager.Instance.CurrentCollected : 0;
            if (current < minCollectiblesRequired)
            {
                Debug.Log($"SceneTrigger en \"{gameObject.name}\": necesitas al menos {minCollectiblesRequired} " +
                          $"coleccionables para pasar (llevas {current}).");
                return;
            }
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning($"SceneTrigger en \"{gameObject.name}\": todavía no tiene una escena asignada.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
