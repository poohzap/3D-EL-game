// ShopItemUI.cs — Assets/_Game/Scripts/Shop/ShopItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button actionButton;
    public TMP_Text actionButtonText;

    private SkinData data;
    private ShopManager shop;

    public void Setup(SkinData skin, ShopManager manager)
    {
        data = skin;
        shop = manager;
        iconImage.sprite = skin.icon;
        nameText.text = skin.displayName;
        actionButton.onClick.AddListener(OnClick);
        Refresh();
    }

    void Refresh()
    {
        bool unlocked = shop.IsUnlocked(data);
        bool selected = PlayerPrefs.GetString("SelectedSkinId", "") == data.skinId;

        priceText.text = unlocked ? "" : data.price.ToString();
        actionButtonText.text = !unlocked ? "Mua" : (selected ? "Đang dùng" : "Chọn");
    }

    void OnClick()
    {
        if (!shop.IsUnlocked(data)) shop.Buy(data);
        else shop.Select(data);
        Refresh();
    }
}