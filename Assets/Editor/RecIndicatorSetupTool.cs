using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Agrega un indicador "● REC" en la esquina superior izquierda de Mini_Build_P,
// simulando el HUD de una cámara grabando (a juego con el filtro VHS).
public static class RecIndicatorSetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";

    [MenuItem("Tools/Laberinto/Añadir Indicador REC en Mini_Build_P")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("RecIndicatorSetupTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Reusa el Canvas del crosshair si ya existe, para no tener dos Canvas de HUD sueltos
        var canvasGO = GameObject.Find("HUD Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("HUD Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        var existing = canvasGO.transform.Find("RecIndicator");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var container = new GameObject("RecIndicator", typeof(RectTransform));
        container.transform.SetParent(canvasGO.transform, false);
        var containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0f, 1f);
        containerRect.anchorMax = new Vector2(0f, 1f);
        containerRect.pivot = new Vector2(0f, 1f);
        containerRect.anchoredPosition = new Vector2(20f, -20f);
        containerRect.sizeDelta = new Vector2(190f, 50f);

        // Punto rojo
        var dotGO = new GameObject("Dot", typeof(RectTransform));
        dotGO.transform.SetParent(container.transform, false);
        var dotImage = dotGO.AddComponent<Image>();
        dotImage.color = Color.red;
        var dotRect = dotGO.GetComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0f, 0.5f);
        dotRect.anchorMax = new Vector2(0f, 0.5f);
        dotRect.pivot = new Vector2(0f, 0.5f);
        dotRect.anchoredPosition = Vector2.zero;
        dotRect.sizeDelta = new Vector2(32f, 32f);

        // Texto "REC"
        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(container.transform, false);
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "REC";
        text.fontSize = 44f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        HUDTextUtility.ApplyWhiteMaterial(text);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.5f);
        textRect.anchorMax = new Vector2(0f, 0.5f);
        textRect.pivot = new Vector2(0f, 0.5f);
        textRect.anchoredPosition = new Vector2(42f, 0f);
        textRect.sizeDelta = new Vector2(150f, 50f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("RecIndicatorSetupTool: indicador REC añadido en Mini_Build_P.");
    }
}
