// ShopManager.cs — Assets/_Game/Scripts/Shop/ShopManager.cs
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public SkinDatabase database; // kéo asset SkinDatabase (Mục 8.2) vào đây
    public Transform shopListContent; // Content của ScrollView
    public ShopItemUI shopItemPrefab;
    public TMP_Text coinBalanceText;

    void Start()
    {
        RefreshCoinText();
        BuildList();
    }

    void BuildList()
    {
        foreach (SkinData skin in database.allSkins)
        {
            ShopItemUI item = Instantiate(shopItemPrefab, shopListContent);
            item.Setup(skin, this);
        }
    }

    public bool IsUnlocked(SkinData skin) => skin.price == 0 || PlayerPrefs.GetInt(UnlockKey(skin), 0) == 1;

    string UnlockKey(SkinData skin) => "Skin_Unlocked_" + skin.skinId;

    public void Buy(SkinData skin)
    {
        if (IsUnlocked(skin)) return;

        if (CurrencyManager.SpendCoins(skin.price))
        {
            PlayerPrefs.SetInt(UnlockKey(skin), 1);
            PlayerPrefs.Save();
            RefreshCoinText();
        }
    }

    public void Select(SkinData skin)
    {
        if (!IsUnlocked(skin)) return;
        PlayerPrefs.SetString("SelectedSkinId", skin.skinId);
        PlayerPrefs.Save();
    }

    void RefreshCoinText() => coinBalanceText.text = CurrencyManager.TotalCoins.ToString();
}