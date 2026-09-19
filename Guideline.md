# Hướng Dẫn & Tài Liệu Thiết Kế (Guideline) - XIII Project

Tài liệu này mô tả chi tiết các hệ thống, tính năng, cơ chế hoạt động và cấu trúc vận hành của dự án Game Turn-based (tạm gọi là XIII).

---

## 1. Mô Tả Tính Năng (Features Overview)

Hệ thống cốt lõi của game là một cơ chế chiến đấu theo lượt (Turn-based Combat) được thiết kế theo phong cách Lên Kế Hoạch (Planning Phase) và Thực Thi (Execution Phase) cùng lúc. 

Các tính năng chính bao gồm:
- **Lập Kế Hoạch (Action Planning)**: Thay vì chọn 1 lệnh đánh ngay lập tức, người chơi có thể lên kế hoạch nhiều hành động cho từng nhân vật. Các hành động được xếp vào các nhịp (Beat) trên thanh Action Bar.
- **Quản lý Tài Nguyên (Red Soul & Limit)**: Game không dùng hệ thống Mana truyền thống. Thay vào đó, người chơi quản lý **Soul** (Điểm linh hồn - tối đa 9) dùng chung cho toàn đội và **Limit** (Nộ) cho từng cá nhân (cả phe ta và phe địch).
- **Điều Khiển Linh Hoạt (Hot-Swap & Grid Nav)**: Cho phép chuyển đổi nhanh giữa các nhân vật (Q/E) và lựa chọn mục tiêu theo lưới không gian 2x2 bằng cụm phím điều hướng (WASD/Arrows).
- **Giao Diện Động (Dynamic HUD)**: Giao diện (Action Menu, Boss HUD) chỉ xuất hiện khi cần thiết, bám theo vị trí 3D của nhân vật trên màn hình.

---

## 2. Cơ Chế Hoạt Động (Mechanics)

### Vòng Lặp Chiến Đấu (Battle Loop)
Trận đấu diễn ra theo các trạng thái (State Machine):
1. **PLAYER_TURN**: Người chơi tự do duyệt qua các nhân vật, mở Menu lệnh và chọn kỹ năng/vật phẩm.
2. **WAIT_TARGET**: Sau khi chọn 1 kỹ năng, game chuyển sang chế độ chọn mục tiêu. Mũi tên (▼) và vòng sáng sẽ làm nổi bật mục tiêu đang được trỏ tới.
3. **EXECUTION**: Khi người chơi nhấn `Enter` (hoặc bấm Execute), hệ thống sẽ khoá UI và lần lượt thực thi các hành động trong Kế Hoạch theo từng nhịp (Beat).
4. **ENEMY_TURN**: Kẻ địch tính toán logic AI và phản công.

### Hệ Thống Điểm Red Soul
- **Cơ chế**: Dùng chung cho toàn đội. Có sức chứa tối đa là 9.
- **Tiêu hao**: Sử dụng kỹ năng mạnh sẽ tốn Red Soul.
- **Hồi phục**: Đánh thường (Attack) hoặc một số kỹ năng đặc thù (VD: "Múa Đại Đao" của Mac) sẽ hồi lại Red Soul.

### Hệ Thống Limit Break
- **Phe ta**: Thanh Limit (tối đa 5 đoạn) tự động tăng khi chịu sát thương. Đầy thanh có thể tung tuyệt kỹ.
- **Phe địch/Boss**: Có thanh Limit chia đoạn (Boss: 5, Địch thường: 2). Khi bị tấn công, thanh này tăng lên. Nếu đạt ngưỡng đầy, địch có thể kích hoạt cơ chế đặc biệt hoặc cuồng nộ.

---

## 3. Cấu Trúc Thiết Kế và Vận Hành (Architecture)

Mã nguồn được chia làm các module Singleton và Component hoạt động độc lập nhưng liên kết chặt chẽ:

### `BattleManager.cs` (Trái Tim Của Trận Đấu)
- Là một Singleton quản lý toàn bộ State (PLAYER_TURN, WAIT_TARGET, EXECUTION...).
- Nơi lưu trữ Kế hoạch phe ta (`playerPlan`) và Kế hoạch địch (`enemyPlan`). Dữ liệu Kế hoạch được lưu dưới dạng List của các `BeatPlan` (Nhịp).
- Xử lý điều hướng Input (Bàn phím) và phân phối luồng lệnh (Chọn nhân vật -> Lên lệnh -> Đẩy vào Action Bar).

### `BattleUIManager.cs` (Quản Lý Giao Diện)
- Gắn với Canvas chính. Lắng nghe các thay đổi từ `BattleManager` để cập nhật hiển thị.
- **Action Bar Grid (Góc phải dưới)**: Chứa Avatar, HP Bar và các Node Hành động (Hiển thị skill chuẩn bị thi triển).
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
- **Backspace**:
  - Lùi lại (Back) khỏi Sub-Menu.
  - Khi đang chọn Mục tiêu, nhấn Backspace để huỷ việc chọn và quay lại Menu nhân vật.
- **Delete**:
  - Hủy hành động (Remove Action): Xóa hành động mới nhất của nhân vật đang được Highlight/Chọn. Tiền (Soul) và Nộ (Limit) đã cọc sẽ được hoàn trả lập tức.
- **F1**: Mở bảng hướng dẫn nút (Key Map).

### Chuột (Mouse)
- Có thể rê chuột để Highlight (phát sáng) mục tiêu.
- Click chuột trái để chọn trực tiếp nhân vật hoặc kẻ địch.
- Hỗ trợ click vào các Nút trên UI (Mặc dù game thiết kế tối ưu cho bàn phím).

---
*Tài liệu này sẽ liên tục được cập nhật song song với quá trình mở rộng các tính năng mới của dự án.*
