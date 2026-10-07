using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TMP_Text coinText;
    public GameObject shopPanel;
    public Slider musicVolumeSlider;
    public PlayerSkinApplier characterPreviewApplier;

    void OnEnable()
    {
        RefreshCoinText();
        RefreshCharacterPreview();
    }

    void Start()
    {
        if (musicVolumeSlider != null)
        {
            float vol = AudioManager.Instance != null ? AudioManager.Instance.CurrentVolume : PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            musicVolumeSlider.value = vol;
            musicVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    void OnVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(val);
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", val);
            PlayerPrefs.Save();
        }
    }

    public void RefreshCoinText()
    {
        if (coinText != null) coinText.text = CurrencyManager.TotalCoins.ToString();
    }

    public void RefreshCharacterPreview()
    {
        if (characterPreviewApplier != null)
        {
            characterPreviewApplier.ApplySelectedSkin();
        }
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
        RefreshCharacterPreview();
    }
}