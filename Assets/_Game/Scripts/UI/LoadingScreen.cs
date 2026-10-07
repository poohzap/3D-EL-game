using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public float minimumDisplayTime = 1f;

    void Start() => StartCoroutine(LoadTargetScene());

    IEnumerator LoadTargetScene()
    {
        float startTime = Time.time;

        string target = string.IsNullOrEmpty(SceneLoader.TargetScene) ? "Gameplay" : SceneLoader.TargetScene;
        AsyncOperation op = SceneManager.LoadSceneAsync(target);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (progressBar != null) progressBar.value = op.progress / 0.9f;
            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;

        while (Time.time - startTime < minimumDisplayTime)
            yield return null;

        op.allowSceneActivation = true;
    }
}