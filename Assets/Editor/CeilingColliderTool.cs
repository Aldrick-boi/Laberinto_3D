using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Agrega un BoxCollider (ajustado automáticamente a la malla) a cualquier pieza de la
// geometría del laberinto que todavía no tenga collider, en la escena Mini_Build_P.
// No se limita a lo que se llama "Ceiling": el laberinto viene con nombres genéricos
// (Object_12, Object_19, etc.) y filtrar solo por nombre dejaba huecos sin cubrir.
// Se puede correr varias veces sin duplicar nada: si una pieza ya tiene collider, se
// deja como está. Ignora Player, Enemy y AmbientAudio, que no son parte del laberinto.
public static class CeilingColliderTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";

    private static readonly string[] ExcludedRootNames = { "Player", "Enemy", "AmbientAudio", "Navigation" };

    [MenuItem("Tools/Laberinto/Añadir Colliders Faltantes al Laberinto")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("CeilingColliderTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        int added = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (System.Array.IndexOf(ExcludedRootNames, root.name) >= 0) continue;

            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var go = renderer.gameObject;
                if (go.GetComponent<Collider>() != null) continue;

                go.AddComponent<BoxCollider>();
                added++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"CeilingColliderTool: se agregaron {added} colliders faltantes en el laberinto.");
    }
}
