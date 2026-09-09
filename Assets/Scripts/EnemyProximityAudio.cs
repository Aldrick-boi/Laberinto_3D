using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyProximityAudio : MonoBehaviour
{
    [Header("Sonido de Proximidad")]
    public AudioClip proximityClip;

    [Header("Distancias")]
    public float maxHearingDistance = 20f;
    public float closeDistance = 2f;
    [Range(0f, 1f)] public float maxVolume = 1f;
    public float volumeSmoothing = 5f;

    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = proximityClip;
        audioSource.loop = true;
        // El volumen lo controla este script según la distancia, no la atenuación 3D de Unity
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (proximityClip != null) audioSource.Play();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float proximity = 1f - Mathf.InverseLerp(closeDistance, maxHearingDistance, distance);
        float targetVolume = Mathf.Clamp01(proximity) * maxVolume;

        // Suaviza el cambio de volumen para evitar saltos bruscos al moverse el enemigo o el jugador
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeSmoothing);
    }
}
