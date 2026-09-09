using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioDistortionFilter))]
public class EnemyProximityAudio : MonoBehaviour
{
    [Header("Sonido de Proximidad")]
    public AudioClip proximityClip;

    [Header("Distancias")]
    public float maxHearingDistance = 20f;
    public float closeDistance = 2f;
    [Range(0f, 1f)] public float maxVolume = 1f;
    public float volumeSmoothing = 5f;

    [Header("Saturación al Acercarse")]
    public float distortionDistance = 5f;
    [Range(0f, 1f)] public float maxDistortion = 0.25f;

    private AudioSource audioSource;
    private AudioDistortionFilter distortionFilter;
    private Transform player;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = proximityClip;
        audioSource.loop = true;
        // El volumen lo controla este script según la distancia, no la atenuación 3D de Unity
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;
        // Sin esto, Unity le cambia el tono al sonido según la velocidad del enemigo
        // (efecto Doppler), aunque el audio sea 2D — de ahí que sonara "alentado"
        audioSource.dopplerLevel = 0f;

        // Por si este componente ya existía en un prefab guardado antes de que se
        // requiriera el AudioDistortionFilter (RequireComponent no lo agrega en retrospectiva)
        distortionFilter = GetComponent<AudioDistortionFilter>();
        if (distortionFilter == null) distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        distortionFilter.distortionLevel = 0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (proximityClip != null) audioSource.Play();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        float volumeProximity = 1f - Mathf.InverseLerp(closeDistance, maxHearingDistance, distance);
        float targetVolume = Mathf.Clamp01(volumeProximity) * maxVolume;

        float distortionProximity = 1f - Mathf.InverseLerp(closeDistance, distortionDistance, distance);
        float targetDistortion = Mathf.Clamp01(distortionProximity) * maxDistortion;

        // Suaviza los cambios para evitar saltos bruscos al moverse el enemigo o el jugador
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeSmoothing);
        distortionFilter.distortionLevel = Mathf.Lerp(distortionFilter.distortionLevel, targetDistortion, Time.deltaTime * volumeSmoothing);
    }
}
