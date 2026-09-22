using UnityEngine;
using System.Collections;
using FrankenXIII.Combat.Domain;

public class CharacterInteraction : MonoBehaviour
{
    private Renderer childRenderer;
    private Material mat;
    public bool isAlly = false;
    public bool isSelected = false;

    public string characterName = "Unknown";
    [SerializeField] private DemoCombatantId demoCombatantId = DemoCombatantId.None;
    public int maxHP = 100;
    public int currentHP = 100;
    public int maxLimit = 2;
    public int currentLimit = 2;
    public bool isDazed = false;
    public bool isDead = false;

    [Header("Boss Mechanics")]
    public bool isProtected = false;
    public int hpGateThreshold = 0;

    [Header("Status Ailments")]
    public bool isPoisoned = false;
    public int poisonDuration = 0;
    
    public bool isCursed = false;
    public int curseDuration = 0;
    
    public bool isVulnerable = false;
    public int vulnerabilityDuration = 0;
    
    public bool isBerserk = false;
    public int berserkDuration = 0;
    
    public bool isBlind = false;
    public int blindDuration = 0;

    // Core Stats (Demo Funding standard)
    public int baseDEF = 0;
    public int currentShield = 0;
    public int baseATK = 20;
    public int baseBreakATK = 1;
    public float baseCritRate = 0.05f;
    public float baseCritDMG = 1.5f;
    public string element = "Vật Lý"; // Physical, Mental, Chemical
    
    // Status Effects
    public bool hasWeakpoint = false; // "Toàn Thức" effect
    public float defShred = 0f; // "Bột Lân Tinh" effect
    public float atkBuff = 0f; // "Ghi Chép Sát Cơ" effect
    public int breakAtkBuff = 0;

    public System.Collections.Generic.List<SkillData> activeSkills = new System.Collections.Generic.List<SkillData>();

    private GameObject selectionCircle;

    public DemoCombatantId CombatantId => demoCombatantId;
    public StanceId currentStance = StanceId.SwordAndGun;

    public Vector3 originalPosition;
    public bool isPosInit = false;

    [Header("Animation")]
    public QuadAnimator quadAnimator;

    [Header("Selection Visual")]
    [SerializeField, Range(1f, 2f)] private float staticGlowPaddingScale = 1.15f;

    private GameObject selectionArrowObj;

    void CreateSelectionArrow()
    {
        selectionArrowObj = new GameObject("SelectionArrowCanvas");
        selectionArrowObj.transform.SetParent(this.transform, false);
        
        CapsuleCollider cap = GetComponent<CapsuleCollider>();
        float yOffset = cap != null ? cap.height / 2f + 0.8f : 2.5f; // Higher than character
        selectionArrowObj.transform.localPosition = new Vector3(0, yOffset, 0); 
        
        Canvas canvas = selectionArrowObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        selectionArrowObj.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 2f);
        selectionArrowObj.AddComponent<Billboard>(); 

        GameObject txtObj = new GameObject("ArrowText");
        txtObj.transform.SetParent(selectionArrowObj.transform, false);
        UnityEngine.UI.Text txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.LowerCenter; // Point exactly at the character
        txt.fontSize = 120; 
        txt.color = isAlly ? new Color(0.2f, 0.9f, 0.2f, 1f) : new Color(0.9f, 0.2f, 0.2f, 1f);
        UnityEngine.UI.Outline outline = txtObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        txt.text = "▼";
        
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        txtRect.localScale = new Vector3(0.015f, 0.015f, 1f); // Scale down for WorldSpace
        
        selectionArrowObj.SetActive(false);
    }

    void Start()
    {
        CreateSelectionArrow();
        
        // Apply debug config if exists
        if (DebugConfigUI.Overrides != null && DebugConfigUI.Overrides.ContainsKey(characterName))
        {
            DebugConfigUI.Overrides[characterName].ApplyTo(this);
        }

        if (!isPosInit)
        {
            originalPosition = transform.position;
            isPosInit = true;
        }
        currentHP = maxHP;

        // Initialize activeSkills fallback if empty
        if (activeSkills == null || activeSkills.Count == 0)
        {
            activeSkills = new System.Collections.Generic.List<SkillData>();
            if (CombatantId == FrankenXIII.Combat.Domain.DemoCombatantId.XIII || characterName.Contains("XIII"))
            {
                if (currentStance == StanceId.SwordAndGun)
                {
                    activeSkills.Add(new SkillData(SkillId.XIII_Attack, "Tà Thi Trảm", SkillCategory.ATTACK, 1.4f, 2, 1) { description = "Tấn công vật lý cơ bản, phá bền tốt." });
                    activeSkills.Add(new SkillData(SkillId.XIII_HeavySlash, "Huyết Đoạn Kích", SkillCategory.ATTACK, 1.8f, 1, 1) { description = "Sát thương cao, khả năng phá bền thấp." });
                    SkillData bH = new SkillData(SkillId.None, "Liều Mạng Bộc Phá", SkillCategory.ATTACK, 2.5f, 4, 2) { description = "Mất 150 HP để gây sát thương và phá bền cực lớn." };
                    bH.selfDamage = 150;
                    activeSkills.Add(bH);
                }
                else if (currentStance == StanceId.TwoHandedSword)
                {
                    activeSkills.Add(new SkillData(SkillId.None, "Trảm Phong", SkillCategory.ATTACK, 2.0f, 3, 2) { description = "Sát thương cao, tốn 2 Soul." });
                }
                else if (currentStance == StanceId.DualGuns)
                {
                    activeSkills.Add(new SkillData(SkillId.None, "Bão Đạn", SkillCategory.ATTACK, 1.2f, 1, 1) { description = "Tấn công nhanh, dễ bạo kích." });
                }
            }
            else if (CombatantId == FrankenXIII.Combat.Domain.DemoCombatantId.Mac || characterName.Contains("Mac"))
            {
                SkillData m1 = new SkillData(SkillId.Mac_Attack, "Quét Gậy", SkillCategory.ATTACK, 1.0f, 1, 1) { description = "Sát thương vật lý và trừ nhẹ Limit." };
                activeSkills.Add(m1);
                SkillData m2 = new SkillData(SkillId.Mac_Blind, "Ném Bột Hóa Chất", SkillCategory.DEBUFF, 0f, 0, 2) { description = "Gây mù mục tiêu, giảm 50% độ chính xác." };
                m2.inflictBlind = true;
                activeSkills.Add(m2);
                SkillData m3 = new SkillData(SkillId.Mac_DirectDamage, "Phóng Dao Hóa Chất", SkillCategory.ATTACK, 1.8f, 1, 2) { description = "Sát thương trực tiếp mạnh mẽ hệ Mixed." };
                activeSkills.Add(m3);
            }
            else if (CombatantId == FrankenXIII.Combat.Domain.DemoCombatantId.An || characterName.Contains("An"))
            {
                SkillData a1 = new SkillData(SkillId.An_HoThanPhu, "Hộ Thân Phù", SkillCategory.SUPPORT, 0f, 0, 2) { description = "Tạo Khiên ảo hấp thụ 350 Sát Thương." };
                a1.shieldAmount = 350;
                activeSkills.Add(a1);
                SkillData a2 = new SkillData(SkillId.An_DanHonThuat, "Dẫn Hồn Thuật", SkillCategory.SUPPORT, 0f, 0, 2) { description = "Hồi 30% HP cho đồng minh." };
                a2.healPercent = 0.3f;
                activeSkills.Add(a2);
                SkillData a3 = new SkillData(SkillId.An_Attack, "Trấn Trạch Lôi Bùa", SkillCategory.ATTACK, 1.3f, 2, 2) { description = "Sát thương Mental. Khắc hệ Tâm Linh." };
                a3.isMental = true;
                activeSkills.Add(a3);
            }
        }

        quadAnimator = GetComponentInChildren<QuadAnimator>();

        // Chỉ lấy Renderer của object chứa QuadAnimator để tránh lấy nhầm Shadow
        if (quadAnimator != null)
        {
            childRenderer = quadAnimator.GetComponent<Renderer>();
            if (childRenderer != null)
                mat = childRenderer.material;
        }
        else
        {
            childRenderer = GetComponentInChildren<Renderer>();
            if (childRenderer != null)
                mat = childRenderer.material;
        }

        if (childRenderer != null)
        {
            EnsureStaticGlowPadding();

            // Đổi màu Glow theo phe
            if (isAlly)
            {
                mat.SetColor("_GlowColor", new Color(0.4f, 0.8f, 1f, 1f)); // Xanh trời nhạt
                mat.SetFloat("_GlowThickness", 5f); // Viền Ally mỏng
            }
            else
            {
                mat.SetColor("_GlowColor", new Color(1f, 0.1f, 0.1f, 1f)); // Đỏ
                mat.SetFloat("_GlowThickness", 5f); // Viền Enemy giữ nguyên mỏng
            }
        }

        // Tạo vòng tròn dưới chân
        CreateSelectionCircle();

        if (!isAlly)
        {
            CreateMiniHPBar();
        }
    }

    private void EnsureStaticGlowPadding()
    {
        if (quadAnimator != null || childRenderer == null || mat == null)
        {
            return;
        }

        if (!mat.HasProperty("_SpriteUVRect"))
        {
            return;
        }

        float padding = Mathf.Max(1f, staticGlowPaddingScale);
        float offset = -(padding - 1f) * 0.5f;
        Vector2 textureScale = Vector2.one * padding;
        Vector2 textureOffset = Vector2.one * offset;

        childRenderer.transform.localScale = Vector3.Scale(
            childRenderer.transform.localScale,
            new Vector3(padding, padding, 1f));

        mat.mainTextureScale = textureScale;
        mat.mainTextureOffset = textureOffset;
        mat.SetVector("_SpriteUVRect", new Vector4(0f, 0f, 1f, 1f));

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTextureScale("_BaseMap", textureScale);
            mat.SetTextureOffset("_BaseMap", textureOffset);
        }
    }

    private RectTransform miniHpFillRect;
    private UnityEngine.UI.Text miniHpText;

    void CreateMiniHPBar()
    {
        GameObject canvasObj = new GameObject("MiniHPCanvas");
        canvasObj.transform.SetParent(this.transform, false);
        
        CapsuleCollider cap = GetComponent<CapsuleCollider>();
        float yOffset = cap != null ? cap.height / 2f + 0.5f : 2.5f;
        canvasObj.transform.localPosition = new Vector3(0, yOffset, 0); // Đỉnh đầu
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 0.2f);
        
        // Cần Billboard cho Canvas này để nó luôn quay mặt vào camera
        canvasObj.AddComponent<Billboard>(); 

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image fillImg = fill.AddComponent<UnityEngine.UI.Image>();
        fillImg.color = Color.red;
        
        miniHpFillRect = fill.GetComponent<RectTransform>();
        miniHpFillRect.anchorMin = Vector2.zero;
        float startFill = (float)currentHP / maxHP;
        miniHpFillRect.anchorMax = new Vector2(startFill, 1f);
        miniHpFillRect.offsetMin = Vector2.zero;
        miniHpFillRect.offsetMax = Vector2.zero;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(canvasObj.transform, false);
        miniHpText = txtObj.AddComponent<UnityEngine.UI.Text>();
        miniHpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        miniHpText.alignment = TextAnchor.MiddleCenter;
        miniHpText.fontSize = 24;
        miniHpText.color = Color.white;
        UnityEngine.UI.Outline outline = txtObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        miniHpText.text = currentHP + "/" + maxHP;
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        txtRect.localScale = new Vector3(0.01f, 0.01f, 1f); // Cực nhỏ vì renderMode là WorldSpace

        // Bổ sung Limit Bar (Chỉ dành cho Enemy/Boss)
        if (!isAlly)
        {
            GameObject limitContainerObject = new GameObject("LimitContainer");
            limitContainerObject.transform.SetParent(canvasObj.transform, false);
            RectTransform limitRect = limitContainerObject.AddComponent<RectTransform>();
            limitRect.anchorMin = new Vector2(0, -0.5f);
            limitRect.anchorMax = new Vector2(1, -0.1f);
            limitRect.offsetMin = Vector2.zero;
            limitRect.offsetMax = Vector2.zero;

            UnityEngine.UI.HorizontalLayoutGroup hLayout = limitContainerObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hLayout.spacing = 0.02f;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            miniLimitContainer = limitContainerObject.transform;
            RebuildMiniLimitSegments();
        }
    }

    private UnityEngine.UI.Image[] miniLimitSegments;
    private Transform miniLimitContainer;

    private Coroutine hpRoutine;

    public int TakeDamage(int intendedDamage)
    {
        if (isProtected) return 0;
        
        int actualDamage = intendedDamage;
        if (hpGateThreshold > 0 && currentHP - actualDamage < hpGateThreshold)
        {
            actualDamage = currentHP - hpGateThreshold;
            if (actualDamage < 0) actualDamage = 0;
        }
        
        currentHP -= actualDamage;
        if (currentHP < 0) currentHP = 0;
        return actualDamage;
    }

    public void UpdateMiniHP()
    {
        if (miniHpFillRect != null)
        {
            if (hpRoutine != null) StopCoroutine(hpRoutine);
            hpRoutine = StartCoroutine(SmoothUpdateMiniHP((float)currentHP / maxHP));
        }
        if (miniHpText != null)
        {
            miniHpText.text = currentHP + "/" + maxHP;
        }
    }

    public void UpdateMiniLimit()
    {
        if (miniLimitSegments == null) return;
        for (int i = 0; i < miniLimitSegments.Length; i++)
        {
            if (i < currentLimit)
            {
                miniLimitSegments[i].color = new Color(0.6f, 0.2f, 0.8f, 1f); // Purple
            }
            else
            {
                miniLimitSegments[i].color = new Color(0.2f, 0.1f, 0.3f, 0.8f); // Dark empty
            }
        }

        if (!isAlly && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateLimitHUD(this);
        }
    }

    public void ConfigureCombatantId(DemoCombatantId combatantId)
    {
        demoCombatantId = combatantId;
    }

    public void SetLimitCapacity(int capacity, bool refill)
    {
        maxLimit = Mathf.Max(1, capacity);
        currentLimit = refill ? maxLimit : Mathf.Clamp(currentLimit, 0, maxLimit);
        RebuildMiniLimitSegments();
        UpdateMiniLimit();
    }

    private void RebuildMiniLimitSegments()
    {
        if (miniLimitContainer == null)
        {
            return;
        }

        for (int i = miniLimitContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(miniLimitContainer.GetChild(i).gameObject);
        }

        miniLimitSegments = new UnityEngine.UI.Image[maxLimit];
        for (int i = 0; i < maxLimit; i++)
        {
            GameObject segmentObject = new GameObject("LimitSegment_" + i);
            segmentObject.transform.SetParent(miniLimitContainer, false);
            UnityEngine.UI.Image segmentImage = segmentObject.AddComponent<UnityEngine.UI.Image>();
            segmentImage.color = i < currentLimit
                ? new Color(0.6f, 0.2f, 0.8f, 1f)
                : new Color(0.2f, 0.1f, 0.3f, 0.8f);
            miniLimitSegments[i] = segmentImage;
        }
    }

    private IEnumerator SmoothUpdateMiniHP(float targetFill)
    {
        float startFill = miniHpFillRect.anchorMax.x;
        float elapsed = 0;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            miniHpFillRect.anchorMax = new Vector2(Mathf.Lerp(startFill, targetFill, elapsed / duration), 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        miniHpFillRect.anchorMax = new Vector2(targetFill, 1f);
    }

    void CreateSelectionCircle()
    {
        selectionCircle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        selectionCircle.name = "SelectionCircle";
        selectionCircle.transform.SetParent(this.transform); // Con của Root
        
        // Căn xuống dưới cùng của Collider (Chân nhân vật)
        CapsuleCollider cap = GetComponent<CapsuleCollider>();
        float yOffset = cap != null ? -cap.height / 2f + 0.1f : 0f;
        selectionCircle.transform.localPosition = new Vector3(0, yOffset, 0); 
        
        selectionCircle.transform.localRotation = Quaternion.Euler(90, 0, 0); // Nằm bẹp xuống đất
        selectionCircle.transform.localScale = new Vector3(4f, 4f, 4f); // Kích thước vòng
        
        Destroy(selectionCircle.GetComponent<Collider>()); // Xóa collider của quad để không lỗi click

        Shader circleShader = Shader.Find("Custom/SelectionCircle");
        if (circleShader != null)
        {
            Material circleMat = new Material(circleShader);
            selectionCircle.GetComponent<Renderer>().material = circleMat;
        }
        else
        {
            Debug.LogError("Không tìm thấy Shader Custom/SelectionCircle");
        }
        
        selectionCircle.SetActive(false); // Ẩn mặc định
    }

    private bool CanTargetThis()
    {
        if (BattleManager.Instance == null) return true;
        
        if (BattleManager.Instance.state == BattleState.PLAYER_TURN)
        {
            return isAlly; // Trạng thái nhàn rỗi: Chỉ được click đồng minh
        }
        else if (BattleManager.Instance.state == BattleState.WAIT_TARGET)
        {
            if (BattleManager.Instance.IsSelectingSpecialTarget)
            {
                return !isAlly;
            }

            bool isSupport = (BattleManager.Instance.pendingAction == ActionType.SKILL && 
                              BattleManager.Instance.pendingSkill != null && 
                              BattleManager.Instance.pendingSkill.category == SkillCategory.SUPPORT);
                              
            bool isAttackCommand = (BattleManager.Instance.pendingAction == ActionType.ATTACK || 
                                    (BattleManager.Instance.pendingAction == ActionType.SKILL && !isSupport));

            if (!isAlly)
            {
                // Click kẻ địch: Cho phép nếu lệnh hiện tại là tấn công / debuff
                return isAttackCommand;
            }
            else
            {
                // Click đồng minh: 
                // 1. Cho phép nếu lệnh hiện tại là Hỗ trợ (buff)
                // 2. Cũng cho phép nếu là lệnh tấn công nhưng người chơi đổi ý muốn chọn đồng minh khác (BattleManager tự lo Cancel)
                return true; 
            }
        }
        return false;
    }

    void OnMouseOver()
    {
        if (mat == null) return;
        
        // Nếu chuột đè lên UI (vd: Button), coi như không hover
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (!isSelected) mat.SetFloat("_IsGlowing", 0);
            return;
        }

        if (!CanTargetThis())
        {
            if (!isSelected) mat.SetFloat("_IsGlowing", 0);
            return;
        }

        if (!isSelected)
        {
            mat.SetFloat("_IsGlowing", 1);
        }
    }

    void OnMouseExit()
    {
        if (!isSelected && mat != null)
        {
            mat.SetFloat("_IsGlowing", 0);
        }
    }

    void OnMouseDown()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (mat != null)
        {
            if (!CanTargetThis())
            {
                return;
            }
                
            // Báo cho BattleManager biết
            if (BattleManager.Instance.state == BattleState.WAIT_TARGET || BattleManager.Instance.state == BattleState.PLAYER_TURN)
            {
                BattleManager.Instance.OnTargetSelected(this);
            }
        }
    }

    public void PlayAnimation(string stateName)
    {
        if (quadAnimator == null)
        {
            quadAnimator = GetComponentInChildren<QuadAnimator>();
        }
        
        if (quadAnimator != null)
        {
            quadAnimator.PlayAnim(stateName);
        }
    }

    public void SelectCharacter()
    {
        isSelected = true;
        if (mat != null)
        {
            mat.SetFloat("_IsGlowing", 1); // Giữ Glow luôn bật khi đã chọn
        }
        if (selectionCircle != null) selectionCircle.SetActive(true); // Bật vòng chân
        if (selectionArrowObj != null) selectionArrowObj.SetActive(true);
        
        // Hiện Boss HUD nếu là quái
        if (!isAlly && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ShowBossHUD(this);
        }
    }

    public void Deselect(bool hideBossHUD = true)
    {
        isSelected = false;
        if (mat != null)
        {
            mat.SetFloat("_IsGlowing", 0);
        }
        if (selectionCircle != null) selectionCircle.SetActive(false);
        if (selectionArrowObj != null) selectionArrowObj.SetActive(false);
        
        if (!isAlly && hideBossHUD && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.HideBossHUD();
        }
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
        isPosInit = true;
    }

    public void TakeHit(Vector3 attackerPos)
    {
        PlayAnimation("hit");
        StartCoroutine(FlashCoroutine());
        StartCoroutine(ResetAnimationAfter("idle", 0.6f));
    }

    public IEnumerator ResetAnimationAfter(string animName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isDead)
        {
            PlayAnimation(animName);
        }
    }

    IEnumerator FlashCoroutine()
    {
        if (mat != null)
        {
            // Kẻ địch nháy xanh lóa (Cyan), Đồng minh nháy đỏ (Red)
            Color flashColor = isAlly ? Color.red : Color.cyan;
            mat.SetColor("_FlashColor", flashColor);
            
            // Nháy 1 lần duy nhất trong 0.2s
            mat.SetFloat("_IsFlashing", 1f);
            yield return new WaitForSeconds(0.2f);
            mat.SetFloat("_IsFlashing", 0f);
        }
    }


    public void Die()
    {
        isDead = true;
        currentHP = 0;
        
        // Tắt vòng sáng
        Deselect();
        
        // Tắt Collider để không click được nữa
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (quadAnimator != null)
        {
            PlayAnimation("downed");
        }
        else
        {
            // Tắt hiển thị (Visual child) nếu không có animator
            Transform visual = transform.Find("Visual");
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
            }
        }

        // Tắt thanh HP Mini
        Transform miniHp = transform.Find("MiniHPCanvas");
        if (miniHp != null)
        {
            miniHp.gameObject.SetActive(false);
        }
    }

    public void RecoverFromTemporaryCollapse(int recoveredHp)
    {
        isDead = false;
        isDazed = false;
        currentHP = Mathf.Clamp(recoveredHp, 1, maxHP);
        currentLimit = maxLimit;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);

        Transform miniHp = transform.Find("MiniHPCanvas");
        if (miniHp != null) miniHp.gameObject.SetActive(true);

        PlayAnimation("idle");
        UpdateMiniHP();
        UpdateMiniLimit();
    }

    public void ApplyStatus(string statusName, int duration)
    {
        if (isDead) return;

        switch (statusName)
        {
            case "Poison":
                isPoisoned = true;
                poisonDuration = duration;
                break;
            case "Curse":
                isCursed = true;
                curseDuration = duration;
                break;
            case "Vulnerability":
                isVulnerable = true;
                vulnerabilityDuration = duration;
                break;
            case "Berserk":
                isBerserk = true;
                berserkDuration = duration;
                break;
        }
        BattleUIManager.Instance.ShowMessage(characterName + " bị dính " + statusName + "!");
    }

    public void CureStatus(string statusName)
    {
        switch (statusName)
        {
            case "Poison":
                isPoisoned = false;
                poisonDuration = 0;
                break;
            case "Curse":
                isCursed = false;
                curseDuration = 0;
                break;
            case "Vulnerability":
                isVulnerable = false;
                vulnerabilityDuration = 0;
                break;
            case "Berserk":
                isBerserk = false;
                berserkDuration = 0;
                break;
        }
    }

    public void TickStatuses()
    {
        if (isDead) return;

        if (isPoisoned)
        {
            int poisonDamage = Mathf.Max(1, maxHP / 20); // 5% max HP per round
            TakeDamage(poisonDamage);
            BattleUIManager.Instance.ShowMessage(characterName + " bị mất máu do Poison!");
            UpdateMiniHP();
            if (currentHP <= 0)
            {
                Die();
                if (isAlly) BattleManager.Instance.allies.Remove(this);
                else BattleManager.Instance.enemies.Remove(this);
                return;
            }

            poisonDuration--;
            if (poisonDuration <= 0) isPoisoned = false;
        }

        if (isCursed)
        {
            curseDuration--;
            if (curseDuration <= 0) isCursed = false;
        }

        if (isVulnerable)
        {
            vulnerabilityDuration--;
            if (vulnerabilityDuration <= 0) isVulnerable = false;
        }

        if (isBerserk)
        {
            berserkDuration--;
            if (berserkDuration <= 0) isBerserk = false;
        }
    }
}
