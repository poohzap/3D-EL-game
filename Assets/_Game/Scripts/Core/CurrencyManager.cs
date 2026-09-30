// CurrencyManager.cs
using UnityEngine;

public static class CurrencyManager
{
    private const string TotalCoinsKey = "TotalCoins";

    public static int RunCoins { get; private set; }
    public static int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);

    public static void ResetRun() => RunCoins = 0;
    public static void AddRunCoin(int amount = 1) => RunCoins += amount;

    public static void CommitRunCoinsToWallet()
    {
        PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins + RunCoins);
        PlayerPrefs.Save();
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