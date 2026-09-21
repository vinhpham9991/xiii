# BẢN GHI LỊCH SỬ DỰ ÁN (PROJECT LOG)
*Cập nhật lần cuối: 21/09/2026*

Dưới đây là nhật ký phát triển và tiến độ thực hiện của dự án, tóm tắt các cột mốc quan trọng tính đến thời điểm hiện tại:

### 1. Khởi tạo Dự Án
- **Khởi tạo cơ bản**: Thiết lập cấu trúc dự án Unity 6 ban đầu cho nguyên mẫu hệ thống chiến đấu (Combat Prototype).
- **Base UI & Logic**: Hình thành `BattleManager` và `BattleUIManager` như hai lõi xử lý chính để kiểm soát luồng giao tranh (Player Turn, Execution, Enemy Turn).

### 2. Tinh chỉnh Giao Diện (UI Refactor) & Tương tác Cơ Bản
- Cải tiến tính năng chọn mục tiêu (Targeting).
- Cho phép hoán đổi nhanh nhân vật thông qua phím tắt (Hot-swap bằng phím Q/E hoặc PageUp/PageDown).
- Hoàn thiện logic Hủy Hành Động (Action Delete) bằng phím Delete: Trả lại toàn bộ tài nguyên (Soul, Limit) ngay lập tức khi người chơi hủy một thao tác.
- Làm nổi bật (Highlight) UI HUD của phe ta (Ally) và hiển thị bảng máu của phe địch (Boss/Enemy) khi tương tác.

### 3. Tài liệu Dự Án (Documentation)
- Khởi tạo và cập nhật tài liệu `Guideline.md` ghi nhận thiết kế, cơ chế hoạt động, cấu trúc điều khiển và các tương tác của toàn bộ màn chơi.
- Cập nhật định kỳ `PROJECT_STATUS.md` và `Ban_Giao.md` để bám sát trạng thái nghiệp vụ, Unit test và tiến trình nghiệm thu.

### 4. Hệ Thống Tài Nguyên (Beat & Dual Soul)
- Xây dựng hệ thống Beat cá nhân hóa (Per-actor Beat planning): Mỗi phe có mức phân bổ ngân sách riêng biệt (Nhân vật: 2, Quái nhỏ: 1, Boss: 3). Cấu trúc lại giao diện hiển thị cho phù hợp.
- Ra mắt cơ chế tiêu hao Red Soul / Blue Soul dựa trên quy tắc (Ưu tiên xài Red trước, Blue nhận được từ các cơ chế như Crit, Weakpoint, Daze).

### 5. Sửa lỗi Giao Diện Sub-Menu & Key Bindings
- **Đồng bộ thanh Limit**: Cân đối giao diện HP / Limit sao cho chiều dài khung chứa của Boss (5 Limit) và Quái nhỏ (2 Limit) hoàn toàn bằng nhau, khắc phục lỗi lệch các ô Beat action.
- **Xóa Nút Back ảo**: Xóa bỏ các nút "Back" trên UI của Sub-Menu (Menu Skill/Item).
- **Nâng cấp Backspace & Right Click**: Quy hoạch lại logic Hủy thao tác (Lùi về Main Menu hoặc hủy chọn quái). Đồng bộ chức năng phím Backspace và Chuột Phải (Right Click) ở mọi vị trí, đồng thời gỡ lỗi khóa bàn phím ở Sub-Menu. 
- Sửa triệt để bug mất tự động highlight skill đầu tiên ở Sub-Menu khi mở lại lần thứ hai.
- Cập nhật nội dung bảng Help (F1) trong game.

### 6. Tích hợp Kỹ năng Đặc biệt (Instant Specials)
- Triển khai logic luồng Special Command Rules cho **Nhập Hồn**, **Toàn Thức** và tuyệt kỹ của **XIII (Bản Ngã Tái Sinh)**.
- Xây dựng 14 Unit Test (EditMode) cho Special Command nhằm kiểm chứng các quy luật khắt khe: Cost = 0 Beat, Cooldown logic, chặn stack, nhận diện Weakpoint.
- Đồng bộ lại state machine trong `BattleManager` để kích hoạt các trạng thái đặc biệt mà không làm gián đoạn Phase lập kế hoạch (Plan Phase).


### 7. Cập nhật Quy tắc Chạm trán Boss (Boss Encounter Rules) & Đồng bộ Trạng thái
- Triển khai logic Domain Tests (`BossEncounterRules.cs`) nhằm xác minh và siết chặt các luật lệ khi tương tác với Boss.
- Kiểm tra chéo (Cross-check) toàn bộ hệ thống tài liệu: Cập nhật thành công `PROJECT_STATUS.md` để ghi nhận mã commit baseline mới nhất (`c85b9d1` -> `88845fa`), và chính thức xóa nhãn "uncommitted" của các tính năng Special Command.

---
**Trạng thái hiện tại:** 
Nguyên mẫu chiến đấu hiện đã ở mức build-được, hệ thống Input được kiểm soát trơn tru và hệ thống Core Domain Test (EditMode) bao phủ tới 32 cases chạy pass hoàn toàn. Chuẩn bị nghiệm thu toàn bộ PlayMode cho Instant Specials.
