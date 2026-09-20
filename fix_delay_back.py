import re

# 1. Update BattleUIManager
ui_path = r"Assets\_Project\Scripts\Combat\BattleUIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_content = f.read()

# Add IsSubMenuOpen and SelectLater
add_methods = """    public bool IsSubMenuOpen()
    {
        return (skillMenu != null && skillMenu.activeSelf) || (itemMenu != null && itemMenu.activeSelf);
    }

    private System.Collections.IEnumerator SelectLater(GameObject obj)
    {
        yield return null; // Wait 1 frame for layout to build
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(obj);
    }

    public bool IsActionMenuOpen()"""
    
if "public bool IsSubMenuOpen()" not in ui_content:
    ui_content = ui_content.replace("    public bool IsActionMenuOpen()", add_methods)


# Replace SetSelectedGameObject with Coroutine in ShowSkillMenu
skill_search = """                // Auto-select first skill
                if (skillMenu.transform.childCount > 0)
                {
                    EventSystem.current.SetSelectedGameObject(skillMenu.transform.GetChild(0).gameObject);
                }"""
skill_replace = """                // Auto-select first skill
                if (skillMenu.transform.childCount > 0)
                {
                    StartCoroutine(SelectLater(skillMenu.transform.GetChild(0).gameObject));
                }"""
if skill_search in ui_content:
    ui_content = ui_content.replace(skill_search, skill_replace)


# Replace SetSelectedGameObject with Coroutine in ShowItemMenu
item_search = """                // Auto-select first item
                if (itemMenu.transform.childCount > 0)
                {
                    EventSystem.current.SetSelectedGameObject(itemMenu.transform.GetChild(0).gameObject);
                }"""
item_replace = """                // Auto-select first item
                if (itemMenu.transform.childCount > 0)
                {
                    StartCoroutine(SelectLater(itemMenu.transform.GetChild(0).gameObject));
                }"""
if item_search in ui_content:
    ui_content = ui_content.replace(item_search, item_replace)


# Also in the Backspace handler
back_skill_search = """                ShowSkillMenu(false);
                if (actionButtons.ContainsKey(ActionType.ATTACK)) EventSystem.current.SetSelectedGameObject(actionButtons[ActionType.ATTACK].gameObject);"""
back_skill_replace = """                ShowSkillMenu(false);
                if (actionButtons.ContainsKey(ActionType.ATTACK)) StartCoroutine(SelectLater(actionButtons[ActionType.ATTACK].gameObject));"""
ui_content = ui_content.replace(back_skill_search, back_skill_replace)

back_item_search = """                ShowItemMenu(false);
                if (actionButtons.ContainsKey(ActionType.ATTACK)) EventSystem.current.SetSelectedGameObject(actionButtons[ActionType.ATTACK].gameObject);"""
back_item_replace = """                ShowItemMenu(false);
                if (actionButtons.ContainsKey(ActionType.ATTACK)) StartCoroutine(SelectLater(actionButtons[ActionType.ATTACK].gameObject));"""
ui_content = ui_content.replace(back_item_search, back_item_replace)


with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_content)


# 2. Update BattleManager
bm_path = r"Assets\_Project\Scripts\Combat\BattleManager.cs"
with open(bm_path, "r", encoding="utf-8") as f:
    bm_content = f.read()

bm_search = """                else if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    CancelActorSelection();
                }"""
bm_replace = """                else if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    if (!BattleUIManager.Instance.IsSubMenuOpen())
                    {
                        CancelActorSelection();
                    }
                }"""

# Only replace the one inside currentActor != null block!
# Let's find the specific one inside currentActor != null.
# It is around line 240.
if bm_search in bm_content:
    # In case it appears multiple times, we just want to ensure we don't break anything. 
    # Actually it might appear exactly once inside currentActor != null.
    bm_content = bm_content.replace(bm_search, bm_replace, 1)

with open(bm_path, "w", encoding="utf-8") as f:
    f.write(bm_content)

print("done")
