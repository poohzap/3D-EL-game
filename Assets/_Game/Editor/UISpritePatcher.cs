// UISpritePatcher.cs  —  Editor Only
// Menu: GameTools / Patch UI Sprites
// hoặc chạy batchmode: -executeMethod UISpritePatcher.PatchAll
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class UISpritePatcher
{
    // ─── GUIDs of UI sprites (khớp với .meta files) ─────────────────────────
    const string GUID_COIN    = "8df5b71aa7f9326c9b04c13f4a2f1250";
    const string GUID_SHIELD  = "5cc3892f3ccc743c8771b011ef9329d5";
    const string GUID_MAGNET  = "e79abcc60421b72a2bb439ec8ea64f11";
    const string GUID_ROCKET  = "4172c5d7ae3b2a30c4f1661318a5bc76";

    const string GUID_BTN_PLAY    = "7eb84cbb7ac8ab44caf9b4bef1ff3cca";
    const string GUID_BTN_SHOP    = "585931ac07f95e26eee813ce54a94421";
    const string GUID_BTN_RESTART = "d89a4e73cec0853aa1ec78df2a81595d";
    const string GUID_BTN_MENU    = "175d942e9d044af905bb9ca4bf888f71";
    const string GUID_BTN_CLOSE   = "80a244f98e228a3f6444669573ac0ac3";
    const string GUID_BTN_BUY     = "83f4ba713cf380a082c2d0971a46531b";
    const string GUID_BTN_SELECT  = "68c5639b84e2bf0e196581d1ac7ddd98";
    const string GUID_BTN_USING   = "cd459a9bf0afe02cf555b49601f8959d";

    // ─── Entry point ─────────────────────────────────────────────────────────
    [MenuItem("GameTools/Patch UI Sprites")]
    public static void PatchAll()
    {
        PatchMainMenu();
        PatchGameplay();
        Debug.Log("[UISpritePatcher] DONE — Tất cả sprites đã được gán.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    static Sprite LoadSprite(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"[UISpritePatcher] Không tìm thấy asset với GUID {guid}");
            return null;
        }
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp == null) Debug.LogWarning($"[UISpritePatcher] Không load được Sprite từ {path}");
        return sp;
    }

    static void SetButtonSprite(GameObject go, string objectName, string guid)
    {
        var t = go.transform.Find(objectName);
        if (t == null) { Debug.LogWarning($"[UISpritePatcher] Không tìm thấy '{objectName}'"); return; }
        var img = t.GetComponent<Image>();
        if (img == null) img = t.GetComponentInChildren<Image>();
        if (img == null) { Debug.LogWarning($"[UISpritePatcher] '{objectName}' không có Image"); return; }
        var sp = LoadSprite(guid);
        if (sp != null) { img.sprite = sp; img.preserveAspect = true; EditorUtility.SetDirty(img); }
    }

    static void SetImageSprite(Image img, string guid)
    {
        if (img == null) return;
        var sp = LoadSprite(guid);
        if (sp != null) { img.sprite = sp; img.preserveAspect = true; EditorUtility.SetDirty(img); }
    }

    // ─── MainMenu scene ───────────────────────────────────────────────────────
    static void PatchMainMenu()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity", OpenSceneMode.Single);

        // Patch buttons on MainMenuUI canvas
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            // Play & Shop buttons — look by name anywhere in hierarchy
            PatchButtonImageByName(root, "PlayButton",  GUID_BTN_PLAY);
            PatchButtonImageByName(root, "ShopButton",  GUID_BTN_SHOP);

            // Coin icon in HUD (if any Image named "CoinIcon")
            PatchImageByName(root, "CoinIcon", GUID_COIN);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[UISpritePatcher] MainMenu patched.");
    }

    // ─── Gameplay scene ───────────────────────────────────────────────────────
    static void PatchGameplay()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/Gameplay.unity", OpenSceneMode.Single);

        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            // Buttons
            PatchButtonImageByName(root, "RestartButton", GUID_BTN_RESTART);
            PatchButtonImageByName(root, "MenuButton",    GUID_BTN_MENU);

            // HUD coin icon
            PatchImageByName(root, "CoinIcon", GUID_COIN);

            // Wire GameplayUI icon sprite fields
            var gui = root.GetComponentInChildren<GameplayUI>(true);
            if (gui != null)
            {
                gui.iconShield = LoadSprite(GUID_SHIELD);
                gui.iconMagnet = LoadSprite(GUID_MAGNET);
                gui.iconRocket = LoadSprite(GUID_ROCKET);

                // Wire coinIcon Image (first Image named CoinIcon)
                gui.coinIcon = FindImageByName(root, "CoinIcon");

                // Wire powerUpIcon Image
                gui.powerUpIcon = FindImageByName(root, "PowerUpIcon");

                EditorUtility.SetDirty(gui);
                Debug.Log("[UISpritePatcher] GameplayUI sprite fields wired.");
            }
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[UISpritePatcher] Gameplay patched.");
    }

    // ─── Recursive search helpers ─────────────────────────────────────────────
    static void PatchButtonImageByName(GameObject root, string name, string guid)
    {
        var t = FindTransformByName(root.transform, name);
        if (t == null) { Debug.LogWarning($"[UISpritePatcher] '{name}' not found under {root.name}"); return; }
        var img = t.GetComponent<Image>();
        if (img == null) img = t.GetComponentInChildren<Image>();
        if (img == null) return;
        SetImageSprite(img, guid);
    }

    static void PatchImageByName(GameObject root, string name, string guid)
    {
        var img = FindImageByName(root, name);
        SetImageSprite(img, guid);
    }

    static Image FindImageByName(GameObject root, string name)
    {
        var t = FindTransformByName(root.transform, name);
        if (t == null) return null;
        return t.GetComponent<Image>();
    }

    static Transform FindTransformByName(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var found = FindTransformByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
