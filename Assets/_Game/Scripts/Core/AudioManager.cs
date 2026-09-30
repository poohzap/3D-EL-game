// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource musicSource;

    private const string VolumeKey = "MusicVolume";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        SetVolume(PlayerPrefs.GetFloat(VolumeKey, 0.7f));
    }

    public float CurrentVolume => musicSource != null ? musicSource.volume : 0.7f;

    public void SetVolume(float volume)
    {
        if (musicSource == null) return;
        musicSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, musicSource.volume);
        PlayerPrefs.Save();
    }
}