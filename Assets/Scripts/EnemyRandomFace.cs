using UnityEngine;

// Elige un par (imagen + música) al azar de faceOptions cada vez que arranca la
// partida: la imagen se aplica como textura del enemigo, la música como su sonido
// de proximidad (EnemyProximityAudio). Usa .material (copia propia) en vez de
// .sharedMaterial para no pisar el asset compartido Enemy.mat.
[RequireComponent(typeof(MeshRenderer))]
public class EnemyRandomFace : MonoBehaviour
{
    [System.Serializable]
    public struct FaceAudioPair
    {
        public Texture2D face;
        public AudioClip music;
    }

    public FaceAudioPair[] faceOptions;

    private const string LastFaceIndexKey = "EnemyRandomFace_LastIndex";

    void Start()
    {
        if (faceOptions == null || faceOptions.Length == 0) return;

        int index = PickIndexAvoidingRepeat();
        PlayerPrefs.SetInt(LastFaceIndexKey, index);

        FaceAudioPair chosen = faceOptions[index];

        if (chosen.face != null)
        {
            GetComponent<MeshRenderer>().material.mainTexture = chosen.face;

            // Reajusta el ancho del plano al aspecto real de la imagen elegida (conservando
            // la altura ya configurada) para que no salga estirada si no es cuadrada
            float height = transform.localScale.y;
            float width = height * (chosen.face.width / (float)chosen.face.height);
            transform.localScale = new Vector3(width, height, transform.localScale.z);
        }

        // EnemyProximityAudio vive en la raíz (este script está en el hijo "Visual")
        var proximityAudio = GetComponentInParent<EnemyProximityAudio>();
        if (proximityAudio != null) proximityAudio.SetProximityClip(chosen.music);
    }

    // Evita repetir la cara de la partida anterior (se recuerda entre sesiones vía PlayerPrefs,
    // ya que cada Play es un Start() nuevo sin memoria propia)
    private int PickIndexAvoidingRepeat()
    {
        if (faceOptions.Length == 1) return 0;

        int lastIndex = PlayerPrefs.GetInt(LastFaceIndexKey, -1);
        int index;
        do
        {
            index = Random.Range(0, faceOptions.Length);
        } while (index == lastIndex);

        return index;
    }
}
