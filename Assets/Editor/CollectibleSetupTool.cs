using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 1. Reemplaza la esfera placeholder del prefab CollectibleItem por el modelo 3D del
//    disquete (Assets/Modelos). 2. Coloca un CollectibleItem como hijo de cada punto
//    dentro de "CollectibleSpawnPoints" en Mini_Build_P y configura el LevelManager
//    para que sepa cuántos hay que recolectar en total.
//
// Los puntos de spawn son objetos vacíos que tú mueves a mano en el editor viendo el
// mapa real — nada de heurísticas geométricas adivinando qué es "adentro" del laberinto.
// Como el coleccionable es hijo del punto, moverlo en la escena mueve al coleccionable
// con él, sin necesidad de volver a correr esta herramienta.
public static class CollectibleSetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";
    private const string PrefabPath = "Assets/PreFabs/CollectibleItem.prefab";
    private const string ModelPath = "Assets/Modelos/arunangshubanerjee-floppy-disk-2052.glb";
    private const string SpawnPointsParentName = "CollectibleSpawnPoints";
    private const int DefaultSpawnPointCount = 5;
    private const float FloatHeight = 0.45f;

    [MenuItem("Tools/Laberinto/Setup Coleccionables en Mini_Build_P")]
    public static void Run()
    {
        UpdatePrefabModel();
        ScatterInScene();
    }

    private static void UpdatePrefabModel()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            Debug.LogError($"CollectibleSetupTool: no se encontró el modelo en {ModelPath}.");
            return;
        }

        using (var editScope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
        {
            var root = editScope.prefabContentsRoot;

            // Quita el placeholder visual anterior (la esfera) o el modelo de una corrida previa
            var oldVisual = root.transform.Find("Sphere");
            if (oldVisual != null) Object.DestroyImmediate(oldVisual.gameObject);
            var oldModel = root.transform.Find("Modelo");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            modelInstance.name = "Modelo";

            // Escala el modelo para que su lado más largo mida ~0.35m, similar al
            // tamaño visual que tenía el placeholder
            Bounds bounds = CalculateBounds(modelInstance);
            float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largestDimension > 0.0001f)
            {
                float scale = 0.35f / largestDimension;
                modelInstance.transform.localScale = Vector3.one * scale;
            }
            modelInstance.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            // El collider que agarraba la esfera se fue con ella; el de la raíz que
            // queda es minúsculo (era solo un respaldo). Lo agranda a un radio de
            // agarre cómodo, sin importar la escala que tenga la raíz del prefab
            var collider = root.GetComponent<SphereCollider>();
            if (collider != null)
            {
                const float targetWorldRadius = 0.25f;
                collider.radius = targetWorldRadius / Mathf.Max(root.transform.localScale.x, 0.0001f);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("CollectibleSetupTool: modelo 3D aplicado al prefab CollectibleItem.");
    }

    private static Bounds CalculateBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static void ScatterInScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("CollectibleSetupTool: cancelado (había cambios sin guardar en la escena actual).");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"CollectibleSetupTool: no se encontró el prefab en {PrefabPath}.");
            return;
        }

        // Limpia el grupo "Collectibles" de corridas anteriores (el sistema viejo de
        // esparcido automático) para que no queden coleccionables duplicados o mal ubicados
        var oldCollectibles = GameObject.Find("Collectibles");
        if (oldCollectibles != null) Object.DestroyImmediate(oldCollectibles);

        // Los puntos de spawn NO se destruyen ni se recrean en cada corrida: si ya los
        // moviste a mano, esta herramienta debe respetar dónde los dejaste
        var spawnPointsParent = GameObject.Find(SpawnPointsParentName);
        if (spawnPointsParent == null)
        {
            spawnPointsParent = CreateDefaultSpawnPoints();
            Debug.Log($"CollectibleSetupTool: no existía \"{SpawnPointsParentName}\", se crearon " +
                      $"{DefaultSpawnPointCount} puntos de ejemplo cerca del spawn del jugador. " +
                      "Muévelos en el editor a donde quieras que aparezcan los coleccionables y vuelve a correr esto.");
        }

        int placed = 0;
        foreach (Transform point in spawnPointsParent.transform)
        {
            // Si ya tiene un coleccionable (de una corrida anterior), no lo duplica
            if (point.Find("Coleccionable") != null)
            {
                placed++;
                continue;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "Coleccionable";
            instance.transform.SetParent(point, false);
            instance.transform.localPosition = Vector3.up * FloatHeight;
            placed++;
        }

        // Asegura que exista un LevelManager en la escena para contar los coleccionables
        var levelManagerGO = GameObject.Find("LevelManager");
        if (levelManagerGO == null)
        {
            levelManagerGO = new GameObject("LevelManager");
            levelManagerGO.AddComponent<LevelManager>();
        }
        levelManagerGO.GetComponent<LevelManager>().totalRequired = placed;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"CollectibleSetupTool: {placed} coleccionables colocados en los puntos de \"{SpawnPointsParentName}\".");
    }

    // Primera vez que se corre la herramienta: crea los puntos como ejemplo alrededor
    // del spawn del jugador (o del origen si no hay Player en la escena) para que tengas
    // algo que arrastrar en vez de partir de la nada
    private static GameObject CreateDefaultSpawnPoints()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 origin = playerObj != null ? playerObj.transform.position : Vector3.zero;

        var parent = new GameObject(SpawnPointsParentName);

        for (int i = 0; i < DefaultSpawnPointCount; i++)
        {
            var point = new GameObject($"Point_{i + 1}");
            point.transform.SetParent(parent.transform);
            float angle = i * (360f / DefaultSpawnPointCount) * Mathf.Deg2Rad;
            point.transform.position = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 3f;
        }

        return parent;
    }
}
