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

        // Limpia una ejecución previa de esta herramienta para no duplicar objetos,
        // pero conserva dónde estaba parado el enemigo si ya lo habías reubicado a mano
        var previousNav = GameObject.Find("Navigation");
        if (previousNav != null) Object.DestroyImmediate(previousNav);

        var previousEnemy = GameObject.Find("Enemy");
        Vector3? previousEnemyPosition = previousEnemy != null ? previousEnemy.transform.position : (Vector3?)null;
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

        // 2. Construir el GameObject del enemigo: la raíz lleva el collider, el NavMeshAgent
        // y los scripts; la parte visual (un plano, no una cápsula, para no distorsionar la
        // imagen) vive en un hijo separado para poder escalarla sin afectar al collider.
        var enemy = new GameObject("Enemy");

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
        agent.updateRotation = false; // la rotación la controla EnemyBillboard, no el agente

        enemy.AddComponent<EnemyBillboard>();

        // Usa el material personalizado del enemigo si ya existe, en vez del Lit por defecto
        var enemyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Mats/Enemy.mat");

        var visual = new GameObject("Visual");
        visual.transform.SetParent(enemy.transform, false);
        // El NavMeshAgent ya sube la raíz "baseOffset" (0.9) sobre el suelo real,
        // así que el plano se centra en (0,0,0) local: si se vuelve a subir aquí,
        // queda flotando el doble de alto sobre el piso
        visual.transform.localPosition = Vector3.zero;

        var tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.AddComponent<MeshFilter>().sharedMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
        var visualRenderer = visual.AddComponent<MeshRenderer>();
        Object.DestroyImmediate(tempQuad);

        // Escala el plano según el aspecto real de la imagen para que no se vea estirada
        float visualHeight = 2.6f; // ocupa casi todo el pasillo, de piso a techo
        float visualWidth = visualHeight;
        if (enemyMaterial != null)
        {
            visualRenderer.sharedMaterial = enemyMaterial;
            if (enemyMaterial.mainTexture != null)
            {
                var tex = enemyMaterial.mainTexture;
                visualWidth = visualHeight * (tex.width / (float)tex.height);
            }
        }
        visual.transform.localScale = new Vector3(visualWidth, visualHeight, 1f);

        // Cara + música: cada imagen de Assets/Media (ya nombrada por persona) se
        // empareja a mano con su pista en Assets/Audios. Al arrancar la partida,
        // EnemyRandomFace elige un par al azar. Si algún nombre no está en esta lista
        // (o su pista todavía no existe), se queda sin música y se avisa por consola.
        var randomFace = visual.AddComponent<EnemyRandomFace>();

        var namedPairs = new (string image, string music)[]
        {
            ("Paulo.jpg", "Farsante.mp3"),
            ("Nadia.jpg", "Te estoy correteando.mp3"),
            ("Moge.jpg", "No Hago Trap.mp3"),
            ("Emi.JPG", "Floral Fury.mp3"),
            ("Beta.JPG", "Ninjago.mp3"),
            ("Amezcua.jpg", "Maximo Samar.mp3"),
        };

        string[] mediaImageGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Media" });
        var faceOptions = new EnemyRandomFace.FaceAudioPair[mediaImageGuids.Length];
        for (int i = 0; i < mediaImageGuids.Length; i++)
        {
            string imagePath = AssetDatabase.GUIDToAssetPath(mediaImageGuids[i]);
            string imageFileName = Path.GetFileName(imagePath);

            string musicFileName = null;
            foreach (var pair in namedPairs)
            {
                if (pair.image == imageFileName) { musicFileName = pair.music; break; }
            }

            AudioClip music = null;
            if (imageFileName == "Shigue.jpg")
            {
                // TEKNOCITY...mp3: nombre de archivo con acentos/corchetes poco confiable
                // para comparar como texto, así que se referencia por GUID directo
                const string teknocityGuid = "0ee811475f0ce4121b471a479ca7aed0";
                string teknocityPath = AssetDatabase.GUIDToAssetPath(teknocityGuid);
                music = AssetDatabase.LoadAssetAtPath<AudioClip>(teknocityPath);
            }
            else if (musicFileName != null)
            {
                string musicPath = "Assets/Audios/" + musicFileName;
                music = AssetDatabase.LoadAssetAtPath<AudioClip>(musicPath);
                if (music == null) Debug.LogWarning($"EnemySetupTool: no se encontró \"{musicPath}\" para {imageFileName}.");
            }
            else
            {
                Debug.LogWarning($"EnemySetupTool: \"{imageFileName}\" no tiene música asignada todavía; se queda sin sonido de proximidad.");
            }

            faceOptions[i] = new EnemyRandomFace.FaceAudioPair
            {
                face = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath),
                music = music
            };
        }
        randomFace.faceOptions = faceOptions;

        var audioSource = enemy.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;
        audioSource.dopplerLevel = 0f; // evita que el movimiento del enemigo le cambie el tono al audio

        enemy.AddComponent<EnemyAI>();
        enemy.AddComponent<EnemyCatchPlayer>();
        // RequireComponent en EnemyProximityAudio agrega automáticamente el AudioDistortionFilter
        var proximityAudio = enemy.AddComponent<EnemyProximityAudio>();
        enemy.GetComponent<AudioDistortionFilter>().distortionLevel = 0f;

        // Clip de proximidad del enemigo (TEKNOCITY...mp3), ubicado por GUID en vez de por
        // carpeta: sigue funcionando aunque el archivo se mueva o haya más audios alrededor
        const string proximityClipGuid = "0ee811475f0ce4121b471a479ca7aed0";
        string proximityClipPath = AssetDatabase.GUIDToAssetPath(proximityClipGuid);
        if (!string.IsNullOrEmpty(proximityClipPath))
        {
            proximityAudio.proximityClip = AssetDatabase.LoadAssetAtPath<AudioClip>(proximityClipPath);
        }

        // 3. Ubicarlo sobre el NavMesh: reusa la posición anterior si ya lo habías movido,
        // si no, lo deja lejos de la esquina donde suele arrancar el jugador
        var desiredSpawn = previousEnemyPosition ?? new Vector3(-46f, 1f, 10f);
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
