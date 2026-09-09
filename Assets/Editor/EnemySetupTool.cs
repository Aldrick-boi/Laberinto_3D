using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

// Herramienta para bakear el NavMesh y colocar el prefab del Enemy en la escena
// Mini_Build_P. Vuelve a ejecutarse (Tools > Laberinto) cada vez que se modifique
// el laberinto y haga falta re-bakear la navegación.
public static class EnemySetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";

    [MenuItem("Tools/Laberinto/Setup Enemy en Mini_Build_P")]
    public static void Run()
    {
        // Evita perder cambios sin guardar de la escena actualmente abierta
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("EnemySetupTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Limpia una ejecución previa de esta herramienta para no duplicar objetos
        var previousNav = GameObject.Find("Navigation");
        if (previousNav != null) Object.DestroyImmediate(previousNav);
        var previousEnemy = GameObject.Find("Enemy");
        if (previousEnemy != null) Object.DestroyImmediate(previousEnemy);

        // 1. Bakear el NavMesh sobre toda la geometría del laberinto
        var navGO = new GameObject("Navigation");
        var surface = navGO.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();

        string navDataDir = "Assets/Scenes/Mini_Build_P";
        if (!Directory.Exists(navDataDir)) Directory.CreateDirectory(navDataDir);
        string navAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(navDataDir, "NavMesh-Navigation.asset"));
        AssetDatabase.CreateAsset(surface.navMeshData, navAssetPath);

        // 2. Construir el GameObject del enemigo
        var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());

        var col = enemy.AddComponent<CapsuleCollider>();
        col.radius = 0.4f;
        col.height = 1.8f;

        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.radius = 0.4f;
        agent.height = 1.8f;
        agent.baseOffset = 0.9f;
        agent.speed = 4.5f;
        agent.acceleration = 999f; // sin rampa: velocidad constante desde que arranca
        agent.stoppingDistance = 0.6f;

        var audioSource = enemy.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;

        enemy.AddComponent<EnemyAI>();
        enemy.AddComponent<EnemyProximityAudio>();

        // 3. Ubicarlo sobre el NavMesh, lejos de la esquina donde suele arrancar el jugador
        var desiredSpawn = new Vector3(-46f, 1f, 10f);
        if (!NavMesh.SamplePosition(desiredSpawn, out var hit, 15f, NavMesh.AllAreas))
        {
            NavMesh.SamplePosition(navGO.transform.position, out hit, 1000f, NavMesh.AllAreas);
        }
        enemy.transform.position = hit.position;

        // 4. Guardarlo como prefab y dejar la instancia de la escena conectada a él
        const string prefabPath = "Assets/PreFabs/Enemy.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(enemy, prefabPath, InteractionMode.AutomatedAction);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("EnemySetupTool: NavMesh bakeado y Enemy colocado en Mini_Build_P correctamente.");
    }
}
