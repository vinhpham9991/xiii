import re

ui_path = r"Assets\_Project\Scripts\Combat\BattleUIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_content = f.read()

# Fix ShowSkillMenu
skill_search = """                // Clear old skills
                foreach (Transform child in skillMenu.transform)
                {
                    Destroy(child.gameObject);
                }"""

skill_replace = """                // Clear old skills
                for (int i = skillMenu.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = skillMenu.transform.GetChild(i);
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }"""

if skill_search in ui_content:
    ui_content = ui_content.replace(skill_search, skill_replace)
else:
    print("WARNING: skill_search not found")

# Fix ShowItemMenu
item_search = """                // Clear old items
                foreach (Transform child in itemMenu.transform)
                {
                    Destroy(child.gameObject);
                }"""

item_replace = """                // Clear old items
                for (int i = itemMenu.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = itemMenu.transform.GetChild(i);
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }"""

if item_search in ui_content:
    ui_content = ui_content.replace(item_search, item_replace)
else:
    print("WARNING: item_search not found")


with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_content)

print("done")
