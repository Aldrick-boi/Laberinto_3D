using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 1. Le agrega un Collider (trigger) + SceneTrigger a "Exit" si todavía no los tiene.
// 2. Crea un punto nuevo "SecretSceneTrigger" (si no existe ya) con lo mismo, como
// entrada a una escena secreta. Ninguno de los dos trae el nombre de la escena puesto
// todavía — eso se escribe a mano en el Inspector una vez que esas escenas existan.
public static class SceneTriggerSetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";
    private const float TriggerSize = 2f;

    [MenuItem("Tools/Laberinto/Añadir Triggers de Escena en Mini_Build_P")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("SceneTriggerSetupTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        SetupExitTrigger();
        CreateSecretSceneTrigger();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("SceneTriggerSetupTool: listo. Cuando tengas las escenas creadas, ponles el nombre exacto " +
                  "en el campo \"Scene To Load\" de cada SceneTrigger (Exit y SecretSceneTrigger) y agrégalas a Build Settings.");
    }

    private static void SetupExitTrigger()
    {
        var exit = GameObject.Find("Exit");
        if (exit == null)
        {
            Debug.LogWarning("SceneTriggerSetupTool: no encontré un objeto \"Exit\" en la escena.");
            return;
        }

        var col = exit.GetComponent<BoxCollider>();
        if (col == null)
        {
            col = exit.AddComponent<BoxCollider>();
            col.size = Vector3.one * TriggerSize;
        }
        col.isTrigger = true;

        if (exit.GetComponent<SceneTrigger>() == null)
        {
            exit.AddComponent<SceneTrigger>();
        }
    }

    private static void CreateSecretSceneTrigger()
    {
        // Si ya existe, no lo toca: podrías haberlo reubicado o ya tener el nombre de escena puesto
        if (GameObject.Find("SecretSceneTrigger") != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 position = playerObj != null ? playerObj.transform.position + Vector3.forward * 2f : Vector3.zero;

        var trigger = new GameObject("SecretSceneTrigger");
        trigger.transform.position = position;

        var col = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one * TriggerSize;

        trigger.AddComponent<SceneTrigger>();
    }
}
