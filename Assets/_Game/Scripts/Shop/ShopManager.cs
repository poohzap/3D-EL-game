using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public SkinDatabase database; // kéo asset SkinDatabase (Mục 8.2) vào đây
    public Transform shopListContent; // Content của ScrollView
    public ShopItemUI shopItemPrefab;
    public TMP_Text coinBalanceText;

    private readonly List<ShopItemUI> spawnedItems = new List<ShopItemUI>();

    void Start()
    {
        RefreshCoinText();
        BuildList();
    }

    void OnEnable()
    {
        RefreshCoinText();
        RefreshAllItems();
    }

    void BuildList()
    {
        if (database == null || shopListContent == null || shopItemPrefab == null) return;

        foreach (Transform child in shopListContent) Destroy(child.gameObject);
        spawnedItems.Clear();

        foreach (SkinData skin in database.allSkins)
        {
            if (skin == null) continue;
            ShopItemUI item = Instantiate(shopItemPrefab, shopListContent);
            item.Setup(skin, this);
            spawnedItems.Add(item);
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
            Select(skin);
            RefreshCoinText();
            RefreshAllItems();
        }
    }

    public void Select(SkinData skin)
    {
        if (!IsUnlocked(skin)) return;
        PlayerPrefs.SetString("SelectedSkinId", skin.skinId);
        PlayerPrefs.Save();
        RefreshAllItems();
    }

    public void RefreshAllItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) item.Refresh();
        }
    }

    void RefreshCoinText()
    {
        if (coinBalanceText != null) coinBalanceText.text = CurrencyManager.TotalCoins.ToString();
    }
}