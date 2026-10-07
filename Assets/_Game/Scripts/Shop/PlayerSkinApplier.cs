// PlayerSkinApplier.cs — Assets/_Game/Scripts/Shop/PlayerSkinApplier.cs
using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    public SkinDatabase database;  // kéo ĐÚNG asset SkinDatabase như ShopManager (Mục 8.2)
    public Transform modelParent;  // vị trí gắn model, là con của Player

    void Awake()
    {
        ApplySelectedSkin();
    }

    public void ApplySelectedSkin()
    {
        if (database == null || database.allSkins == null || database.allSkins.Length == 0) return;
        Transform targetParent = modelParent != null ? modelParent : transform;

        // Clear existing model children
        for (int i = targetParent.childCount - 1; i >= 0; i--)
        {
            Destroy(targetParent.GetChild(i).gameObject);
        }

        string selectedId = PlayerPrefs.GetString("SelectedSkinId", "");
        SkinData skin = System.Array.Find(database.allSkins, s => s != null && s.skinId == selectedId);
        if (skin == null) skin = database.allSkins[0]; // fallback: default skin

        if (skin != null && skin.modelPrefab != null)
        {
            Instantiate(skin.modelPrefab, targetParent);
        }
    }
}