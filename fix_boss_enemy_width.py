import re

ui_path = r"Assets\_Project\Scripts\Combat\BattleUIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_content = f.read()

# 1. Revert beatCounts to {3, 1, 1}
beat_search = """        string[] enemyLabels = new string[] { "Boss", "Enemy 1", "Enemy 2" };
        int[] beatCounts = new int[] { 3, 3, 3 }; 
        int[] limitCounts = new int[] { 5, 2, 2 }; // Boss: 5, Enemy: 2"""
beat_replace = """        string[] enemyLabels = new string[] { "Boss", "Enemy 1", "Enemy 2" };
        int[] beatCounts = new int[] { 3, 1, 1 }; 
        int[] limitCounts = new int[] { 5, 2, 2 }; // Boss: 5, Enemy: 2"""

if beat_search in ui_content:
    ui_content = ui_content.replace(beat_search, beat_replace)
    

# 2. Revert UpdateEnemyActionBar isActive
update_search = """            for (int i = 0; i < kvp.Value.Length; i++)
            {
                Image nodeBg = kvp.Value[i];
                bool isActive = (i < 3); // Boss và Enemy đều có 3 ô
                nodeBg.color = isActive ? new Color(0.15f, 0.15f, 0.25f, 1f) : new Color(0.1f, 0.1f, 0.12f, 0.5f);"""
update_replace = """            for (int i = 0; i < kvp.Value.Length; i++)
            {
                Image nodeBg = kvp.Value[i];
                bool isActive = (kvp.Key == "Boss" && i < 3) || (kvp.Key != "Boss" && i < 1);
                nodeBg.color = isActive ? new Color(0.15f, 0.15f, 0.25f, 1f) : new Color(0.1f, 0.1f, 0.12f, 0.5f);"""

if update_search in ui_content:
    ui_content = ui_content.replace(update_search, update_replace)
    

# 3. Fix infoCol width so HP and Limit are identical width for Boss and Enemy
info_search = """            // Info Column (HP + Limit)
            GameObject infoCol = new GameObject("InfoCol");
            infoCol.transform.SetParent(rowObj.transform, false);
            LayoutElement infoLe = infoCol.AddComponent<LayoutElement>();
            infoLe.minWidth = 60;
            VerticalLayoutGroup infoVLayout = infoCol.AddComponent<VerticalLayoutGroup>();"""
info_replace = """            // Info Column (HP + Limit)
            GameObject infoCol = new GameObject("InfoCol");
            infoCol.transform.SetParent(rowObj.transform, false);
            LayoutElement infoLe = infoCol.AddComponent<LayoutElement>();
            infoLe.minWidth = 140;
            infoLe.preferredWidth = 140;
            infoLe.flexibleWidth = 0; // Fix width completely
            VerticalLayoutGroup infoVLayout = infoCol.AddComponent<VerticalLayoutGroup>();"""

if info_search in ui_content:
    ui_content = ui_content.replace(info_search, info_replace)

with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_content)

print("done")
