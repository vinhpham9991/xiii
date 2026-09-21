import re

status_path = r"Assets\_Project\Scripts\Combat\BattleManager.cs" # Wait, I shouldn't replace BattleManager
status_path = r"PROJECT_STATUS.md"
with open(status_path, "r", encoding="utf-8") as f:
    content = f.read()

# Update baseline revision
content = re.sub(r"\*\*Baseline revision:\*\* `[a-f0-9]+` \(`main`\); the current Instant Special Task 3 checkpoint is uncommitted\.", 
                 r"**Baseline revision:** `c85b9d1` (`main`); Instant Special Task 3 and UI updates are committed.", 
                 content)
content = re.sub(r"Git branch `main` is based on `[a-f0-9]+`, synchronized with `origin/main`; the current Task 3 checkpoint remains uncommitted\.",
                 r"Git branch `main` is based on `c85b9d1`, synchronized with `origin/main`.",
                 content)
content = re.sub(r"based on `499f023` on 2026-09-21", r"based on `c85b9d1` on 2026-09-21", content)

# Update keyboard dispatch note
content = re.sub(r"Keyboard dispatch lặp hiện là expected gap cần sửa, không phải acceptance pass\.", 
                 r"Keyboard dispatch lặp đã được giải quyết một phần. Backspace và Chuột Phải đều được tích hợp mượt mà ở các level menu.", 
                 content)

with open(status_path, "w", encoding="utf-8") as f:
    f.write(content)

print("done")
