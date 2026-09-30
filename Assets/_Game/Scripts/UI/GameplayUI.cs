// GameplayUI.cs
using UnityEngine;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [Header("Kéo các UI Text/Panel đã tạo ở Mục 2.5 vào đây")]
    public TMP_Text scoreText;
    public TMP_Text coinText;
    public TMP_Text powerUpText;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalCoinText;

    [Header("Kéo component PowerUpManager của Player vào đây")]
    public PowerUpManager playerPowerUps;

    void OnEnable()
    {
        if (playerPowerUps != null) playerPowerUps.OnPowerUpChanged += HandlePowerUpChanged;
    }

    void OnDisable()
    {
        if (playerPowerUps != null) playerPowerUps.OnPowerUpChanged -= HandlePowerUpChanged;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        scoreText.text = "Điểm: " + GameManager.Instance.Score;
        coinText.text = "Xu: " + CurrencyManager.RunCoins;
    }

    void HandlePowerUpChanged(PowerUpType? type, float remaining)
    {
        powerUpText.text = type.HasValue ? $"{type.Value}: {remaining:0.0}s" : "";
    }

    public void ShowGameOver(int score, int coins)
    {
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Điểm: " + score;
        finalCoinText.text = "Xu: " + coins;
    }

    public void OnClickRestart() => GameManager.Instance.RestartGame();
    public void OnClickMenu() => GameManager.Instance.GoToMenu();
}