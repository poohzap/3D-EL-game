// SceneryBuilder.cs — Assets/_Game/Editor/SceneryBuilder.cs
// Loại: static class Editor (KHÔNG gắn vào GameObject) — chỉ tạo menu Tools/Endless Runner/Scenery/...
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneryBuilder
{
    const string MaterialsFolder = "Assets/_Game/Materials";
    const string SceneryPrefabFolder = "Assets/_Game/Prefabs/Scenery";
    const string CloudPrefabPath = SceneryPrefabFolder + "/Cloud.prefab";
    const float RoadHalfWidth = 4.5f; // tile rộng 9 (3 làn × 3)
    const float GrassWidth = 60f;     // bề ngang dải cỏ mỗi bên

    // ---------- 1. Thêm cỏ + TrackScenery vào prefab TrackTile đang chọn ----------
    [MenuItem("Tools/Endless Runner/Scenery/1. Thêm cỏ + TrackScenery vào TrackTile đang chọn")]
    static void SetupTrackTile()
    {
        GameObject asset = Selection.activeObject as GameObject;
        if (asset == null || !EditorUtility.IsPersistent(asset))
        {
            Debug.LogError("Hãy chọn prefab TrackTile_Straight trong cửa sổ Project trước khi bấm menu này.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(asset);
        GameObject root = PrefabUtility.LoadPrefabContents(path);

        if (root.GetComponent<TrackScenery>() != null)
        {
            Debug.LogWarning("Prefab này đã có TrackScenery, bỏ qua.");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        // Tile gốc là Plane bị scale (0.9, 1, 2) nên mọi object con sẽ bị méo theo.
        // Object "Scenery" scale ngược lại để các con bên trong có kích thước thật.
        Transform scenery = new GameObject("Scenery").transform;
        scenery.SetParent(root.transform, false);
        Vector3 s = root.transform.localScale;
        scenery.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);

        Transform props = new GameObject("Props").transform;
        props.SetParent(scenery, false);

        Material grass = GetOrCreateMaterial(MaterialsFolder + "/Mat_Grass.mat", new Color(0.35f, 0.6f, 0.25f));
        foreach (int side in new[] { -1, 1 })
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
            g.name = side < 0 ? "Grass_L" : "Grass_R";
            Object.DestroyImmediate(g.GetComponent<Collider>()); // cỏ chỉ để nhìn, không va chạm
            g.GetComponent<Renderer>().sharedMaterial = grass;
            g.transform.SetParent(scenery, false);
            g.transform.localPosition = new Vector3(side * (RoadHalfWidth + GrassWidth / 2f), 0f, 0f);
            g.transform.localScale = new Vector3(GrassWidth / 10f, 1f, 2f); // Plane gốc 10×10 → ra GrassWidth × 20
        }

        root.AddComponent<TrackScenery>().props = props;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Đã thêm cỏ + TrackScenery vào " + path);
    }

    // ---------- 2. Tạo prefab mây (4 quả cầu dẹt gộp lại) ----------
    [MenuItem("Tools/Endless Runner/Scenery/2. Tạo prefab mây")]
    static void CreateCloudPrefab()
    {
        Material white = GetOrCreateMaterial(MaterialsFolder + "/Mat_Cloud.mat", Color.white);
        GameObject cloud = new GameObject("Cloud");

        // x, y, z, đường kính của từng cục mây
        Vector4[] blobs =
        {
            new Vector4(0f, 0f, 0f, 3f),
            new Vector4(1.8f, -0.3f, 0.3f, 2.2f),
            new Vector4(-1.8f, -0.3f, -0.2f, 2.4f),
            new Vector4(0.6f, 0.6f, 0f, 2f),
        };

        foreach (Vector4 b in blobs)
        {
            GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(blob.GetComponent<Collider>());
            Renderer r = blob.GetComponent<Renderer>();
            r.sharedMaterial = white;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            blob.transform.SetParent(cloud.transform, false);
            blob.transform.localPosition = new Vector3(b.x, b.y, b.z);
            blob.transform.localScale = new Vector3(b.w, b.w * 0.6f, b.w);
        }

        if (!AssetDatabase.IsValidFolder(SceneryPrefabFolder))
            AssetDatabase.CreateFolder("Assets/_Game/Prefabs", "Scenery");

        PrefabUtility.SaveAsPrefabAsset(cloud, CloudPrefabPath);
        Object.DestroyImmediate(cloud);
        Debug.Log("Đã tạo " + CloudPrefabPath);
    }

    // ---------- 3. Gắn DayNightCycle + tạo SkyBackdrop trong scene Gameplay đang mở ----------
    [MenuItem("Tools/Endless Runner/Scenery/3. Thêm DayNight + SkyBackdrop vào scene đang mở")]
    static void SetupGameplayScene()
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        GameObject cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CloudPrefabPath);
        if (player == null || cloudPrefab == null)
        {
            Debug.LogError("Cần mở scene Gameplay (có Player) và đã chạy bước 2 (tạo prefab mây) trước.");
            return;
        }

        Light sun = null;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) { sun = l; break; }

        if (sun == null)
        {
            Debug.LogError("Scene không có Directional Light.");
            return;
        }

        if (sun.GetComponent<DayNightCycle>() == null)
            Undo.AddComponent<DayNightCycle>(sun.gameObject);

        if (Object.FindFirstObjectByType<SkyBackdrop>() == null)
        {
            GameObject go = new GameObject("SkyBackdrop");
            Undo.RegisterCreatedObjectUndo(go, "Create SkyBackdrop");
            SkyBackdrop backdrop = go.AddComponent<SkyBackdrop>();
            backdrop.player = player.transform;
            backdrop.cloudPrefabs = new[] { cloudPrefab };
            backdrop.mountainPrefabs = new GameObject[0];
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Đã thêm DayNightCycle và SkyBackdrop. Nhớ kéo model núi vào SkyBackdrop > Mountain Prefabs.");
    }

    // ---------- Helper ----------
    static Material GetOrCreateMaterial(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
