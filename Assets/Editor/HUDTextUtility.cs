using TMPro;
using UnityEditor;
using UnityEngine;

// El material default de TextMesh Pro (LiberationSans SDF Material) trae un
// _FaceColor casi negro; se multiplica con el color del componente, así que aunque
// el texto diga "blanco" por dentro, se ve negro en pantalla. Esta variante es una
// copia del mismo material con _FaceColor en blanco, como asset aparte para no
// tocar el default (también lo usa el Menú).
public static class HUDTextUtility
{
    private const string MaterialPath = "Assets/Mats/HUDText_White.mat";

    public static void ApplyWhiteMaterial(TextMeshProUGUI text)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(text.fontSharedMaterial);
            material.SetColor("_FaceColor", Color.white);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        text.fontSharedMaterial = material;
    }
}
