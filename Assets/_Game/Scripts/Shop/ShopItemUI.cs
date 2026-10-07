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
        if (iconImage != null)
        {
            iconImage.sprite = skin.icon;
            iconImage.enabled = skin.icon != null;
        }
        if (nameText != null) nameText.text = skin.displayName;
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClick);
        }
        Refresh();
    }

    public void Refresh()
    {
        if (shop == null || data == null) return;
        bool unlocked = shop.IsUnlocked(data);
        string currentSelected = PlayerPrefs.GetString("SelectedSkinId", "");
        if (string.IsNullOrEmpty(currentSelected) && data.price == 0)
            currentSelected = data.skinId;

        bool selected = currentSelected == data.skinId;

        if (priceText != null) priceText.text = unlocked ? "" : data.price.ToString();
        if (actionButtonText != null)
            actionButtonText.text = !unlocked ? "Mua" : (selected ? "Đang dùng" : "Chọn");
    }

    void OnClick()
    {
        if (!shop.IsUnlocked(data)) shop.Buy(data);
        else shop.Select(data);
    }
}