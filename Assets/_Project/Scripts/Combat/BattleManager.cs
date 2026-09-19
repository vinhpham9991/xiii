using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { START, PLAYER_TURN, WAIT_TARGET, ENEMY_TURN, WON, LOST }
public enum ActionType { ATTACK, SKILL, ITEM }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public BattleState state;
    public ActionType pendingAction;
    public SkillData pendingSkill;
    public ItemData pendingItem;

    public CharacterInteraction currentHighlight;

    // Inventory
    public Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();

    public List<CharacterInteraction> allies = new List<CharacterInteraction>();
    public List<CharacterInteraction> enemies = new List<CharacterInteraction>();
    
    // Plans
    public List<BeatPlan> playerPlan = new List<BeatPlan>();
    public List<BeatPlan> enemyPlan = new List<BeatPlan>();

    // Planning Phase
    private bool isExecuting = false;

    private void Awake()
    {
        Instance = this;
        
        // Cài đặt BGM
        AudioSource bgmSource = gameObject.AddComponent<AudioSource>();
        AudioClip bgmClip = Resources.Load<AudioClip>("BGM/battle_bgm");
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;
            bgmSource.Play();
        }
    }

    public void PlaySFX(string sfxName, float volume = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>("SFX/" + sfxName);
        if (clip != null)
        {
            GameObject sfxObj = new GameObject("SFX_" + sfxName);
            AudioSource src = sfxObj.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = volume;
            src.Play();
            Destroy(sfxObj, clip.length + 0.1f);
        }
    }

    public CharacterInteraction currentActor;
    public CharacterInteraction currentTarget; // Dùng để xác nhận mục tiêu (click lần 2)

    private Vector3 baseCamPos;
    private float baseCamFOV;

    private Camera mainCam;
    private Quaternion defaultCamRot;
    private Quaternion rightCamRot;
    private Quaternion leftCamRot;
    private Coroutine cameraRoutine;

    void Start()
    {
        state = BattleState.START;
        
        mainCam = Camera.main;
        if (mainCam != null)
        {
            defaultCamRot = mainCam.transform.rotation;
            // Nhân Quaternion.Euler ở bên trái để xoay quanh trục Y toàn cục (World Y), tránh bị nghiêng (tilt) camera
            rightCamRot = Quaternion.Euler(0, 10f, 0) * defaultCamRot;
            leftCamRot = Quaternion.Euler(0, -10f, 0) * defaultCamRot;
        }

        // Tự động tìm các nhân vật trên sân
        CharacterInteraction[] chars = FindObjectsOfType<CharacterInteraction>();
        foreach(var c in chars)
        {
            if (c.isAlly) allies.Add(c);
            else enemies.Add(c);
        }

        BattleUIManager.Instance.SetupUI();

        // Demo items
        ItemData potion = ScriptableObject.CreateInstance<ItemData>();
        potion.itemName = "Health Potion";
        potion.itemType = ItemType.HEAL;
        potion.healAmount = 100;
        potion.description = "Hồi 100 HP";
        inventory.Add(potion, 3);

        ItemData buffPotion = ScriptableObject.CreateInstance<ItemData>();
        buffPotion.itemName = "Power Elixir";
        buffPotion.itemType = ItemType.BUFF_STATS;
        buffPotion.atkBuff = 0.5f; // Buff 50% sức tấn công
        buffPotion.description = "Tăng 50% Sức Tấn Công";
        inventory.Add(buffPotion, 3);

        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // Chờ diễn hoạt lao vào trận (khoảng 2 giây)
        yield return new WaitForSeconds(2f);
        
        // Cập nhật UI thông báo
        BattleUIManager.Instance.ShowMessage("BẮT ĐẦU CHIẾN ĐẤU!");
        yield return new WaitForSeconds(1f);

        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        currentRedSoul = MAX_RED_SOUL;
        if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateSoulUI();
        
        playerPlan.Clear();
        if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateActionBar(playerPlan);
        PlayerTurn();
    }

    void PlayerTurn()
    {
        state = BattleState.PLAYER_TURN;
        currentTarget = null;
        BattleUIManager.Instance.ShowMessage("Lượt của Phe Ta\n(WASD/Q/E: chọn nhân vật | Space: xác nhận | Enter: thực thi)");
        BattleUIManager.Instance.ShowActionMenu(false); // Ẩn đi chờ người chơi chọn
        RotateCameraTo(defaultCamRot);

        // Tự động highlight nhân vật XIII hoặc nhân vật đầu tiên
        currentHighlight = null;
        foreach (var ally in allies)
        {
            if (!ally.isDead && ally.characterName.Contains("XIII"))
            {
                SetHighlight(ally);
                break;
            }
        }
        if (currentHighlight == null && allies.Count > 0)
        {
            SetHighlight(allies[0]);
        }
        
        // Show Execute instruction
        if (playerPlan.Count > 0)
        {
            int totalActions = 0;
            foreach (var beat in playerPlan) totalActions += beat.actions.Count;
            BattleUIManager.Instance.ShowMessage("Lượt của Phe Ta\n(Nhấn SPACE để THỰC THI " + totalActions + " lệnh)");
        }
    }

    public void CancelActorSelection()
    {
        currentActor = null;
        PlayerTurn();
    }

    private void SetHighlight(CharacterInteraction character)
    {
        if (currentHighlight != null && currentHighlight != currentTarget && currentHighlight != currentActor)
        {
            currentHighlight.Deselect(false); // Bỏ glow nhưng không giấu Boss HUD nếu là enemy
        }
        
        currentHighlight = character;
        
        if (currentHighlight != null)
        {
            currentHighlight.SelectCharacter();
        }
    }

    private void Update()
    {
        // Global Shortcuts (Placeholders for UI/Toggles)
        if (Input.GetKeyDown(KeyCode.Escape)) {
            // Settings / Pause
            Debug.Log("Settings / Pause Toggled");
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            Debug.Log("Toggle Tips");
        }
        if (Input.GetKeyDown(KeyCode.Home)) {
            Debug.Log("Toggle Beat Table");
        }
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("Inspect Detail");
        }
        if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
            Debug.Log("Secondary Character Switch");
        }

        if (state == BattleState.PLAYER_TURN && currentActor == null && !isExecuting)
        {
            // Execute planned actions (Enter)
            if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && playerPlan.Count > 0)
            {
                StartCoroutine(ExecutePlanRoutine());
                return;
            }

            // Clear Action (Delete)
            if (Input.GetKeyDown(KeyCode.Delete) && playerPlan.Count > 0)
            {
                playerPlan.Clear();
                currentRedSoul = MAX_RED_SOUL;
                foreach (var a in allies) { 
                    if (!a.isDead) { a.currentLimit = a.maxLimit; a.UpdateMiniLimit(); }
                }
                BattleUIManager.Instance.UpdateSoulUI();
                BattleUIManager.Instance.UpdateActionBar(playerPlan);
                BattleUIManager.Instance.ShowMessage("Đã xóa toàn bộ kế hoạch Phe Ta!");
                return;
            }

            // Chọn nhân vật bằng D-Pad / Arrow keys / WASD / Q / E
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q))
            {
                CycleHighlight(allies, -1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E))
            {
                CycleHighlight(allies, 1);
            }
            // Confirm (Space)
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (currentHighlight != null && currentHighlight.isAlly && !currentHighlight.isDead)
                {
                    OnTargetSelected(currentHighlight);
                }
            }
        }
        else if (state == BattleState.WAIT_TARGET)
        {
            // Xác định danh sách mục tiêu hợp lệ
            bool isSupportAction = (pendingAction == ActionType.ITEM) || (pendingAction == ActionType.SKILL && pendingSkill != null && pendingSkill.category == SkillCategory.SUPPORT);
            List<CharacterInteraction> validTargets = isSupportAction ? allies : enemies;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q))
            {
                CycleHighlight(validTargets, -1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E))
            {
                CycleHighlight(validTargets, 1);
            }
            else if (Input.GetKeyDown(KeyCode.Space)) // Confirm is Space
            {
                if (currentHighlight != null && !currentHighlight.isDead)
                {
                    OnTargetSelected(currentHighlight);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Backspace)) // Cancel is only Backspace (Esc is reserved)
            {
                // Hủy lệnh quay lại chọn lệnh
                CancelActorSelection();
            }
        }

        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.frameCount == lastActionFrame) return;

            // Bỏ qua nếu đang bấm vào UI (Button, Panel, HUD)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Kiểm tra bấm ra ngoài không khí hoặc bấm vào mặt đất
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.GetComponent<CharacterInteraction>() == null)
            {
                DeselectAll();
            }
        }
    }

    private void CycleHighlight(List<CharacterInteraction> list, int direction)
    {
        List<CharacterInteraction> aliveList = new List<CharacterInteraction>();
        foreach (var c in list) if (!c.isDead) aliveList.Add(c);

        if (aliveList.Count == 0) return;

        int currentIndex = aliveList.IndexOf(currentHighlight);
        if (currentIndex == -1) currentIndex = 0;
        else
        {
            currentIndex = (currentIndex + direction + aliveList.Count) % aliveList.Count;
        }

        SetHighlight(aliveList[currentIndex]);
    }

    private void RotateCameraTo(Quaternion targetRot)
    {
        if (mainCam == null) return;
        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        cameraRoutine = StartCoroutine(SmoothRotateCamera(targetRot));
    }

    private IEnumerator SmoothRotateCamera(Quaternion targetRot)
    {
        float elapsed = 0;
        float duration = 0.15f; // Nhanh hơn (cinematic)
        Quaternion startRot = mainCam.transform.rotation;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // SmoothStep (Ease in-out)
            mainCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCam.transform.rotation = targetRot;
    }

    private CharacterInteraction GetNearestEnemy(CharacterInteraction actor)
    {
        CharacterInteraction nearest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in enemies)
        {
            if (enemy.isDead) continue;
            float dist = Vector3.Distance(actor.transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    public bool ConsumeSouls(int cost)
    {
        if (currentRedSoul + currentBlueSoul < cost) return false;
        
        int redToConsume = Mathf.Min(currentRedSoul, cost);
        currentRedSoul -= redToConsume;
        
        int remainingCost = cost - redToConsume;
        if (remainingCost > 0)
        {
            currentBlueSoul -= remainingCost;
        }
        
        if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateSoulUI();
        return true;
    }

    public void AddBlueSoul(int amount, CharacterInteraction actor)
    {
        StartCoroutine(AnimateBlueSoulDrop(amount, actor));
    }

    IEnumerator AnimateBlueSoulDrop(int amount, CharacterInteraction actor)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject soulObj = new GameObject("BlueSoulDrop");
            soulObj.transform.position = actor.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, -2f);
            SpriteRenderer sr = soulObj.AddComponent<SpriteRenderer>();
            
            // Nếu bluesoul chưa được set Import Setting là Sprite, Resources.Load<Sprite> sẽ null. 
            // Giải pháp an toàn: Load Texture2D rồi tạo Sprite từ nó.
            Sprite soulSprite = Resources.Load<Sprite>("bluesoul");
            if (soulSprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("bluesoul");
                if (tex != null)
                {
                    soulSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            sr.sprite = soulSprite;
            // Thu nhỏ soul scale lại (từ 0.2 xuống 0.1)
            soulObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            // Pop out animation (rơi xuống nền)
            Vector3 startPos = soulObj.transform.position;
            Vector3 targetPos = startPos + new Vector3(Random.Range(-1.5f, 1.5f), 0, 0);
            targetPos.y = 0.2f; 
            
            if (BattleManager.Instance != null) BattleManager.Instance.PlaySFX("sfx_soul_drop", 0.3f);
            
            float elapsed = 0;
            // Rơi ra trong 0.5s
            while(elapsed < 0.5f)
            {
                float t = elapsed / 0.5f;
                // Parabola pop
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f; // Nảy lên cong một chút rồi rơi xuống
                soulObj.transform.position = currentPos;
                elapsed += Time.deltaTime;
                yield return null;
            }
            soulObj.transform.position = targetPos;
            
            // Delay 1s
            yield return new WaitForSeconds(1f);
            
            // Fly to HUD
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.soulVessel != null)
            {
                if (BattleManager.Instance != null) BattleManager.Instance.PlaySFX("sfx_soul_fly", 0.3f);
                
                RectTransform targetRect = BattleUIManager.Instance.soulVessel.GetComponent<RectTransform>();
                float flyElapsed = 0;
                Vector3 startFly = soulObj.transform.position;
                // Bay thật nhanh lên UI trong 0.5s
                while (flyElapsed < 0.5f)
                {
                    // Lấy tọa độ Screen của HUD
                    Vector3 screenPos = targetRect.position;
                    // Dịch màn hình sang thế giới (Camera Z = 10 -> offset 10)
                    Vector3 uiWorld = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x + 50f, screenPos.y - 50f, Mathf.Abs(Camera.main.transform.position.z)));
                    
                    soulObj.transform.position = Vector3.Lerp(startFly, uiWorld, flyElapsed / 0.5f);
                    flyElapsed += Time.deltaTime;
                    yield return null;
                }
            }
            
            Destroy(soulObj);
            
            // Cộng thực tế
            int oldBlue = currentBlueSoul;
            currentBlueSoul = Mathf.Min(MAX_BLUE_SOUL, currentBlueSoul + 1);
            if (currentBlueSoul > oldBlue && BattleUIManager.Instance != null) 
            {
                BattleUIManager.Instance.UpdateSoulUI();
            }
            
            // Đợi xíu trước khi văng hạt tiếp theo (nếu amount > 1)
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void OnActionSelected(ActionType action)
    {
        if (state != BattleState.PLAYER_TURN) return;

        lastActionFrame = Time.frameCount;
        
        pendingAction = action;
        state = BattleState.WAIT_TARGET;
        currentTarget = null;
        BattleUIManager.Instance.ShowActionMenu(false);
        BattleUIManager.Instance.ShowMessage("Chọn Mục Tiêu...");
        
        // Chờ người chơi chọn mục tiêu
        // Auto select target based on action type
        if (currentActor != null)
        {
            bool isSupport = (action == ActionType.ITEM) || (action == ActionType.SKILL && pendingSkill != null && pendingSkill.category == SkillCategory.SUPPORT);
            
            if (isSupport)
            {
                // Chọn chính mình làm mục tiêu mặc định
                SetHighlight(currentActor);
                OnTargetSelected(currentActor);
            }
            else
            {
                // Xoay sang trái để nhìn địch
                RotateCameraTo(leftCamRot);
                CharacterInteraction nearest = GetNearestEnemy(currentActor);
                if (nearest != null)
                {
                    SetHighlight(nearest);
                    OnTargetSelected(nearest);
                }
            }
        }
    }

    public void OnTargetSelected(CharacterInteraction target)
    {
        if (state == BattleState.PLAYER_TURN)
        {
            if (target.isAlly)
            {
                // Tắt chọn các nhân vật khác
                CharacterInteraction[] chars = FindObjectsOfType<CharacterInteraction>();
                foreach (var c in chars) if (c != target) c.Deselect();
                
                // Bật visual cho nhân vật mới
                target.SelectCharacter();

                currentActor = target;
                BattleUIManager.Instance.ShowActionMenu(true, target.transform);
                
                // Chọn nhân vật và mở menu -> xoay phải
                RotateCameraTo(rightCamRot);
            }
        }
        else if (state == BattleState.WAIT_TARGET)
        {
            bool isSupportAction = (pendingAction == ActionType.ITEM) || (pendingAction == ActionType.SKILL && pendingSkill != null && pendingSkill.category == SkillCategory.SUPPORT);

            // Nếu click đồng minh mà lệnh không phải Hỗ trợ -> Cancel lệnh
            if (target.isAlly && !isSupportAction)
            {
                CancelActorSelection();
                return;
            }

            // Nếu click kẻ địch mà lệnh là Hỗ trợ -> Bỏ qua
            if (!target.isAlly && isSupportAction)
            {
                return;
            }

            if (currentTarget == target)
            {
                // Đã chọn trước đó -> Lên Kế Hoạch (Add to Plan)
                PlannedAction newAction = new PlannedAction
                {
                    actor = currentActor,
                    target = target,
                    type = pendingAction,
                    skill = pendingSkill,
                    item = pendingItem
                };

                AddActionToPlan(newAction);

                // Reset selection
                DeselectAll(false);
                CancelActorSelection();
            }
            else
            {
                // Chọn mục tiêu lần đầu (để xác nhận ở lần click/nhấn sau)
                currentTarget = target;
                SetHighlight(target);
                
                // Tắt chọn các nhân vật khác
                CharacterInteraction[] chars = FindObjectsOfType<CharacterInteraction>();
                foreach (var c in chars) if (c != target) c.Deselect(false); // đừng tắt HUD boss
                
                target.SelectCharacter();
                
                if (!target.isAlly)
                {
                    BattleUIManager.Instance.ShowBossHUD(target);
                    BattleUIManager.Instance.ShowMessage("Mục tiêu: " + target.characterName + ". [Space] lần nữa để xác nhận | [Backspace] để hủy");
                }
                else
                {
                    BattleUIManager.Instance.ShowMessage("Mục tiêu hỗ trợ: " + target.characterName + ". [Space] lần nữa để xác nhận | [Backspace] để hủy");
                }
            }
        }
    }

    private void AddActionToPlan(PlannedAction newAction)
    {
        // Đếm xem actor này đã có bao nhiêu action rồi
        int actorActionCount = 0;
        foreach (var beat in playerPlan)
        {
            foreach (var a in beat.actions)
            {
                if (a.actor == newAction.actor) actorActionCount++;
            }
        }

        if (actorActionCount >= 2)
        {
            BattleUIManager.Instance.ShowMessage(newAction.actor.characterName + " đã đầy lệnh (tối đa 2 lệnh/round)!");
            return;
        }
        
        if (newAction.type == ActionType.SKILL && newAction.skill != null)
        {
            int cost = newAction.skill.soulCost;
            if (currentRedSoul + currentBlueSoul < cost)
            {
                BattleUIManager.Instance.ShowMessage("Không đủ Hồn Năng!");
                return;
            }
            
            if (currentRedSoul >= cost)
            {
                currentRedSoul -= cost;
            }
            else
            {
                int remainingCost = cost - currentRedSoul;
                currentRedSoul = 0;
                currentBlueSoul -= remainingCost;
            }
        }

        // Khởi tạo Beat nếu chưa có
        while (playerPlan.Count <= actorActionCount)
        {
            playerPlan.Add(new BeatPlan());
        }

        playerPlan[actorActionCount].AddAction(newAction);
        
        // Cập nhật UI
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateSoulUI();
            BattleUIManager.Instance.UpdateActionBar(playerPlan);
        }
    }

    public void OnExecuteButtonClicked()
    {
        if (state == BattleState.PLAYER_TURN && currentActor == null && !isExecuting && playerPlan.Count > 0)
        {
            StartCoroutine(ExecutePlanRoutine());
        }
    }

    private bool HasActionInBeat(CharacterInteraction actor, int beatIndex)
    {
        if (beatIndex < 0 || beatIndex >= playerPlan.Count) return false;
        foreach(var a in playerPlan[beatIndex].actions) {
            if (a.actor == actor && !a.isCancelled) return true;
        }
        return false;
    }

    private CharacterInteraction GetTargetInBeat(CharacterInteraction actor, int beatIndex)
    {
        if (beatIndex < 0 || beatIndex >= playerPlan.Count) return null;
        foreach(var a in playerPlan[beatIndex].actions) {
            if (a.actor == actor) return a.target;
        }
        return null;
    }

    private IEnumerator ExecuteActionAsync(PlannedAction action, bool isFirst, bool isLast, CharacterInteraction prevTarget)
    {
        if (action.type == ActionType.SKILL && action.skill != null && action.skill.category == SkillCategory.SUPPORT)
        {
            yield return StartCoroutine(ExecuteSupport(action.actor, action.target, action.skill, isFirst, isLast, prevTarget));
        }
        else if (action.type == ActionType.ITEM)
        {
            yield return StartCoroutine(ExecuteItem(action.actor, action.target, action.item, isFirst, isLast, prevTarget));
        }
        else if (action.type == ActionType.SKILL)
        {
            yield return StartCoroutine(ExecuteAttack(action.actor, action.target, action.type, action.skill, isFirst, isLast, prevTarget));
        }
        else
        {
            yield return StartCoroutine(ExecuteAttack(action.actor, action.target, action.type, null, isFirst, isLast, prevTarget));
        }
        action.isResolved = true;
    }

    private IEnumerator ExecutePlanRoutine()
    {
        isExecuting = true;
        RotateCameraTo(defaultCamRot);
        baseCamPos = Camera.main.transform.position;
        baseCamFOV = Camera.main.fieldOfView;

        BattleUIManager.Instance.ShowMessage("THỰC THI KẾ HOẠCH!");
        
        for (int beatIndex = 0; beatIndex < playerPlan.Count; beatIndex++)
        {
            var beat = playerPlan[beatIndex];
            
            foreach (var action in beat.actions)
            {
                if (action.actor.isDead) 
                {
                    action.isCancelled = true;
                    continue;
                }
                
                if (action.target.isDead && action.type != ActionType.ITEM)
                {
                    action.isCancelled = true;
                    BattleUIManager.Instance.ShowMessage(action.actor.characterName + " hủy lệnh vì mục tiêu đã chết!");
                    continue;
                }

                bool isFirst = (beatIndex == 0) || !HasActionInBeat(action.actor, beatIndex - 1);
                bool isLast = (beatIndex == playerPlan.Count - 1) || !HasActionInBeat(action.actor, beatIndex + 1);
                CharacterInteraction prevTarget = null;
                if (!isFirst) prevTarget = GetTargetInBeat(action.actor, beatIndex - 1);

                // Add random slight delay (Presentation Offset) so they don't hit at the exact same frame
                StartCoroutine(ExecuteActionWithOffset(action, isFirst, isLast, prevTarget, Random.Range(0f, 0.2f)));
            }

            // Chờ cho tất cả hành động trong beat này resolved hoặc cancelled
            while (true)
            {
                bool allDone = true;
                foreach (var action in beat.actions)
                {
                    if (!action.isResolved && !action.isCancelled)
                    {
                        allDone = false;
                        break;
                    }
                }
                if (allDone) break;
                yield return null;
            }

            yield return new WaitForSeconds(0.5f); // Beat Barrier
        }

        playerPlan.Clear();
        if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateActionBar(playerPlan);
        isExecuting = false;
        
        // Turn over
        EnemyTurn();
    }

    private IEnumerator ExecuteActionWithOffset(PlannedAction action, bool isFirst, bool isLast, CharacterInteraction prevTarget, float offset)
    {
        if (offset > 0) yield return new WaitForSeconds(offset);
        yield return StartCoroutine(ExecuteActionAsync(action, isFirst, isLast, prevTarget));
    }

    [Header("UI")]
    public GameObject actionMenu; // Canvas Menu chứa các nút (Attack, Skill...)
    
    [Header("Soul Economy")]
    public int currentRedSoul = 3;
    public int currentBlueSoul = 0;
    public const int MAX_RED_SOUL = 3;
    public const int MAX_BLUE_SOUL = 6;

    public GameObject slashVFXPrefab;
    public GameObject hitVFXPrefab;

    private int CalculateDamage(CharacterInteraction actor, CharacterInteraction target, SkillData skill, out int limitDamage, out bool isCrit)
    {
        isCrit = false;
        limitDamage = 0;

        // TẦNG 1: SỨC TẤN CÔNG THỰC TẾ (ATK_eff)
        float atkEff = actor.baseATK * (1f + actor.atkBuff);

        // TẦNG 2: SÁT THƯƠNG ĐẦU RA CƠ BẢN (DMG_base)
        float skillMultiplier = (skill != null) ? skill.powerMultiplier : 1.0f;
        float empowerMultiplier = 1.0f; // TODO: "Nhập Hồn" of An
        float dmgBase = atkEff * skillMultiplier * empowerMultiplier;

        // TẦNG 3: KHẤU TRỪ PHÒNG NGỰ CÓ BÀO MÒN (MITIGATED DMG)
        float defEff = Mathf.Max(0, target.baseDEF * (1f - target.defShred));
        float minDmg = Mathf.Max(1, Mathf.Floor(dmgBase * 0.10f));
        float dmgMitigated = Mathf.Max(minDmg, dmgBase - defEff);

        // TẦNG 4: HỆ SỐ KHẮC CHẾ & ĐIỂM YẾU
        float weakMult = 1.0f;
        if (skill != null && skill.isMental && target.element == "Mental") weakMult += 0.4f; // Ví dụ
        if (target.hasWeakpoint) weakMult += 0.4f;

        // TẦNG 5: PHÂN ĐỊNH BẠO KÍCH (CRIT)
        float critRateFinal = actor.baseCritRate + ((skill != null) ? skill.extraCritRate : 0f);
        if (target.isDazed)
        {
            isCrit = true;
        }
        else
        {
            isCrit = Random.value < critRateFinal;
        }

        float critMult = isCrit ? actor.baseCritDMG : 1.0f;
        float dmgRawFinal = Mathf.Floor(dmgMitigated * weakMult * critMult);

        // TẦNG 6: SÁT THƯƠNG PHÁ BỀN / LIMIT BREAK
        int breakBase = 0;
        if (skill != null)
        {
            // Chỉ Kỹ năng (SKILL) mới gây sát thương phá bền theo yêu cầu
            breakBase = actor.baseBreakATK + actor.breakAtkBuff + skill.baseBreakLimit;
        }

        if (breakBase > 0)
        {
            limitDamage = Mathf.FloorToInt(breakBase * empowerMultiplier * (target.hasWeakpoint ? 2f : 1f));
        }

        // TẦNG 7: SHIELD (Đã tính trong Hậu Kỳ hoặc xử lý ở đây nếu target có Shield)
        // Hiện tại target là Enemy, chưa có logic Shield. 
        if (target.currentShield > 0)
        {
            if (target.currentShield >= dmgRawFinal)
            {
                target.currentShield -= Mathf.FloorToInt(dmgRawFinal);
                dmgRawFinal = 0;
            }
            else
            {
                dmgRawFinal -= target.currentShield;
                target.currentShield = 0;
            }
        }

        // Debuffs from skill
        if (skill != null && skill.category == SkillCategory.DEBUFF)
        {
            if (skill.defShred > 0) target.defShred = skill.defShred;
            // Bột lân tinh không gây damage, trả về damage đã tính (28 DMG)
        }

        return Mathf.FloorToInt(dmgRawFinal);
    }

    IEnumerator ExecuteAttack(CharacterInteraction actor, CharacterInteraction target, ActionType actionType = ActionType.ATTACK, SkillData skill = null, bool isFirst = true, bool isLast = true, CharacterInteraction prevTarget = null)
    {
        // Khóa vật lý ngay trước khi di chuyển để tuyệt đối không bị đẩy văng
        Rigidbody actorRb = actor.GetComponentInChildren<Rigidbody>();
        Rigidbody targetRb = target.GetComponentInChildren<Rigidbody>();
        if (actorRb != null) actorRb.isKinematic = true;
        if (targetRb != null) targetRb.isKinematic = true;
        
        // Tắt vòng sáng của mục tiêu
        if (target != null) target.Deselect(false); 
        if (actor != null) actor.Deselect(false); 
        BattleUIManager.Instance.ShowMessage("Phe Ta đang tấn công " + target.characterName);

        Vector3 stancePos = actor.originalPosition;
        float direction = (target.transform.position.x > stancePos.x) ? 1f : -1f;
        Vector3 stepPos = target.transform.position - new Vector3(direction * 1.5f, 0, 0);
        stepPos.y = stancePos.y; 

        // Bỏ Camera zoom trong ExecuteAttack để tránh conflict khi chạy song song nhiều action trong 1 beat

        // NẾU LÀ ĐÒN ĐẦU TIÊN HOẶC ĐỔI MỤC TIÊU THÌ MỚI DI CHUYỂN
        if (isFirst || prevTarget != target)
        {
            Vector3 startPos = actor.transform.position;
            float moveTime = (isFirst) ? 0.1f : 0.15f; 
            float elapsed = 0;
            actor.PlayAnimation("jump");
            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                Vector3 currentPos = Vector3.Lerp(startPos, stepPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
                actor.transform.position = currentPos;
                elapsed += Time.deltaTime;
                yield return null;
            }
            actor.transform.position = stepPos;
        }

        actor.PlayAnimation("attack");

        // Delay chờ tấn công
        yield return new WaitForSeconds(0.4f);

        // 3. Tung đòn! (Tạo Slash VFX)
        if (actionType == ActionType.SKILL)
        {
            PlaySFX("sfx_skill", 0.8f);
        }
        else
        {
            PlaySFX("sfx_slash", 1f); // Tăng âm lượng slash
        }
        
        if (slashVFXPrefab != null)
        {
            // Spawn slash ngay trên người mục tiêu (tại vị trí tiếp xúc)
            GameObject slash = Instantiate(slashVFXPrefab, target.transform.position + new Vector3(direction * 0.2f, 1.5f, -1f), Quaternion.identity);
            slash.transform.localScale = new Vector3(-Mathf.Abs(slash.transform.localScale.x), slash.transform.localScale.y, slash.transform.localScale.z); // Flip cho Ally nhưng giữ nguyên độ to
        }
        
        // Chờ Slash chém trúng (khoảng 0.1s - 0.2s)
        yield return new WaitForSeconds(0.15f);

        // Báo cho đối phương giật lùi và nháy sáng
        target.TakeHit(stepPos);

        // Soul cost đã được trừ tại AddActionToPlan khi người chơi xếp lệnh
        // Không trừ lại ở đây để tránh double deduction

        int damage = 0;
        int limitDamage = 0;
        bool isCrit = false;
        int generatedSouls = 0;

        // --- 7-LAYER DAMAGE PIPELINE ---
        if (actionType == ActionType.ATTACK)
        {
            damage = CalculateDamage(actor, target, null, out limitDamage, out isCrit);
        }
        else if (actionType == ActionType.SKILL && skill != null)
        {
            if (skill.category == SkillCategory.ATTACK || skill.category == SkillCategory.DEBUFF)
            {
                damage = CalculateDamage(actor, target, skill, out limitDamage, out isCrit);
                // Check weakpoint logic internally implemented in CalculateDamage, but let's just use target.hasWeakpoint here
            }
            else if (skill.category == SkillCategory.SUPPORT)
            {
                // Support skill logic handled elsewhere
            }
        }

        if (CameraShake.Instance != null && (actionType == ActionType.SKILL || isCrit))
        {
            CameraShake.Instance.TriggerShake(0.2f, 0.5f);
        }

        if (isCrit) generatedSouls++;
        if (target.hasWeakpoint) generatedSouls++;

        // --- Hậu Kỳ (Lifesteal & Cost) ---
        if (actionType == ActionType.SKILL && skill != null)
        {
            if (skill.selfDamage > 0)
            {
                // Apply self-damage (prioritize shield)
                if (actor.currentShield >= skill.selfDamage)
                {
                    actor.currentShield -= skill.selfDamage;
                }
                else
                {
                    int remainder = skill.selfDamage - actor.currentShield;
                    actor.currentShield = 0;
                    actor.currentHP -= remainder;
                }
            }
            
            if (skill.skillName == "Huyết Đoạn Kích")
            {
                int heal = Mathf.FloorToInt(damage * 0.2f);
                actor.currentHP = Mathf.Min(actor.maxHP, actor.currentHP + heal);
                actor.UpdateMiniHP();
            }
        }

        // Spawn Floating Text
        GameObject dmgTextObj = new GameObject("DamageText");
        dmgTextObj.transform.position = target.transform.position + new Vector3(0, 1.5f, -0.5f);
        DamageText dmgText = dmgTextObj.AddComponent<DamageText>();
        dmgText.Setup(damage, isCrit, false, Color.white); 
        
        target.currentHP -= damage;
        
        // Trừ Limit
        if (!target.isDead && limitDamage > 0)
        {
            target.currentLimit -= limitDamage;
            if (target.currentLimit <= 0)
            {
                target.currentLimit = 0;
                target.isDazed = true;
                generatedSouls++;
                BattleUIManager.Instance.ShowMessage(target.characterName + " bị ĐÁNH CHOÁNG (DAZE)!");
            }
            target.UpdateMiniLimit();
        }

        if (generatedSouls > 0)
        {
            AddBlueSoul(generatedSouls, actor);
        }

        if (target.currentHP <= 0) 
        {
            target.currentHP = 0;
            target.Die();
            enemies.Remove(target);
            BattleUIManager.Instance.ShowMessage(target.characterName + " đã bị tiêu diệt!");
        }
        
        target.UpdateMiniHP();
        BattleUIManager.Instance.UpdateHP(target);

        // Chờ mục tiêu giật lùi và nảy về (TakeHit tốn tầm 0.5s)
        if (isLast)
        {
            yield return new WaitForSeconds(0.6f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f); // Giảm mạnh thời gian chờ nếu đang múa combo
        }

        // NẾU LÀ ĐÒN CUỐI THÌ MỚI LÙI VỀ
        if (isLast)
        {
            float elapsed = 0;
            float moveTime = 0.25f;
            Vector3 startPos = actor.transform.position;
            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                Vector3 currentPos = Vector3.Lerp(startPos, stancePos, t);
                // Parabola nhảy lên
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
                actor.transform.position = currentPos;
                elapsed += Time.deltaTime;
                yield return null;
            }
            actor.transform.position = stancePos;
        }
        
        // Trở về tư thế stance (chờ đòn tiếp theo hoặc đã lùi về)
        actor.PlayAnimation("idle");

        // Triệt tiêu gia tốc vật lý
        if (actorRb != null && !actorRb.isKinematic)
        {
            actorRb.linearVelocity = Vector3.zero;
            actorRb.angularVelocity = Vector3.zero;
        }

        // Kiểm tra Win/Lose
        if (enemies.Count == 0)
        {
            BattleUIManager.Instance.ShowMessage("VICTORY!");
            yield break;
        }
        if (allies.Count == 0)
        {
            BattleUIManager.Instance.ShowMessage("DEFEAT!");
            yield break;
        }

        // Kết thúc lượt - đã xóa delay thừa (ExecutePlanRoutine xử lý Beat Barrier riêng)
    }

    void EnemyTurn()
    {
        state = BattleState.ENEMY_TURN;
        BattleUIManager.Instance.ShowMessage("Lượt của Kẻ Địch");
        GenerateEnemyPlan();
        StartCoroutine(EnemyAction());
    }

    IEnumerator ExecuteSupport(CharacterInteraction actor, CharacterInteraction target, SkillData skill, bool isFirst = true, bool isLast = true, CharacterInteraction prevTarget = null)
    {
        Rigidbody actorRb = actor.GetComponentInChildren<Rigidbody>();
        if (actorRb != null) actorRb.isKinematic = true;
        
        target.Deselect(false);
        actor.Deselect(false);
        BattleUIManager.Instance.ShowMessage("Phe Ta dùng Kỹ năng Hỗ trợ lên " + target.characterName);

        Vector3 originalPos = actor.transform.position;
        Vector3 stepPos = originalPos + new Vector3(0, 0.2f, 0); // Nhích nhẹ lên trên

        float moveTime = 0.2f;
        float elapsed = 0;
        while (elapsed < moveTime)
        {
            actor.transform.position = Vector3.Lerp(originalPos, stepPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        actor.transform.position = stepPos;

        // 2. Wind-up
        yield return new WaitForSeconds(0.5f);

        // 3. Thi triển hiệu ứng
        if (skill != null)
        {
            if (skill.shieldAmount > 0)
            {
                target.currentShield += skill.shieldAmount;
                BattleUIManager.Instance.ShowMessage(target.characterName + " nhận được " + skill.shieldAmount + " Hộ Giáp!");
            }
            if (skill.healAmount > 0)
            {
                target.currentHP = Mathf.Min(target.maxHP, target.currentHP + skill.healAmount);
                BattleUIManager.Instance.ShowMessage(target.characterName + " hồi " + skill.healAmount + " Sinh lực!");
                
                // Spawn Floating Text (Màu Xanh lá cho Hồi máu)
                GameObject dmgTextObj = new GameObject("HealText");
                dmgTextObj.transform.position = target.transform.position + new Vector3(0, 1.5f, -0.5f);
                DamageText dmgText = dmgTextObj.AddComponent<DamageText>();
                dmgText.Setup(skill.healAmount, false, false, Color.green); 
            }
            if (skill.atkBuff > 0)
            {
                target.atkBuff += skill.atkBuff;
                BattleUIManager.Instance.ShowMessage(target.characterName + " được cường hóa Sức Tấn Công!");
            }
        }
        
        target.UpdateMiniHP();
        BattleUIManager.Instance.UpdateHP(target);

        yield return new WaitForSeconds(0.5f);

        // 4. Lùi về vị trí cũ
        elapsed = 0;
        actor.PlayAnimation("jump");
        while (elapsed < moveTime)
        {
            float t = elapsed / moveTime;
            Vector3 currentPos = Vector3.Lerp(stepPos, originalPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
            actor.transform.position = currentPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        actor.transform.position = originalPos;
        if (actorRb != null) actorRb.isKinematic = false;
    }

    IEnumerator ExecuteItem(CharacterInteraction actor, CharacterInteraction target, ItemData item, bool isFirst = true, bool isLast = true, CharacterInteraction prevTarget = null)
    {
        BattleUIManager.Instance.ShowActionMenu(false);
        BattleUIManager.Instance.ShowMessage(actor.characterName + " dùng " + item.itemName + " lên " + target.characterName);
        
        // Trừ số lượng
        if (inventory.ContainsKey(item))
        {
            inventory[item]--;
            if (inventory[item] <= 0)
            {
                inventory.Remove(item);
            }
        }

        // Tạm mượn animation
        Vector3 originalPos = actor.transform.position;
        Vector3 stepPos = originalPos + new Vector3(0, 0.2f, 0);
        float elapsed = 0;
        while(elapsed < 0.2f)
        {
            actor.transform.position = Vector3.Lerp(originalPos, stepPos, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        if (item.itemType == ItemType.HEAL)
        {
            target.currentHP = Mathf.Min(target.maxHP, target.currentHP + item.healAmount);
            
            GameObject dmgTextObj = new GameObject("HealText");
            dmgTextObj.transform.position = target.transform.position + new Vector3(0, 1.5f, -0.5f);
            DamageText dmgText = dmgTextObj.AddComponent<DamageText>();
            dmgText.Setup(item.healAmount, false, false, Color.green); 
            
            BattleUIManager.Instance.UpdateHP(target);
        }
        else if (item.itemType == ItemType.BUFF_STATS)
        {
            if (item.atkBuff > 0)
            {
                target.atkBuff += item.atkBuff;
                BattleUIManager.Instance.ShowMessage(target.characterName + " được cường hóa Sức Tấn Công!");
                
                GameObject dmgTextObj = new GameObject("BuffText");
                dmgTextObj.transform.position = target.transform.position + new Vector3(0, 1.5f, -0.5f);
                DamageText dmgText = dmgTextObj.AddComponent<DamageText>();
                TextMesh tm = dmgTextObj.AddComponent<TextMesh>();
                tm.text = "ATK UP!";
                tm.color = new Color(1f, 0.5f, 0f); // Màu cam
                tm.characterSize = 0.5f;
                tm.anchor = TextAnchor.MiddleCenter;
                
                Destroy(dmgText); 
                MonoBehaviour.Destroy(dmgTextObj, 1.5f);
                StartCoroutine(FloatTextUp(dmgTextObj.transform));
            }
        }

        yield return new WaitForSeconds(0.5f);
        
        elapsed = 0;
        while(elapsed < 0.2f)
        {
            actor.transform.position = Vector3.Lerp(stepPos, originalPos, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        actor.transform.position = originalPos;
    }

    private void GenerateEnemyPlan()
    {
        enemyPlan.Clear();
        
        List<CharacterInteraction> aliveEnemies = new List<CharacterInteraction>();
        foreach (var e in enemies)
        {
            if (!e.isDead) aliveEnemies.Add(e);
        }

        foreach (var enemy in aliveEnemies)
        {
            bool isBoss = enemy.characterName.ToLower().Contains("boss");
            int beatCount = isBoss ? 3 : 1;
            
            for (int b = 0; b < beatCount; b++)
            {
                while (enemyPlan.Count <= b)
                {
                    enemyPlan.Add(new BeatPlan());
                }
                
                PlannedAction pAction = new PlannedAction();
                pAction.actor = enemy;

                // Boss Beat cuối (b==2): 40% dùng AOE
                if (isBoss && b == 2 && Random.value < 0.4f)
                {
                    pAction.type = ActionType.SKILL; // Dùng SKILL để đánh dấu AOE
                    // target = null, sẽ hit tất cả allies
                }
                else
                {
                    // Chọn mục tiêu ngẫu nhiên
                    if (allies.Count > 0)
                        pAction.target = allies[Random.Range(0, allies.Count)];
                    pAction.type = ActionType.ATTACK;
                }
                
                enemyPlan[b].AddAction(pAction);
            }
        }
    }

    IEnumerator EnemyAction()
    {
        yield return new WaitForSeconds(0.5f);

        if (enemyPlan.Count == 0)
        {
            if (enemies.Count > 0 && allies.Count > 0)
            {
                BattleUIManager.Instance.ShowMessage("Địch bỏ qua lượt!");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            Dictionary<CharacterInteraction, CharacterInteraction> prevTargetMap = new Dictionary<CharacterInteraction, CharacterInteraction>();

            for (int beatIndex = 0; beatIndex < enemyPlan.Count; beatIndex++)
            {
                if (allies.Count == 0) break;

                var beat = enemyPlan[beatIndex];
                
                foreach (var action in beat.actions)
                {
                    if (action.actor.isDead) 
                    {
                        action.isCancelled = true;
                        continue;
                    }
                    bool isFirst = true;
                    for (int i = 0; i < beatIndex; i++) {
                        if (enemyPlan[i].actions.Exists(a => a.actor == action.actor)) isFirst = false;
                    }
                    bool isLast = true;
                    for (int i = beatIndex + 1; i < enemyPlan.Count; i++) {
                        if (enemyPlan[i].actions.Exists(a => a.actor == action.actor)) isLast = false;
                    }

                    CharacterInteraction prevTarget = prevTargetMap.ContainsKey(action.actor) ? prevTargetMap[action.actor] : null;
                    // Note: Nếu action này là AOE (SKILL), target = null, gán luôn target = null.
                    prevTargetMap[action.actor] = action.target;

                    float offset = Random.Range(0f, 0.2f);
                    StartCoroutine(ExecuteEnemyActionWithOffset(action, offset, isFirst, isLast, prevTarget));
                }

                // Chờ cho tất cả hành động trong beat này resolved hoặc cancelled
                while (true)
                {
                    bool allDone = true;
                    foreach (var action in beat.actions)
                    {
                        if (!action.isResolved && !action.isCancelled)
                        {
                            allDone = false;
                            break;
                        }
                    }
                    if (allDone) break;
                    yield return null;
                }

                yield return new WaitForSeconds(0.5f); // Beat Barrier
            }
        }

        // Kiểm tra Win/Lose
        if (allies.Count == 0)
        {
            BattleUIManager.Instance.ShowMessage("DEFEAT!");
            yield break;
        }
        if (enemies.Count == 0)
        {
            BattleUIManager.Instance.ShowMessage("VICTORY!");
            yield break;
        }

        yield return new WaitForSeconds(0.5f);
        StartPlayerTurn(); // Reset Red Soul về max và clear plan trước khi bắt đầu lượt mới
    }

    private IEnumerator ExecuteEnemyActionWithOffset(PlannedAction action, float offset, bool isFirst, bool isLast, CharacterInteraction prevTarget = null)
    {
        yield return new WaitForSeconds(offset);

        if (action.actor.isDead)
        {
            action.isCancelled = true;
            yield break;
        }

        if (action.actor.isDazed)
        {
            BattleUIManager.Instance.ShowMessage(action.actor.characterName + " đang bị choáng! Bỏ qua lượt.");
            action.actor.isDazed = false;
            action.actor.currentLimit = action.actor.maxLimit;
            action.actor.UpdateMiniLimit();
            BattleUIManager.Instance.UpdateHP(action.actor);
            action.isCancelled = true;
            yield break;
        }

        // AOE SKILL (Boss)
        if (action.type == ActionType.SKILL)
        {
            yield return StartCoroutine(ExecuteBossAOE(action.actor));
            action.isResolved = true;
            yield break;
        }

        // Normal ATTACK — retarget nếu mục tiêu đã chết
        if (action.target == null || action.target.isDead)
        {
            List<CharacterInteraction> aliveAllies = new List<CharacterInteraction>();
            foreach (var a in allies)
            {
                if (!a.isDead) aliveAllies.Add(a);
            }

            if (aliveAllies.Count > 0)
                action.target = aliveAllies[Random.Range(0, aliveAllies.Count)];
            else
            {
                action.isCancelled = true;
                yield break;
            }
        }

        BattleUIManager.Instance.ShowMessage(action.actor.characterName + " tấn công " + action.target.characterName);
        yield return StartCoroutine(ExecuteEnemyAttack(action.actor, action.target, isFirst, isLast, prevTarget));
        action.isResolved = true;
    }

    IEnumerator ExecuteEnemyAttack(CharacterInteraction enemyActor, CharacterInteraction target, bool isFirst = true, bool isLast = true, CharacterInteraction prevTarget = null)
    {
        // Xoay mặt quái về phía mục tiêu
        enemyActor.transform.LookAt(new Vector3(target.transform.position.x, enemyActor.transform.position.y, target.transform.position.z));

        Rigidbody actorRb = enemyActor.GetComponentInChildren<Rigidbody>();
        Rigidbody targetRb = target.GetComponentInChildren<Rigidbody>();
        if (actorRb != null) actorRb.isKinematic = true;
        if (targetRb != null) targetRb.isKinematic = true;

        Vector3 originalPos = enemyActor.isPosInit
            ? enemyActor.originalPosition
            : enemyActor.transform.position;

        float direction = (target.transform.position.x > originalPos.x) ? 1f : -1f;
        Vector3 stepPos = target.transform.position - new Vector3(direction * 1.5f, 0, 0);
        stepPos.y = originalPos.y;

        if (isFirst || prevTarget != target)
        {
            float moveTime = 0.15f;
            float elapsed = 0;
            Vector3 startPos = enemyActor.transform.position;
            enemyActor.PlayAnimation("jump");
            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                Vector3 currentPos = Vector3.Lerp(startPos, stepPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
                enemyActor.transform.position = currentPos;
                elapsed += Time.deltaTime;
                yield return null;
            }
            enemyActor.transform.position = stepPos;
        }

        enemyActor.PlayAnimation("attack");

        // Wind-up
        yield return new WaitForSeconds(0.4f);

        // Tung đòn & VFX (Tạo Slash)
        PlaySFX("sfx_slash", 1f); // Tăng âm lượng slash
        if (slashVFXPrefab != null)
            Instantiate(slashVFXPrefab, target.transform.position + new Vector3(direction * 0.2f, 1.5f, -1f), Quaternion.identity);
        
        // Chờ Slash chém trúng (khoảng 0.1s - 0.2s)
        yield return new WaitForSeconds(0.15f);

        // Trừ máu
        int damage = Random.Range(30, 45);
        bool isCrit = damage > 40;

        // Báo cho đối phương giật lùi và nháy sáng
        target.TakeHit(stepPos);
        
        if (CameraShake.Instance != null && isCrit)
        {
            CameraShake.Instance.TriggerShake(0.2f, 0.5f);
        }
        
        // Spawn Floating Text
        GameObject dmgTextObj = new GameObject("DamageText");
        dmgTextObj.transform.position = target.transform.position + new Vector3(0, 1.5f, -0.5f);
        DamageText dmgText = dmgTextObj.AddComponent<DamageText>();
        dmgText.Setup(damage, damage > 40, false, Color.red);
        
        target.currentHP -= damage;
        if (target.currentHP <= 0) 
        {
            target.currentHP = 0;
            target.Die();
            allies.Remove(target);
            BattleUIManager.Instance.ShowMessage(target.characterName + " đã gục ngã!");
        }
        
        target.UpdateMiniHP();
        BattleUIManager.Instance.UpdateHP(target);

        // Chờ mục tiêu giật lùi và nảy về (TakeHit tốn tầm 0.5s)
        yield return new WaitForSeconds(0.6f);

        // Nếu là chiêu cuối cùng trong lượt thì nhảy lùi về
        if (isLast)
        {
            float elapsed = 0;
            float moveTime = 0.25f;
            Vector3 startPos = enemyActor.transform.position;
            enemyActor.PlayAnimation("jump");
            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                Vector3 currentPos = Vector3.Lerp(startPos, originalPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
                enemyActor.transform.position = currentPos;
                elapsed += Time.deltaTime;
                yield return null;
            }
            enemyActor.transform.position = originalPos;
        }
        else 
        {
            enemyActor.PlayAnimation("idle");
        }

        // Triệt tiêu gia tốc vật lý
        Rigidbody rb = enemyActor.GetComponentInChildren<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator ExecuteBossAOE(CharacterInteraction boss)
    {
        BattleUIManager.Instance.ShowMessage(boss.characterName + " tung chiêu AoE!");

        // Boss nhảy về vị trí charge (ở nhà)
        Vector3 homePos = boss.isPosInit ? boss.originalPosition : boss.transform.position;
        Vector3 chargePos = homePos + new Vector3(0, 0.3f, 0);

        Rigidbody bossRb = boss.GetComponentInChildren<Rigidbody>();
        if (bossRb != null) bossRb.isKinematic = true;

        float elapsed = 0f;
        Vector3 startPos = boss.transform.position;
        while (elapsed < 0.3f)
        {
            boss.transform.position = Vector3.Lerp(startPos, chargePos, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        boss.transform.position = chargePos;

        // Wind-up
        yield return new WaitForSeconds(0.4f);

        PlaySFX("sfx_slash", 1.2f);

        // Tung AOE — hit tất cả ally đang sống
        int aoeBaseDmg = Random.Range(20, 35); // Placeholder: damage thấp hơn single target
        List<CharacterInteraction> aliveAtHit = new List<CharacterInteraction>(allies);
        foreach (var ally in aliveAtHit)
        {
            if (ally.isDead) continue;

            int dmg = aoeBaseDmg + Random.Range(-5, 5);
            ally.TakeHit(boss.transform.position);

            if (CameraShake.Instance != null)
                CameraShake.Instance.TriggerShake(0.15f, 0.3f);

            // Floating text
            GameObject dmgTextObj = new GameObject("DmgAOE");
            dmgTextObj.transform.position = ally.transform.position + new Vector3(0, 1.5f, -0.5f);
            DamageText dmgTxt = dmgTextObj.AddComponent<DamageText>();
            dmgTxt.Setup(dmg, false, false, new Color(1f, 0.4f, 0f)); // Màu cam AOE

            // Slash VFX
            if (slashVFXPrefab != null)
                Instantiate(slashVFXPrefab, ally.transform.position + new Vector3(0, 1.5f, -1f), Quaternion.identity);

            ally.currentHP -= dmg;
            if (ally.currentHP <= 0)
            {
                ally.currentHP = 0;
                ally.Die();
                BattleUIManager.Instance.ShowMessage(ally.characterName + " đã gục ngã!");
            }
            ally.UpdateMiniHP();
            BattleUIManager.Instance.UpdateHP(ally);

            yield return new WaitForSeconds(0.1f); // nhỏ delay giữa từng ally bị hit
        }

        // Xóa khỏi danh sách những ai đã chết
        allies.RemoveAll(a => a.isDead);

        yield return new WaitForSeconds(0.5f);

        // Boss quay về vị trí gốc
        elapsed = 0f;
        while (elapsed < 0.2f)
        {
            boss.transform.position = Vector3.Lerp(chargePos, homePos, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        boss.transform.position = homePos;

        if (bossRb != null) bossRb.isKinematic = false;
    }

    private int lastActionFrame = -1;

    public void DeselectAll(bool hideBossHUD = true)
    {
        CharacterInteraction[] chars = FindObjectsOfType<CharacterInteraction>();
        foreach (var c in chars)
        {
            c.Deselect(hideBossHUD);
        }
        
        if (state == BattleState.WAIT_TARGET)
        {
            // Hủy trạng thái chờ chọn mục tiêu, cho phép chọn lại lệnh
            state = BattleState.PLAYER_TURN;
        }

        // Reset camera về trạng thái mặc định khi hủy chọn hoàn toàn
        RotateCameraTo(defaultCamRot);

        currentActor = null;
        currentTarget = null;
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ShowActionMenu(false);
            if (hideBossHUD)
            {
                BattleUIManager.Instance.HideBossHUD();
            }
        }
    }

    private IEnumerator FloatTextUp(Transform t)
    {
        float elapsed = 0;
        Vector3 start = t.position;
        while(elapsed < 1.5f && t != null)
        {
            t.position = start + new Vector3(0, elapsed * 0.5f, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}

