using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// 1. Reemplaza la esfera placeholder del prefab CollectibleItem por el modelo 3D del
//    disquete (Assets/Modelos). 2. Esparce varias copias de ese prefab en puntos
//    repartidos del laberinto en Mini_Build_P y configura el LevelManager para que
//    sepa cuántos hay que recolectar en total.
public static class CollectibleSetupTool
{
    private const string ScenePath = "Assets/Scenes/Mini_Build_P.unity";
    private const string PrefabPath = "Assets/PreFabs/CollectibleItem.prefab";
    private const string ModelPath = "Assets/Modelos/arunangshubanerjee-floppy-disk-2052.glb";
    private const int CollectibleCount = 5;
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

        // Limpia coleccionables puestos en una corrida anterior de esta herramienta
        var existingParent = GameObject.Find("Collectibles");
        if (existingParent != null) Object.DestroyImmediate(existingParent);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"CollectibleSetupTool: no se encontró el prefab en {PrefabPath}.");
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Rect wallFootprint = ComputeWallFootprint(scene);
        Vector3[] candidates = GetValidPoints(wallFootprint, playerObj != null ? playerObj.transform.position : (Vector3?)null);
        List<Vector3> chosenPoints = PickFarthestPoints(candidates, CollectibleCount);

        var parent = new GameObject("Collectibles");

        int placed = 0;
        foreach (var point in chosenPoints)
        {
            if (!NavMesh.SamplePosition(point, out var hit, 2f, NavMesh.AllAreas)) continue;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetParent(parent.transform);
            instance.transform.position = hit.position + Vector3.up * FloatHeight;
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

        Debug.Log($"CollectibleSetupTool: se colocaron {placed} coleccionables en el laberinto.");
    }

    private static readonly string[] NonMazeRootNames = { "Player", "Enemy", "AmbientAudio", "Navigation", "Collectibles", "LevelManager" };

    // Las heurísticas por rayos/pathfinding no bastaron: Mini_Build_P es una construcción
    // parcial y el piso "de afuera" resultaba tan caminable y techado como el de adentro.
    // En vez de adivinar, se calcula directamente el rectángulo (en XZ) que ocupan las
    // paredes reales: cualquier collider "alto" (no un piso/techo plano) cuenta como pared,
    // y se toma la caja que las envuelve a todas. Esto puede recortar de más si el laberinto
    // no es rectangular, pero es mucho más confiable que intentar distinguir "adentro" con
    // rayos sueltos.
    private static Rect ComputeWallFootprint(Scene scene)
    {
        const float minWallHeight = 1f;
        Bounds? footprint = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (System.Array.IndexOf(NonMazeRootNames, root.name) >= 0) continue;

            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                if (col.bounds.size.y < minWallHeight) continue; // descarta piso/techo (planos)

                if (footprint == null)
                {
                    footprint = new Bounds(col.bounds.center, col.bounds.size);
                }
                else
                {
                    var b = footprint.Value;
                    b.Encapsulate(col.bounds);
                    footprint = b;
                }
            }
        }

        if (footprint == null) return new Rect(-10000f, -10000f, 20000f, 20000f);

        Bounds bounds = footprint.Value;
        return new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
    }

    private static Vector3[] GetValidPoints(Rect wallFootprint, Vector3? referencePoint)
    {
        var triangulation = NavMesh.CalculateTriangulation();
        var valid = new List<Vector3>();
        var path = new NavMeshPath();

        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            Vector3 a = triangulation.vertices[triangulation.indices[i]];
            Vector3 b = triangulation.vertices[triangulation.indices[i + 1]];
            Vector3 c = triangulation.vertices[triangulation.indices[i + 2]];
            Vector3 centroid = (a + b + c) / 3f;

            if (!wallFootprint.Contains(new Vector2(centroid.x, centroid.z))) continue;

            if (referencePoint.HasValue)
            {
                bool reachable = NavMesh.CalculatePath(referencePoint.Value, centroid, NavMesh.AllAreas, path)
                                  && path.status == NavMeshPathStatus.PathComplete;
                if (!reachable) continue;
            }

            valid.Add(centroid);
        }

        Debug.Log($"CollectibleSetupTool: {valid.Count} de {triangulation.indices.Length / 3} triángulos quedaron dentro del rectángulo de paredes.");

        // Si por algún motivo nada resultó válido, mejor usar los vértices sin
        // filtrar que no colocar ningún coleccionable
        return valid.Count > 0 ? valid.ToArray() : triangulation.vertices;
    }

    // Muestreo por "punto más lejano": arranca de un vértice al azar y en cada paso
    // agrega el candidato cuya distancia al más cercano de los ya elegidos sea la
    // mayor posible. A diferencia de un simple mínimo de separación, esto reparte
    // los puntos activamente hacia los extremos del laberinto en vez de solo evitar
    // que queden pegados.
    private static List<Vector3> PickFarthestPoints(Vector3[] candidates, int count)
    {
        var chosen = new List<Vector3>();
        if (candidates.Length == 0) return chosen;

        chosen.Add(candidates[Random.Range(0, candidates.Length)]);

        while (chosen.Count < count && chosen.Count < candidates.Length)
        {
            Vector3 best = candidates[0];
            float bestDistance = -1f;

            foreach (var candidate in candidates)
            {
                float nearestChosenDistance = float.MaxValue;
                foreach (var picked in chosen)
                {
                    float d = Vector3.Distance(candidate, picked);
                    if (d < nearestChosenDistance) nearestChosenDistance = d;
                }

                if (nearestChosenDistance > bestDistance)
                {
                    bestDistance = nearestChosenDistance;
                    best = candidate;
                }
            }

            chosen.Add(best);
        }

        return chosen;
    }
}
