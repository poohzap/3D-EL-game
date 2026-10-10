using UnityEditor;
using UnityEngine;

// Đảm bảo toàn bộ file trong Assets/_Game/Art/UI/HUD/ được import đúng dạng Sprite (Single).
// Menu: Tools/Setup HUD Sprites  — hoặc được SetupGameplayHUD gọi tự động.
public class SetupHUDSprites : EditorWindow
{
    const string HudDir = "Assets/_Game/Art/UI/HUD/";

    static readonly string[] IconNames =
    {
        "icon_star",
        "icon_coin",
        "icon_pause",
        "icon_shield",
        "icon_jump_boots",
        "icon_rocket"
    };

    [MenuItem("Tools/Setup HUD Sprites")]
    public static void Setup()
    {
        // hud_pill: thêm 9-slice border (mép trái/phải = 1/2 chiều cao)
        EnsureSprite(HudDir + "hud_pill.png", true);

        // Các icon còn lại: Sprite đơn giản
        foreach (string n in IconNames)
            EnsureSprite(HudDir + n + ".png", false);

        Debug.Log("[SetupHUDSprites] OK — 7 file trong Art/UI/HUD đã là Sprite (Single).");
    }

    static void EnsureSprite(string path, bool isPill)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("[SetupHUDSprites] Không tìm thấy asset: " + path);
            return;
        }

        bool dirty = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            dirty = true;
        }
        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            dirty = true;
        }
        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            dirty = true;
        }

        if (isPill)
        {
            // 9-slice: border trái/phải = 1/2 chiều cao texture, trên/dưới = 0
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                var border = new Vector4(tex.height * 0.5f, 0f, tex.height * 0.5f, 0f);
                if (importer.spriteBorder != border)
                {
                    importer.spriteBorder = border;
                    dirty = true;
                }
            }
        }

        if (dirty)
        {
            importer.SaveAndReimport();
            Debug.Log("[SetupHUDSprites] Đã sửa importer của: " + path);
        }
    }
}
