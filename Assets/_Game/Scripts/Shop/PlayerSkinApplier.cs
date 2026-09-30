// PlayerSkinApplier.cs — Assets/_Game/Scripts/Shop/PlayerSkinApplier.cs
using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    public SkinDatabase database;  // kéo ĐÚNG asset SkinDatabase như ShopManager (Mục 8.2)
    public Transform modelParent;  // vị trí gắn model, là con của Player

    void Start()
    {
        string selectedId = PlayerPrefs.GetString("SelectedSkinId", "");
        SkinData skin = System.Array.Find(database.allSkins, s => s.skinId == selectedId);
        if (skin == null && database.allSkins.Length > 0) skin = database.allSkins[0]; // fallback: skin đầu tiên

        if (skin != null && skin.modelPrefab != null)
            Instantiate(skin.modelPrefab, modelParent);
    }
}