// GameplayUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [Header("HUD Texts")]
    public TMP_Text scoreText;
    public TMP_Text coinText;
    public TMP_Text powerUpText;

    [Header("HUD Icons (kéo sprite vào đây)")]
    public Image coinIcon;          // icon_coin sprite
    public Image powerUpIcon;       // icon hiện tại của powerup
    public Sprite iconShield;       // icon_shield
    public Sprite iconMagnet;       // icon_magnet
    public Sprite iconRocket;       // icon_rocket

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalCoinText;

    [Header("PowerUpManager (auto-find nếu bỏ trống)")]
    public PowerUpManager playerPowerUps;

    void Awake()
    {
        if (playerPowerUps == null)
            playerPowerUps = Object.FindAnyObjectByType<PowerUpManager>();
    }

    void OnEnable()
    {
        if (playerPowerUps != null) playerPowerUps.OnPowerUpChanged += HandlePowerUpChanged;
        // Ẩn powerup icon lúc đầu
        SetPowerUpIconVisible(false);
    }

    void OnDisable()
    {
        if (playerPowerUps != null) playerPowerUps.OnPowerUpChanged -= HandlePowerUpChanged;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (scoreText != null) scoreText.text = "Điểm: " + GameManager.Instance.Score;
        if (coinText != null) coinText.text = "" + CurrencyManager.RunCoins;
    }

    void HandlePowerUpChanged(PowerUpType? type, float remaining)
    {
        if (!type.HasValue)
        {
            if (powerUpText != null) powerUpText.text = "";
            SetPowerUpIconVisible(false);
            return;
        }

        if (powerUpText != null)
            powerUpText.text = $"{remaining:0.0}s";

        // Đổi icon theo loại powerup
        if (powerUpIcon != null)
        {
            Sprite s = type.Value switch
            {
                PowerUpType.Shield    => iconShield,
                PowerUpType.Rocket    => iconRocket,
                PowerUpType.JumpBoots => iconMagnet,
                _                     => null
            };
            if (s != null)
            {
                powerUpIcon.sprite = s;
                SetPowerUpIconVisible(true);
            }
        }
    }

    void SetPowerUpIconVisible(bool visible)
    {
        if (powerUpIcon != null) powerUpIcon.gameObject.SetActive(visible);
    }

    public void ShowGameOver(int score, int coins)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Điểm: " + score;
        if (finalCoinText != null) finalCoinText.text = "Xu: " + coins;
    }

    public void OnClickRestart() => GameManager.Instance.RestartGame();
    public void OnClickMenu() => GameManager.Instance.GoToMenu();
}