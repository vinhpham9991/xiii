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


### 8. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-21 11:35:36
- **Task được yêu cầu**: Cập nhật định dạng chuẩn khi viết log (Bắt buộc gồm: ngày giờ, task, cách thức, trạng thái).
- **Cách thức thực hiện**: Đã cấu hình trực tiếp vào Core Rules (`GEMINI.md`) để AI tự động tuân thủ format này vào cuối mỗi tiến trình làm việc. Từ nay, mọi hành động sửa code, test hay config đều sẽ được ghi nhận chi tiết theo 4 gạch đầu dòng này vào phần Nhật ký.
- **Trạng thái**: Hoàn tất. Đã lưu quy tắc.


### 9. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-21 20:02:55
- **Task được yêu cầu**: Sửa lỗi compiler CS0103 thiếu hàm `InitializeBossEncounter` và `RecoverTemporarilyCollapsedDrawers` trong `BattleManager.cs`.
- **Cách thức thực hiện**: Thêm 2 hàm còn thiếu vào `BattleManager.cs` để khởi tạo trạng thái Boss Phase 1 và phục hồi Hộc Tử Thi đầu mỗi turn người chơi. Khởi tạo stub logic chuẩn theo Domain Rule.
- **Trạng thái**: Hoàn tất. Đã sửa lỗi compile.


### 10. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-21 20:10:05
- **Task được yêu cầu**: Sửa lỗi phím Space không hoạt động để xác nhận trong Sub-Menu (Skill/Item).
- **Cách thức thực hiện**: Cập nhật hàm `Update` trong `BattleManager.cs`. Chặn logic tự động chọn Target (OnTargetSelected) nếu `IsSubMenuOpen()` đang bật. Thay vào đó, nếu Sub-Menu đang mở, lệnh nhấn Space sẽ tự động gọi `onClick.Invoke()` trên nút đang được Highlight của `EventSystem`.
- **Trạng thái**: Hoàn tất. Đã sửa xong.


### 11. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-21 20:42:45
- **Task được yêu cầu**: Sửa lỗi văng Out Of Memory do gán nhầm block phím Space ở trạng thái tự do (`currentActor == null`).
- **Cách thức thực hiện**: Hoàn tác (revert) thay đổi ở block `currentActor == null` và chuyển chính xác đoạn code kích hoạt `btn.onClick.Invoke()` bằng phím Space vào đúng block `currentActor != null` (Khi nhân vật đang được chọn và đang mở Menu).
- **Trạng thái**: Hoàn tất. Đã dập lỗi OOM.


### 12. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-21 21:52:43
- **Task được yêu cầu**: Sửa lỗi Back từ Sub-Menu ra ngoài chọn lại Skill/Item bị nhảy thẳng vào lệnh Attack (Target Selection).
- **Cách thức thực hiện**: Xóa bỏ logic gán cứng EventSystem vào nút Attack (`SelectLater`) trong hàm `TryGoBack()` của `BattleUIManager.cs`. Việc giữ nguyên selection cũ của EventSystem đã gây xung đột với hệ thống navigate bằng biến `activeMenuIndex` nội bộ, khiến khi ấn Space game nhận nhầm là ấn Attack. Thay vào đó set `SelectedGameObject(null)`.
- **Trạng thái**: Hoàn tất.

### 13. Nhật ký thao tác chi tiết
- **Ngày gi� **: 2026-09-21 22:11:23
- **Task được yêu cầu**: Sửa lỗi hiển thị thông tin và UI của Enemy (Boss & Enemy 2 thiếu Limit, sai số lượng action block, và Action highlight không mất sau khi thực thi).
- **Cách thức thực hiện**: 
  1. Sửa lỗi không load HP & Limit ban đầu bằng cách g� i BattleUIManager.Instance.UpdateHP cho toàn bộ kă hình lúc Start.
  2. Viết lại hàm tìm tên để phân biệt Boss, Enemy 1 và Enemy 2 dùa trên tên gốc (VD: XIII -> Boss, Left -> Enemy 1, Right -> Enemy 2).
  3. Sửa số beat thừa của Enemy 1 & 2 thành trong suốt (deactivate node của những beat không dùng thay vì chỉ đổi màu mò).
  4. Viết hàm ClearActionHighlight và g� i lúc kết thúc mỗi hành động để lập tức làm biến mất vi� n sáng và chữ của Node action đó (thay vì ch�  tới cuối lượt).
- **Trạng thái**: Hoàn tất.

### 14. Nhật ký thao tác chi tiết
- **Ngày gi� **: 2026-09-21 23:12:59
- **Task được yêu cầu**: Cân đối lại kích thước Icon Boss với Icon Enemy, sửa lỗi HP Bar của Boss bị dịch trái và Limit Boss/Enemy 2 chưa hiện lên.
- **Cách thức thực hiện**: 
  1. Chuyển hệ thống nhận diện tên UI từ việc so sánh chuỗi (characterName) sang sử dụng trực tiếp biến \CombatantId\ (DemoCombatantId) để đảm bảo độ chính xác tuyệt đối 100%. Việc này giải quyết triệt để tình trạng Boss và Enemy 2 bị nh�n diện nhầm là Enemy 1 nên thanh Limit không hiện.
  2. Chỉnh thuộc tính \preserveAspect = false\ cho Avatar của các quái vật trên UI. Nh�  đó, dù sprite của Boss có tỷ lệ khác với icon lính, nó vẫn sẽ được căn kéo đ� u ra vừa vặn khung 32x32 mà không bị co bóp lại, giải quyết hiện tượng có khoảng trống khiến thanh HP cảm giác như bị lệch sang trái.
- **Trạng thái**: Hoàn tất.

### 15. Nhật ký thao tác chi tiết
- **Ngày gi� **: 2026-09-21 23:14:54
- **Task được yêu cầu**: Khắc phục lỗi biên dịch do truy� n nhầm kiểu dữ liệu vào hàm GetDisplayLabelForEnemy.
- **Cách thức thực hiện**: 
  1. Sửa lỗi truy� n sai kiểu string \aseName\ vào \GetDisplayLabelForEnemy\ tìm thấy ở 3 hàm: \UpdateEnemyActionBar\, \ClearActionHighlight\, \UpdateHP\.
  2. Sưa lại thành các đối tượng kênh \CharacterInteraction\ tương ứng (\ction.actor\, \ctor\, \character\).
- **Trạng thái**: Hoàn tất.

### 16. Nhật ký thao tác chi tiết
- **Ngày gi� **: 2026-09-21 23:21:18
- **Task được yêu cầu**: Khắc phục tình trạng Avatar của Enemy bị dãn rộng (Icon Boss trở nên nh�  hơn) khiến thanh HP/Limit của Boss và Lính bị lệch nhau.
- **Cách thức thực hiện**: 
  1. Phát hiện t�nh năng mặc định \childForceExpandWidth = true\ của \HorizontalLayoutGroup\ gây ra tình trạng chia đ� u khoảng trống dư thừa cho các Node (dòng Enemy có ít Node hơn nên Avatar và InfoCol bị kéo dãn ra nhi� u hơn so với dòng Boss).
  2. Thêm thuộc t�nh \hLayout.childForceExpandWidth = false\ vào hàm \CreateEnemyPlanHud\. Thay đổi này ép \HorizontalLayoutGroup\ phải tuân thủ ch�nh xác k�ch thước 32x32 của Avatar và 140px của HP Bar, đảm bảo căn l�  chuẩn xác 100%.
- **Trạng thái**: Hoàn tất.

### 17. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-22 00:03:27
- **Task được yêu cầu**: Thực thi Implementation Plan Đợt 1 (F2 Config UI, Item System, Boss Phase & HP Gating, Data Stability, VFX Hooks).
- **Cách thức thực hiện**: 
  1. Tạo `DebugConfigUI.cs` gọi bằng OnGUI để chỉnh sửa nhanh chỉ số nhân vật/Boss và ấn F2 (có zoom x1.5 và background tối để dễ nhìn).
  2. Bổ sung `SoulRestore` và `CurePoison` vào enum ItemType. Tạo 4 loại Item (Health Potion, Power Elixir, Soul+1, Antidote) x9 và gỡ bỏ giới hạn số lượng dùng trong round.
  3. Áp dụng quy tắc miền (Domain rules) từ `BossEncounterRules` vào `BattleManager.cs` để quản lý chuyển Phase (Phase 1, 2, 3 dựa trên HP threshold) và cờ Immune (miễn nhiễm) khi Hộc Tử Thi còn sống.
  4. Thay thế mọi logic kiểm tra mục tiêu bằng chuỗi (`characterName.Contains`) thành so sánh ID hằng số (`CombatantId`) để đảm bảo code ổn định lâu dài.
  5. Cắm các cổng VFX (Prefabs) cho kỹ năng Nhập Hồn, Toàn Thức (Reticle tự động bám), Bản Ngã Tái Sinh (Cut-in).
- **Trạng thái**: Hoàn tất.

### 18. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-22 00:52:37
- **Task được yêu cầu**: Redesign UI Sub Menu (Skill & Item) và tối ưu hóa trải nghiệm người dùng.
- **Cách thức thực hiện**: 
  1. Thêm nút Close [X] vào Debug Config UI (F2) và thêm Text hiển thị "Config (F2)" cạnh nút Help trên HUD gốc.
  2. Xóa bỏ nút SPECIAL độc lập ngoài menu chính.
  3. Bổ sung trường `description` cho `SkillData` và thiết lập dữ liệu mô tả cho 9 kỹ năng của 3 nhân vật (Nhập Hồn, Toàn Thức, Bản Ngã Tái Sinh, ...).
  4. Viết lại hàm `CreateSkillButton` và `CreateItemButton` trong `BattleUIManager.cs` để sử dụng LayoutElement (minHeight = 80) và Rich Text nhằm hiển thị nút bấm với 3 dòng thông tin: Tên, Mô tả chi tiết, Số liệu Tiêu hao / Số lượng.
  5. Tích hợp lệnh Special (Tuyệt Kỹ) vào cuối danh sách của bảng Skill. Xử lý đổi màu linh hoạt cho Cooldown và điều kiện Active.
  6. Sửa lỗi chính tả, đổi tên item thành "Hồn Hoàn" và sửa UI tag thành `Soul x1`.
  7. Thay đổi quyền truy cập `GetSpecialCommand` sang public để giải quyết lỗi biên dịch CS0122.
- **Trạng thái**: Hoàn tất.

### 19. Nhật ký thao tác chi tiết
- **Ngày giờ**: 2026-09-22 01:03:00
- **Task được yêu cầu**: Bóc tách logic và viết Unit Test cho Damage Pipeline.
- **Cách thức thực hiện**: 
  1. Refactor hàm `CalculateDamage` từ `BattleManager` sang `FrankenXIII.Combat.Domain.DamageCalculatorRules` (Pure C#).
  2. Tạo các struct trung gian `CombatStats`, `SkillImpact` để truyền dữ liệu.
  3. Viết 6 test cases trong `DamageCalculatorRulesTests.cs` (EditMode) bao phủ logic trừ giáp, hệ, bạo kích, phá bền, và khiên.
- **Trạng thái**: Hoàn tất.
