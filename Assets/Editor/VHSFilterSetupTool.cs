using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Agrega el filtro de estática VHS como un Renderer Feature de pantalla completa
// en el Renderer que usa la cámara del jugador (Assets/Settings/PC_Renderer.asset).
// Se puede volver a correr sin duplicar nada: si ya existe, solo actualiza el material.
public static class VHSFilterSetupTool
{
    private const string RendererPath = "Assets/Settings/PC_Renderer.asset";
    private const string ShaderPath = "Assets/Shaders/VHSStatic.shader";
    private const string MaterialPath = "Assets/Mats/VHSStatic.mat";
    private const string FeatureName = "VHSStatic";

    [MenuItem("Tools/Laberinto/Añadir Filtro VHS a la Cámara")]
    public static void Run()
    {
        var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererPath);
        if (rendererData == null)
        {
            Debug.LogError($"VHSFilterSetupTool: no se encontró el Renderer Data en {RendererPath}.");
            return;
        }

        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null)
        {
            Debug.LogError($"VHSFilterSetupTool: no se encontró el shader en {ShaderPath}.");
            return;
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = FeatureName };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        // Si ya se había agregado antes, solo refresca el material y termina
        foreach (var existing in rendererData.rendererFeatures)
        {
            if (existing is FullScreenPassRendererFeature && existing.name == FeatureName)
            {
                ((FullScreenPassRendererFeature)existing).passMaterial = material;
                EditorUtility.SetDirty(rendererData);
                AssetDatabase.SaveAssets();
                Debug.Log("VHSFilterSetupTool: el filtro ya existía, se actualizó el material.");
                return;
            }
        }

        var feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
        feature.name = FeatureName;
        feature.passMaterial = material;
        feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
        feature.fetchColorBuffer = true;

        AssetDatabase.AddObjectToAsset(feature, rendererData);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);

        // Mismo mecanismo que usa el botón "Add Renderer Feature" del Inspector de Unity
        var serializedRenderer = new SerializedObject(rendererData);
        var featuresProp = serializedRenderer.FindProperty("m_RendererFeatures");
        var mapProp = serializedRenderer.FindProperty("m_RendererFeatureMap");

        featuresProp.arraySize++;
        featuresProp.GetArrayElementAtIndex(featuresProp.arraySize - 1).objectReferenceValue = feature;

        mapProp.arraySize++;
        mapProp.GetArrayElementAtIndex(mapProp.arraySize - 1).longValue = localId;

        serializedRenderer.ApplyModifiedProperties();

        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("VHSFilterSetupTool: filtro VHS añadido a PC_Renderer.");
    }
}
