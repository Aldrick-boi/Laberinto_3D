using UnityEngine;
using UnityEngine.SceneManagement;

// Reinicia el nivel en cuanto el collider del enemigo toca al jugador
public class EnemyCatchPlayer : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
