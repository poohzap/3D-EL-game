//Setupgameplayhud.cs 
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Tạo HUD mới cho scene Gameplay (Canvas 1080x1920 Portrait).
// Menu: Tools/Setup Gameplay HUD (Portrait)
// Batch: -executeMethod SetupGameplayHUD.SetupAndExit
public class SetupGameplayHUD : EditorWindow
{
    // ===== Layout (reference 1080x1920, chừa ~96px phía trên) =====
    const float TopY = -96f;   // chừa top ~80-96px (an toàn notch)
    const float RightMargin = 24f;
    const float LeftMargin = 24f;
    const float Gap = 12f;
    const float ScorePillW = 240f;
    const float ScorePillH = 74f;
    const float CoinPillW = 240f;
    const float CoinPillH = 66f;
    const float PauseSize = 66f;
    const float PowerUpPanelW = 260f;
    const float PowerUpPanelH = 76f;

    const string HudDir = "Assets/_Game/Art/UI/HUD/";

    [MenuItem("Tools/Setup Gameplay HUD (Portrait)")]
    public static void Setup()
    {
        string scenePath = "Assets/_Game/Scenes/Gameplay.unity";
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.path != scenePath)
        {
            if (activeScene.isDirty)
            {
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log("[SetupGameplayHUD] Đã tự lưu scene đang mở trước khi chuyển sang Gameplay.");
            }
            activeScene = EditorSceneManager.OpenScene(scenePath);
        }

        // 1) Đảm bảo sprites HUD đúng importer (idempotent)
        SetupHUDSprites.Setup();

        var canvasGO = GameObject.Find("GameplayCanvas");
        if (canvasGO == null)
        {
            Debug.LogError("[SetupGameplayHUD] Không tìm thấy GameplayCanvas trong scene Gameplay!");
            return;
        }

        // 2) Canvas & Scaler (giữ cấu hình portrait 1080x1920)
        var canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;   // giữ nguyên cấu hình hiện tại của scene
        scaler.referencePixelsPerUnit = 100;

        if (canvasGO.GetComponent<GraphicRaycaster>() == null)
            canvasGO.AddComponent<GraphicRaycaster>();

        // 3) Load sprites & font (font theo yêu cầu: Assets/_Game/Art/UI/Fonts/ —
        //    folder này KHÔNG tồn tại nên dùng LiberationSans SDF + material Outline)
        Sprite LoadHud(string fileName) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(HudDir + fileName);

        var hudPill = LoadHud("hud_pill.png");
        var iconStar = LoadHud("icon_star.png");
        var iconCoin = LoadHud("icon_coin.png");
        var iconPause = LoadHud("icon_pause.png");
        var iconShield = LoadHud("icon_shield.png");
        var iconBoots = LoadHud("icon_jump_boots.png");
        var iconRocket = LoadHud("icon_rocket.png");

        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF - Fallback");
        var outlineMat = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");

        if (hudPill == null) Debug.LogError("[SetupGameplayHUD] Không load được hud_pill.png");
        if (iconStar == null) Debug.LogError("[SetupGameplayHUD] Không load được icon_star.png");
        if (iconCoin == null) Debug.LogError("[SetupGameplayHUD] Không load được icon_coin.png");
        if (iconPause == null) Debug.LogError("[SetupGameplayHUD] Không load được icon_pause.png");
        if (iconShield == null) Debug.LogError("[SetupGameplayHUD] Không load được icon_shield.png");
        if (iconBoots == null) Debug.LogError("[SetupGameplayHUD] Không load được icon_jump_boots.png");
        if (iconRocket == null) Debug.LogError("[SetupGameplayHUD] Không load được icon_rocket.png");
        if (font == null) Debug.LogError("[SetupGameplayHUD] Không load được TMP font!");

        var gameplayUI = canvasGO.GetComponent<GameplayUI>();
        if (gameplayUI == null) gameplayUI = canvasGO.AddComponent<GameplayUI>();

        // Gán sprite/font vào GameplayUI (icon powerup dùng đúng sprite folder HUD —
        // sửa luôn lỗi cũ: iconBoots từng trỏ nhầm icon_magnet)
        gameplayUI.hudFont = font;
        gameplayUI.hudPillSprite = hudPill;
        gameplayUI.iconStar = iconStar;
        gameplayUI.iconCoin = iconCoin;
        gameplayUI.iconPause = iconPause;
        gameplayUI.iconShield = iconShield;
        gameplayUI.iconBoots = iconBoots;
        gameplayUI.iconRocket = iconRocket;

        // 4) Dọn HUD cũ (ẩn text cũ — giữ field tham chiếu, không xóa object)
        CleanupOldHUD(canvasGO);

        // ===== 5) SCORE PILL — góc trên phải =====
        var scorePill = CreatePill("ScorePill", canvasGO.transform, hudPill,
            new Vector2(1, 1), new Vector2(-RightMargin, TopY), new Vector2(ScorePillW, ScorePillH));
        gameplayUI.scorePillBg = scorePill.GetComponent<Image>();

        var scoreIcon = CreateImage("ScoreIcon", scorePill.transform, iconStar,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(44, 44), new Vector2(16, 0));
        gameplayUI.scoreIcon = scoreIcon;

        var scoreValue = CreateTMPText("ScoreValue", scorePill.transform, font, outlineMat,
            "000000", 42f,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-18, 0), new Vector2(150, 56));
        scoreValue.alignment = TextAlignmentOptions.MidlineRight;
        gameplayUI.scoreValueText = scoreValue;

        // ===== 6) COIN PILL — dưới Score, canh phải =====
        var coinPill = CreatePill("CoinPill", canvasGO.transform, hudPill,
            new Vector2(1, 1), new Vector2(-RightMargin, TopY - ScorePillH - Gap), new Vector2(CoinPillW, CoinPillH));
        gameplayUI.coinPillBg = coinPill.GetComponent<Image>();

        var coinPillIcon = CreateImage("CoinPillIcon", coinPill.transform, iconCoin,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, 40), new Vector2(14, 0));
        gameplayUI.coinPillIcon = coinPillIcon;

        var coinValue = CreateTMPText("CoinValue", coinPill.transform, font, outlineMat,
            "0", 36f,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-18, 0), new Vector2(150, 50));
        coinValue.alignment = TextAlignmentOptions.MidlineRight;
        gameplayUI.coinValueText = coinValue;

        // ===== 7) PAUSE BUTTON — góc trên trái =====
        // GameManager CHƯA có logic Pause → tạo nhưng để inactive (GameplayUI cũng chặn lúc Play)
        var pauseBtnGO = new GameObject("PauseButton", typeof(RectTransform), typeof(Button), typeof(Image));
        pauseBtnGO.transform.SetParent(canvasGO.transform, false);
        var pauseRect = pauseBtnGO.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0, 1);
        pauseRect.anchorMax = new Vector2(0, 1);
        pauseRect.pivot = new Vector2(0, 1);
        pauseRect.anchoredPosition = new Vector2(LeftMargin, TopY);
        pauseRect.sizeDelta = new Vector2(PauseSize, PauseSize);
        var pauseImg = pauseBtnGO.GetComponent<Image>();
        pauseImg.sprite = iconPause;
        pauseImg.type = Image.Type.Simple;
        pauseImg.preserveAspect = true;
        pauseImg.raycastTarget = true;
        var pauseBtn = pauseBtnGO.GetComponent<Button>();
        pauseBtn.targetGraphic = pauseImg;
        gameplayUI.pauseButton = pauseBtn;
        gameplayUI.pauseButtonIcon = pauseImg;
        pauseBtnGO.SetActive(false);

        // ===== 8) POWERUP PANEL — giữa trên (ẩn khi không có item) =====
        var puPanel = new GameObject("PowerUpPanel", typeof(RectTransform), typeof(Image));
        puPanel.transform.SetParent(canvasGO.transform, false);
        var puRect = puPanel.GetComponent<RectTransform>();
        puRect.anchorMin = new Vector2(0.5f, 1);
        puRect.anchorMax = new Vector2(0.5f, 1);
        puRect.pivot = new Vector2(0.5f, 1);
        puRect.anchoredPosition = new Vector2(0, TopY);
        puRect.sizeDelta = new Vector2(PowerUpPanelW, PowerUpPanelH);
        var puBg = puPanel.GetComponent<Image>();
        puBg.sprite = hudPill;
        puBg.type = Image.Type.Sliced;
        puBg.color = new Color(0f, 0f, 0f, 0.6f);
        puBg.raycastTarget = false;
        gameplayUI.powerUpPanel = puPanel;

        // Icon loại powerup (bên trái trong panel)
        var puIcon = CreateImage("PowerUpPanelIcon", puPanel.transform, null,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(48, 48), new Vector2(18, 0));
        gameplayUI.powerUpPanelIcon = puIcon;

        // Thanh timer: track (nền) + fill (Filled Horizontal, còn lại/durations)
        var puTrack = CreateImage("PowerUpBarTrack", puPanel.transform, null,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(170, 16), new Vector2(-16, 0));
        puTrack.color = new Color(0f, 0f, 0f, 0.55f);

        var puFill = CreateImage("PowerUpBarFill", puTrack.transform, null,
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        puFill.color = new Color(1f, 0.82f, 0.2f, 1f);   // vàng gold
        puFill.type = Image.Type.Filled;
        puFill.fillMethod = Image.FillMethod.Horizontal;
        puFill.fillOrigin = 0;
        puFill.fillClockwise = true;
        puFill.fillAmount = 0f;
        puFill.preserveAspect = false;
        gameplayUI.powerUpFillImage = puFill;

        // Ẩn trạng thái ban đầu (Play sẽ tự hiện khi có item)
        puIcon.gameObject.SetActive(false);
        puFill.gameObject.SetActive(false);
        puPanel.SetActive(false);

        // 9) Style text cũ còn hiển thị (GameOver panel) cho đồng bộ font đậm + viền tối
        ApplyStyle(gameplayUI.finalScoreText, font, outlineMat);
        ApplyStyle(gameplayUI.finalCoinText, font, outlineMat);

        // 10) Gán lại tham chiếu cũ nếu thiếu
        AssignLegacyRefs(gameplayUI, canvasGO);

        // 11) Lưu scene
        EditorUtility.SetDirty(gameplayUI);
        EditorUtility.SetDirty(canvasGO);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        if (Verify())
            Debug.Log("[SetupGameplayHUD] ✅ Setup HUD hoàn tất. Mở scene Gameplay & Play (Game view 1080x1920 Portrait) để kiểm tra.");
        else
            Debug.LogError("[SetupGameplayHUD] ⚠️ Setup chạy xong nhưng VERIFY phát hiện field còn thiếu (xem lỗi phía trên).");
    }

    // Gọi từ command line (batchmode) — thoát với mã lỗi để kiểm tra tự động:
    // Unity.exe -batchmode -projectPath "<project>" -executeMethod SetupGameplayHUD.SetupAndExit -logFile <log>
    public static void SetupAndExit()
    {
        try
        {
            Setup();
            EditorApplication.Exit(Verify() ? 0 : 1);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SetupGameplayHUD] LỖI khi setup: " + e);
            EditorApplication.Exit(1);
        }
    }

    // ===== VERIFY: kiểm tra mọi field HUD đã được gán =====
    public static bool Verify()
    {
        var canvasGO = GameObject.Find("GameplayCanvas");
        var ui = canvasGO != null ? canvasGO.GetComponent<GameplayUI>() : null;
        if (ui == null)
        {
            Debug.LogError("[Verify] ❌ Không tìm thấy GameplayUI trên GameplayCanvas!");
            return false;
        }

        bool ok = true;
        ok &= Check(ui.scorePillBg != null, "scorePillBg");
        ok &= Check(ui.scoreIcon != null, "scoreIcon");
        ok &= Check(ui.scoreValueText != null, "scoreValueText");
        ok &= Check(ui.coinPillBg != null, "coinPillBg");
        ok &= Check(ui.coinPillIcon != null, "coinPillIcon");
        ok &= Check(ui.coinValueText != null, "coinValueText");
        ok &= Check(ui.pauseButton != null, "pauseButton");
        ok &= Check(ui.pauseButtonIcon != null, "pauseButtonIcon");
        ok &= Check(ui.powerUpPanel != null, "powerUpPanel");
        ok &= Check(ui.powerUpPanelIcon != null, "powerUpPanelIcon");
        ok &= Check(ui.powerUpFillImage != null, "powerUpFillImage");
        ok &= Check(ui.hudPillSprite != null, "hudPillSprite");
        ok &= Check(ui.iconStar != null, "iconStar");
        ok &= Check(ui.iconCoin != null, "iconCoin");
        ok &= Check(ui.iconPause != null, "iconPause");
        ok &= Check(ui.iconShield != null, "iconShield (HUD)");
        ok &= Check(ui.iconBoots != null, "iconBoots (HUD)");
        ok &= Check(ui.iconRocket != null, "iconRocket (HUD)");
        ok &= Check(ui.hudFont != null, "hudFont");

        // SỬA LỖI (2026-10-10): Transform.Find("PowerUpBarFill") chỉ tìm CON TRỰC TIẾP của
        // powerUpPanel, nhưng PowerUpBarFill thực ra là con của PowerUpBarTrack (tức CHÁU của
        // powerUpPanel) → Find() luôn trả về null dù powerUpFillImage đã được gán đúng ở trên,
        // khiến Verify() báo lỗi giả "Thiếu: PowerUpBarFill". Dùng path đầy đủ qua PowerUpBarTrack.
        if (ui.powerUpPanel != null)
            ok &= Check(ui.powerUpPanel.transform.Find("PowerUpBarTrack/PowerUpBarFill") != null, "PowerUpBarFill (trong PowerUpBarTrack)");

        return ok;
    }

    static bool Check(bool condition, string label)
    {
        if (!condition) Debug.LogError("[Verify] ❌ Thiếu: " + label);
        return condition;
    }

    // ===== Helpers =====
    static void CleanupOldHUD(GameObject canvasGO)
    {
        // Xóa UI cũ nếu chạy lại tool (idempotent)
        string[] newHudNames = { "ScorePill", "CoinPill", "PauseButton", "PowerUpPanel" };
        foreach (var name in newHudNames)
        {
            var old = canvasGO.transform.Find(name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        // ẩn text cũ (giữ nguyên object + field reference)
        var oldScore = canvasGO.transform.Find("ScoreText");
        if (oldScore != null) oldScore.gameObject.SetActive(false);

        var oldCoin = canvasGO.transform.Find("CoinText");
        if (oldCoin != null) oldCoin.gameObject.SetActive(false);

        var oldPowerUp = canvasGO.transform.Find("PowerUpText");
        if (oldPowerUp != null) oldPowerUp.gameObject.SetActive(false);
    }

    static void AssignLegacyRefs(GameplayUI ui, GameObject canvasGO)
    {
        var scoreText = canvasGO.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
        var coinText = canvasGO.transform.Find("CoinText")?.GetComponent<TMP_Text>();
        var powerUpText = canvasGO.transform.Find("PowerUpText")?.GetComponent<TMP_Text>();
        var gameOverPanel = canvasGO.transform.Find("GameOverPanel")?.gameObject;
        var finalScoreText = canvasGO.transform.Find("GameOverPanel/FinalScoreText")?.GetComponent<TMP_Text>();
        var finalCoinText = canvasGO.transform.Find("GameOverPanel/FinalCoinText")?.GetComponent<TMP_Text>();

        if (scoreText != null && ui.scoreText == null) ui.scoreText = scoreText;
        if (coinText != null && ui.coinText == null) ui.coinText = coinText;
        if (powerUpText != null && ui.powerUpText == null) ui.powerUpText = powerUpText;
        if (gameOverPanel != null && ui.gameOverPanel == null) ui.gameOverPanel = gameOverPanel;
        if (finalScoreText != null && ui.finalScoreText == null) ui.finalScoreText = finalScoreText;
        if (finalCoinText != null && ui.finalCoinText == null) ui.finalCoinText = finalCoinText;

        if (ui.playerPowerUps == null)
            ui.playerPowerUps = Object.FindAnyObjectByType<PowerUpManager>();
    }

    static GameObject CreatePill(string name, Transform parent, Sprite sprite,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0f, 0f, 0f, 0.6f);
        img.raycastTarget = false;
        return go;
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = (anchorMin + anchorMax) * 0.5f;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    // Tạo TextMeshProUGUI (class CON — KHÔNG dùng TMP_Text abstract như bản cũ: causes crash)
    static TMP_Text CreateTMPText(string name, Transform parent, TMP_FontAsset font, Material outlineMat,
        string text, float fontSize, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 rectSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = (anchorMin + anchorMax) * 0.5f;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = rectSize;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        if (outlineMat != null) tmp.fontSharedMaterial = outlineMat;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.enableVertexGradient = false;
        tmp.alignment = TextAlignmentOptions.Midline;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void ApplyStyle(TMP_Text t, TMP_FontAsset font, Material outlineMat)
    {
        if (t == null) return;
        if (font != null) t.font = font;
        if (outlineMat != null) t.fontSharedMaterial = outlineMat;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;
        t.enableVertexGradient = false;
        t.raycastTarget = false;
    }
}