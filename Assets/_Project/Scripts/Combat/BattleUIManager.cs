using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    private GameObject actionMenu;
    private GameObject keyMapPanel;
    private Text infoText;
    private Transform activeActorTransform;
    
    private void Update()
    {
        if (actionMenu != null && actionMenu.activeSelf && activeActorTransform != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(activeActorTransform.position + Vector3.right * 1f + Vector3.up * 0.5f);
            actionMenu.GetComponent<RectTransform>().position = screenPos;
        }

        // F1 Toggle Key Map Panel
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (keyMapPanel != null)
                keyMapPanel.SetActive(!keyMapPanel.activeSelf);
        }

        // Xử lý phím Backspace để đóng menu Skill/Item
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (skillMenu != null && skillMenu.activeSelf)
            {
                ShowSkillMenu(false);
                if (actionButtons.ContainsKey(ActionType.ATTACK)) EventSystem.current.SetSelectedGameObject(actionButtons[ActionType.ATTACK].gameObject);
            }
            else if (itemMenu != null && itemMenu.activeSelf)
            {
                ShowItemMenu(false);
                if (actionButtons.ContainsKey(ActionType.ATTACK)) EventSystem.current.SetSelectedGameObject(actionButtons[ActionType.ATTACK].gameObject);
            }
            else if (actionMenu != null && actionMenu.activeSelf)
            {
                // Nếu đang ở action menu mà ấn back, thì báo cho BattleManager huỷ chọn nhân vật
                if (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.PLAYER_TURN)
                {
                    BattleManager.Instance.CancelActorSelection();
                }
            }
        }
    }
    
    // HUD
    private GameObject alliesHud;
    private GameObject bossHud;
    private Text bossNameText;
    private RectTransform bossHpFillRect;
    private Text bossHpText;
    private Image[] bossLimitSegments;
    private Dictionary<CharacterInteraction, RectTransform> allyHpFills = new Dictionary<CharacterInteraction, RectTransform>();
    private Dictionary<CharacterInteraction, Text> allyHpTexts = new Dictionary<CharacterInteraction, Text>();

    // SOUL UI
    public GameObject soulVessel;
    private Text soulText;
    private List<Image> soulIcons = new List<Image>();

    private void Awake()
    {
        Instance = this;
    }

    // Phải gọi sau khi BattleManager đã gom đủ Allies
    private Dictionary<ActionType, Image> actionButtons = new Dictionary<ActionType, Image>();

    private void CreateButton(string label, ActionType type, Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);
        
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        actionButtons[type] = btnImg;

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => OnActionButtonClicked(type));

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 16;
        txt.color = Color.white;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
    }

    public void SetupUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            // Sử dụng module của hệ thống Input mới
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        GameObject canvasObj = new GameObject("BattleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- ALLIES HUD (Bottom Center, HP bars) ---
        alliesHud = new GameObject("AlliesHUD");
        alliesHud.transform.SetParent(canvasObj.transform, false);
        RectTransform alliesRect = alliesHud.AddComponent<RectTransform>();
        alliesRect.anchorMin = new Vector2(0.5f, 0f);
        alliesRect.anchorMax = new Vector2(0.5f, 0f);
        alliesRect.pivot = new Vector2(0.5f, 0f);
        alliesRect.anchoredPosition = new Vector2(0f, 160f); // Nằm trên các Action Plan
        alliesRect.sizeDelta = new Vector2(600, 60);

        HorizontalLayoutGroup hLayout = alliesHud.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 5;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlHeight = true;
        hLayout.childControlWidth = true;
        hLayout.childForceExpandWidth = false;

        foreach (var ally in BattleManager.Instance.allies)
        {
            CreateAllyHUDBlock(ally, alliesHud.transform);
        }

        // --- BOSS HUD (Top Center, hidden by default) ---
        bossHud = new GameObject("BossHUD");
        bossHud.transform.SetParent(canvasObj.transform, false);
        Image bossBg = bossHud.AddComponent<Image>();
        bossBg.color = new Color(0.2f, 0, 0, 0.8f);

        RectTransform bossRect = bossHud.GetComponent<RectTransform>();
        bossRect.anchorMin = new Vector2(0.5f, 1);
        bossRect.anchorMax = new Vector2(0.5f, 1);
        bossRect.pivot = new Vector2(0.5f, 1);
        bossRect.anchoredPosition = new Vector2(0, -10);
        bossRect.sizeDelta = new Vector2(300, 50);

        GameObject bNameObj = new GameObject("BossName");
        bNameObj.transform.SetParent(bossHud.transform, false);
        bossNameText = bNameObj.AddComponent<Text>();
        bossNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bossNameText.alignment = TextAnchor.UpperCenter;
        bossNameText.fontSize = 18;
        bossNameText.color = Color.white;
        RectTransform bNameRect = bNameObj.GetComponent<RectTransform>();
        bNameRect.anchorMin = new Vector2(0, 0.5f);
        bNameRect.anchorMax = new Vector2(1, 1);
        bNameRect.offsetMin = Vector2.zero;
        bNameRect.offsetMax = Vector2.zero;

        GameObject bHpBg = new GameObject("HP_BG");
        bHpBg.transform.SetParent(bossHud.transform, false);
        Image bHpBgImg = bHpBg.AddComponent<Image>();
        bHpBgImg.color = Color.black;
        RectTransform bHpBgRect = bHpBg.GetComponent<RectTransform>();
        bHpBgRect.anchorMin = new Vector2(0.1f, 0.1f);
        bHpBgRect.anchorMax = new Vector2(0.9f, 0.4f);
        bHpBgRect.offsetMin = Vector2.zero;
        bHpBgRect.offsetMax = Vector2.zero;

        GameObject bHpFillObj = new GameObject("HP_Fill");
        bHpFillObj.transform.SetParent(bHpBg.transform, false);
        Image bHpFillImg = bHpFillObj.AddComponent<Image>();
        bHpFillImg.color = Color.red;
        
        bossHpFillRect = bHpFillObj.GetComponent<RectTransform>();
        bossHpFillRect.anchorMin = Vector2.zero;
        bossHpFillRect.anchorMax = new Vector2(1f, 1f);
        bossHpFillRect.offsetMin = Vector2.zero;
        bossHpFillRect.offsetMax = Vector2.zero;

        GameObject bHpTextObj = new GameObject("HP_Text");
        bHpTextObj.transform.SetParent(bHpBg.transform, false);
        bossHpText = bHpTextObj.AddComponent<Text>();
        bossHpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bossHpText.alignment = TextAnchor.MiddleCenter;
        bossHpText.fontSize = 16;
        bossHpText.color = Color.white;
        Outline bOutline = bHpTextObj.AddComponent<Outline>();
        bOutline.effectColor = Color.black;
        bOutline.effectDistance = new Vector2(1, -1);
        RectTransform bHpTextRect = bHpTextObj.GetComponent<RectTransform>();
        bHpTextRect.anchorMin = Vector2.zero;
        bHpTextRect.anchorMax = Vector2.one;
        bHpTextRect.offsetMin = Vector2.zero;
        bHpTextRect.offsetMax = Vector2.zero;

        // Limit Bar cho Boss
        GameObject bossLimitContainer = new GameObject("LimitContainer");
        bossLimitContainer.transform.SetParent(bossHud.transform, false);
        RectTransform bossLimitRect = bossLimitContainer.AddComponent<RectTransform>();
        bossLimitRect.anchorMin = new Vector2(0.1f, -0.3f);
        bossLimitRect.anchorMax = new Vector2(0.9f, 0f);
        bossLimitRect.offsetMin = Vector2.zero;
        bossLimitRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup limitLayout = bossLimitContainer.AddComponent<HorizontalLayoutGroup>();
        limitLayout.spacing = 5f;
        limitLayout.childControlWidth = true;
        limitLayout.childControlHeight = true;

        bossLimitSegments = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject segObj = new GameObject("LimitSegment");
            segObj.transform.SetParent(bossLimitContainer.transform, false);
            Image segImg = segObj.AddComponent<Image>();
            segImg.color = new Color(0.6f, 0.2f, 0.8f, 1f);
            bossLimitSegments[i] = segImg;
        }

        bossHud.SetActive(false);

        // =====================================================
        // --- ENEMY ACTION PLAN (Góc dưới-trái) ---
        // =====================================================
        GameObject enemyPlanAnchor = new GameObject("EnemyPlanAnchor");
        enemyPlanAnchor.transform.SetParent(canvasObj.transform, false);
        RectTransform epRect = enemyPlanAnchor.AddComponent<RectTransform>();
        epRect.anchorMin = new Vector2(0f, 0f);
        epRect.anchorMax = new Vector2(0f, 0f);
        epRect.pivot = new Vector2(0f, 0f);
        epRect.anchoredPosition = new Vector2(10f, 10f);
        epRect.sizeDelta = new Vector2(350, 160);

        CreateEnemyPlanHud(enemyPlanAnchor.transform);

        // =====================================================
        // --- PLAYER ACTION PLAN (Góc dưới-phải, nhường chỗ cho Soul panel) ---
        // =====================================================
        GameObject playerPlanAnchor = new GameObject("PlayerPlanAnchor");
        playerPlanAnchor.transform.SetParent(canvasObj.transform, false);
        RectTransform ppRect = playerPlanAnchor.AddComponent<RectTransform>();
        ppRect.anchorMin = new Vector2(1f, 0f);
        ppRect.anchorMax = new Vector2(1f, 0f);
        ppRect.pivot = new Vector2(1f, 0f);
        ppRect.anchoredPosition = new Vector2(-120f, 10f); // nhường 120px cho Soul+Execute panel
        ppRect.sizeDelta = new Vector2(320, 160);

        CreateActionBar(playerPlanAnchor.transform);

        // =====================================================
        // --- SOUL + EXECUTE PANEL (Cực phải, góc dưới) ---
        // =====================================================
        soulVessel = new GameObject("SoulVessel");
        soulVessel.transform.SetParent(canvasObj.transform, false);
        RectTransform soulRect = soulVessel.AddComponent<RectTransform>();
        soulRect.anchorMin = new Vector2(1f, 0f);
        soulRect.anchorMax = new Vector2(1f, 0f);
        soulRect.pivot = new Vector2(1f, 0f);
        soulRect.anchoredPosition = new Vector2(-10f, 10f);
        soulRect.sizeDelta = new Vector2(110f, 160f);

        Image soulPanelBg = soulVessel.AddComponent<Image>();
        soulPanelBg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        VerticalLayoutGroup soulVLayout = soulVessel.AddComponent<VerticalLayoutGroup>();
        soulVLayout.padding = new RectOffset(8, 8, 8, 8);
        soulVLayout.spacing = 6;
        soulVLayout.childAlignment = TextAnchor.UpperCenter;
        soulVLayout.childControlHeight = true;
        soulVLayout.childControlWidth = true;
        soulVLayout.childForceExpandHeight = false;
        soulVLayout.childForceExpandWidth = true;

        // Soul Capacity Text
        GameObject soulCapLabel = new GameObject("SoulCapLabel");
        soulCapLabel.transform.SetParent(soulVessel.transform, false);
        LayoutElement soulCapLe = soulCapLabel.AddComponent<LayoutElement>();
        soulCapLe.minHeight = 45;
        soulCapLe.flexibleHeight = 0;
        Image soulCapBg = soulCapLabel.AddComponent<Image>();
        soulCapBg.color = new Color(0.25f, 0.25f, 0.35f, 1f);

        GameObject soulCapTextObj = new GameObject("SoulCapText");
        soulCapTextObj.transform.SetParent(soulCapLabel.transform, false);
        soulText = soulCapTextObj.AddComponent<Text>();
        soulText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        soulText.text = "Soul capacity\n0/9";
        soulText.fontSize = 13;
        soulText.color = Color.white;
        soulText.alignment = TextAnchor.MiddleCenter;
        RectTransform soulCapTextRect = soulCapTextObj.GetComponent<RectTransform>();
        soulCapTextRect.anchorMin = Vector2.zero;
        soulCapTextRect.anchorMax = Vector2.one;
        soulCapTextRect.offsetMin = new Vector2(2, 2);
        soulCapTextRect.offsetMax = new Vector2(-2, -2);

        // Soul icon row (nhỏ)
        GameObject iconContainer = new GameObject("IconContainer");
        iconContainer.transform.SetParent(soulVessel.transform, false);
        LayoutElement iconContLe = iconContainer.AddComponent<LayoutElement>();
        iconContLe.minHeight = 14;
        iconContLe.flexibleHeight = 0;
        HorizontalLayoutGroup iconLayout = iconContainer.AddComponent<HorizontalLayoutGroup>();
        iconLayout.spacing = 3;
        iconLayout.childAlignment = TextAnchor.MiddleCenter;
        iconLayout.childControlWidth = false;
        iconLayout.childControlHeight = false;

        for (int i = 0; i < 9; i++)
        {
            GameObject iconObj = new GameObject("SoulIcon_" + i);
            iconObj.transform.SetParent(iconContainer.transform, false);
            Image img = iconObj.AddComponent<Image>();
            img.rectTransform.sizeDelta = new Vector2(8, 8);
            soulIcons.Add(img);
        }

        // Execute Button - to, màu vàng cam, co giãn chiếm phần còn lại
        GameObject exeObj = new GameObject("ExecuteBtn");
        exeObj.transform.SetParent(soulVessel.transform, false);
        LayoutElement exeLe = exeObj.AddComponent<LayoutElement>();
        exeLe.minHeight = 55;
        exeLe.flexibleHeight = 1; // Co giãn chiếm phần còn lại

        Image exeBg = exeObj.AddComponent<Image>();
        exeBg.color = new Color(0.9f, 0.6f, 0.0f, 1f); // Màu cam vàng như thiết kế

        Button exeBtn = exeObj.AddComponent<Button>();
        executeBtn = exeBtn;
        exeBtn.onClick.AddListener(() =>
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnExecuteButtonClicked();
        });

        GameObject exeTxtObj = new GameObject("Text");
        exeTxtObj.transform.SetParent(exeObj.transform, false);
        Text exeTxt = exeTxtObj.AddComponent<Text>();
        exeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        exeTxt.text = "Execute";
        exeTxt.alignment = TextAnchor.MiddleCenter;
        exeTxt.fontSize = 16;
        exeTxt.fontStyle = FontStyle.Bold;
        exeTxt.color = Color.white;
        RectTransform exeTxtRect = exeTxtObj.GetComponent<RectTransform>();
        exeTxtRect.anchorMin = Vector2.zero;
        exeTxtRect.anchorMax = Vector2.one;
        exeTxtRect.offsetMin = Vector2.zero;
        exeTxtRect.offsetMax = Vector2.zero;


        actionMenu = new GameObject("ActionMenu");
        actionMenu.transform.SetParent(canvasObj.transform, false);
        Image panelImage = actionMenu.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        panelImage.raycastTarget = false; // Xuyên thấu click để không chặn tương tác nhân vật bên dưới

        RectTransform menuRect = actionMenu.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0, 0); // Đổi anchor để di chuyển theo vị trí màn hình dễ hơn
        menuRect.anchorMax = new Vector2(0, 0);
        menuRect.pivot = new Vector2(0, 0.5f); // Pivot bên trái ở giữa
        menuRect.anchoredPosition = new Vector2(-1000, -1000); // Mặc định giấu đi
        menuRect.sizeDelta = new Vector2(120, 150); // Thu nhỏ Action Menu

        VerticalLayoutGroup menuLayout = actionMenu.AddComponent<VerticalLayoutGroup>();
        menuLayout.padding = new RectOffset(10, 10, 10, 10);
        menuLayout.spacing = 5;
        menuLayout.childAlignment = TextAnchor.MiddleCenter;
        menuLayout.childControlHeight = true;
        menuLayout.childControlWidth = true;

        CreateButton("ATTACK", ActionType.ATTACK, actionMenu.transform);
        CreateButton("SKILL", ActionType.SKILL, actionMenu.transform);
        CreateButton("ITEM", ActionType.ITEM, actionMenu.transform);

        actionMenu.SetActive(false);

        // --- SKILL MENU ---
        skillMenu = new GameObject("SkillMenu");
        skillMenu.transform.SetParent(canvasObj.transform, false);
        Image skillPanelImg = skillMenu.AddComponent<Image>();
        skillPanelImg.color = new Color(0, 0, 0, 0.8f);
        skillPanelImg.raycastTarget = false;

        RectTransform skillMenuRect = skillMenu.GetComponent<RectTransform>();
        skillMenuRect.anchorMin = new Vector2(0, 0);
        skillMenuRect.anchorMax = new Vector2(0, 0);
        skillMenuRect.pivot = new Vector2(0, 0.5f);
        skillMenuRect.anchoredPosition = new Vector2(-1000, -1000);
        skillMenuRect.sizeDelta = new Vector2(200, 200);

        VerticalLayoutGroup skillLayout = skillMenu.AddComponent<VerticalLayoutGroup>();
        skillLayout.padding = new RectOffset(10, 10, 10, 10);
        skillLayout.spacing = 5;
        skillLayout.childAlignment = TextAnchor.MiddleCenter;
        skillLayout.childControlHeight = true;
        skillLayout.childControlWidth = true;

        skillMenu.SetActive(false);

        // --- ITEM MENU ---
        itemMenu = new GameObject("ItemMenu");
        itemMenu.transform.SetParent(canvasObj.transform, false);
        Image itemPanelImg = itemMenu.AddComponent<Image>();
        itemPanelImg.color = new Color(0, 0, 0, 0.8f);
        itemPanelImg.raycastTarget = false;

        RectTransform itemMenuRect = itemMenu.GetComponent<RectTransform>();
        itemMenuRect.anchorMin = new Vector2(0, 0);
        itemMenuRect.anchorMax = new Vector2(0, 0);
        itemMenuRect.pivot = new Vector2(0, 0.5f);
        itemMenuRect.anchoredPosition = new Vector2(-1000, -1000);
        itemMenuRect.sizeDelta = new Vector2(200, 200);

        VerticalLayoutGroup itemLayout = itemMenu.AddComponent<VerticalLayoutGroup>();
        itemLayout.padding = new RectOffset(10, 10, 10, 10);
        itemLayout.spacing = 5;
        itemLayout.childAlignment = TextAnchor.MiddleCenter;
        itemLayout.childControlHeight = true;
        itemLayout.childControlWidth = true;

        itemMenu.SetActive(false);

        // --- HELP BUTTON & KEYMAP PANEL ---
        CreateHelpUI(canvasObj.transform);
    }

    private void CreateHelpUI(Transform parentCanvas)
    {
        // Button Help
        GameObject helpBtnObj = new GameObject("HelpButton");
        helpBtnObj.transform.SetParent(parentCanvas, false);
        Image helpImg = helpBtnObj.AddComponent<Image>();
        helpImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        Button helpBtn = helpBtnObj.AddComponent<Button>();

        RectTransform helpRect = helpBtnObj.GetComponent<RectTransform>();
        helpRect.anchorMin = new Vector2(1, 1);
        helpRect.anchorMax = new Vector2(1, 1);
        helpRect.pivot = new Vector2(1, 1);
        helpRect.anchoredPosition = new Vector2(-20, -20);
        helpRect.sizeDelta = new Vector2(100, 40);

        GameObject helpTextObj = new GameObject("Text");
        helpTextObj.transform.SetParent(helpBtnObj.transform, false);
        Text helpText = helpTextObj.AddComponent<Text>();
        helpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        helpText.text = "Help (F1)";
        helpText.alignment = TextAnchor.MiddleCenter;
        helpText.color = Color.white;
        helpText.fontSize = 20;

        RectTransform htRect = helpText.GetComponent<RectTransform>();
        htRect.anchorMin = Vector2.zero;
        htRect.anchorMax = Vector2.one;
        htRect.sizeDelta = Vector2.zero;
        htRect.anchoredPosition = Vector2.zero;

        // Panel KeyMap
        keyMapPanel = new GameObject("KeyMapPanel");
        keyMapPanel.transform.SetParent(parentCanvas, false);
        Image panelImg = keyMapPanel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.95f);

        RectTransform panelRect = keyMapPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 500);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(keyMapPanel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.text = "KEY MAPPINGS";
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = Color.yellow;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(0, 40);

        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(keyMapPanel.transform, false);
        Text contentText = contentObj.AddComponent<Text>();
        contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contentText.text = 
            "WASD / Arrow \t Navigate\n" +
            "Space \t\t Confirm\n" +
            "Backspace \t Back / Cancel\n" +
            "Q / E \t\t Previous / Next Character\n" +
            "PageUp / PageDown \t Secondary Character Switch\n" +
            "Enter \t\t Execute\n" +
            "Esc \t\t Settings / Pause\n" +
            "Tab \t\t Toggle Tips\n" +
            "Home \t\t Toggle Beat Table\n" +
            "I / R \t\t Inspect Detail\n" +
            "Delete \t\t Clear Action";
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.color = Color.white;
        contentText.fontSize = 20;
        contentText.lineSpacing = 1.5f;

        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0, -40); // Offset below title
        contentRect.sizeDelta = new Vector2(-60, -100); // Margin

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(keyMapPanel.transform, false);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();

        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0);
        closeRect.anchorMax = new Vector2(0.5f, 0);
        closeRect.pivot = new Vector2(0.5f, 0);
        closeRect.anchoredPosition = new Vector2(0, 20);
        closeRect.sizeDelta = new Vector2(120, 40);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.text = "Close";
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        closeText.fontSize = 20;

        RectTransform ctRect = closeText.GetComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.sizeDelta = Vector2.zero;
        ctRect.anchoredPosition = Vector2.zero;

        // Listeners
        helpBtn.onClick.AddListener(() => keyMapPanel.SetActive(true));
        closeBtn.onClick.AddListener(() => keyMapPanel.SetActive(false));

        keyMapPanel.SetActive(false);
    }

    private GameObject actionBarHud;
    
    // Lưu các node UI: actionNodes[actorName][beatIndex]
    private Dictionary<string, Image[]> actionNodesMap = new Dictionary<string, Image[]>();
    
    private Button executeBtn;

    private void CreateActionBar(Transform parent)
    {
        actionBarHud = new GameObject("ActionBarGrid");
        actionBarHud.transform.SetParent(parent, false);

        VerticalLayoutGroup vLayout = actionBarHud.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 2;
        vLayout.padding = new RectOffset(4, 4, 4, 4);
        vLayout.childAlignment = TextAnchor.LowerLeft;
        vLayout.childControlHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childForceExpandWidth = false;

        Image gridBg = actionBarHud.AddComponent<Image>();
        gridBg.color = new Color(0f, 0f, 0f, 0.55f);

        // --- HEADER ROW (Beat labels) ---
        {
            GameObject headerRow = new GameObject("HeaderRow");
            headerRow.transform.SetParent(actionBarHud.transform, false);
            HorizontalLayoutGroup hl = headerRow.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 4;
            hl.childControlHeight = true;
            hl.childControlWidth = true;

            // Blank cell để canh với cột Avatar
            CreateHeaderCell(headerRow.transform, "", 36);
            // Blank cell để canh với cột Name
            CreateHeaderCell(headerRow.transform, "", 52);
            // Beat labels
            CreateHeaderCell(headerRow.transform, "Beat 1", 56);
            CreateHeaderCell(headerRow.transform, "Beat 2", 56);
        }

        // --- DATA ROWS ---
        string[] actorNames = new string[] { "XIII", "An", "Mac" };

        foreach (string actorName in actorNames)
        {
            GameObject rowObj = new GameObject("Row_" + actorName);
            rowObj.transform.SetParent(actionBarHud.transform, false);
            
            HorizontalLayoutGroup hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 4;
            hLayout.padding = new RectOffset(0, 0, 1, 1);
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;

            // Avatar
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(rowObj.transform, false);
            LayoutElement avatarLe = avatarObj.AddComponent<LayoutElement>();
            avatarLe.minWidth = 36;
            avatarLe.minHeight = 28;
            Image avatarImg = avatarObj.AddComponent<Image>();
            
            Sprite sp = Resources.Load<Sprite>("UI/" + actorName + "-icon");
            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("UI/" + actorName + "-icon");
                if (tex != null) sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            if (sp != null) avatarImg.sprite = sp;
            else avatarImg.color = new Color(0.35f, 0.35f, 0.45f, 1f);

            // Name label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);
            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.minWidth = 52;
            labelLe.minHeight = 28;
            Text labelTxt = labelObj.AddComponent<Text>();
            labelTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelTxt.text = actorName;
            labelTxt.fontSize = 11;
            labelTxt.color = new Color(0.85f, 0.85f, 0.9f, 1f);
            labelTxt.alignment = TextAnchor.MiddleLeft;

            // Beat nodes (2 beats)
            Image[] nodes = new Image[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject nodeObj = new GameObject("Node_" + i);
                nodeObj.transform.SetParent(rowObj.transform, false);
                LayoutElement le = nodeObj.AddComponent<LayoutElement>();
                le.minWidth = 56;
                le.minHeight = 28;
                Image bg = nodeObj.AddComponent<Image>();
                bg.color = new Color(0.18f, 0.18f, 0.25f, 0.9f);

                Outline outline = nodeObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.4f, 0.4f, 0.6f, 0.6f);
                outline.effectDistance = new Vector2(1, -1);

                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(nodeObj.transform, false);
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.color = Color.white;
                iconImg.enabled = false;

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(3, 3);
                iconRect.offsetMax = new Vector2(-3, -3);

                nodes[i] = iconImg;
            }
            actionNodesMap[actorName] = nodes;
        }
    }

    private void CreateHeaderCell(Transform parent, string label, float minW)
    {
        GameObject cell = new GameObject("Header_" + label);
        cell.transform.SetParent(parent, false);
        LayoutElement le = cell.AddComponent<LayoutElement>();
        le.minWidth = minW;
        le.minHeight = 20;
        Text txt = cell.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = label;
        txt.fontSize = 11;
        txt.color = new Color(0.7f, 0.7f, 0.8f, 1f);
        txt.alignment = TextAnchor.MiddleCenter;
    }

    public void UpdateActionBar(List<BeatPlan> plan)
    {
        // Clear old visual
        foreach (var nodes in actionNodesMap.Values)
        {
            nodes[0].enabled = false;
            nodes[1].enabled = false;
        }

        // Đếm số action cho từng nhân vật để biết đặt vào cột nào
        Dictionary<string, int> actorActionCount = new Dictionary<string, int>();

        foreach (var beat in plan)
        {
            foreach (var action in beat.actions)
            {
                if (action.actor == null) continue;
                string baseName = action.actor.characterName;
                if (baseName.Contains("XIII")) baseName = "XIII";
                else if (baseName.Contains("An")) baseName = "An";
                else if (baseName.Contains("Mac")) baseName = "Mac";
                
                if (!actorActionCount.ContainsKey(baseName)) actorActionCount[baseName] = 0;
                
                int slotIndex = actorActionCount[baseName];
                if (slotIndex < 2 && actionNodesMap.ContainsKey(baseName))
                {
                    Image nodeImg = actionNodesMap[baseName][slotIndex];
                    
                    // Lấy icon của mục tiêu
                    string iconName = action.target.characterName + "-icon";
                    Sprite sp = Resources.Load<Sprite>("UI/" + iconName);
                    if (sp == null)
                    {
                        Texture2D tex = Resources.Load<Texture2D>("UI/" + iconName);
                        if (tex != null) sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                    if (sp != null)
                    {
                        nodeImg.sprite = sp;
                        nodeImg.enabled = true;
                    }
                    else
                    {
                        // Fallback: icon vũ khí/skill
                        nodeImg.color = (action.type == ActionType.ATTACK) ? Color.red : Color.blue;
                        nodeImg.enabled = true;
                    }
                    
                    actorActionCount[baseName]++;
                }
            }
        }
    }

    private void CreateEnemyPlanHud(Transform parent)
    {
        // Main grid container
        GameObject gridObj = new GameObject("EnemyGrid");
        gridObj.transform.SetParent(parent, false);
        Image gridBg = gridObj.AddComponent<Image>();
        gridBg.color = new Color(0f, 0f, 0f, 0.55f);

        RectTransform gridRect = gridObj.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vLayout = gridObj.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 2;
        vLayout.padding = new RectOffset(4, 4, 4, 4);
        vLayout.childAlignment = TextAnchor.LowerLeft;
        vLayout.childControlHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childForceExpandWidth = false;

        // --- HEADER ROW ---
        // Beat counts: Boss=3, E1=1, E2=1. Dùng max=3 cột beat cho header
        {
            GameObject headerRow = new GameObject("EnemyHeaderRow");
            headerRow.transform.SetParent(gridObj.transform, false);
            HorizontalLayoutGroup hl = headerRow.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 4;
            hl.childControlHeight = true;
            hl.childControlWidth = true;

            // Blanks căn cột Avatar + Name
            CreateHeaderCell(headerRow.transform, "", 36);
            CreateHeaderCell(headerRow.transform, "", 52);
            // Beat headers
            CreateHeaderCell(headerRow.transform, "Beat 1", 50);
            CreateHeaderCell(headerRow.transform, "Beat 2", 50);
            CreateHeaderCell(headerRow.transform, "Beat 3", 50);
        }

        // --- DATA ROWS ---
        // Chỉ 3 hàng: Boss (đầu), Enemy 1, Enemy 2
        string[] enemyLabels = new string[] { "Boss", "Enemy 1", "Enemy 2" };
        Color[] enemyColors = new Color[]
        {
            new Color(0.9f, 0.4f, 0.0f, 1f),   // Boss: cam
            new Color(0.7f, 0.2f, 0.2f, 1f),   // E1: đỏ
            new Color(0.7f, 0.2f, 0.2f, 1f),   // E2: đỏ
        };
        int[] beatCounts = new int[] { 3, 1, 1 }; // Boss: 3 beat, lính: 1 beat

        for (int e = 0; e < enemyLabels.Length; e++)
        {
            bool isBoss = (e == 0);
            GameObject rowObj = new GameObject("EnemyRow_" + e);
            rowObj.transform.SetParent(gridObj.transform, false);

            HorizontalLayoutGroup hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 4;
            hLayout.padding = new RectOffset(0, 0, 1, 1);
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;

            // Avatar placeholder
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(rowObj.transform, false);
            LayoutElement avatarLe = avatarObj.AddComponent<LayoutElement>();
            avatarLe.minWidth = 36;
            avatarLe.minHeight = 24;
            Image avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.color = new Color(enemyColors[e].r * 0.5f, enemyColors[e].g * 0.5f, enemyColors[e].b * 0.5f, 1f);

            // Name label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);
            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.minWidth = 52;
            labelLe.minHeight = 24;
            Text labelTxt = labelObj.AddComponent<Text>();
            labelTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelTxt.text = enemyLabels[e];
            labelTxt.fontSize = isBoss ? 12 : 10;
            labelTxt.fontStyle = isBoss ? FontStyle.Bold : FontStyle.Normal;
            labelTxt.color = enemyColors[e];
            labelTxt.alignment = TextAnchor.MiddleLeft;

            // Beat node cells
            for (int i = 0; i < 3; i++)
            {
                bool hasAction = (i < beatCounts[e]);
                GameObject nodeObj = new GameObject("EnemyNode_" + i);
                nodeObj.transform.SetParent(rowObj.transform, false);
                LayoutElement le = nodeObj.AddComponent<LayoutElement>();
                le.minWidth = 50;
                le.minHeight = 24;
                Image bg = nodeObj.AddComponent<Image>();

                if (hasAction)
                {
                    bg.color = isBoss
                        ? new Color(0.3f, 0.12f, 0.0f, 0.9f)  // Boss: cam thẫm
                        : new Color(0.25f, 0.08f, 0.08f, 0.9f); // Enemy: đỏ thẫm

                    Outline outline = nodeObj.AddComponent<Outline>();
                    outline.effectColor = isBoss
                        ? new Color(0.8f, 0.4f, 0.0f, 0.6f)
                        : new Color(0.6f, 0.2f, 0.2f, 0.6f);
                    outline.effectDistance = new Vector2(1, -1);

                    GameObject qMarkObj = new GameObject("QMark");
                    qMarkObj.transform.SetParent(nodeObj.transform, false);
                    Text qMark = qMarkObj.AddComponent<Text>();
                    qMark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    qMark.text = "?";
                    qMark.fontSize = 14;
                    qMark.fontStyle = FontStyle.Bold;
                    qMark.color = isBoss
                        ? new Color(0.9f, 0.5f, 0.0f, 0.7f)
                        : new Color(0.7f, 0.25f, 0.25f, 0.7f);
                    qMark.alignment = TextAnchor.MiddleCenter;
                    RectTransform qRect = qMarkObj.GetComponent<RectTransform>();
                    qRect.anchorMin = Vector2.zero;
                    qRect.anchorMax = Vector2.one;
                    qRect.offsetMin = Vector2.zero;
                    qRect.offsetMax = Vector2.zero;
                }
                else
                {
                    // Ô trống (enemy không đủ beat)
                    bg.color = new Color(0.1f, 0.1f, 0.12f, 0.5f);
                }
            }
        }
    }

    private GameObject skillMenu;
    private GameObject itemMenu;

    private void CreateAllyHUDBlock(CharacterInteraction ally, Transform parent)
    {
        GameObject block = new GameObject("AllyBlock_" + ally.characterName);
        block.transform.SetParent(parent, false);
        
        // Thêm LayoutElement để định cỡ cho block
        LayoutElement le = block.AddComponent<LayoutElement>();
        le.minWidth = 220; // Tăng kích thước block
        le.minHeight = 60;

        Image bg = block.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Bấm vào HUD block này cũng chọn nhân vật
        Button blockBtn = block.AddComponent<Button>();
        blockBtn.onClick.AddListener(() => {
            if (BattleManager.Instance != null && 
               (BattleManager.Instance.state == BattleState.WAIT_TARGET || BattleManager.Instance.state == BattleState.PLAYER_TURN))
            {
                BattleManager.Instance.OnTargetSelected(ally);
            }
        });

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(block.transform, false);
        Text nameTxt = nameObj.AddComponent<Text>();
        nameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameTxt.text = ally.characterName;
        nameTxt.alignment = TextAnchor.UpperCenter;
        nameTxt.fontSize = 16;
        nameTxt.color = Color.white;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        GameObject hpBg = new GameObject("HP_BG");
        hpBg.transform.SetParent(block.transform, false);
        Image hpBgImg = hpBg.AddComponent<Image>();
        hpBgImg.color = Color.black;
        RectTransform hpBgRect = hpBg.GetComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0.1f, 0.1f);
        hpBgRect.anchorMax = new Vector2(0.9f, 0.4f);
        hpBgRect.offsetMin = Vector2.zero;
        hpBgRect.offsetMax = Vector2.zero;

        GameObject hpFillObj = new GameObject("HP_Fill");
        hpFillObj.transform.SetParent(hpBg.transform, false);
        Image hpFillImg = hpFillObj.AddComponent<Image>();
        hpFillImg.color = Color.green;
        
        RectTransform hpFillRect = hpFillObj.GetComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero;
        float startFill = (float)ally.currentHP / ally.maxHP;
        hpFillRect.anchorMax = new Vector2(startFill, 1f);
        hpFillRect.offsetMin = Vector2.zero;
        hpFillRect.offsetMax = Vector2.zero;

        GameObject hpTextObj = new GameObject("HP_Text");
        hpTextObj.transform.SetParent(hpBg.transform, false);
        Text hpTxt = hpTextObj.AddComponent<Text>();
        hpTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpTxt.text = ally.currentHP + "/" + ally.maxHP;
        hpTxt.alignment = TextAnchor.MiddleCenter;
        hpTxt.fontSize = 16;
        hpTxt.color = Color.white;
        Outline aOutline = hpTextObj.AddComponent<Outline>();
        aOutline.effectColor = Color.black;
        aOutline.effectDistance = new Vector2(1, -1);
        RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
        hpTextRect.anchorMin = Vector2.zero;
        hpTextRect.anchorMax = Vector2.one;
        hpTextRect.offsetMin = Vector2.zero;
        hpTextRect.offsetMax = Vector2.zero;

        allyHpFills[ally] = hpFillRect;
        allyHpTexts[ally] = hpTxt;
    }

    // Move CreateButton up so SetupUI can use it, but since I already did that, I will just delete this one down here


    private void OnActionButtonClicked(ActionType type)
    {
        if (type == ActionType.SKILL)
        {
            ShowSkillMenu(true);
            ShowItemMenu(false);
        }
        else if (type == ActionType.ITEM)
        {
            ShowItemMenu(true);
            ShowSkillMenu(false);
        }
        else if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnActionSelected(type);
        }
    }

    public void OnSkillSelected(SkillData skill)
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.pendingSkill = skill;
            BattleManager.Instance.OnActionSelected(ActionType.SKILL);
        }
    }

    public void OnItemSelected(ItemData item)
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.pendingItem = item;
            BattleManager.Instance.OnActionSelected(ActionType.ITEM);
        }
    }

    public void ShowMessage(string msg)
    {
        if (infoText != null)
        {
            infoText.text = msg;
        }
    }

    public void UpdateSoulUI()
    {
        if (BattleManager.Instance == null || soulText == null) return;
        
        int red = BattleManager.Instance.currentRedSoul;
        int blue = BattleManager.Instance.currentBlueSoul;
        int total = red + blue;
        
        // Format gọn cho panel nhỏ bên phải: "Soul capacity\n6/9"
        soulText.text = "Soul capacity\n" + total + "/9";
        
        // Cập nhật icon (nhỏ 8x8px)
        for (int i = 0; i < 9; i++)
        {
            if (i < soulIcons.Count)
            {
                if (i < 3) // 3 slot đầu là Đỏ
                {
                    soulIcons[i].color = (i < red) ? Color.red : new Color(0.3f, 0, 0, 0.4f);
                }
                else // 6 slot sau là Xanh
                {
                    soulIcons[i].color = (i - 3 < blue) ? Color.cyan : new Color(0, 0.3f, 0.3f, 0.4f);
                }
            }
        }
    }

    public void ShowSkillMenu(bool show)
    {
        if (skillMenu != null)
        {
            skillMenu.SetActive(show);
            // Đừng ẩn actionMenu, để nó bên cạnh
            // actionMenu.SetActive(!show); 
            
            if (actionButtons.ContainsKey(ActionType.SKILL))
            {
                actionButtons[ActionType.SKILL].color = show ? new Color(0.8f, 0.6f, 0.1f, 1f) : new Color(0.2f, 0.2f, 0.3f, 1f);
            }
            if (show && actionButtons.ContainsKey(ActionType.ITEM))
            {
                actionButtons[ActionType.ITEM].color = new Color(0.2f, 0.2f, 0.3f, 1f);
            }

            if (show && activeActorTransform != null)
            {
                // Clear old skills
                foreach (Transform child in skillMenu.transform)
                {
                    Destroy(child.gameObject);
                }

                CharacterInteraction actor = activeActorTransform.GetComponent<CharacterInteraction>();
                if (actor != null && actor.activeSkills != null)
                {
                    foreach (SkillData skill in actor.activeSkills)
                    {
                        CreateSkillButton(skill, skillMenu.transform);
                    }
                }
                
                // Nút Cancel để quay lại Action Menu
                CreateSkillCancelButton(skillMenu.transform);

                // Lấy vị trí của ActionMenu hiện tại và cộng thêm bề ngang (ví dụ 130px) để nó xổ ngang
                RectTransform actionRect = actionMenu.GetComponent<RectTransform>();
                skillMenu.GetComponent<RectTransform>().position = actionRect.position + new Vector3(130f, 0, 0);

                // Auto-select first skill
                if (skillMenu.transform.childCount > 0)
                {
                    EventSystem.current.SetSelectedGameObject(skillMenu.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    private void CreateSkillButton(SkillData skill, Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_" + skill.skillName);
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        Button btn = btnObj.AddComponent<Button>();

        int currentSouls = 0;
        if (BattleManager.Instance != null) currentSouls = BattleManager.Instance.currentRedSoul + BattleManager.Instance.currentBlueSoul;
        bool hasEnoughSoul = currentSouls >= skill.soulCost;

        if (hasEnoughSoul)
        {
            btnImg.color = new Color(0.3f, 0.2f, 0.4f, 1f);
            btn.onClick.AddListener(() => OnSkillSelected(skill));
        }
        else
        {
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Xám
            btn.interactable = false;
        }

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = skill.skillName + " (Cost: " + skill.soulCost + ")";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 16;
        txt.color = Color.white;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
    }

    private void CreateSkillCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Cancel");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            ShowSkillMenu(false);
            // ShowActionMenu(true, activeActorTransform); // Không cần hiện lại vì nó vốn không bị ẩn
        });

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "BACK";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 16;
        txt.color = Color.red;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
    }

    public void ShowItemMenu(bool show)
    {
        if (itemMenu != null)
        {
            itemMenu.SetActive(show);
            
            if (actionButtons.ContainsKey(ActionType.ITEM))
            {
                actionButtons[ActionType.ITEM].color = show ? new Color(0.8f, 0.6f, 0.1f, 1f) : new Color(0.2f, 0.2f, 0.3f, 1f);
            }
            if (show && actionButtons.ContainsKey(ActionType.SKILL))
            {
                actionButtons[ActionType.SKILL].color = new Color(0.2f, 0.2f, 0.3f, 1f);
            }

            if (show)
            {
                // Clear old items
                foreach (Transform child in itemMenu.transform)
                {
                    Destroy(child.gameObject);
                }

                if (BattleManager.Instance != null && BattleManager.Instance.inventory != null)
                {
                    foreach (var kvp in BattleManager.Instance.inventory)
                    {
                        CreateItemButton(kvp.Key, kvp.Value, itemMenu.transform);
                    }
                }
                
                CreateItemCancelButton(itemMenu.transform);

                RectTransform actionRect = actionMenu.GetComponent<RectTransform>();
                itemMenu.GetComponent<RectTransform>().position = actionRect.position + new Vector3(130f, 0, 0);

                // Auto-select first item
                if (itemMenu.transform.childCount > 0)
                {
                    EventSystem.current.SetSelectedGameObject(itemMenu.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    private void CreateItemButton(ItemData item, int amount, Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_" + item.itemName);
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        Button btn = btnObj.AddComponent<Button>();

        if (amount > 0)
        {
            btnImg.color = new Color(0.2f, 0.5f, 0.3f, 1f);
            btn.onClick.AddListener(() => OnItemSelected(item));
        }
        else
        {
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            btn.interactable = false;
        }

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = item.itemName + " (x" + amount + ")";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 16;
        txt.color = Color.white;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
    }

    private void CreateItemCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Cancel");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            ShowItemMenu(false);
        });

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "BACK";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 16;
        txt.color = Color.red;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
    }

    public void ShowActionMenu(bool show, Transform actorTransform = null)
    {
        if (actionMenu != null)
        {
            actionMenu.SetActive(show);
            if (!show)
            {
                ShowSkillMenu(false);
                ShowItemMenu(false);
            }

            if (show && actorTransform != null)
            {
                activeActorTransform = actorTransform;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(activeActorTransform.position + Vector3.right * 1f + Vector3.up * 0.5f);
                actionMenu.GetComponent<RectTransform>().position = screenPos;

                // Auto-select ATTACK
                if (actionButtons.ContainsKey(ActionType.ATTACK))
                {
                    EventSystem.current.SetSelectedGameObject(actionButtons[ActionType.ATTACK].gameObject);
                }
            }
            else
            {
                activeActorTransform = null;
            }
        }
    }


    public void ShowBossHUD(CharacterInteraction enemy)
    {
        if (bossHud != null && enemy != null)
        {
            bossHud.SetActive(true);
            bossNameText.text = enemy.characterName;
            float target = (float)enemy.currentHP / enemy.maxHP;
            UpdateHPImage(bossHpFillRect, target);
            if (bossHpText != null)
            {
                bossHpText.text = enemy.currentHP + "/" + enemy.maxHP;
            }
            UpdateLimitHUD(enemy);
        }
    }

    public void UpdateLimitHUD(CharacterInteraction enemy)
    {
        if (bossHud != null && bossHud.activeSelf && bossNameText.text == enemy.characterName)
        {
            if (bossLimitSegments != null)
            {
                for (int i = 0; i < bossLimitSegments.Length; i++)
                {
                    if (i < enemy.maxLimit)
                    {
                        bossLimitSegments[i].gameObject.SetActive(true);
                        bossLimitSegments[i].color = (i < enemy.currentLimit) ? new Color(0.6f, 0.2f, 0.8f, 1f) : new Color(0.2f, 0.1f, 0.3f, 0.8f);
                    }
                    else
                    {
                        bossLimitSegments[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void HideBossHUD()
    {
        if (bossHud != null) bossHud.SetActive(false);
    }

    private Dictionary<RectTransform, Coroutine> hpRoutines = new Dictionary<RectTransform, Coroutine>();

    public void UpdateHP(CharacterInteraction character)
    {
        if (character.isAlly && allyHpFills.ContainsKey(character))
        {
            UpdateHPImage(allyHpFills[character], (float)character.currentHP / character.maxHP);
            if (allyHpTexts.ContainsKey(character) && allyHpTexts[character] != null)
            {
                allyHpTexts[character].text = character.currentHP + "/" + character.maxHP;
            }
        }
        else if (bossHud != null && bossHud.activeSelf && bossNameText.text == character.characterName)
        {
            UpdateHPImage(bossHpFillRect, (float)character.currentHP / character.maxHP);
            if (bossHpText != null)
            {
                bossHpText.text = character.currentHP + "/" + character.maxHP;
            }

            // Update Boss Limit
            if (bossLimitSegments != null)
            {
                for (int i = 0; i < bossLimitSegments.Length; i++)
                {
                    if (i < character.maxLimit)
                    {
                        if (i < character.currentLimit)
                            bossLimitSegments[i].color = new Color(0.6f, 0.2f, 0.8f, 1f); // Purple
                        else
                            bossLimitSegments[i].color = new Color(0.2f, 0.1f, 0.3f, 0.8f); // Dark
                    }
                }
            }
        }
    }

    public Vector3 GetHPBarWorldPosition(CharacterInteraction character)
    {
        RectTransform hpRect = null;
        if (character.isAlly && allyHpFills.ContainsKey(character))
        {
            hpRect = allyHpFills[character];
        }
        else if (bossHud != null && bossHud.activeSelf && bossNameText.text == character.characterName)
        {
            hpRect = bossHpFillRect;
        }
        
        if (hpRect != null)
        {
            // Convert UI screen position to world position where text is rendered (Camera Z = 10 -> offset 10)
            Vector3 screenPos = hpRect.position;
            return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
        }
        return character.transform.position + new Vector3(0, 1.5f, 0);
    }

    private void UpdateHPImage(RectTransform rect, float targetFill)
    {
        if (hpRoutines.ContainsKey(rect) && hpRoutines[rect] != null)
        {
            StopCoroutine(hpRoutines[rect]);
        }
        hpRoutines[rect] = StartCoroutine(SmoothUpdateHP(rect, targetFill));
    }

    private IEnumerator SmoothUpdateHP(RectTransform hpRect, float targetFill)
    {
        float startFill = hpRect.anchorMax.x;
        float elapsed = 0;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            hpRect.anchorMax = new Vector2(Mathf.Lerp(startFill, targetFill, elapsed / duration), 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        hpRect.anchorMax = new Vector2(targetFill, 1f);
    }
}
