import re

ui_path = r"Assets\_Project\Scripts\Combat\BattleUIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_content = f.read()

# 1. Update CreateSkillButton
skill_search = """    private void CreateSkillButton(SkillData skill, Transform parent)
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
        }"""
        
skill_replace = """    private void CreateSkillButton(SkillData skill, Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_" + skill.skillName);
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        Button btn = btnObj.AddComponent<Button>();

        int currentSouls = 0;
        if (BattleManager.Instance != null) currentSouls = BattleManager.Instance.currentRedSoul + BattleManager.Instance.currentBlueSoul;
        bool hasEnoughSoul = currentSouls >= skill.soulCost;

        btnImg.color = Color.white;
        ColorBlock cb = btn.colors;
        if (hasEnoughSoul)
        {
            cb.normalColor = new Color(0.3f, 0.2f, 0.4f, 1f);
            cb.highlightedColor = new Color(0.6f, 0.4f, 0.8f, 1f);
            cb.selectedColor = new Color(0.6f, 0.4f, 0.8f, 1f);
            cb.pressedColor = new Color(0.8f, 0.6f, 1f, 1f);
            btn.onClick.AddListener(() => OnSkillSelected(skill));
        }
        else
        {
            cb.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            cb.disabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            btn.interactable = false;
        }
        btn.colors = cb;"""

if "btnImg.color = Color.white;\n        ColorBlock cb = btn.colors;" not in ui_content:
    if skill_search in ui_content:
        ui_content = ui_content.replace(skill_search, skill_replace)
    else:
        # Regex fallback
        pattern = re.compile(r"private void CreateSkillButton.*?btn\.interactable = false;\n\s*\}", re.DOTALL)
        match = pattern.search(ui_content)
        if match:
            ui_content = ui_content.replace(match.group(0), skill_replace)


# 2. Update CreateItemButton
item_search = """    private void CreateItemButton(ItemData item, int amount, Transform parent)
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
        }"""
        
item_replace = """    private void CreateItemButton(ItemData item, int amount, Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_" + item.itemName);
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        Button btn = btnObj.AddComponent<Button>();

        btnImg.color = Color.white;
        ColorBlock cb = btn.colors;
        if (amount > 0)
        {
            cb.normalColor = new Color(0.2f, 0.5f, 0.3f, 1f);
            cb.highlightedColor = new Color(0.4f, 0.8f, 0.5f, 1f);
            cb.selectedColor = new Color(0.4f, 0.8f, 0.5f, 1f);
            cb.pressedColor = new Color(0.6f, 1.0f, 0.7f, 1f);
            btn.onClick.AddListener(() => OnItemSelected(item));
        }
        else
        {
            cb.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            cb.disabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            btn.interactable = false;
        }
        btn.colors = cb;"""

if "btnImg.color = Color.white;\n        ColorBlock cb = btn.colors;" not in ui_content.split("CreateItemButton")[1][:500]:
    if item_search in ui_content:
        ui_content = ui_content.replace(item_search, item_replace)
    else:
        pattern = re.compile(r"private void CreateItemButton.*?btn\.interactable = false;\n\s*\}", re.DOTALL)
        match = pattern.search(ui_content)
        if match:
            ui_content = ui_content.replace(match.group(0), item_replace)


# 3. Update CreateSkillCancelButton
skill_cancel_search = """    private void CreateSkillCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Cancel");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {"""
        
skill_cancel_replace = """    private void CreateSkillCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Cancel");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        cb.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        cb.selectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        cb.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(() => {"""

if skill_cancel_search in ui_content:
    ui_content = ui_content.replace(skill_cancel_search, skill_cancel_replace)
    

# 4. Update CreateItemCancelButton
item_cancel_search = """    private void CreateItemCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Cancel");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {"""
        
item_cancel_replace = """    private void CreateItemCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Cancel");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        cb.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        cb.selectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        cb.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(() => {"""

if item_cancel_search in ui_content:
    ui_content = ui_content.replace(item_cancel_search, item_cancel_replace)

with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_content)

print("done")
