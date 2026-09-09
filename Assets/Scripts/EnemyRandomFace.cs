using UnityEngine;

// Elige una imagen al azar de faceOptions y la aplica como textura del enemigo
// cada vez que arranca la partida. Usa .material (copia propia) en vez de
// .sharedMaterial para no pisar el asset compartido Enemy.mat.
[RequireComponent(typeof(MeshRenderer))]
public class EnemyRandomFace : MonoBehaviour
{
    public Texture2D[] faceOptions;

    private const string LastFaceIndexKey = "EnemyRandomFace_LastIndex";

    void Start()
    {
        if (faceOptions == null || faceOptions.Length == 0) return;

        int index = PickIndexAvoidingRepeat();
        PlayerPrefs.SetInt(LastFaceIndexKey, index);

        Texture2D chosen = faceOptions[index];
        GetComponent<MeshRenderer>().material.mainTexture = chosen;

        // Reajusta el ancho del plano al aspecto real de la imagen elegida (conservando
        // la altura ya configurada) para que no salga estirada si no es cuadrada
        float height = transform.localScale.y;
        float width = height * (chosen.width / (float)chosen.height);
        transform.localScale = new Vector3(width, height, transform.localScale.z);
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
