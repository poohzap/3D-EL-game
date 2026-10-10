#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu: GameTools / Setup Obstacle Prefabs
/// Tạo 3 prefab obstacle từ Art/test, gắn tag + trigger collider + ObstacleData,
/// rồi đăng ký vào TrackManager trên scene Gameplay.
/// </summary>
public static class ObstaclePrefabSetup
{
    const string PrefabDir = "Assets/_Game/Prefabs/Obstacles";
    const string ScenePath = "Assets/_Game/Scenes/Gameplay.unity";

    [MenuItem("GameTools/Setup Obstacle Prefabs")]
    public static void SetupAll()
    {
        if (!AssetDatabase.IsValidFolder(PrefabDir))
            AssetDatabase.CreateFolder("Assets/_Game/Prefabs", "Obstacles");

        GameObject barrier = SaveObstaclePrefab(
            "Assets/_Game/Art/test/Barrier_Single.fbx",
            PrefabDir + "/Obstacle_BarrierSingle.prefab",
            ObstacleBehavior.JumpOnly,
            new Vector3(2.6f, 0.8f, 0.5f),
            new Vector3(0f, 0.4f, 0f));

        GameObject traffic = SaveObstaclePrefab(
            "Assets/_Game/Art/test/TrafficBarrier_2.fbx",
            PrefabDir + "/Obstacle_TrafficBarrier2.prefab",
            ObstacleBehavior.JumpOrSlide,
            new Vector3(2.6f, 0.55f, 0.5f),
            new Vector3(0f, 0.275f, 0f));

        GameObject container = SaveObstaclePrefab(
            "Assets/_Game/Art/test/Shipping Container.fbx",
            PrefabDir + "/Obstacle_ShippingContainer.prefab",
            ObstacleBehavior.RequiresJumpBoots,
            new Vector3(2.6f, 2.3f, 1.2f),
            new Vector3(0f, 1.15f, 0f));

        WireTrackManager(barrier, traffic, container);
        VerifyPickups();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ObstaclePrefabSetup] Hoàn tất setup obstacle prefabs + TrackManager.");
    }

    static GameObject SaveObstaclePrefab(
        string modelPath,
        string prefabPath,
        ObstacleBehavior behavior,
        Vector3 colliderSize,
        Vector3 colliderCenter)
    {
        GameObject root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(prefabPath));
        root.tag = "Obstacle";

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = colliderSize;
        box.center = colliderCenter;

        ObstacleData data = root.AddComponent<ObstacleData>();
        data.behavior = behavior;

        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = model.name;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
        }
        else
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(fallback.GetComponent<BoxCollider>());
            fallback.name = "FallbackVisual";
            fallback.transform.SetParent(root.transform, false);
            fallback.transform.localPosition = colliderCenter;
            fallback.transform.localScale = colliderSize;
            Debug.LogWarning("[ObstaclePrefabSetup] Không load được model: " + modelPath + " — dùng cube tạm.");
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("[ObstaclePrefabSetup] Prefab: " + prefabPath + " behavior=" + behavior);
        return prefab;
    }

    static void WireTrackManager(GameObject barrier, GameObject traffic, GameObject container)
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        TrackManager track = Object.FindAnyObjectByType<TrackManager>();
        if (track == null)
        {
            Debug.LogError("[ObstaclePrefabSetup] Không tìm thấy TrackManager trong Gameplay.unity");
            return;
        }

        track.obstaclePrefabs = new[] { barrier, traffic, container };

        if (track.powerUpPrefabs == null || track.powerUpPrefabs.Length == 0)
        {
            track.powerUpPrefabs = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/PowerUps/PowerUp_Shield.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/PowerUps/PowerUp_JumpBoots.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/PowerUps/PowerUp_Rocket.prefab"),
            };
        }

        EditorUtility.SetDirty(track);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void VerifyPickups()
    {
        VerifyPowerUp("Assets/_Game/Prefabs/PowerUps/PowerUp_Shield.prefab", PowerUpType.Shield);
        VerifyPowerUp("Assets/_Game/Prefabs/PowerUps/PowerUp_JumpBoots.prefab", PowerUpType.JumpBoots);
        VerifyPowerUp("Assets/_Game/Prefabs/PowerUps/PowerUp_Rocket.prefab", PowerUpType.Rocket);

        GameObject coin = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Coins/Coin.prefab");
        if (coin == null || coin.tag != "Coin")
            Debug.LogError("[ObstaclePrefabSetup] Coin.prefab thiếu hoặc sai tag Coin.");
        else
        {
            Collider col = coin.GetComponent<Collider>();
            if (col == null || !col.isTrigger)
                Debug.LogError("[ObstaclePrefabSetup] Coin.prefab cần collider Is Trigger.");
            else
                Debug.Log("[ObstaclePrefabSetup] Coin.prefab OK (tag Coin, trigger).");
        }
    }

    static void VerifyPowerUp(string path, PowerUpType expected)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null)
        {
            Debug.LogError("[ObstaclePrefabSetup] Thiếu prefab: " + path);
            return;
        }

        if (go.tag != "PowerUp")
            Debug.LogError("[ObstaclePrefabSetup] " + path + " sai tag (cần PowerUp).");

        PowerUpPickup pickup = go.GetComponent<PowerUpPickup>();
        if (pickup == null)
            Debug.LogError("[ObstaclePrefabSetup] " + path + " thiếu PowerUpPickup.");
        else if (pickup.type != expected)
            Debug.LogError("[ObstaclePrefabSetup] " + path + " type=" + pickup.type + " (cần " + expected + ").");
        else
            Debug.Log("[ObstaclePrefabSetup] " + System.IO.Path.GetFileName(path) + " OK → " + expected);

        Collider col = go.GetComponent<Collider>();
        if (col == null || !col.isTrigger)
            Debug.LogError("[ObstaclePrefabSetup] " + path + " cần collider Is Trigger.");
    }
}
#endif
