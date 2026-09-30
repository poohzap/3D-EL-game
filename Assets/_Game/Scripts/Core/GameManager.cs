// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("Kéo trong Inspector")]
    public Transform player;
    public GameplayUI gameplayUI;

    public int Score => Mathf.FloorToInt(player.position.z);

    void Awake()
    {
        Instance = this;
        CurrencyManager.ResetRun();
    }

    public void EndGame()
    {
        if (CurrentState == GameState.GameOver) return;

        CurrentState = GameState.GameOver;
        int finalScore = Score;
        int finalCoins = CurrencyManager.RunCoins;

        CurrencyManager.CommitRunCoinsToWallet();
        gameplayUI.ShowGameOver(finalScore, finalCoins);
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void GoToMenu() => SceneLoader.LoadWithLoadingScreen("MainMenu");
}