#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

public static class MainMenuBuilder
{
    private const string SCENE_PATH = "Assets/_Game/Scenes/MainMenu.unity";
    private const string DATABASE_PATH = "Assets/_Game/Data/SkinDatabase.asset";
    private const string TRACK_PREFAB_PATH = "Assets/_Game/Prefabs/Track/TrackTile_Straight.prefab";
    private const string SHOP_ITEM_PREFAB_PATH = "Assets/_Game/Prefabs/UI/ShopItemRow.prefab";

    // Asset paths for UI Sprites
    private const string LOGO_PATH = "Assets/_Game/Art/UI/Logo/Logo_HeroRush.png";
    private const string BTN_PLAY_PATH = "Assets/_Game/Art/UI/Buttons/Btn_Play.png";
    private const string BTN_SHOP_PATH = "Assets/_Game/Art/UI/Buttons/Btn_Shop.png";
    private const string COIN_ICON_PATH = "Assets/_Game/Art/UI/Icons/Icon_Coin.png";
    private const string FRAME_GOLD_PATH = "Assets/_Game/Art/UI/Frames/Frame_GoldBorder.png";
    private const string PANEL_KHUNGMO_PATH = "Assets/_Game/Art/UI/Frames/Panel_KhungMo.png";
    private const string SPEAKER_ICON_PATH = "Assets/_Game/Art/UI/Slider/Icon_Speaker.png";
    private const string SLIDER_TRACK_PATH = "Assets/_Game/Art/UI/Slider/Slider_Track.png";
    private const string SLIDER_FILL_PATH = "Assets/_Game/Art/UI/Slider/Slider_Fill.png";
    private const string SLIDER_HANDLE_PATH = "Assets/_Game/Art/UI/Slider/Slider_Handle.png";

    [MenuItem("GameTools/Rebuild MainMenu Scene")]
    public static void BuildMainMenu()
    {
        Debug.Log("=== BẮT ĐẦU DỰNG MENU THEO SƠ ĐỒ ===");

        // 1. Configure texture import settings
        ConfigureSpriteImporter(LOGO_PATH, Vector4.zero);
        ConfigureSpriteImporter(BTN_PLAY_PATH, Vector4.zero);
        ConfigureSpriteImporter(BTN_SHOP_PATH, Vector4.zero);
        ConfigureSpriteImporter(COIN_ICON_PATH, Vector4.zero);
        ConfigureSpriteImporter(SPEAKER_ICON_PATH, Vector4.zero);
        ConfigureSpriteImporter(SLIDER_HANDLE_PATH, Vector4.zero);
        ConfigureSpriteImporter(FRAME_GOLD_PATH, new Vector4(48, 48, 48, 48));
        ConfigureSpriteImporter(PANEL_KHUNGMO_PATH, new Vector4(44, 44, 44, 44));
        ConfigureSpriteImporter(SLIDER_TRACK_PATH, new Vector4(32, 16, 32, 16));
        ConfigureSpriteImporter(SLIDER_FILL_PATH, new Vector4(26, 12, 26, 12));

        AssetDatabase.Refresh();

        // 2. Open MainMenu scene
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 3. Setup Camera & Lighting for Gameplay background
        SetupEnvironmentAndCamera();

        // 4. Setup 3D Gameplay track and 3D Character
        SetupGameplay3DBackground();

        // 5. Setup Canvas and UI according to the diagram
        SetupDiagramUI();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        CaptureScreenshot();

        Debug.Log("=== HOÀN THÀNH DỰNG MENU THEO SƠ ĐỒ THÀNH CÔNG 100%! ===");
    }

    private static void ConfigureSpriteImporter(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (border != Vector4.zero && importer.spriteBorder != border)
        {
            importer.spriteBorder = border;
            changed = true;
        }

        if (importer.alphaIsTransparency != true)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp != null) return sp;

        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        if (all != null)
        {
            foreach (Object o in all)
            {
                if (o is Sprite s) return s;
            }
        }
        return null;
    }

    private static void SetupEnvironmentAndCamera()
    {
        // Directional Light
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null) lightObj = new GameObject("Directional Light");
        Light light = lightObj.GetComponent<Light>();
        if (light == null) light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.88f);
        light.intensity = 1.3f;
        lightObj.transform.position = new Vector3(0, 10, -5);
        lightObj.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

        // Main Camera
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj == null) camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.GetComponent<Camera>();
        if (cam == null) cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 500f;

        // Position camera to look down the track framing the 3D character nicely
        camObj.transform.position = new Vector3(0f, 2.3f, -4.5f);
        camObj.transform.rotation = Quaternion.Euler(11f, 0f, 0f);

        if (camObj.GetComponent<AudioListener>() == null)
            camObj.AddComponent<AudioListener>();
    }

    private static void SetupGameplay3DBackground()
    {
        // 1. Road Track
        GameObject envObj = GameObject.Find("GameplayEnvironment");
        if (envObj != null) Object.DestroyImmediate(envObj);
        envObj = new GameObject("GameplayEnvironment");
        envObj.transform.position = Vector3.zero;

        GameObject trackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TRACK_PREFAB_PATH);
        if (trackPrefab != null)
        {
            // Spawn consecutive track tiles to create long road into horizon
            float[] zOffsets = new float[] { -20f, 0f, 20f, 40f, 60f, 80f };
            foreach (float z in zOffsets)
            {
                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(trackPrefab, envObj.transform);
                tile.transform.position = new Vector3(0f, 0f, z);
                tile.transform.rotation = Quaternion.identity;
            }
        }

        // 2. 3D Character (Nhân vật 3D)
        GameObject charObj = GameObject.Find("CharacterPreview");
        if (charObj != null) Object.DestroyImmediate(charObj);
        charObj = new GameObject("CharacterPreview");
        charObj.transform.position = new Vector3(0f, 0f, 0.4f);
        // Face forward / towards camera (180 deg) as shown in the diagram
        charObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        GameObject modelSlot = new GameObject("ModelSlot");
        modelSlot.transform.SetParent(charObj.transform, false);
        modelSlot.transform.localPosition = new Vector3(0f, 0f, 0f);
        modelSlot.transform.localRotation = Quaternion.identity;

        PlayerSkinApplier skinApplier = charObj.AddComponent<PlayerSkinApplier>();
        skinApplier.database = AssetDatabase.LoadAssetAtPath<SkinDatabase>(DATABASE_PATH);
        skinApplier.modelParent = modelSlot.transform;
        skinApplier.ApplySelectedSkin();
    }

    private static void SetupDiagramUI()
    {
        // Canvas setup
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f; // Match width for mobile portrait

        MainMenuUI menuUI = canvas.GetComponent<MainMenuUI>();
        if (menuUI == null) menuUI = canvas.gameObject.AddComponent<MainMenuUI>();

        // Link CharacterPreview to MainMenuUI
        GameObject charObj = GameObject.Find("CharacterPreview");
        if (charObj != null) menuUI.characterPreviewApplier = charObj.GetComponent<PlayerSkinApplier>();

        // Clean up any old duplicate CoinText directly on Canvas root
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform c = canvas.transform.GetChild(i);
            if (c.name == "CoinText")
                Object.DestroyImmediate(c.gameObject);
        }

        // Ensure EventSystem
        EnsureEventSystem();

        // 1. Viền khung (Golden border frame)
        SetupBorderFrame(canvas.transform);

        // 2. Số xu hiện có (Coin HUD at top-right)
        TMP_Text coinText = SetupCoinHUD(canvas.transform);
        menuUI.coinText = coinText;

        // 3. Tên game (Logo_HeroRush replacing "endless runner")
        SetupGameLogo(canvas.transform);

        // 4. Khung mờ & Buttons & Volume slider
        Slider volSlider = SetupBottomPanel(canvas.transform, menuUI);
        menuUI.musicVolumeSlider = volSlider;

        // 5. ShopPanel
        SetupShopPanel(canvas.transform, menuUI);
    }

    private static void SetupBorderFrame(Transform parent)
    {
        Transform old = parent.Find("ScreenBorderFrame");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject frameObj = new GameObject("ScreenBorderFrame", typeof(RectTransform), typeof(Image));
        frameObj.transform.SetParent(parent, false);
        frameObj.transform.SetAsFirstSibling(); // behind UI elements

        RectTransform rt = frameObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(24f, 24f);
        rt.offsetMax = new Vector2(-24f, -24f);

        Image img = frameObj.GetComponent<Image>();
        img.sprite = LoadSprite(FRAME_GOLD_PATH);
        img.type = Image.Type.Sliced;
        img.fillCenter = false;
        img.raycastTarget = false;
        img.color = Color.white;
    }

    private static TMP_Text SetupCoinHUD(Transform parent)
    {
        Transform old = parent.Find("CoinHUD");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Parent pill container
        GameObject hudObj = new GameObject("CoinHUD", typeof(RectTransform), typeof(Image));
        hudObj.transform.SetParent(parent, false);

        RectTransform rt = hudObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-55f, -55f);
        rt.sizeDelta = new Vector2(300f, 85f);

        Image bgImg = hudObj.GetComponent<Image>();
        bgImg.sprite = LoadSprite(PANEL_KHUNGMO_PATH);
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.08f, 0.12f, 0.18f, 0.85f);

        // Coin Icon (right side of pill)
        GameObject iconObj = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(hudObj.transform, false);
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(1f, 0.5f);
        iconRt.anchorMax = new Vector2(1f, 0.5f);
        iconRt.pivot = new Vector2(1f, 0.5f);
        iconRt.anchoredPosition = new Vector2(-12f, 0f);
        iconRt.sizeDelta = new Vector2(68f, 68f);

        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.sprite = LoadSprite(COIN_ICON_PATH);
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        // Coin Text
        GameObject txtObj = new GameObject("CoinText", typeof(RectTransform));
        txtObj.transform.SetParent(hudObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0f, 0f);
        txtRt.anchorMax = new Vector2(1f, 1f);
        txtRt.offsetMin = new Vector2(15f, 0f);
        txtRt.offsetMax = new Vector2(-80f, 0f);

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "1250";
        tmp.fontSize = 44;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = new Color(1f, 0.92f, 0.35f, 1f);
        tmp.raycastTarget = false;

        return tmp;
    }

    private static void SetupGameLogo(Transform parent)
    {
        Transform old = parent.Find("GameLogo");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Also remove legacy plain text if any
        Transform oldTitle = parent.Find("GameTitle");
        if (oldTitle != null) Object.DestroyImmediate(oldTitle.gameObject);

        GameObject logoObj = new GameObject("GameLogo", typeof(RectTransform), typeof(Image));
        logoObj.transform.SetParent(parent, false);

        RectTransform rt = logoObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -220f);
        rt.sizeDelta = new Vector2(720f, 320f);

        Image img = logoObj.GetComponent<Image>();
        img.sprite = LoadSprite(LOGO_PATH);
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    private static Slider SetupBottomPanel(Transform parent, MainMenuUI menuUI)
    {
        Transform old = parent.Find("BottomPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Remove old standalone Play/Shop/Volume objects on Canvas root if any
        Transform oldPlay = parent.Find("PlayButton");
        if (oldPlay != null) Object.DestroyImmediate(oldPlay.gameObject);
        Transform oldShop = parent.Find("ShopButton");
        if (oldShop != null) Object.DestroyImmediate(oldShop.gameObject);
        Transform oldVol = parent.Find("MusicVolumeSlider");
        if (oldVol != null) Object.DestroyImmediate(oldVol.gameObject);
        Transform oldVolLbl = parent.Find("VolumeLabel");
        if (oldVolLbl != null) Object.DestroyImmediate(oldVolLbl.gameObject);

        // Khung mờ (BottomPanel)
        GameObject panelObj = new GameObject("BottomPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(parent, false);

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = new Vector2(0f, 90f);
        panelRt.sizeDelta = new Vector2(880f, 660f);

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.sprite = LoadSprite(PANEL_KHUNGMO_PATH);
        panelImg.type = Image.Type.Sliced;
        panelImg.color = Color.white;

        // 1. Nút Play
        GameObject playBtnObj = new GameObject("PlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
        playBtnObj.transform.SetParent(panelObj.transform, false);

        RectTransform playRt = playBtnObj.GetComponent<RectTransform>();
        playRt.anchorMin = new Vector2(0.5f, 1f);
        playRt.anchorMax = new Vector2(0.5f, 1f);
        playRt.pivot = new Vector2(0.5f, 1f);
        playRt.anchoredPosition = new Vector2(0f, -50f);
        playRt.sizeDelta = new Vector2(640f, 200f);

        Image playImg = playBtnObj.GetComponent<Image>();
        playImg.sprite = LoadSprite(BTN_PLAY_PATH);
        playImg.preserveAspect = true;

        Button playBtn = playBtnObj.GetComponent<Button>();
        playBtn.targetGraphic = playImg;
        UnityEventTools.RemovePersistentListener(playBtn.onClick, menuUI.OnClickPlay);
        UnityEventTools.AddPersistentListener(playBtn.onClick, menuUI.OnClickPlay);

        // 2. Nút Shop
        GameObject shopBtnObj = new GameObject("ShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
        shopBtnObj.transform.SetParent(panelObj.transform, false);

        RectTransform shopRt = shopBtnObj.GetComponent<RectTransform>();
        shopRt.anchorMin = new Vector2(0.5f, 1f);
        shopRt.anchorMax = new Vector2(0.5f, 1f);
        shopRt.pivot = new Vector2(0.5f, 1f);
        shopRt.anchoredPosition = new Vector2(0f, -270f);
        shopRt.sizeDelta = new Vector2(640f, 200f);

        Image shopImg = shopBtnObj.GetComponent<Image>();
        shopImg.sprite = LoadSprite(BTN_SHOP_PATH);
        shopImg.preserveAspect = true;

        Button shopBtn = shopBtnObj.GetComponent<Button>();
        shopBtn.targetGraphic = shopImg;
        UnityEventTools.RemovePersistentListener(shopBtn.onClick, menuUI.OnClickOpenShop);
        UnityEventTools.AddPersistentListener(shopBtn.onClick, menuUI.OnClickOpenShop);

        // 3. Thanh âm lượng (Volume Container)
        GameObject volRow = new GameObject("VolumeRow", typeof(RectTransform));
        volRow.transform.SetParent(panelObj.transform, false);

        RectTransform rowRt = volRow.GetComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0f);
        rowRt.anchorMax = new Vector2(0.5f, 0f);
        rowRt.pivot = new Vector2(0.5f, 0f);
        rowRt.anchoredPosition = new Vector2(0f, 50f);
        rowRt.sizeDelta = new Vector2(740f, 90f);

        // Speaker Icon
        GameObject speakerObj = new GameObject("SpeakerIcon", typeof(RectTransform), typeof(Image));
        speakerObj.transform.SetParent(volRow.transform, false);

        RectTransform spkRt = speakerObj.GetComponent<RectTransform>();
        spkRt.anchorMin = new Vector2(0f, 0.5f);
        spkRt.anchorMax = new Vector2(0f, 0.5f);
        spkRt.pivot = new Vector2(0f, 0.5f);
        spkRt.anchoredPosition = new Vector2(10f, 0f);
        spkRt.sizeDelta = new Vector2(72f, 72f);

        Image spkImg = speakerObj.GetComponent<Image>();
        spkImg.sprite = LoadSprite(SPEAKER_ICON_PATH);
        spkImg.preserveAspect = true;
        spkImg.raycastTarget = false;

        // Slider
        GameObject sliderObj = new GameObject("MusicVolumeSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(volRow.transform, false);

        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0f, 0.5f);
        sliderRt.anchorMax = new Vector2(1f, 0.5f);
        sliderRt.pivot = new Vector2(0.5f, 0.5f);
        sliderRt.offsetMin = new Vector2(105f, -24f);
        sliderRt.offsetMax = new Vector2(-10f, 24f);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;

        // Slider Track (Background)
        GameObject trackObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        trackObj.transform.SetParent(sliderObj.transform, false);
        RectTransform trackRt = trackObj.GetComponent<RectTransform>();
        trackRt.anchorMin = Vector2.zero;
        trackRt.anchorMax = Vector2.one;
        trackRt.offsetMin = Vector2.zero;
        trackRt.offsetMax = Vector2.zero;

        Image trackImg = trackObj.GetComponent<Image>();
        trackImg.sprite = LoadSprite(SLIDER_TRACK_PATH);
        trackImg.type = Image.Type.Sliced;
        trackImg.color = Color.white;

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(12f, 6f);
        fillAreaRt.offsetMax = new Vector2(-12f, -6f);

        // Fill Image
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.sprite = LoadSprite(SLIDER_FILL_PATH);
        fillImg.type = Image.Type.Sliced;
        fillImg.color = Color.white;

        // Handle Slide Area
        GameObject handleAreaObj = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleAreaObj.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(16f, 0f);
        handleAreaRt.offsetMax = new Vector2(-16f, 0f);

        // Handle
        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        RectTransform handleRt = handleObj.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(58f, 58f);

        Image handleImg = handleObj.GetComponent<Image>();
        handleImg.sprite = LoadSprite(SLIDER_HANDLE_PATH);
        handleImg.preserveAspect = true;

        slider.targetGraphic = handleImg;
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static void SetupShopPanel(Transform canvas, MainMenuUI menuUI)
    {
        Transform shopPanelTf = canvas.Find("ShopPanel");
        GameObject shopPanelObj = shopPanelTf != null ? shopPanelTf.gameObject : null;
        if (shopPanelObj == null)
        {
            shopPanelObj = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
            shopPanelObj.transform.SetParent(canvas, false);
            RectTransform spRt = shopPanelObj.GetComponent<RectTransform>();
            spRt.anchorMin = Vector2.zero;
            spRt.anchorMax = Vector2.one;
            spRt.offsetMin = Vector2.zero;
            spRt.offsetMax = Vector2.zero;
            shopPanelObj.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.14f, 0.98f);
        }

        menuUI.shopPanel = shopPanelObj;

        ShopManager shopMgr = shopPanelObj.GetComponent<ShopManager>();
        if (shopMgr == null) shopMgr = shopPanelObj.AddComponent<ShopManager>();

        SkinDatabase skinDb = AssetDatabase.LoadAssetAtPath<SkinDatabase>(DATABASE_PATH);
        shopMgr.database = skinDb;

        GameObject itemPrefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(SHOP_ITEM_PREFAB_PATH);
        if (itemPrefabObj != null)
            shopMgr.shopItemPrefab = itemPrefabObj.GetComponent<ShopItemUI>();

        // Wire Close Button
        Transform closeBtnTf = shopPanelObj.transform.Find("CloseButton");
        if (closeBtnTf != null)
        {
            Button closeBtn = closeBtnTf.GetComponent<Button>();
            if (closeBtn != null)
            {
                UnityEventTools.RemovePersistentListener(closeBtn.onClick, menuUI.OnClickCloseShop);
                UnityEventTools.AddPersistentListener(closeBtn.onClick, menuUI.OnClickCloseShop);
            }
        }

        // Wire CoinBalance
        Transform coinBalTf = shopPanelObj.transform.Find("CoinBalance");
        if (coinBalTf != null)
        {
            shopMgr.coinBalanceText = coinBalTf.GetComponent<TextMeshProUGUI>();
        }

        // Wire ScrollView Content
        Transform svTf = shopPanelObj.transform.Find("ScrollView");
        if (svTf != null)
        {
            Transform vp = svTf.Find("Viewport");
            if (vp != null)
            {
                Transform content = vp.Find("Content");
                if (content != null) shopMgr.shopListContent = content;
            }
        }

        shopPanelObj.SetActive(false);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }
    }

    public static void CaptureScreenshot()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (cam == null || canvas == null) return;

        RenderMode prevMode = canvas.renderMode;
        Camera prevCam = canvas.worldCamera;
        float prevPlane = canvas.planeDistance;

        // Temporarily render with ScreenSpaceCamera to capture UI + 3D
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 2f;

        int width = 540;
        int height = 960;
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture prevRT = cam.targetTexture;
        cam.targetTexture = rt;

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = prevRT;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        Object.DestroyImmediate(screenShot);

        string savePath = "Assets/_Game/mainmenu_preview.png";
        File.WriteAllBytes(savePath, bytes);

        // Restore canvas mode
        canvas.renderMode = prevMode;
        canvas.worldCamera = prevCam;
        canvas.planeDistance = prevPlane;

        AssetDatabase.ImportAsset(savePath);
        Debug.Log("[MainMenuBuilder] Đã lưu preview ảnh tại: " + savePath);
    }
}
#endif
