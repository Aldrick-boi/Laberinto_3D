using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel del menú de pausa")]
    [Tooltip("Arrastra aquí el GameObject (Panel) que contiene los 3 botones")]
    public GameObject panelPausa;

    [Header("Nombre de la escena del menú principal")]
    public string nombreEscenaMenu = "Menu";

    private bool juegoPausado = false;

    void Start()
    {
        // Aseguramos que el panel empiece desactivado
        if (panelPausa != null)
            panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
                Reanudar();
            else
                Pausar();
        }
    }

    void LateUpdate()
    {
        // Si está pausado, forzamos el cursor visible y libre en CADA frame,
        // sin importar que otro script (ej. mouse look) intente re-bloquearlo.
        // LateUpdate corre después de todos los Update(), por eso "gana" siempre.
        if (juegoPausado)
        {
            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;

            if (!Cursor.visible)
                Cursor.visible = true;
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);
        Time.timeScale = 0f; // Congela el juego
        juegoPausado = true;

        // Liberamos el cursor para poder interactuar con los botones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f; // Reanuda el juego
        juegoPausado = false;

        // Volvemos a bloquear el cursor para el modo de juego (típico FPS)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Botón "Continuar"
    public void OnContinuarPresionado()
    {
        Reanudar();
    }

    // Botón "Menú"
    public void OnMenuPresionado()
    {
        Time.timeScale = 1f; // Importante: restaurar el tiempo antes de cambiar de escena
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    // Botón "Salir"
    public void OnSalirPresionado()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
