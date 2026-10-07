// SplashScreen.cs — Assets/_Game/Scripts/UI/SplashScreen.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float displayTime = 2f;

    void Start() => Invoke(nameof(GoToMenu), displayTime);

    void GoToMenu() => SceneManager.LoadScene("MainMenu");
}
