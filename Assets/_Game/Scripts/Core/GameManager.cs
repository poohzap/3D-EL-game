// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver, Paused }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("Kéo trong Inspector")]
    public Transform player;
    public GameplayUI gameplayUI;

    public int Score => player != null ? Mathf.FloorToInt(player.position.z) : 0;

    void Awake()
    {
        Time.timeScale = 1f;
        Instance = this;
        CurrencyManager.ResetRun();
    }

    public void EndGame()
    {
        Time.timeScale = 1f;
        if (CurrentState == GameState.GameOver) return;

        CurrentState = GameState.GameOver;
        int finalScore = Score;
        int finalCoins = CurrencyManager.RunCoins;

        // Cộng dồn coin nhặt được vào tổng số coin
        CurrencyManager.CommitRunCoinsToWallet();

        if (gameplayUI != null)
            gameplayUI.ShowGameOver(finalScore, finalCoins);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;                 // đóng băng animation, power-up, coroutine
        if (gameplayUI != null) gameplayUI.ShowPause(true);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        if (gameplayUI != null) gameplayUI.ShowPause(false);
    }

    void Update()
    {
        // Phím Esc trong Editor, nút Back trên Android
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing) PauseGame();
            else if (CurrentState == GameState.Paused) ResumeGame();
        }
    }

    // Tự pause khi người chơi thoát ra màn hình chính điện thoại
    void OnApplicationPause(bool paused)
    {
        if (paused) PauseGame();
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        CurrencyManager.CommitRunCoinsToWallet();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        CurrencyManager.CommitRunCoinsToWallet();
        SceneLoader.LoadWithLoadingScreen("MainMenu");
    }

    void OnApplicationQuit()
    {
        CurrencyManager.CommitRunCoinsToWallet();
    }
}