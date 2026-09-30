using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TMP_Text coinText;
    public GameObject shopPanel;
    public Slider musicVolumeSlider;

    void OnEnable() => RefreshCoinText();

    void Start()
    {
        if (musicVolumeSlider != null && AudioManager.Instance != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.CurrentVolume;
            musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetVolume);
        }
    }

    void RefreshCoinText()
    {
        if (coinText != null) coinText.text = CurrencyManager.TotalCoins.ToString();
    }

    public void OnClickPlay() => SceneLoader.LoadWithLoadingScreen("Gameplay");

    public void OnClickOpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
    }

    public void OnClickCloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        RefreshCoinText();
    }
}