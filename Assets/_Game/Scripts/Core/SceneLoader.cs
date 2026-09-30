// SceneLoader.cs
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string TargetScene { get; private set; }

    public static void LoadWithLoadingScreen(string targetScene)
    {
        TargetScene = targetScene;
        SceneManager.LoadScene("Loading");
    }
}