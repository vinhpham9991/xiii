import re

ui_path = r"Assets\_Project\Scripts\Combat\BattleUIManager.cs"
bm_path = r"Assets\_Project\Scripts\Combat\BattleManager.cs"

with open(ui_path, "r", encoding="utf-8") as f:
    ui_content = f.read()

# 1. Remove CreateSkillCancelButton calls
ui_content = re.sub(r"[ \t]*CreateSkillCancelButton\(skillMenu\.transform\);\n", "", ui_content)
ui_content = re.sub(r"[ \t]*CreateItemCancelButton\(itemMenu\.transform\);\n", "", ui_content)

# 2. Remove the actual CreateSkillCancelButton method
pattern_skill = re.compile(r"\s*private void CreateSkillCancelButton\(Transform parent\)\s*\{.*?txtRect\.offsetMax = Vector2\.zero;\s*\}", re.DOTALL)
ui_content = pattern_skill.sub("", ui_content)

pattern_item = re.compile(r"\s*private void CreateItemCancelButton\(Transform parent\)\s*\{.*?txtRect\.offsetMax = Vector2\.zero;\s*\}", re.DOTALL)
ui_content = pattern_item.sub("", ui_content)

# 3. Add TryGoBack and remove Backspace from Update
update_backspace_pattern = re.compile(r"\s*// Xử lý phím Backspace để đóng menu Skill/Item\s*if\s*\(Input\.GetKeyDown\(KeyCode\.Backspace\)\)\s*\{.*?\n        \}\n", re.DOTALL)

try_go_back_method = """
    public bool TryGoBack()
    {
        if (skillMenu != null && skillMenu.activeSelf)
        {
            ShowSkillMenu(false);
            if (actionButtons.ContainsKey(ActionType.ATTACK)) StartCoroutine(SelectLater(actionButtons[ActionType.ATTACK].gameObject));
            return true;
        }
        else if (itemMenu != null && itemMenu.activeSelf)
        {
            ShowItemMenu(false);
            if (actionButtons.ContainsKey(ActionType.ATTACK)) StartCoroutine(SelectLater(actionButtons[ActionType.ATTACK].gameObject));
            return true;
        }
        return false;
    }
"""

if update_backspace_pattern.search(ui_content):
    ui_content = update_backspace_pattern.sub("", ui_content)

if "public bool TryGoBack()" not in ui_content:
    # Add TryGoBack near IsSubMenuOpen
    ui_content = ui_content.replace("public bool IsSubMenuOpen()", try_go_back_method + "\n    public bool IsSubMenuOpen()")

with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_content)

# 4. Modify BattleManager.cs
with open(bm_path, "r", encoding="utf-8") as f:
    bm_content = f.read()

bm_replace_1 = """                else if (Input.GetKeyDown(KeyCode.Backspace) || (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame))
                {
                    if (!BattleUIManager.Instance.TryGoBack())
                    {
                        CancelActorSelection();
                    }
                }"""

bm_pattern_1 = re.compile(r"\s*else if \(Input\.GetKeyDown\(KeyCode\.Backspace\)\)\s*\{\s*if \(!BattleUIManager\.Instance\.IsSubMenuOpen\(\)\)\s*\{\s*CancelActorSelection\(\);\s*\}\s*\}", re.DOTALL)
bm_content = bm_pattern_1.sub("\n" + bm_replace_1, bm_content)


bm_replace_2 = """            else if (Input.GetKeyDown(KeyCode.Backspace) || (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)) // Cancel is only Backspace or Right Click (Esc is reserved)
            {
                // Hủy lệnh quay lại chọn lệnh
                CancelActorSelection();
            }"""

bm_pattern_2 = re.compile(r"\s*else if \(Input\.GetKeyDown\(KeyCode\.Backspace\)\) // Cancel is only Backspace \(Esc is reserved\)\s*\{\s*// Hủy lệnh quay lại chọn lệnh\s*CancelActorSelection\(\);\s*\}", re.DOTALL)
bm_content = bm_pattern_2.sub("\n" + bm_replace_2, bm_content)

# Also update the tooltip to mention Right Click
tooltip_search = 'BattleUIManager.Instance.ShowMessage("Mục tiêu: " + target.characterName + ". [Space] lần nữa để xác nhận | [Backspace] để hủy");'
tooltip_replace = 'BattleUIManager.Instance.ShowMessage("Mục tiêu: " + target.characterName + ". [Space] lần nữa để xác nhận | [Backspace / Right Click] để hủy");'
bm_content = bm_content.replace(tooltip_search, tooltip_replace)

tooltip_search2 = 'BattleUIManager.Instance.ShowMessage("Mục tiêu hỗ trợ: " + target.characterName + ". [Space] lần nữa để xác nhận | [Backspace] để hủy");'
tooltip_replace2 = 'BattleUIManager.Instance.ShowMessage("Mục tiêu hỗ trợ: " + target.characterName + ". [Space] lần nữa để xác nhận | [Backspace / Right Click] để hủy");'
bm_content = bm_content.replace(tooltip_search2, tooltip_replace2)

with open(bm_path, "w", encoding="utf-8") as f:
    f.write(bm_content)

print("done")
