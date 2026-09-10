using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Agrega un crosshair (un punto fijo en el centro de la pantalla) a Mini_Build_P.
// Como la cámara siempre mira hacia el centro de la pantalla, un punto ahí ya
// coincide con hacia dónde apunta el raycast de PlayerInteraction — no hace falta
// calcular nada dinámicamente.
public static class CrosshairSetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";
    private const float DotSize = 14f;

    [MenuItem("Tools/Laberinto/Añadir Crosshair en Mini_Build_P")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("CrosshairSetupTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var canvasGO = GameObject.Find("HUD Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("HUD Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Sin EventSystem, ningún elemento de UI (ni este ni futuros) recibe input
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // Limpia una corrida anterior para no duplicar
        var existingCrosshair = canvasGO.transform.Find("Crosshair");
        if (existingCrosshair != null) Object.DestroyImmediate(existingCrosshair.gameObject);

        var crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvasGO.transform, false);
        var image = crosshairGO.AddComponent<Image>();
        image.color = Color.white;

        var rect = crosshairGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(DotSize, DotSize);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("CrosshairSetupTool: crosshair añadido en Mini_Build_P.");
    }
}
