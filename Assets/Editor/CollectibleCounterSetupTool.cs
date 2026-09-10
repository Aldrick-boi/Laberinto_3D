using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Agrega un texto "Disquetes: X / Y" en la esquina inferior izquierda de Mini_Build_P,
// reusando el mismo HUD Canvas del crosshair y el indicador REC.
public static class CollectibleCounterSetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";

    [MenuItem("Tools/Laberinto/Añadir Contador de Coleccionables en Mini_Build_P")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("CollectibleCounterSetupTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var canvasGO = GameObject.Find("HUD Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("HUD Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        var existing = canvasGO.transform.Find("CollectibleCounter");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var counterGO = new GameObject("CollectibleCounter", typeof(RectTransform));
        counterGO.transform.SetParent(canvasGO.transform, false);

        var rect = counterGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(20f, 20f);
        rect.sizeDelta = new Vector2(420f, 64f);

        var text = counterGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = 40f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        HUDTextUtility.ApplyWhiteMaterial(text);
        text.text = "Disquetes: 0 / 0";

        counterGO.AddComponent<CollectibleCounterUI>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("CollectibleCounterSetupTool: contador de coleccionables añadido en Mini_Build_P.");
    }
}
