using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Header("Opcional: objeto que se muestra mientras carga el video")]
    public GameObject loadingIndicator;

    [Header("Reproducir automáticamente al iniciar")]
    public bool playOnStart = true;

    [Header("Volver al juego con ESC")]
    [Tooltip("Si está activado, al presionar ESC se detiene el video y se vuelve al juego.")]
    public bool allowEscapeToReturn = true;

    [Tooltip("Nombre del GameObject (Canvas/Panel del video) que se debe ocultar al volver. Déjalo vacío si el video está en su propia escena.")]
    public GameObject videoRoot;

    [Tooltip("Nombre de la escena del juego a la que volver. Déjalo vacío si el video está dentro de la misma escena del juego (solo se ocultará y se reanudará el tiempo).")]
    public string gameSceneName = "";

    private float previousTimeScale = 1f;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    void Start()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // Guarda el timeScale actual por si el juego estaba en pausa antes del video
        previousTimeScale = Time.timeScale;

        videoPlayer.playOnAwake = false;
        videoPlayer.Prepare();
    }

    void Update()
    {
        if (allowEscapeToReturn && Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToGame();
        }
    }

    private void ReturnToGame()
    {
        videoPlayer.Stop();

        // Restaura el estado de tiempo del juego (por si se había pausado)
        Time.timeScale = previousTimeScale;

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            // El video está en una escena aparte: carga la escena del juego
            SceneManager.LoadScene(gameSceneName);
        }
        else if (videoRoot != null)
        {
            // El video está dentro de la misma escena: solo lo oculta
            videoRoot.SetActive(false);
        }
        else
        {
            // Por defecto, oculta este mismo objeto
            gameObject.SetActive(false);
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        if (playOnStart)
            vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("El video terminó de reproducirse.");
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("Error al reproducir el video: " + message);
    }

    // --- Métodos públicos, útiles para conectar a botones UI ---

    public void PlayVideo()
    {
        videoPlayer.Play();
    }

    public void PauseVideo()
    {
        videoPlayer.Pause();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
    }

    public void RestartVideo()
    {
        videoPlayer.time = 0;
        videoPlayer.Play();
    }
}
