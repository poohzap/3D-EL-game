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
    public Image coinIcon;
    public Image powerUpIcon;
    public Sprite iconShield;
    public Sprite iconBoots;
    public Sprite iconRocket;

    [Header("Shield Virtual Life")]
    [Tooltip("Hiển thị icon mạng ảo khi shield đang có hiệu lực")]
    public Image shieldLifeIcon;    // icon khiên ảo (optional)

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalCoinText;

    [Header("PowerUpManager (auto-find nếu bỏ trống)")]
    public PowerUpManager playerPowerUps;

    // ===== Pretty HUD (giữ nguyên tên field cũ để không mất reference) =====
    [Header("=== Pretty HUD (Portrait 1080x1920) ===")]
    [Tooltip("Font TMP cho HUD (bỏ trống sẽ dùng LiberationSans SDF)")]
    public TMP_FontAsset hudFont;

    [Tooltip("Sprite hud_pill (9-slice) làm nền pill Score/Coin/PowerUp")]
    public Sprite hudPillSprite;

    [Tooltip("Icon star cho Score pill")]
    public Sprite iconStar;

    [Tooltip("Icon coin cho Coin pill")]
    public Sprite iconCoin;

    [Tooltip("Icon pause (góc trên trái)")]
    public Sprite iconPause;

    [Header("Score Pill (góc trên phải)")]
    public Image scorePillBg;        // Image (Sliced) dùng hudPillSprite
    public Image scoreIcon;          // Image iconStar
    public TMP_Text scoreValueText;  // Text "D6"

    [Header("Coin Pill (dưới Score, canh phải)")]
    public Image coinPillBg;         // Image (Sliced) dùng hudPillSprite
    public Image coinPillIcon;       // Image iconCoin
    public TMP_Text coinValueText;   // Text RunCoins

    [Header("Pause Button (góc trên trái)")]
    public Button pauseButton;       // Button với iconPause
    public Image pauseButtonIcon;    // Image iconPause (con của Button)

    [Header("PowerUp Panel (giữa trên, ẩn khi không có item)")]
    public GameObject powerUpPanel;          // Root panel
    public Image powerUpPanelIcon;           // Icon powerup hiện tại
    public Image powerUpFillImage;           // Filled Image (Horizontal) cho timer
    public TMP_Text powerUpTimeText;         // Text thời gian còn lại (optional — nếu có)

    // Trạng thái nội bộ
    bool prettyHudActive;

    void Awake()
    {
        if (playerPowerUps == null)
            playerPowerUps = Object.FindAnyObjectByType<PowerUpManager>();

        InitializePrettyHUD();
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            var btn = pauseButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickPause);
            }
        }
    }

    // Cấu hình/style HUD mới — idempotent.
    // UI object do editor tool "Tools/Setup Gameplay HUD (Portrait)" tạo trong scene;
    // nếu field chưa được gán thì mọi chỗ đều guard null (không NRE).
    void InitializePrettyHUD()
    {
        prettyHudActive = scoreValueText != null || coinValueText != null;

        // Font: theo yêu cầu là Assets/_Game/Art/UI/Fonts/ nhưng folder không tồn tại
        // → dùng LiberationSans SDF (đậm) + material Outline (viền tối).
        TMP_FontAsset font = hudFont != null
            ? hudFont
            : Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF - Fallback");
        Material outlineMat = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");

        // --- Score Pill ---
        SetupPill(scorePillBg, hudPillSprite);
        SetupIcon(scoreIcon, iconStar);
        SetupValueText(scoreValueText, font, outlineMat);

        // --- Coin Pill ---
        SetupPill(coinPillBg, hudPillSprite);
        SetupIcon(coinPillIcon, iconCoin);
        SetupValueText(coinValueText, font, outlineMat);

        // --- Pause Button ---
        // GameManager đã có PauseGame()/ResumeGame() — nút Pause hoạt động thật.
        // Awake() (chạy ngay sau hàm này) sẽ bật nút và gán onClick → chỉ cần gán icon ở đây.
        if (pauseButton != null && pauseButtonIcon != null && iconPause != null)
        {
            pauseButtonIcon.sprite = iconPause;
        }

        // --- PowerUp Panel ---
        if (powerUpPanel != null)
        {
            SetupPill(powerUpPanel.GetComponent<Image>(), hudPillSprite);
            SetupIcon(powerUpPanelIcon, null);
            if (powerUpFillImage != null)
            {
                powerUpFillImage.type = Image.Type.Filled;
                powerUpFillImage.fillMethod = Image.FillMethod.Horizontal;
                powerUpFillImage.fillOrigin = 0;          // trái → phải
                powerUpFillImage.fillAmount = 0f;
                powerUpFillImage.raycastTarget = false;
                powerUpFillImage.gameObject.SetActive(false);
            }
            SetupValueText(powerUpTimeText, font, outlineMat);
            powerUpPanel.SetActive(false);                // ẩn khi chưa có item
        }

        // Ẩn 3 text cũ khi HUD mới đã có (tránh chữ đè lên pill)
        if (prettyHudActive)
        {
            if (scoreText != null) scoreText.gameObject.SetActive(false);
            if (coinText != null) coinText.gameObject.SetActive(false);
            if (powerUpText != null) powerUpText.gameObject.SetActive(false);
        }

        // Text cũ còn hiển thị (GameOver panel) cùng style trắng/đậm/viền tối
        SetupValueText(scoreText, font, outlineMat);
        SetupValueText(coinText, font, outlineMat);
        SetupValueText(powerUpText, font, outlineMat);
        SetupValueText(finalScoreText, font, outlineMat);
        SetupValueText(finalCoinText, font, outlineMat);
    }

    static void SetupPill(Image img, Sprite pillSprite)
    {
        if (img == null) return;
        if (pillSprite != null) img.sprite = pillSprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0f, 0f, 0f, 0.6f);
        img.raycastTarget = false;
    }

    static void SetupIcon(Image img, Sprite sprite)
    {
        if (img == null) return;
        if (sprite != null) img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    static void SetupValueText(TMP_Text t, TMP_FontAsset font, Material outlineMat)
    {
        if (t == null) return;
        if (font != null) t.font = font;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;
        t.enableVertexGradient = false;
        t.raycastTarget = false;
        if (outlineMat != null) t.fontSharedMaterial = outlineMat;
    }

    void OnEnable()
    {
        if (playerPowerUps != null) playerPowerUps.OnPowerUpChanged += HandlePowerUpChanged;
        SetPowerUpIconVisible(false);
        SetShieldLifeVisible(false);
        SetNewPowerUpPanelVisible(false);
    }

    void OnDisable()
    {
        if (playerPowerUps != null) playerPowerUps.OnPowerUpChanged -= HandlePowerUpChanged;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // --- Điểm / Xu ---
        if (prettyHudActive)
        {
            if (scoreValueText != null) scoreValueText.text = GameManager.Instance.Score.ToString("D6");
            if (coinValueText != null) coinValueText.text = CurrencyManager.RunCoins.ToString();
        }
        else
        {
            // Fallback: HUD cũ (khi chưa chạy tool setup)
            if (scoreText != null) scoreText.text = "Điểm: " + GameManager.Instance.Score;
            if (coinText != null) coinText.text = CurrencyManager.RunCoins.ToString();
        }

        // --- PowerUp Panel (poll trạng thái — không phụ thuộc 100% vào event) ---
        RefreshPowerUpPanel();

        // Icon mạng ảo shield realtime
        if (playerPowerUps != null)
            SetShieldLifeVisible(playerPowerUps.IsShieldActive && playerPowerUps.HasVirtualLife);
    }

    void HandlePowerUpChanged(PowerUpType? type, float remaining)
    {
        RefreshPowerUpPanel();

        if (!type.HasValue)
        {
            SetShieldLifeVisible(false);
            if (!prettyHudActive)
            {
                if (powerUpText != null) powerUpText.text = "";
                SetPowerUpIconVisible(false);
            }
            return;
        }

        // Text/icon cũ (chỉ dùng khi chưa có HUD mới)
        if (!prettyHudActive)
        {
            if (powerUpText != null) powerUpText.text = remaining.ToString("0.0") + "s";
            if (powerUpIcon != null)
            {
                Sprite s = GetSpriteForType(type.Value);
                if (s != null)
                {
                    powerUpIcon.sprite = s;
                    SetPowerUpIconVisible(true);
                }
            }
        }
    }

    // Đồng bộ panel powerup mỗi frame: hiện đúng icon + thanh Filled (remaining/duration).
    // Shield không có timer (duration <= 0) → chỉ hiện icon, ẩn thanh.
    void RefreshPowerUpPanel()
    {
        if (powerUpPanel == null) return;

        bool hasItem = playerPowerUps != null && playerPowerUps.CurrentType.HasValue;
        if (!hasItem)
        {
            if (powerUpPanel.activeSelf) powerUpPanel.SetActive(false);
            return;
        }
        if (!powerUpPanel.activeSelf) powerUpPanel.SetActive(true);

        PowerUpType type = playerPowerUps.CurrentType.Value;

        if (powerUpPanelIcon != null)
        {
            Sprite s = GetSpriteForType(type);
            if (s != null) powerUpPanelIcon.sprite = s;
            bool showIcon = s != null;
            if (powerUpPanelIcon.gameObject.activeSelf != showIcon)
                powerUpPanelIcon.gameObject.SetActive(showIcon);
        }

        float maxDuration = GetMaxDuration(type);
        float remaining = Mathf.Max(playerPowerUps.RemainingTime, 0f);
        bool hasTimer = maxDuration > 0f && remaining > 0f;

        if (powerUpFillImage != null)
        {
            if (powerUpFillImage.gameObject.activeSelf != hasTimer)
                powerUpFillImage.gameObject.SetActive(hasTimer);
            if (hasTimer)
                powerUpFillImage.fillAmount = Mathf.Clamp01(remaining / maxDuration);
        }

        if (powerUpTimeText != null)
        {
            if (powerUpTimeText.gameObject.activeSelf != hasTimer)
                powerUpTimeText.gameObject.SetActive(hasTimer);
            if (hasTimer)
                powerUpTimeText.text = remaining.ToString("0.0") + "s";
        }
    }

    float GetMaxDuration(PowerUpType type)
    {
        if (playerPowerUps == null) return 0f;
        return type switch
        {
            PowerUpType.Shield => playerPowerUps.shieldDuration,
            PowerUpType.JumpBoots => playerPowerUps.jumpBootsDuration,
            PowerUpType.Rocket => playerPowerUps.rocketDuration,
            _ => 0f
        };
    }

    Sprite GetSpriteForType(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Shield => iconShield,
            PowerUpType.Rocket => iconRocket,
            PowerUpType.JumpBoots => iconBoots,
            _ => null
        };
    }

    void SetNewPowerUpPanelVisible(bool visible)
    {
        if (powerUpPanel != null && powerUpPanel.activeSelf != visible)
            powerUpPanel.SetActive(visible);
    }

    void SetPowerUpIconVisible(bool visible)
    {
        if (powerUpIcon != null && powerUpIcon.gameObject.activeSelf != visible)
            powerUpIcon.gameObject.SetActive(visible);
    }

    void SetShieldLifeVisible(bool visible)
    {
        if (shieldLifeIcon != null && shieldLifeIcon.gameObject.activeSelf != visible)
            shieldLifeIcon.gameObject.SetActive(visible);
    }

    public void ShowGameOver(int score, int coins)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Điểm: " + score;
        if (finalCoinText != null) finalCoinText.text = "Xu: " + coins;
    }

    public void OnClickRestart() => GameManager.Instance.RestartGame();

    public void ShowPause(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
    }
    public void OnClickPause() => GameManager.Instance.PauseGame();
    public void OnClickResume() => GameManager.Instance.ResumeGame();
    public void OnClickMenu() => GameManager.Instance.GoToMenu();
}