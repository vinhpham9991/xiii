import re

log_path = r"PROJECT_LOG.md"
with open(log_path, "r", encoding="utf-8") as f:
    content = f.read()

# Append section 7 about Boss Encounter and Status Sync
new_section = """
### 7. Cập nhật Quy tắc Chạm trán Boss (Boss Encounter Rules) & Đồng bộ Trạng thái
- Triển khai logic Domain Tests (`BossEncounterRules.cs`) nhằm xác minh và siết chặt các luật lệ khi tương tác với Boss.
- Kiểm tra chéo (Cross-check) toàn bộ hệ thống tài liệu: Cập nhật thành công `PROJECT_STATUS.md` để ghi nhận mã commit baseline mới nhất (`c85b9d1` -> `88845fa`), và chính thức xóa nhãn "uncommitted" của các tính năng Special Command.

---"""

content = content.replace("---\n**Trạng thái hiện tại:**", new_section + "\n**Trạng thái hiện tại:**")

with open(log_path, "w", encoding="utf-8") as f:
    f.write(content)

print("done")
