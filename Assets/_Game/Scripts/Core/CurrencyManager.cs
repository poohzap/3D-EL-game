// CurrencyManager.cs
using UnityEngine;

public static class CurrencyManager
{
    private const string TotalCoinsKey = "TotalCoins";

    public static int RunCoins { get; private set; }
    public static int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);

    public static void ResetRun() => RunCoins = 0;

    public static void AddRunCoin(int amount = 1)
    {
        RunCoins += amount;
    }

    /// <summary>
    /// Cộng dồn số coin nhặt được trong ván vào tổng số coin của ví và lưu lại.
    /// </summary>
    public static void CommitRunCoinsToWallet()
    {
        if (RunCoins <= 0) return;

        int newTotal = TotalCoins + RunCoins;
        PlayerPrefs.SetInt(TotalCoinsKey, newTotal);
        PlayerPrefs.Save();
        Debug.Log($"[CurrencyManager] Đã cộng dồn {RunCoins} xu vào ví! Tổng xu hiện tại: {newTotal}");
        RunCoins = 0;
    }

    public static bool SpendCoins(int amount)
    {
        if (TotalCoins < amount) return false;
        PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins - amount);
        PlayerPrefs.Save();
        return true;
    }
}