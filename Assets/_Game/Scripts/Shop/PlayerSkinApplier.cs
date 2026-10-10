// PlayerSkinApplier.cs — Assets/_Game/Scripts/Shop/PlayerSkinApplier.cs
using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    public SkinDatabase database;  // kéo ĐÚNG asset SkinDatabase như ShopManager
    public Transform modelParent;  // vị trí gắn model, là con của Player (ModelSlot)

    [Header("Tỉ lệ thu nhỏ nhân vật")]
    [Tooltip("Tỉ lệ thu nhỏ model nhân vật (0.75 = bé hơn để vừa vặn với làn đường)")]
    public float modelScale = 0.75f;

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
            GameObject model = Instantiate(skin.modelPrefab, targetParent);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * modelScale;
        }

        // Cũng thu nhỏ collider tương ứng nếu có CharacterController
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.height = 1.6f;
            cc.radius = 0.38f;
            cc.center = new Vector3(0f, 0.8f, 0f);
        }
    }
}