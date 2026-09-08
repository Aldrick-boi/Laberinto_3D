using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("Fondo (video)")]
    [SerializeField] private RectTransform fondoRect;
    [SerializeField] private float distanciaMovimiento = 150f;
    [SerializeField] private float velocidadMovimiento = 30f;

    [Header("Menu principal")]
    [SerializeField] private GameObject panelMenuPrincipal;

    [Header("Info")]
    [SerializeField] private GameObject panelInfo;

    [Header("Creditos")]
    [SerializeField] private GameObject panelCreditos;
    [SerializeField] private VideoPlayer videoCreditos;
    [SerializeField] private bool volverAlMenuAlTerminarCreditos = true;

    [Header("Controles")]
    [SerializeField] private GameObject panelControles;

    [Header("Escena")]
    [SerializeField] private string nombreEscenaJuego = "Backrooms";

    private Vector2 posicionInicialFondo;
    private float direccion = 1f; 

    
    private GameObject panelAbiertoActualmente;

    private void Start()
    {
        if (fondoRect != null)
        {
            posicionInicialFondo = fondoRect.anchoredPosition;
        }

        CerrarTodosLosPaneles();

        if (panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(true);
        }

        if (videoCreditos != null)
        {
            videoCreditos.loopPointReached += OnCreditosTerminaron;
        }
    }

    private void Update()
    {
        MoverFondo();

        // Escape cierra el panel que esté abierto (Info, Creditos o Controles)
        if (panelAbiertoActualmente != null && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarPanelActual();
        }
    }

    private void MoverFondo()
    {
        if (fondoRect == null) return;

        Vector2 pos = fondoRect.anchoredPosition;
        pos.x += direccion * velocidadMovimiento * Time.deltaTime;

        if (pos.x >= posicionInicialFondo.x + distanciaMovimiento)
        {
            pos.x = posicionInicialFondo.x + distanciaMovimiento;
            direccion = -1f;
        }
        else if (pos.x <= posicionInicialFondo.x - distanciaMovimiento)
        {
            pos.x = posicionInicialFondo.x - distanciaMovimiento;
            direccion = 1f;
        }

        fondoRect.anchoredPosition = pos;
    }

    // ---------- Jugar ----------

    public void OnClickJugar()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // ---------- Info ----------

    public void OnClickInfo()
    {
        AbrirPanel(panelInfo);
    }

    public void OnClickCerrarInfo()
    {
        CerrarPanelActual();
    }

    // ---------- Créditos ----------

    public void OnClickCreditos()
    {
        AbrirPanel(panelCreditos);

        if (videoCreditos != null)
        {
            videoCreditos.Stop();
            videoCreditos.Play();
        }
    }

    public void OnClickCerrarCreditos()
    {
        if (videoCreditos != null)
        {
            videoCreditos.Stop();
        }

        CerrarPanelActual();
    }

    private void OnCreditosTerminaron(VideoPlayer vp)
    {
        if (volverAlMenuAlTerminarCreditos)
        {
            OnClickCerrarCreditos();
        }
    }

    // ---------- Controles ----------

    public void OnClickControles()
    {
        AbrirPanel(panelControles);
    }

    public void OnClickCerrarControles()
    {
        CerrarPanelActual();
    }

    // ---------- Salir ----------

    public void OnClickSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Helpers de paneles ----------

    private void AbrirPanel(GameObject panel)
    {
        if (panel == null) return;

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);

        panel.SetActive(true);
        panelAbiertoActualmente = panel;
    }

    private void CerrarPanelActual()
    {
        if (panelAbiertoActualmente != null)
        {
            panelAbiertoActualmente.SetActive(false);
            panelAbiertoActualmente = null;
        }

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }

    private void CerrarTodosLosPaneles()
    {
        if (panelInfo != null) panelInfo.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
        panelAbiertoActualmente = null;
    }
}
