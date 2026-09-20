# Hướng Dẫn Triển Khai Prototype (Guideline) - XIII Project

> **Vai trò tài liệu:** Mô tả cách prototype hiện tại vận hành và cách điều khiển bản battle scene. Đây không phải GDD có quyền cao nhất và không được dùng để ghi đè các rule LOCK của Funding Demo.
>
> **Nguồn trạng thái chính:** Đọc `PROJECT_STATUS.md` trước khi đánh giá mức hoàn thiện hoặc bắt đầu triển khai. Design target nằm trong GDD Standard Pack; code/scene hiện tại chỉ phản ánh implementation snapshot.
>
> **Phân loại hiện tại:** Battle-mechanics prototype — build được, chưa phải Funding Demo vertical slice đã nghiệm thu.

Tài liệu này mô tả các hệ thống, tính năng, cơ chế hoạt động và cấu trúc vận hành đang có của prototype turn-based XIII. Các đoạn mô tả hành vi mục tiêu phải được đối chiếu với Funding Demo contract trước khi dùng làm acceptance criteria.

---

## 1. Mô Tả Tính Năng (Features Overview)

Hệ thống cốt lõi của game là một cơ chế chiến đấu theo lượt (Turn-based Combat) được thiết kế theo phong cách Lên Kế Hoạch (Planning Phase) và Thực Thi (Execution Phase) cùng lúc.

Các tính năng chính bao gồm:
- **Lập Kế Hoạch (Action Planning)**: Mỗi Character sở hữu 2 Beat riêng trong một round. Enemy thường có 1 Beat, Boss có 3 Beat. Tổng tối đa sáu action phe người chơi đến từ `3 Character × 2 Beat`, không phải một queue dùng chung.
- **Quản lý Tài Nguyên (Dual Soul & Limit)**: Funding Demo dùng bình 9 Soul gồm **3 Red Soul + tối đa 6 Blue Soul**, dùng chung cho cả đội. Chi phí tiêu Red trước rồi mới đến Blue. Red hồi đầu Player Phase; Blue tích lũy xuyên round. Mỗi planned action lưu Red/Blue đã reserve để xóa lệnh hoàn đúng từng loại.
- **Điều Khiển Linh Hoạt (Hot-Swap & Grid Nav)**: Cho phép chuyển đổi nhanh giữa các nhân vật (Q/E) và lựa chọn mục tiêu theo lưới không gian 2x2 bằng cụm phím điều hướng (WASD/Arrows).
- **Giao Diện Động (Dynamic HUD)**: Giao diện (Action Menu, Boss HUD) chỉ xuất hiện khi cần thiết, bám theo vị trí 3D của nhân vật trên màn hình.

---

## 2. Cơ Chế Hoạt Động (Mechanics)

### Vòng Lặp Chiến Đấu (Battle Loop)
Trận đấu diễn ra theo các trạng thái (State Machine):
1. **PLAYER_TURN**: Người chơi tự do duyệt qua các nhân vật, mở Menu lệnh và chọn kỹ năng/vật phẩm.
2. **WAIT_TARGET**: Sau khi chọn 1 kỹ năng, game chuyển sang chế độ chọn mục tiêu. Mũi tên (▼) và vòng sáng sẽ làm nổi bật mục tiêu đang được trỏ tới.
3. **EXECUTION (khái niệm)**: Khi người chơi nhấn `Enter` (hoặc bấm Execute), hệ thống khóa UI và resolve theo barrier Beat. Tất cả action trong Beat 1 phải resolve/cancel trước Beat 2; Beat 2 hoàn tất trước Beat 3. Các actor trong cùng Beat thuộc cùng một nhịp; policy thứ tự áp dụng effect nội bộ Beat vẫn OPEN và prototype hiện chạy các coroutine gần như đồng thời. Code hiện dùng cờ `isExecuting`; enum `BattleState` chưa có giá trị `EXECUTION` riêng.
4. **ENEMY_TURN**: Kẻ địch tính toán logic AI và phản công.

### Hệ Thống Dual Soul
- **Red Soul**: Tối đa 3, dùng chung toàn đội, hồi đầy khi bắt đầu Player Phase.
- **Blue Soul**: Tối đa 6, nhận từ Crit, Weakpoint và Break/Daze theo Funding Demo contract; không tự refill và được giữ qua round.
- **Tiêu hao**: Kỹ năng tiêu Red trước, sau đó mới tiêu Blue.
- **Thời điểm sử dụng**: Soul nhận trong Execute chỉ dùng được từ Player Phase kế tiếp.
- **Hoàn Soul**: Mỗi planned action lưu riêng số Red/Blue đã reserve; xóa action sẽ hoàn đúng từng pool và compact các node còn lại sang trái.

### Hệ Thống Limit Break
- **Phe ta**: Funding Demo contract không chốt một thanh Limit chung cho nhân vật người chơi. Instant Specials dùng Soul/cooldown; `Bản Ngã Tái Sinh` mở theo mốc boss dưới 20% HP. Các field `currentLimit` hiện có trên component phe ta là implementation legacy, không phải design canon.
- **Phe địch/Boss**: Limit là dữ liệu authored theo từng enemy/phase, không dùng một con số chung. Funding Demo matrix hiện chốt Tutorial enemy = 3, hai Hộc Tử Thi = 4, Bách Mệnh Quan Phase 1/2/3 = 10/14/8. Khi Limit giảm về 0, mục tiêu vào Daze và bị ngắt action theo contract.

---

## 3. Cấu Trúc Thiết Kế và Vận Hành (Architecture)

Prototype hiện tập trung phần lớn logic trong hai Singleton lớn (`BattleManager`, `BattleUIManager`) cùng các component nhân vật/visual. Đây là mô tả implementation hiện tại, không phải kiến trúc đích đã được nghiệm thu:

### `BattleManager.cs` (Trái Tim Của Trận Đấu)
- Là một Singleton quản lý toàn bộ State (PLAYER_TURN, WAIT_TARGET, EXECUTION...).
- Kế hoạch phe ta và phe địch đều dùng `List<BeatPlan>`. Mỗi `BeatPlan` chứa tối đa một action của mỗi actor trong nhịp đó; budget được kiểm soát theo actor: Character = 2, Enemy = 1, Boss = 3.
- Xử lý điều hướng Input (Bàn phím) và phân phối luồng lệnh (Chọn nhân vật -> Lên lệnh -> Đẩy vào Action Bar).

### `BattleUIManager.cs` (Quản Lý Giao Diện)
- Gắn với Canvas chính. Lắng nghe các thay đổi từ `BattleManager` để cập nhật hiển thị.
- **Per-character Beat Grid (Góc phải dưới)**: Mỗi Character có một hàng riêng gồm avatar, HP và 2 ô Beat. Ô đã dùng Blue Soul có viền xanh ngọc.
- **Boss/Enemy HUD (Giữa phía trên)**: Chứa Tên, Máu và Limit của quái. Chỉ hiện lên khi chỉ định đánh quái đó.
- **Soul Vessel**: Hiển thị số chấm Soul hiện có và nút Execute.
- Có khả năng map UI (Action Menu) bám theo toạ độ 3D của `CharacterInteraction`.

### `CharacterInteraction.cs` & `QuadAnimator.cs` (Thực Thể 3D & Sprite)
- **CharacterInteraction**: Quản lý HP, Limit, kỹ năng mang theo (SkillData). Xử lý hiệu ứng hình ảnh khi được chọn (Glow Outline, Vòng tròn dưới chân, Mũi tên trên đầu).
- **QuadAnimator & Billboard**: Chuyển đổi Sprite 2D (như SpriteSheet 8 hướng) trong môi trường 3D. Component `Billboard` giúp Sprite và UI Canvas của nhân vật (như Mini HP Bar) luôn xoay mặt về phía Camera.

---

## 4. Các Tương Tác (Interactions)

Hệ thống ưu tiên sử dụng Bàn phím để mang lại tốc độ thao tác (Input) nhanh nhất cho người chơi, tuy nhiên vẫn hỗ trợ Chuột.

### Bàn phím (Keyboard)
- **W, A, S, D / Mũi tên**:
  - Di chuyển vùng chọn giữa các nhân vật/kẻ địch (tương tác theo lưới không gian).
  - Di chuyển lên/xuống trong Menu Hành động (Attack, Skill, Item).
- **Q / E (hoặc PageUp / PageDown)**:
  - **Hot-Swap**: Chuyển đổi nhanh qua lại giữa các nhân vật phe mình mà không cần đóng Menu (Dù đang ở Menu gốc hay Sub-menu, bấm là nhảy).
- **Space (Phím Cách)**:
  - Xác nhận (Confirm) lựa chọn Menu.
  - Xác nhận mục tiêu thi triển kỹ năng.
- **Enter**:
  - Khi đang rảnh tay (không mở menu), bấm Enter để chốt sổ (Execute) và bắt đầu xả skill.
- **Backspace / Chuột phải (Right Click)**:
  - Lùi lại (Back) khỏi Sub-Menu (Skill, Item).
  - Khi đang chọn Mục tiêu, nhấn Backspace hoặc Chuột phải để huỷ việc chọn và quay lại Menu nhân vật.
- **Delete**:
  - Hủy hành động (Remove Action): Xóa hành động mới nhất của nhân vật đang được Highlight/Chọn. Tiền (Soul) và Nộ (Limit) đã cọc sẽ được hoàn trả lập tức.
- **F1**: Mở bảng hướng dẫn nút (Key Map).

### Chuột (Mouse)
- Có thể rê chuột để Highlight (phát sáng) mục tiêu.
- Click chuột trái để chọn trực tiếp nhân vật hoặc kẻ địch.
- Click chuột phải (Right Click) có chức năng tương đương Backspace (Lùi lại/Hủy).
- Hỗ trợ click vào các Nút trên UI (Mặc dù game thiết kế tối ưu cho bàn phím).
---

## 5. Giới Hạn Xác Minh

- Có 18 EditMode domain tests cho Beat budget/deletion lookup, Soul economy và reward rules. Các menu `Tools/.../Test...` vẫn chỉ là editor diagnostics, không phải Unity Test Framework suite.
- Build thành công không chứng minh combat loop không soft-lock hoặc toàn bộ UI đúng ở mọi aspect ratio.
- Các hệ thống Narrative, Exploration, Playable Knowledge, Instant Specials và boss multi-entity phase contract chưa có implementation hoàn chỉnh.
- Mọi tuyên bố tiến độ và nghiệm thu phải cập nhật ở `PROJECT_STATUS.md` trước, sau đó mới đồng bộ vào tài liệu này.

*Tài liệu này phải được cập nhật song song với implementation, nhưng không được tự nâng trạng thái từ PARTIAL lên VERIFIED khi chưa có bằng chứng test/playthrough mới.*
