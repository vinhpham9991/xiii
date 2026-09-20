using System.Collections;
using System.Collections.Generic;
using FrankenXIII.Combat.Domain;
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
    private List<ActionType> currentMenuOptions = new List<ActionType>();
    private int activeMenuIndex = 0;

    
    public void ChangeMenuSelection(int direction)
    {
        if (currentMenuOptions.Count == 0) return;
        activeMenuIndex = (activeMenuIndex + direction + currentMenuOptions.Count) % currentMenuOptions.Count;
        HighlightActiveMenuOption();
    }

    public void ConfirmMenuSelection()
    {
        if (currentMenuOptions.Count > 0 && activeMenuIndex >= 0 && activeMenuIndex < currentMenuOptions.Count)
        {
            ActionType selectedType = currentMenuOptions[activeMenuIndex];
            OnActionButtonClicked(selectedType);
        }
    }

    private void HighlightActiveMenuOption()
    {
        for (int i = 0; i < currentMenuOptions.Count; i++)
        {
            ActionType t = currentMenuOptions[i];
            if (actionButtons.ContainsKey(t))
            {
                if (i == activeMenuIndex)
                {
                    // Highlight color (Bright Gold)
                    actionButtons[t].color = new Color(0.9f, 0.7f, 0.1f, 1f);
                    // EventSystem.current.SetSelectedGameObject(actionButtons[t].gameObject);
                }
                else
                {
                    // Default color
                    actionButtons[t].color = new Color(0.2f, 0.2f, 0.3f, 1f);
                }
            }
        }
    }

    private void CreateButton(string label, ActionType type, Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);
        
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        actionButtons[type] = btnImg;
        currentMenuOptions.Add(type);

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
        soulRect.anchoredPosition = new Vector2(-20f, 40f);
        soulRect.sizeDelta = new Vector2(130f, 130f);

        // SoulVessel bg is invisible because it's just a wrapper
        Image soulPanelBg = soulVessel.AddComponent<Image>();
        soulPanelBg.color = new Color(0f, 0f, 0f, 0f);

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
        soulCapBg.color = new Color(0.12f, 0.12f, 0.18f, 1f);

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
        exeBg.color = new Color(0.9f, 0.55f, 0.0f, 1f); // Màu cam vàng như thiết kế

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
    private GameObject enemyActionBarHud;
    
    private Dictionary<string, Image[]> actionNodesMap = new Dictionary<string, Image[]>();
    private Dictionary<string, Outline[]> actionNodeOutlinesMap = new Dictionary<string, Outline[]>();
    private Dictionary<string, Image[]> enemyActionNodesMap = new Dictionary<string, Image[]>();
    
    private Dictionary<string, Text> allyHpTextsMap = new Dictionary<string, Text>();
    private Dictionary<string, Text> enemyHpTextsMap = new Dictionary<string, Text>();
    
    private Dictionary<string, Image> allyHpFillsMap = new Dictionary<string, Image>();
    private Dictionary<string, Image> allyRowBgsMap = new Dictionary<string, Image>();
    private Dictionary<string, Image> enemyHpFillsMap = new Dictionary<string, Image>();
    private Dictionary<string, Image[]> enemyLimitSegmentsMap = new Dictionary<string, Image[]>();
    
    private Button executeBtn;

    private Image[] soulDots;

    private void CreateActionBar(Transform parent)
    {
        // Wrapper for Action Bar
        GameObject rightHudWrapper = new GameObject("RightHudWrapper");
        rightHudWrapper.transform.SetParent(parent, false);
        HorizontalLayoutGroup wrapperLayout = rightHudWrapper.AddComponent<HorizontalLayoutGroup>();
        wrapperLayout.spacing = 0;
        wrapperLayout.childControlHeight = true;
        wrapperLayout.childControlWidth = true;
        wrapperLayout.childAlignment = TextAnchor.LowerRight;

        // Action Bar Grid
        actionBarHud = new GameObject("ActionBarGrid");
        actionBarHud.transform.SetParent(rightHudWrapper.transform, false);
        VerticalLayoutGroup vLayout = actionBarHud.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 2;
        vLayout.padding = new RectOffset(6, 6, 6, 6);
        vLayout.childAlignment = TextAnchor.LowerLeft;
        vLayout.childControlHeight = true;
        vLayout.childControlWidth = true;

        Image gridBg = actionBarHud.AddComponent<Image>();
        gridBg.color = new Color(0f, 0f, 0f, 0.85f); // Black background

        string[] actorNames = new string[] { "XIII", "An", "Mac" };
        
        allyHpTextsMap.Clear();
        allyHpFillsMap.Clear();

        allyRowBgsMap.Clear();
        actionNodesMap.Clear();
        actionNodeOutlinesMap.Clear();
        foreach (string actorName in actorNames)
        {
            GameObject rowObj = new GameObject("Row_" + actorName);
            rowObj.transform.SetParent(actionBarHud.transform, false);
            Image rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.1f, 0.4f, 0.8f, 0f); // Default transparent
            allyRowBgsMap[actorName] = rowBg;

            
            HorizontalLayoutGroup hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 2;
            hLayout.padding = new RectOffset(0, 0, 0, 0);
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;

            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(rowObj.transform, false);
            LayoutElement avatarLe = avatarObj.AddComponent<LayoutElement>();
            avatarLe.minWidth = 32;
            avatarLe.minHeight = 32;
            avatarLe.preferredWidth = 32;
            avatarLe.preferredHeight = 32;
            avatarLe.flexibleWidth = 0;
            Image avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.preserveAspect = true;
            
            Sprite sp = Resources.Load<Sprite>("UI/" + actorName + "-icon");
            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("UI/" + actorName + "-icon");
                if (tex != null)
                {
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }

            if (sp != null) avatarImg.sprite = sp;
            else avatarImg.color = new Color(0.27f, 0.27f, 0.33f, 1f);

            // HP Bar Wrapper
            GameObject hpWrapper = new GameObject("HpWrapper");
            hpWrapper.transform.SetParent(rowObj.transform, false);
            LayoutElement hpLe = hpWrapper.AddComponent<LayoutElement>();
            hpLe.minWidth = 60;
            hpLe.minHeight = 16;
            Image hpBg = hpWrapper.AddComponent<Image>();
            hpBg.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
            
            GameObject hpFillObj = new GameObject("HpFill");
            hpFillObj.transform.SetParent(hpWrapper.transform, false);
            Image hpFill = hpFillObj.AddComponent<Image>();
            hpFill.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green health
            RectTransform fillRect = hpFillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            allyHpFillsMap[actorName] = hpFill;

            GameObject hpTextObj = new GameObject("HpText");
            hpTextObj.transform.SetParent(hpWrapper.transform, false);
            Text labelTxt = hpTextObj.AddComponent<Text>();
            labelTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelTxt.text = "HP: ?/?";
            labelTxt.fontSize = 10;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform txtRect = hpTextObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
            
            allyHpTextsMap[actorName] = labelTxt;

            Image[] actorNodes = new Image[ActorBeatRules.DemoPlayerBeatCount];
            Outline[] actorNodeOutlines = new Outline[ActorBeatRules.DemoPlayerBeatCount];
            for (int beatIndex = 0; beatIndex < ActorBeatRules.DemoPlayerBeatCount; beatIndex++)
            {
                GameObject nodeObj = new GameObject("Beat_" + (beatIndex + 1));
                nodeObj.transform.SetParent(rowObj.transform, false);
                LayoutElement nodeLayout = nodeObj.AddComponent<LayoutElement>();
                nodeLayout.minWidth = 72;
                nodeLayout.preferredWidth = 72;
                nodeLayout.minHeight = 42;
                nodeLayout.preferredHeight = 42;

                Image nodeBackground = nodeObj.AddComponent<Image>();
                nodeBackground.color = new Color(0.12f, 0.12f, 0.18f, 1f);
                actorNodes[beatIndex] = nodeBackground;

                Outline nodeOutline = nodeObj.AddComponent<Outline>();
                nodeOutline.effectColor = new Color(0.1f, 1f, 0.85f, 1f);
                nodeOutline.effectDistance = new Vector2(2f, -2f);
                nodeOutline.enabled = false;
                actorNodeOutlines[beatIndex] = nodeOutline;

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(nodeObj.transform, false);
                Text actionText = textObj.AddComponent<Text>();
                actionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                actionText.text = "Beat " + (beatIndex + 1);
                actionText.fontSize = 10;
                actionText.color = new Color(0.55f, 0.55f, 0.62f, 1f);
                actionText.alignment = TextAnchor.MiddleCenter;

                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            actionNodesMap[actorName] = actorNodes;
            actionNodeOutlinesMap[actorName] = actorNodeOutlines;
        }

        RectTransform mainRect = rightHudWrapper.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(1, 0); mainRect.anchorMax = new Vector2(1, 0);
        mainRect.pivot = new Vector2(1, 0);
        mainRect.sizeDelta = new Vector2(320, 170);
        mainRect.anchoredPosition = new Vector2(-35, 24);
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

    public void UpdateActionBar(IReadOnlyList<BeatPlan> plan)
    {
        foreach (KeyValuePair<string, Image[]> pair in actionNodesMap)
        {
            for (int beatIndex = 0; beatIndex < pair.Value.Length; beatIndex++)
            {
                Image nodeBackground = pair.Value[beatIndex];
                nodeBackground.color = new Color(0.12f, 0.12f, 0.18f, 1f);
                actionNodeOutlinesMap[pair.Key][beatIndex].enabled = false;

                if (nodeBackground.transform.childCount > 0)
                {
                    Text text = nodeBackground.transform.GetChild(0).GetComponent<Text>();
                    if (text != null)
                    {
                        text.text = "Beat " + (beatIndex + 1);
                        text.color = new Color(0.55f, 0.55f, 0.62f, 1f);
                    }
                }
            }
        }

        if (plan == null)
        {
            return;
        }

        int visibleBeatCount = Mathf.Min(plan.Count, ActorBeatRules.DemoPlayerBeatCount);
        for (int beatIndex = 0; beatIndex < visibleBeatCount; beatIndex++)
        {
            foreach (PlannedAction action in plan[beatIndex].actions)
            {
                if (action == null || action.actor == null)
                {
                    continue;
                }

                string actorName = action.actor.characterName
                    .Replace("Ally_", string.Empty)
                    .Replace("Enemy_", string.Empty);
                if (!actionNodesMap.TryGetValue(actorName, out Image[] actorNodes))
                {
                    continue;
                }

                Image nodeBackground = actorNodes[beatIndex];
                nodeBackground.color = GetActionNodeColor(action.type);
                actionNodeOutlinesMap[actorName][beatIndex].enabled =
                    action.SoulReservation.ReservedBlue > 0;

                if (nodeBackground.transform.childCount == 0)
                {
                    continue;
                }

                Text text = nodeBackground.transform.GetChild(0).GetComponent<Text>();
                if (text != null)
                {
                    text.text = "Beat " + (beatIndex + 1) + "\n" + GetActionLabel(action);
                    text.color = Color.white;
                }
            }
        }
    }

    private static Color GetActionNodeColor(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.ATTACK:
                return new Color(0.22f, 0.35f, 0.58f, 1f);
            case ActionType.ITEM:
                return new Color(0.18f, 0.48f, 0.32f, 1f);
            default:
                return new Color(0.58f, 0.2f, 0.24f, 1f);
        }
    }

    private static string GetActionLabel(PlannedAction action)
    {
        if (action.type == ActionType.ATTACK)
        {
            return "Attack";
        }

        if (action.type == ActionType.ITEM)
        {
            return action.item != null ? action.item.itemName : "Item";
        }

        return action.skill != null ? action.skill.skillName : "Skill";
    }

    public void UpdateEnemyActionBar(List<BeatPlan> plan)
    {
        if (enemyActionBarHud == null) return;

        // Clear old visual
        foreach (var kvp in enemyActionNodesMap)
        {
            for (int i = 0; i < kvp.Value.Length; i++)
            {
                Image nodeBg = kvp.Value[i];
                bool isActive = (kvp.Key == "Boss" && i < 3) || (kvp.Key != "Boss" && i < 1);
                nodeBg.color = isActive ? new Color(0.15f, 0.15f, 0.25f, 1f) : new Color(0.1f, 0.1f, 0.12f, 0.5f);
                if (nodeBg.transform.childCount > 0)
                {
                    Text txt = nodeBg.transform.GetChild(0).GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.text = isActive ? "-" : "";
                        txt.color = isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                }
            }
        }

        if (plan == null) return;
        Dictionary<string, int> actorActionCount = new Dictionary<string, int>();

        foreach (var beat in plan)
        {
            foreach (var action in beat.actions)
            {
                if (action.actor == null || action.actor.isAlly) continue;
                string baseName = action.actor.characterName.Replace("Ally_", "").Replace("Enemy_", "");
                
                // Map internal names to display labels
                string displayLabel = "Enemy 1";
                if (baseName.ToLower().Contains("boss")) displayLabel = "Boss";
                else if (baseName.Contains("2")) displayLabel = "Enemy 2";
                
                if (!actorActionCount.ContainsKey(displayLabel)) actorActionCount[displayLabel] = 0;

                int i = actorActionCount[displayLabel];
                if (enemyActionNodesMap.ContainsKey(displayLabel) && i < enemyActionNodesMap[displayLabel].Length)
                {
                    Image nodeBg = enemyActionNodesMap[displayLabel][i];
                    nodeBg.color = (action.type == ActionType.ATTACK) ? new Color(0.3f, 0.4f, 0.6f, 1f) : new Color(0.7f, 0.2f, 0.2f, 1f);
                    
                    if (nodeBg.transform.childCount > 0)
                    {
                        Text txt = nodeBg.transform.GetChild(0).GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.text = (action.type == ActionType.ATTACK) ? "Attack" : "Skill";
                            txt.color = Color.white;
                        }
                    }
                    actorActionCount[displayLabel]++;
                }
            }
        }
    }

    private void CreateEnemyPlanHud(Transform parent)
    {
        if (enemyActionBarHud != null) return;
        
        enemyActionBarHud = new GameObject("EnemyActionBarGrid");
        enemyActionBarHud.transform.SetParent(parent, false);

        VerticalLayoutGroup vLayout = enemyActionBarHud.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 2;
        vLayout.padding = new RectOffset(6, 6, 6, 6);
        vLayout.childAlignment = TextAnchor.LowerLeft;
        vLayout.childControlHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childForceExpandWidth = false;

        Image gridBg = enemyActionBarHud.AddComponent<Image>();
        gridBg.color = new Color(0f, 0f, 0f, 0.85f); // Black background

        string[] enemyLabels = new string[] { "Boss", "Enemy 1", "Enemy 2" };
        int[] beatCounts = new int[] { 3, 1, 1 }; 
        int[] limitCounts = new int[] { 5, 2, 2 }; // Boss: 5, Enemy: 2

        enemyHpTextsMap.Clear();
        enemyHpFillsMap.Clear();
        enemyLimitSegmentsMap.Clear();

        for (int e = 0; e < enemyLabels.Length; e++)
        {
            string eLabel = enemyLabels[e];
            GameObject rowObj = new GameObject("Row_" + eLabel);
            rowObj.transform.SetParent(enemyActionBarHud.transform, false);
            HorizontalLayoutGroup hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 2;
            hLayout.padding = new RectOffset(0, 0, 0, 0);
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;

            // Avatar placeholder
            GameObject avatarObj = new GameObject("Avatar");
            avatarObj.transform.SetParent(rowObj.transform, false);
            LayoutElement avatarLe = avatarObj.AddComponent<LayoutElement>();
            avatarLe.minWidth = 32;
            avatarLe.minHeight = 32;
            avatarLe.preferredWidth = 32;
            avatarLe.preferredHeight = 32;
            avatarLe.flexibleWidth = 0;
            Image avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.preserveAspect = true;
            
            string spriteName = (eLabel == "Boss") ? "Boss-icon" : "enemy-icon";
            Sprite sp = Resources.Load<Sprite>("UI/" + spriteName);
            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("UI/" + spriteName);
                if (tex != null)
                {
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }

            if (sp != null) avatarImg.sprite = sp;
            else avatarImg.color = (eLabel == "Boss") ? new Color(0.8f, 0.23f, 0.23f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);

            // Info Column (HP + Limit)
            GameObject infoCol = new GameObject("InfoCol");
            infoCol.transform.SetParent(rowObj.transform, false);
            LayoutElement infoLe = infoCol.AddComponent<LayoutElement>();
            infoLe.minWidth = 60;
            VerticalLayoutGroup infoVLayout = infoCol.AddComponent<VerticalLayoutGroup>();
            infoVLayout.spacing = 1;
            infoVLayout.childControlHeight = true;
            infoVLayout.childControlWidth = true;
            infoVLayout.childAlignment = TextAnchor.MiddleLeft;
            
            // HP Bar Wrapper
            GameObject hpWrapper = new GameObject("HpWrapper");
            hpWrapper.transform.SetParent(infoCol.transform, false);
            LayoutElement hpLe = hpWrapper.AddComponent<LayoutElement>();
            hpLe.minHeight = 16;
            Image hpBg = hpWrapper.AddComponent<Image>();
            hpBg.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
            
            GameObject hpFillObj = new GameObject("HpFill");
            hpFillObj.transform.SetParent(hpWrapper.transform, false);
            Image hpFill = hpFillObj.AddComponent<Image>();
            hpFill.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red health
            RectTransform fillRect = hpFillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            enemyHpFillsMap[eLabel] = hpFill;

            GameObject hpTextObj = new GameObject("HpText");
            hpTextObj.transform.SetParent(hpWrapper.transform, false);
            Text hpTxt = hpTextObj.AddComponent<Text>();
            hpTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hpTxt.text = "HP: ?/?";
            hpTxt.fontSize = 10;
            hpTxt.color = Color.white;
            hpTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform txtRect = hpTextObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
            enemyHpTextsMap[eLabel] = hpTxt;
            
            // Limit Wrapper
            GameObject limitWrapper = new GameObject("LimitWrapper");
            limitWrapper.transform.SetParent(infoCol.transform, false);
            LayoutElement limitLe = limitWrapper.AddComponent<LayoutElement>();
            limitLe.minHeight = 8;
            HorizontalLayoutGroup limitHLayout = limitWrapper.AddComponent<HorizontalLayoutGroup>();
            limitHLayout.spacing = 1;
            limitHLayout.childControlWidth = true;
            limitHLayout.childControlHeight = true;
            
            int maxLim = limitCounts[e];
            Image[] limitSegs = new Image[maxLim];
            for(int j=0; j<maxLim; j++) {
                GameObject segObj = new GameObject("Seg_" + j);
                segObj.transform.SetParent(limitWrapper.transform, false);
                Image segImg = segObj.AddComponent<Image>();
                segImg.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Empty
                limitSegs[j] = segImg;
            }
            enemyLimitSegmentsMap[eLabel] = limitSegs;

            // Cells
            Image[] nodes = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                bool isActive = (i < beatCounts[e]);
                GameObject nodeObj = new GameObject("EnemyNode_" + i);
                nodeObj.transform.SetParent(rowObj.transform, false);
                LayoutElement nodeLe = nodeObj.AddComponent<LayoutElement>();
                nodeLe.minWidth = 36;
                nodeLe.minHeight = 32;
                
                Image bg = nodeObj.AddComponent<Image>();
                bg.color = isActive ? new Color(0.12f, 0.12f, 0.18f, 1f) : new Color(0f, 0f, 0f, 0f);
                
                if (isActive)
                {
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(nodeObj.transform, false);
                    Text actionText = textObj.AddComponent<Text>();
                    actionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    actionText.text = "-";
                    actionText.fontSize = 12;
                    actionText.color = Color.white;
                    actionText.alignment = TextAnchor.MiddleCenter;

                    RectTransform textRect = actionText.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
                }
                
                nodes[i] = bg;
            }
            enemyActionNodesMap[eLabel] = nodes;
        }
        
        RectTransform rect = enemyActionBarHud.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(300, 130);
        rect.anchoredPosition = new Vector2(30, 24);
    }

    private GameObject skillMenu;
    private GameObject itemMenu;

    private void CreateAllyHUDBlock(CharacterInteraction ally, Transform parent)
    {
        // Removed as per request (HP is now in Action Bar)
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
        if (soulText != null && BattleManager.Instance != null)
        {
            int red = BattleManager.Instance.currentRedSoul;
            int blue = BattleManager.Instance.currentBlueSoul;
            int soul = red + blue;
            int limit = 9; // Giả sử max là 9
            soulText.text = "Soul capacity\n" + soul + "/" + limit;
            
            if (soulIcons != null) {
                for(int i=0; i<soulIcons.Count; i++) {
                    if(soulIcons[i] != null) {
                        if (i < red) {
                            soulIcons[i].color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
                        } else if (i < red + blue) {
                            soulIcons[i].color = new Color(0.2f, 0.7f, 0.9f, 1f); // Blue
                        } else {
                            soulIcons[i].color = new Color(1f, 1f, 1f, 0.2f); // Empty
                        }
                    }
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
            
            // Disable keyboard nav for menu if skill is open
            if (show)
            {
                if (actionButtons.ContainsKey(ActionType.SKILL))
                    actionButtons[ActionType.SKILL].color = new Color(0.8f, 0.6f, 0.1f, 1f);
            }
            else 
            {
                HighlightActiveMenuOption();
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
            
            if (show)
            {
                if (actionButtons.ContainsKey(ActionType.ITEM))
                    actionButtons[ActionType.ITEM].color = new Color(0.8f, 0.6f, 0.1f, 1f);
            }
            else
            {
                HighlightActiveMenuOption();
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

    
    public bool IsActionMenuOpen()
    {
        return actionMenu != null && actionMenu.activeSelf && (skillMenu == null || !skillMenu.activeSelf) && (itemMenu == null || !itemMenu.activeSelf);
    }

    public void ShowActionMenu(bool show, Transform actorTransform = null)
    {
        // Update HUD Backgrounds
        if (allyRowBgsMap != null)
        {
            string selectedName = "";
            if (show && actorTransform != null)
            {
                selectedName = actorTransform.name.Replace("Ally_", "");
            }
            foreach (var kvp in allyRowBgsMap)
            {
                if (kvp.Key == selectedName)
                {
                    kvp.Value.color = new Color(0.1f, 0.4f, 0.8f, 0.5f); // Blue semi-transparent
                }
                else
                {
                    kvp.Value.color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

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
                // Reset navigation
                activeMenuIndex = 0;
                HighlightActiveMenuOption();
            }
            else
            {
                activeActorTransform = null;
            }
        }
    }


    public void ShowBossHUD(CharacterInteraction enemy)
    {
        // Removed as per request (HP is now in Action Bar)
    }

    public void UpdateLimitHUD(CharacterInteraction enemy)
    {
        // Removed as per request (HP is now in Action Bar)
    }

    public void HideBossHUD()
    {
        // Removed as per request (HP is now in Action Bar)
    }

    private Dictionary<RectTransform, Coroutine> hpRoutines = new Dictionary<RectTransform, Coroutine>();

    public void UpdateHP(CharacterInteraction character)
    {
        string baseName = character.characterName.Replace("Ally_", "").Replace("Enemy_", "");
        float hpPercent = (float)character.currentHP / character.maxHP;
        
        if (character.isAlly)
        {
            if (allyHpTextsMap.ContainsKey(baseName) && allyHpTextsMap[baseName] != null)
            {
                allyHpTextsMap[baseName].text = "HP: " + character.currentHP + "/" + character.maxHP;
            }
            if (allyHpFillsMap.ContainsKey(baseName) && allyHpFillsMap[baseName] != null)
            {
                UpdateHPImage(allyHpFillsMap[baseName].rectTransform, hpPercent);
            }
        }
        else
        {
            string displayLabel = "Enemy 1";
            if (baseName.ToLower().Contains("boss")) displayLabel = "Boss";
            else if (baseName.Contains("2")) displayLabel = "Enemy 2";
            
            if (enemyHpTextsMap.ContainsKey(displayLabel) && enemyHpTextsMap[displayLabel] != null)
            {
                enemyHpTextsMap[displayLabel].text = "HP: " + character.currentHP + "/" + character.maxHP;
            }
            if (enemyHpFillsMap.ContainsKey(displayLabel) && enemyHpFillsMap[displayLabel] != null)
            {
                UpdateHPImage(enemyHpFillsMap[displayLabel].rectTransform, hpPercent);
            }
            UpdateEnemyLimitHUD(character, displayLabel);
        }
    }
    
    public void UpdateEnemyLimitHUD(CharacterInteraction enemy, string displayLabel)
    {
        if (enemyLimitSegmentsMap.ContainsKey(displayLabel) && enemyLimitSegmentsMap[displayLabel] != null)
        {
            Image[] segs = enemyLimitSegmentsMap[displayLabel];
            for (int i = 0; i < segs.Length; i++)
            {
                if (segs[i] != null)
                {
                    segs[i].color = (i < enemy.currentLimit) ? new Color(0.6f, 0.2f, 0.8f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);
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
