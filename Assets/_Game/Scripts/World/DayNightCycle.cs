// DayNightCycle.cs — Assets/_Game/Scripts/World/DayNightCycle.cs
// Loại: MonoBehaviour — gắn lên Directional Light trong scene Gameplay.
using UnityEngine;
using UnityEngine.Rendering;

// Xoay "mặt trời" theo chu kỳ, đổi màu ánh sáng / ambient / sương mù theo từng mốc giờ.
// Thời gian t chạy 0 → 1: 0 = bình minh, 0.25 = trưa, 0.5 = hoàng hôn, 0.75 = nửa đêm.
[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    [System.Serializable]
    public struct Phase
    {
        [Range(0f, 1f)] public float time;
        public Color lightColor;
        public Color ambientColor;
        public Color fogColor;
    }

    [Header("Chu kỳ")]
    public float cycleDuration = 240f;             // số giây cho 1 vòng sáng → tối → sáng
    [Range(0f, 1f)] public float startTime = 0.25f; // bắt đầu ván lúc trưa
    public float sunYaw = 150f;                    // hướng đường đi của mặt trời quanh trục Y
    public float sunIntensity = 1.2f;
    public float nightSpeedMultiplier = 3f;        // ban đêm trôi nhanh gấp mấy lần ban ngày

    [Header("Sương mù")]
    public float fogStart = 80f;
    public float fogEnd = 320f;

    [Header("Các mốc giờ (phải xếp theo time tăng dần)")]
    public Phase[] phases =
    {
        new Phase { time = 0.00f, lightColor = new Color(1.00f, 0.55f, 0.30f), ambientColor = new Color(0.45f, 0.40f, 0.50f), fogColor = new Color(0.95f, 0.65f, 0.50f) }, // bình minh
        new Phase { time = 0.25f, lightColor = new Color(1.00f, 0.96f, 0.88f), ambientColor = new Color(0.55f, 0.62f, 0.75f), fogColor = new Color(0.70f, 0.82f, 0.95f) }, // trưa
        new Phase { time = 0.45f, lightColor = new Color(1.00f, 0.45f, 0.20f), ambientColor = new Color(0.50f, 0.38f, 0.42f), fogColor = new Color(0.95f, 0.50f, 0.35f) }, // hoàng hôn
        new Phase { time = 0.55f, lightColor = new Color(0.30f, 0.30f, 0.60f), ambientColor = new Color(0.20f, 0.20f, 0.36f), fogColor = new Color(0.25f, 0.20f, 0.38f) }, // chạng vạng
        new Phase { time = 0.75f, lightColor = new Color(0.20f, 0.25f, 0.50f), ambientColor = new Color(0.20f, 0.24f, 0.42f), fogColor = new Color(0.06f, 0.08f, 0.18f) }, // đêm
        new Phase { time = 0.92f, lightColor = new Color(0.40f, 0.30f, 0.40f), ambientColor = new Color(0.30f, 0.28f, 0.40f), fogColor = new Color(0.45f, 0.35f, 0.45f) }, // trước bình minh
    };

    private Light sun;
    [Range(0f, 1f)] public float time; // hiện trong Inspector để theo dõi/kéo thử khi Play

    void Awake()
    {
        sun = GetComponent<Light>();
        time = startTime;

        RenderSettings.sun = sun;                          // để Skybox/Procedural đổi màu trời theo hướng mặt trời
        RenderSettings.ambientMode = AmbientMode.Flat;     // ambient màu phẳng, rẻ hơn nhiều so với ambient từ skybox trên mobile
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;
    }

    void Update()
    {
        float speed = time > 0.5f ? nightSpeedMultiplier : 1f; // time > 0.5 = đang là đêm
        time = (time + Time.deltaTime / cycleDuration * speed) % 1f;
        Apply(time);
    }

    void Apply(float t)
    {
        // Tìm 2 mốc kẹp quanh t (vòng tròn: sau mốc cuối là mốc đầu của ngày kế tiếp)
        int next = 0;
        while (next < phases.Length && phases[next].time <= t) next++;
        next %= phases.Length;
        int prev = (next - 1 + phases.Length) % phases.Length;

        float t0 = phases[prev].time;
        float t1 = phases[next].time;
        if (t1 <= t0)
        {
            t1 += 1f;
            if (t < t0) t += 1f;
        }
        float k = Mathf.InverseLerp(t0, t1, t);

        sun.color = Color.Lerp(phases[prev].lightColor, phases[next].lightColor, k);
        RenderSettings.ambientLight = Color.Lerp(phases[prev].ambientColor, phases[next].ambientColor, k);
        RenderSettings.fogColor = Color.Lerp(phases[prev].fogColor, phases[next].fogColor, k);

        // Độ cao mặt trời so với đường chân trời: 0 lúc bình minh/hoàng hôn, 1 lúc trưa, âm khi ban đêm.
        // Ánh sáng tắt hẳn khi mặt trời xuống dưới chân trời (tránh chiếu ngược từ dưới đất lên).
        float elevation = Mathf.Sin(t * Mathf.PI * 2f);
        sun.intensity = sunIntensity * Mathf.Clamp01(elevation * 4f);

        transform.rotation = Quaternion.Euler(t * 360f, sunYaw, 0f);
    }
}