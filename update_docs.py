import re

# 1. Update BattleUIManager.cs
ui_path = r"Assets\_Project\Scripts\Combat\BattleUIManager.cs"
with open(ui_path, "r", encoding="utf-8") as f:
    ui_content = f.read()

search_text = '"Backspace \\t Back / Cancel\\n" +'
replace_text = '"Backspace / RMB \\t Back / Cancel\\n" +'
if search_text in ui_content:
    ui_content = ui_content.replace(search_text, replace_text)
else:
    print("Warning: could not find Backspace string in BattleUIManager")

with open(ui_path, "w", encoding="utf-8") as f:
    f.write(ui_content)

# 2. Update Guideline.md
guide_path = r"Guideline.md"
with open(guide_path, "r", encoding="utf-8") as f:
    guide_content = f.read()

guide_replace_1 = """- **Backspace**:
  - Lùi lại (Back) khỏi Sub-Menu (Skill, Item).
  - Khi đang chọn Mục tiêu, nhấn Backspace để huỷ việc chọn và quay lại Menu nhân vật."""

guide_replace_2 = """- **Backspace / Chuột phải (Right Click)**:
  - Lùi lại (Back) khỏi Sub-Menu (Skill, Item).
  - Khi đang chọn Mục tiêu, nhấn Backspace hoặc Chuột phải để huỷ việc chọn và quay lại Menu nhân vật."""

if "- **Backspace**:\n  - Lùi lại (Back)" in guide_content:
    guide_content = guide_content.replace("- **Backspace**:\n  - Lùi lại (Back) khỏi Sub-Menu.\n  - Khi đang chọn Mục tiêu, nhấn Backspace để huỷ việc chọn và quay lại Menu nhân vật.", guide_replace_2)
    # Just in case it's slightly different
    guide_content = re.sub(r"- \*\*Backspace\*\*:.*?Menu nhân vật\.", guide_replace_2, guide_content, flags=re.DOTALL)
else:
    print("Replacing fallback for Backspace")
    guide_content = re.sub(r"- \*\*Backspace\*\*:.*?(?=- \*\*Delete\*\*|\Z)", guide_replace_2 + "\n", guide_content, flags=re.DOTALL)

# Add Right Click to mouse section
mouse_replace = """### Chuột (Mouse)
- Có thể rê chuột để Highlight (phát sáng) mục tiêu.
- Click chuột trái để chọn trực tiếp nhân vật hoặc kẻ địch.
- Click chuột phải (Right Click) có chức năng tương đương Backspace (Lùi lại/Hủy).
- Hỗ trợ click vào các Nút trên UI (Mặc dù game thiết kế tối ưu cho bàn phím)."""

guide_content = re.sub(r"### Chuột \(Mouse\).*?(?=\n---|\Z)", mouse_replace, guide_content, flags=re.DOTALL)


with open(guide_path, "w", encoding="utf-8") as f:
    f.write(guide_content)

print("done")
