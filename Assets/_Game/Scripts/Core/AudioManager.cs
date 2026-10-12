// AudioManager.cs — Assets/_Game/Scripts/Core/AudioManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum Sfx { Coin, Motion, Hit, GameOver, PowerUp, PowerUpEnd, ShieldBreak, Rocket, UiClick }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SfxEntry
    {
        public Sfx id;
        public AudioClip clip;
    }

    [Header("Nhạc nền")]
    public AudioSource musicSource;
    [Range(0f, 1f)] public float musicVolume = 0.6f; // mức nhạc so với SFX, KHÔNG phải slider

    [Header("Nhạc theo scene (kéo clip vào)")]
    public AudioClip menuMusic;      // music_menu
    public AudioClip gameplayMusic;  // music_gameplay

    [Header("Danh sách SFX (kéo clip vào)")]
    public SfxEntry[] sfxEntries;

    private const string VolumeKey = "MasterVolume";

    private readonly Dictionary<Sfx, AudioClip> sfx = new Dictionary<Sfx, AudioClip>();
    private AudioSource sfxSource;
    private AudioSource coinSource; // riêng để đổi pitch mà không ảnh hưởng SFX khác
    private float lastCoinTime;
    private int coinStreak;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        if (musicSource != null) musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        coinSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = coinSource.playOnAwake = false;

        foreach (SfxEntry e in sfxEntries)
            if (e.clip != null) sfx[e.id] = e.clip;

        SetVolume(PlayerPrefs.GetFloat(VolumeKey, 0.7f));

        // Dòng chẩn đoán: xem Console khi Play từ Splash (xoá sau khi mọi thứ ổn)
        Debug.Log("[Audio] Sẵn sàng. Âm lượng chung = " + AudioListener.volume.ToString("0.00")
                  + ", SFX đã gán = " + sfx.Count + "/9, musicSource = " + (musicSource != null));
    }

    // Âm click cho MỌI nút UI: tự phát hiện khi chạm vào 1 Button, không cần gắn gì lên từng nút.
    // (Chạm cảm ứng trên Android cũng tính là chuột nhấn khi dùng Input Manager cũ.)
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryPlayButtonClick(Input.mousePosition);
    }

    void TryPlayButtonClick(Vector2 screenPos)
    {
        if (EventSystem.current == null) return;

        var data = new PointerEventData(EventSystem.current) { position = screenPos };
        uiHits.Clear();
        EventSystem.current.RaycastAll(data, uiHits);
        if (uiHits.Count == 0) return;

        // Chỉ xét vật UI trên cùng tại điểm chạm: là Button (hoặc con của Button) và còn bấm được thì phát tiếng
        Button button = uiHits[0].gameObject.GetComponentInParent<Button>();
        if (button != null && button.interactable) PlaySfx(Sfx.UiClick);
    }

    // Slider ở MainMenu đã gọi hàm này (MainMenuUI.cs) — 1 slider chỉnh cả nhạc lẫn SFX
    public float CurrentVolume => AudioListener.volume;

    public void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);

    // Đang phát đúng bài đó rồi thì bỏ qua (Restart không làm nhạc bắt đầu lại từ đầu)
    void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySfx(Sfx id)
    {
        if (sfx.TryGetValue(id, out AudioClip clip)) sfxSource.PlayOneShot(clip);
        else Debug.LogWarning("[Audio] Chưa gán clip cho " + id + " trong Sfx Entries (scene Splash).");
    }

    // Nhặt xu liên tiếp trong 0.6s thì cao độ tăng dần (tối đa 12 bậc)
    public void PlayCoin()
    {
        if (!sfx.TryGetValue(Sfx.Coin, out AudioClip clip)) return;

        coinStreak = Time.unscaledTime - lastCoinTime < 0.6f ? Mathf.Min(coinStreak + 1, 12) : 0;
        lastCoinTime = Time.unscaledTime;
        coinSource.pitch = 1f + coinStreak * 0.05f;
        coinSource.PlayOneShot(clip);
    }
}