#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEditor.Build;

public static class GameSceneAutoSetup
{
    [MenuItem("GameTools/Setup Game Scenes and UI")]
    public static void SetupAll()
    {
        Debug.Log("=== BẮT ĐẦU THIẾT LẬP SCENE & UI ===");
        
        CreateShopItemPrefab();
        SetupSplashScene();
        SetupLoadingScene();
        SetupMainMenuScene();
        SetupGameplayScene();
        ConfigureProjectSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== HOÀN TẤT THIẾT LẬP SCENE & UI 100%! ===");
    }

    private static void CreateShopItemPrefab()
    {
        string dir = "Assets/_Game/Prefabs/UI";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        string prefabPath = Path.Combine(dir, "ShopItemRow.prefab");

        // Root GameObject
        GameObject root = new GameObject("ShopItemRow", typeof(RectTransform), typeof(Image));
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(800, 130);

        Image bg = root.GetComponent<Image>();
        bg.color = new Color(0.15f, 0.17f, 0.22f, 0.95f);

        ShopItemUI shopItemUI = root.AddComponent<ShopItemUI>();

        // 1. Icon Image
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(root.transform, false);
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0.5f);
        iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(90, 90);
        iconRt.anchoredPosition = new Vector2(65, 0);
        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.color = Color.white;

        // 2. Name Text
        GameObject nameObj = new GameObject("NameText", typeof(RectTransform));
        nameObj.transform.SetParent(root.transform, false);
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.5f);
        nameRt.anchorMax = new Vector2(0f, 0.5f);
        nameRt.pivot = new Vector2(0f, 0.5f);
        nameRt.sizeDelta = new Vector2(350, 45);
        nameRt.anchoredPosition = new Vector2(130, 20);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 32;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = Color.white;
        nameText.text = "Skin Name";

        // 3. Price Text
        GameObject priceObj = new GameObject("PriceText", typeof(RectTransform));
        priceObj.transform.SetParent(root.transform, false);
        RectTransform priceRt = priceObj.GetComponent<RectTransform>();
        priceRt.anchorMin = new Vector2(0f, 0.5f);
        priceRt.anchorMax = new Vector2(0f, 0.5f);
        priceRt.pivot = new Vector2(0f, 0.5f);
        priceRt.sizeDelta = new Vector2(350, 35);
        priceRt.anchoredPosition = new Vector2(130, -22);
        TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
        priceText.fontSize = 26;
        priceText.alignment = TextAlignmentOptions.Left;
        priceText.color = new Color(1f, 0.85f, 0.2f, 1f); // Gold
        priceText.text = "100";

        // 4. Action Button
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject buttonObj = DefaultControls.CreateButton(res);
        buttonObj.name = "ActionButton";
        buttonObj.transform.SetParent(root.transform, false);
        RectTransform btnRt = buttonObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 0.5f);
        btnRt.anchorMax = new Vector2(1f, 0.5f);
        btnRt.pivot = new Vector2(1f, 0.5f);
        btnRt.sizeDelta = new Vector2(170, 60);
        btnRt.anchoredPosition = new Vector2(-30, 0);

        Button btn = buttonObj.GetComponent<Button>();
        Image btnImg = buttonObj.GetComponent<Image>();
        btnImg.color = new Color(0.18f, 0.65f, 0.35f, 1f); // Green

        // Replace default Text inside button with TextMeshProUGUI
        Text legacyBtnText = buttonObj.GetComponentInChildren<Text>();
        if (legacyBtnText != null) Object.DestroyImmediate(legacyBtnText.gameObject);

        GameObject btnTextObj = new GameObject("Text (TMP)", typeof(RectTransform));
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.fontSize = 28;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        btnText.text = "Chọn";

        // Connect references
        shopItemUI.iconImage = iconImg;
        shopItemUI.nameText = nameText;
        shopItemUI.priceText = priceText;
        shopItemUI.actionButton = btn;
        shopItemUI.actionButtonText = btnText;

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("[GameSceneAutoSetup] Đã tạo Prefab ShopItemRow thành công tại: " + prefabPath);
    }

    private static void SetupSplashScene()
    {
        string scenePath = "Assets/_Game/Scenes/Splash.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EnsureEventSystem();

        // Canvas
        Canvas canvas = EnsureCanvas("Canvas");

        // Background
        Transform oldBg = canvas.transform.Find("Background");
        if (oldBg != null) Object.DestroyImmediate(oldBg.gameObject);

        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvas.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.08f, 0.09f, 0.14f, 1f);

        // Title
        Transform oldTitle = canvas.transform.Find("TitleText");
        if (oldTitle != null) Object.DestroyImmediate(oldTitle.gameObject);

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(canvas.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(900, 160);
        titleRt.anchoredPosition = new Vector2(0, 100);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "3D ENDLESS RUNNER";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.85f, 0.2f, 1f); // Gold

        // Subtitle
        Transform oldSub = canvas.transform.Find("SubtitleText");
        if (oldSub != null) Object.DestroyImmediate(oldSub.gameObject);

        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform));
        subObj.transform.SetParent(canvas.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.5f);
        subRt.anchorMax = new Vector2(0.5f, 0.5f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.sizeDelta = new Vector2(800, 80);
        subRt.anchoredPosition = new Vector2(0, -20);
        TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
        subText.text = "GET READY TO RUN!";
        subText.fontSize = 36;
        subText.alignment = TextAlignmentOptions.Center;
        subText.color = new Color(0.85f, 0.88f, 0.95f, 0.9f);

        // SplashScreen GameObject
        GameObject splashObj = GameObject.Find("SplashScreen");
        if (splashObj == null) splashObj = new GameObject("SplashScreen");
        SplashScreen splash = splashObj.GetComponent<SplashScreen>();
        if (splash == null) splash = splashObj.AddComponent<SplashScreen>();
        splash.displayTime = 2f;

        // AudioManager GameObject
        GameObject audioObj = GameObject.Find("AudioManager");
        if (audioObj == null) audioObj = new GameObject("AudioManager");
        AudioSource audioSource = audioObj.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;

        AudioManager audioMgr = audioObj.GetComponent<AudioManager>();
        if (audioMgr == null) audioMgr = audioObj.AddComponent<AudioManager>();
        audioMgr.musicSource = audioSource;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneAutoSetup] Đã dựng scene Splash thành công!");
    }

    private static void SetupLoadingScene()
    {
        string scenePath = "Assets/_Game/Scenes/Loading.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EnsureEventSystem();

        Canvas canvas = EnsureCanvas("Canvas");

        // Background
        Transform oldBg = canvas.transform.Find("Background");
        if (oldBg != null) Object.DestroyImmediate(oldBg.gameObject);

        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvas.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.08f, 0.09f, 0.14f, 1f);

        // Loading Text
        Transform oldTxt = canvas.transform.Find("LoadingText");
        if (oldTxt != null) Object.DestroyImmediate(oldTxt.gameObject);

        GameObject txtObj = new GameObject("LoadingText", typeof(RectTransform));
        txtObj.transform.SetParent(canvas.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0.5f, 0.5f);
        txtRt.anchorMax = new Vector2(0.5f, 0.5f);
        txtRt.pivot = new Vector2(0.5f, 0.5f);
        txtRt.sizeDelta = new Vector2(600, 100);
        txtRt.anchoredPosition = new Vector2(0, 120);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = "LOADING...";
        txt.fontSize = 54;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        // ProgressBar Slider
        Transform oldSlider = canvas.transform.Find("ProgressBar");
        if (oldSlider != null) Object.DestroyImmediate(oldSlider.gameObject);

        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject sliderObj = DefaultControls.CreateSlider(res);
        sliderObj.name = "ProgressBar";
        sliderObj.transform.SetParent(canvas.transform, false);
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRt.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRt.pivot = new Vector2(0.5f, 0.5f);
        sliderRt.sizeDelta = new Vector2(750, 45);
        sliderRt.anchoredPosition = new Vector2(0, -60);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        // Remove handle for a clean flat progress bar
        Transform handle = sliderObj.transform.Find("Handle Slide Area");
        if (handle != null) Object.DestroyImmediate(handle.gameObject);

        // LoadingScreen GameObject
        GameObject lmObj = GameObject.Find("LoadingManager");
        if (lmObj == null) lmObj = new GameObject("LoadingManager");
        LoadingScreen loadingScreen = lmObj.GetComponent<LoadingScreen>();
        if (loadingScreen == null) loadingScreen = lmObj.AddComponent<LoadingScreen>();
        loadingScreen.progressBar = slider;
        loadingScreen.minimumDisplayTime = 1f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneAutoSetup] Đã dựng scene Loading thành công!");
    }

    private static void SetupMainMenuScene()
    {
        string scenePath = "Assets/_Game/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) canvas = EnsureCanvas("Canvas");

        MainMenuUI menuUI = canvas.GetComponent<MainMenuUI>();
        if (menuUI == null) menuUI = canvas.gameObject.AddComponent<MainMenuUI>();

        // 1. CoinText wiring
        GameObject coinTextObj = GameObject.Find("CoinText");
        if (coinTextObj != null)
        {
            TMP_Text tmp = coinTextObj.GetComponent<TMP_Text>();
            if (tmp != null) menuUI.coinText = tmp;
        }

        // 2. PlayButton wiring
        GameObject playBtnObj = GameObject.Find("PlayButton");
        if (playBtnObj != null)
        {
            Button playBtn = playBtnObj.GetComponent<Button>();
            if (playBtn != null)
            {
                UnityEventTools.RemovePersistentListener(playBtn.onClick, menuUI.OnClickPlay);
                UnityEventTools.AddPersistentListener(playBtn.onClick, menuUI.OnClickPlay);
            }
        }

        // 3. ShopButton wiring
        GameObject shopBtnObj = GameObject.Find("ShopButton");
        if (shopBtnObj != null)
        {
            Button shopBtn = shopBtnObj.GetComponent<Button>();
            if (shopBtn != null)
            {
                UnityEventTools.RemovePersistentListener(shopBtn.onClick, menuUI.OnClickOpenShop);
                UnityEventTools.AddPersistentListener(shopBtn.onClick, menuUI.OnClickOpenShop);
            }
        }

        // 4. ShopPanel setup
        Transform shopPanelTf = canvas.transform.Find("ShopPanel");
        GameObject shopPanelObj = shopPanelTf != null ? shopPanelTf.gameObject : null;
        if (shopPanelObj == null)
        {
            shopPanelObj = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
            shopPanelObj.transform.SetParent(canvas.transform, false);
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

        // SkinDatabase asset
        SkinDatabase skinDb = AssetDatabase.LoadAssetAtPath<SkinDatabase>("Assets/_Game/Data/SkinDatabase.asset");
        shopMgr.database = skinDb;

        // ShopItemRow prefab
        GameObject itemPrefabObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/UI/ShopItemRow.prefab");
        if (itemPrefabObj != null)
            shopMgr.shopItemPrefab = itemPrefabObj.GetComponent<ShopItemUI>();

        // Shop Title
        Transform oldShopTitle = shopPanelObj.transform.Find("ShopTitle");
        if (oldShopTitle != null) Object.DestroyImmediate(oldShopTitle.gameObject);

        GameObject shopTitleObj = new GameObject("ShopTitle", typeof(RectTransform));
        shopTitleObj.transform.SetParent(shopPanelObj.transform, false);
        RectTransform titleRt = shopTitleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(600, 80);
        titleRt.anchoredPosition = new Vector2(0, -60);
        TextMeshProUGUI shopTitle = shopTitleObj.AddComponent<TextMeshProUGUI>();
        shopTitle.text = "CỬA HÀNG SKIN";
        shopTitle.fontSize = 52;
        shopTitle.fontStyle = FontStyles.Bold;
        shopTitle.alignment = TextAlignmentOptions.Center;
        shopTitle.color = new Color(1f, 0.85f, 0.2f, 1f);

        // CoinBalance in ShopPanel
        Transform oldCoinBal = shopPanelObj.transform.Find("CoinBalance");
        if (oldCoinBal != null) Object.DestroyImmediate(oldCoinBal.gameObject);

        GameObject coinBalObj = new GameObject("CoinBalance", typeof(RectTransform));
        coinBalObj.transform.SetParent(shopPanelObj.transform, false);
        RectTransform coinBalRt = coinBalObj.GetComponent<RectTransform>();
        coinBalRt.anchorMin = new Vector2(0.5f, 1f);
        coinBalRt.anchorMax = new Vector2(0.5f, 1f);
        coinBalRt.pivot = new Vector2(0.5f, 1f);
        coinBalRt.sizeDelta = new Vector2(500, 50);
        coinBalRt.anchoredPosition = new Vector2(0, -150);
        TextMeshProUGUI coinBalText = coinBalObj.AddComponent<TextMeshProUGUI>();
        coinBalText.text = "0";
        coinBalText.fontSize = 38;
        coinBalText.alignment = TextAlignmentOptions.Center;
        coinBalText.color = new Color(1f, 0.9f, 0.3f, 1f);
        shopMgr.coinBalanceText = coinBalText;

        // CloseButton
        Transform oldClose = shopPanelObj.transform.Find("CloseButton");
        if (oldClose != null) Object.DestroyImmediate(oldClose.gameObject);

        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject closeBtnObj = DefaultControls.CreateButton(res);
        closeBtnObj.name = "CloseButton";
        closeBtnObj.transform.SetParent(shopPanelObj.transform, false);
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1f, 1f);
        closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot = new Vector2(1f, 1f);
        closeRt.sizeDelta = new Vector2(90, 90);
        closeRt.anchoredPosition = new Vector2(-40, -50);

        Button closeBtn = closeBtnObj.GetComponent<Button>();
        Image closeImg = closeBtnObj.GetComponent<Image>();
        closeImg.color = new Color(0.85f, 0.25f, 0.25f, 1f); // Red
        UnityEventTools.RemovePersistentListener(closeBtn.onClick, menuUI.OnClickCloseShop);
        UnityEventTools.AddPersistentListener(closeBtn.onClick, menuUI.OnClickCloseShop);

        Text oldCloseTxt = closeBtnObj.GetComponentInChildren<Text>();
        if (oldCloseTxt != null) Object.DestroyImmediate(oldCloseTxt.gameObject);
        GameObject closeTxtObj = new GameObject("Text (TMP)", typeof(RectTransform));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform cTxtRt = closeTxtObj.GetComponent<RectTransform>();
        cTxtRt.anchorMin = Vector2.zero;
        cTxtRt.anchorMax = Vector2.one;
        cTxtRt.sizeDelta = Vector2.zero;
        cTxtRt.offsetMin = Vector2.zero;
        cTxtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
        closeTxt.text = "X";
        closeTxt.fontSize = 44;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        // ScrollView for Skins
        Transform oldSv = shopPanelObj.transform.Find("ScrollView");
        if (oldSv != null) Object.DestroyImmediate(oldSv.gameObject);

        GameObject svObj = DefaultControls.CreateScrollView(res);
        svObj.name = "ScrollView";
        svObj.transform.SetParent(shopPanelObj.transform, false);
        RectTransform svRt = svObj.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0.5f, 0.5f);
        svRt.anchorMax = new Vector2(0.5f, 0.5f);
        svRt.pivot = new Vector2(0.5f, 0.5f);
        svRt.sizeDelta = new Vector2(900, 1300);
        svRt.anchoredPosition = new Vector2(0, -80);

        ScrollRect sr = svObj.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;

        // Remove horizontal scrollbar
        Transform hScroll = svObj.transform.Find("Scrollbar Horizontal");
        if (hScroll != null) Object.DestroyImmediate(hScroll.gameObject);

        // Content
        Transform viewport = svObj.transform.Find("Viewport");
        Transform content = viewport != null ? viewport.Find("Content") : null;
        if (content != null)
        {
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            shopMgr.shopListContent = content;
        }

        // Hide ShopPanel by default
        shopPanelObj.SetActive(false);

        // 5. MusicVolumeSlider setup in MainMenu Canvas
        Transform oldVol = canvas.transform.Find("MusicVolumeSlider");
        if (oldVol != null) Object.DestroyImmediate(oldVol.gameObject);

        GameObject volObj = DefaultControls.CreateSlider(res);
        volObj.name = "MusicVolumeSlider";
        volObj.transform.SetParent(canvas.transform, false);
        RectTransform volRt = volObj.GetComponent<RectTransform>();
        volRt.anchorMin = new Vector2(0.5f, 0f);
        volRt.anchorMax = new Vector2(0.5f, 0f);
        volRt.pivot = new Vector2(0.5f, 0f);
        volRt.sizeDelta = new Vector2(600, 40);
        volRt.anchoredPosition = new Vector2(0, 180);

        Slider volSlider = volObj.GetComponent<Slider>();
        volSlider.minValue = 0f;
        volSlider.maxValue = 1f;
        volSlider.value = 0.7f;
        menuUI.musicVolumeSlider = volSlider;

        // Label above volume slider
        Transform oldVolLabel = canvas.transform.Find("VolumeLabel");
        if (oldVolLabel != null) Object.DestroyImmediate(oldVolLabel.gameObject);

        GameObject lblObj = new GameObject("VolumeLabel", typeof(RectTransform));
        lblObj.transform.SetParent(canvas.transform, false);
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0.5f, 0f);
        lblRt.anchorMax = new Vector2(0.5f, 0f);
        lblRt.pivot = new Vector2(0.5f, 0f);
        lblRt.sizeDelta = new Vector2(400, 35);
        lblRt.anchoredPosition = new Vector2(0, 230);
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.text = "Âm lượng nhạc";
        lbl.fontSize = 26;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color = new Color(0.9f, 0.9f, 0.95f, 0.9f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneAutoSetup] Đã dựng scene MainMenu & ShopPanel thành công!");
    }

    private static void SetupGameplayScene()
    {
        string scenePath = "Assets/_Game/Scenes/Gameplay.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject playerObj = null;
        GameObject canvasObj = null;
        GameObject gmObj = null;

        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "Player") playerObj = go;
            else if (go.name == "GameplayCanvas") canvasObj = go;
            else if (go.name == "GameManager") gmObj = go;
        }

        if (gmObj == null) gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.GetComponent<GameManager>();
        if (gm == null) gm = gmObj.AddComponent<GameManager>();

        if (playerObj != null) gm.player = playerObj.transform;

        if (canvasObj != null)
        {
            canvasObj.SetActive(true); // Must be active so UI renders!
            GameplayUI gameplayUI = canvasObj.GetComponent<GameplayUI>();
            if (gameplayUI == null) gameplayUI = canvasObj.AddComponent<GameplayUI>();
            gm.gameplayUI = gameplayUI;

            // Wire text & panel fields
            Transform scoreTf = canvasObj.transform.Find("ScoreText");
            if (scoreTf != null) gameplayUI.scoreText = scoreTf.GetComponent<TMP_Text>();

            Transform coinTf = canvasObj.transform.Find("CoinText");
            if (coinTf != null) gameplayUI.coinText = coinTf.GetComponent<TMP_Text>();

            Transform powerTf = canvasObj.transform.Find("PowerUpText");
            if (powerTf != null) gameplayUI.powerUpText = powerTf.GetComponent<TMP_Text>();

            Transform goPanelTf = canvasObj.transform.Find("GameOverPanel");
            if (goPanelTf != null)
            {
                gameplayUI.gameOverPanel = goPanelTf.gameObject;

                Transform fScoreTf = goPanelTf.Find("FinalScoreText");
                if (fScoreTf != null) gameplayUI.finalScoreText = fScoreTf.GetComponent<TMP_Text>();

                Transform fCoinTf = goPanelTf.Find("FinalCoinText");
                if (fCoinTf != null) gameplayUI.finalCoinText = fCoinTf.GetComponent<TMP_Text>();

                Transform restartBtnTf = goPanelTf.Find("RestartButton");
                if (restartBtnTf != null)
                {
                    Button restartBtn = restartBtnTf.GetComponent<Button>();
                    if (restartBtn != null)
                    {
                        UnityEventTools.RemovePersistentListener(restartBtn.onClick, gameplayUI.OnClickRestart);
                        UnityEventTools.AddPersistentListener(restartBtn.onClick, gameplayUI.OnClickRestart);
                    }
                }

                Transform menuBtnTf = goPanelTf.Find("MenuButton");
                if (menuBtnTf != null)
                {
                    Button menuBtn = menuBtnTf.GetComponent<Button>();
                    if (menuBtn != null)
                    {
                        UnityEventTools.RemovePersistentListener(menuBtn.onClick, gameplayUI.OnClickMenu);
                        UnityEventTools.AddPersistentListener(menuBtn.onClick, gameplayUI.OnClickMenu);
                    }
                }

                // Deactivate GameOverPanel by default
                goPanelTf.gameObject.SetActive(false);
            }

            if (playerObj != null)
            {
                PowerUpManager pum = playerObj.GetComponent<PowerUpManager>();
                if (pum != null) gameplayUI.playerPowerUps = pum;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneAutoSetup] Đã dựng scene Gameplay & GameManager thành công!");
    }

    private static void ConfigureProjectSettings()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        PlayerSettings.productName = "3D Endless Runner";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.student.endlessrunner3d");
        
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

        Debug.Log("[GameSceneAutoSetup] Đã cấu hình PlayerSettings cho Android thành công!");
    }

    private static Canvas EnsureCanvas(string name)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
#endif
