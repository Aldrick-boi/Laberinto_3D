using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Header("Opcional: objeto que se muestra mientras carga el video")]
    public GameObject loadingIndicator;

    [Header("Reproducir automáticamente al iniciar")]
    public bool playOnStart = true;

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

        videoPlayer.playOnAwake = false;
        videoPlayer.Prepare();
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
