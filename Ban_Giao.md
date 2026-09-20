# BÁO CÁO BÀN GIAO HIỆN TRẠNG — FRANKEN XIII

**Ngày xác minh:** 20/09/2026

**Repository:** `D:\Work\UNity\git\xiii\xiii`

**Baseline:** `855c8ef` (`main`); working tree Task 1–3 hiện chưa commit.

**Unity:** `6000.5.8f1`

**Phân loại:** Battle-mechanics prototype — buildable, chưa phải Funding Demo vertical slice đã nghiệm thu

> Đọc `PROJECT_STATUS.md` trước. File đó quy định thứ bậc nguồn sự thật, các rule Funding Demo đang LOCK và ma trận IMPLEMENTED / PARTIAL / MISSING / VERIFIED.

---

## 1. Kết luận bàn giao

Repository hiện chứa một prototype trận đánh 2.5D có thể compile và tạo Windows player build. Prototype có luồng chọn nhân vật, xếp action theo Beat riêng của từng actor, Execute theo barrier Beat, enemy turn, Dual Soul, HP/Limit/Daze, skill/item demo, HUD, VFX và audio cơ bản.

Director decision ngày 20/09/2026 đã thay thế shared queue 6 node: mỗi Character có 2 Beat, Enemy thường có 1 Beat, Boss có 3 Beat. Beat 1 hoàn tất trước Beat 2; Beat 2 hoàn tất trước Beat 3. Tổng tối đa sáu action của phe người chơi là kết quả của `3 Character × 2 Beat`, không phải ngân sách dùng chung linh hoạt.

Thứ tự áp dụng effect giữa nhiều actor trong cùng một Beat vẫn là quyết định OPEN. Prototype hiện khởi chạy các action đó đồng thời với presentation offset nhỏ; không được coi đây là priority deterministic đã khóa.

Không có bằng chứng hiện hành để tuyên bố:

- Combat hoàn thiện 100%.
- Funding Demo đã playable end-to-end.
- Có 24/24 hoặc 18/18 integration tests đang tồn tại và pass.
- Narrative, Exploration hoặc Playable Knowledge đã được triển khai.
- Boss Bách Mệnh Quan ba phase và hai Hộc Tử Thi đã tuân thủ full contract.
- Kiến trúc đã data-driven hoặc production-ready.

Những tuyên bố PASS trong session summary/log lịch sử chỉ có giá trị tham khảo cho đến khi có test source và lần chạy mới trên revision hiện tại.

---

## 2. Bằng chứng đã xác minh

### Compile và build

- Fresh Unity batch compile trên `6000.5.8f1`: thành công, không có C# error.
- Fresh Windows player build: thành công.
- Build Settings bật `Assets/_Project/Scenes/BattlePlaceholder.unity`.
- Build sinh năm warning CS0618 do dùng `FindObjectOfType` / `FindObjectsOfType` obsolete.
- Working tree hiện chứa checkpoint Task 1–3 chưa commit. Không có thay đổi ngoài phạm vi trong `Packages`, `ProjectSettings` hoặc URP settings sau các lần xác minh.

### Asset database và repository

- Không phát hiện asset thiếu `.meta`.
- Không phát hiện orphan `.meta`.
- Không phát hiện nhóm GUID trùng.
- `Library`, `Temp`, `Logs`, generated solution/project files và archive cục bộ đã được ignore.
- Test assembly `FrankenXIII.Combat.Domain.Tests` hiện có 18 EditMode cases; lần chạy Unity `6000.5.8f1` ngày 20/09/2026 pass 18/18, fail 0, skip 0. Result artifact: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-BeatDecision-Validation\EditMode-results-final.xml`.

Build thành công chỉ xác nhận pipeline tạo player hoạt động. Nó không thay thế PlayMode test, full battle playthrough, UI responsive check hoặc balance validation.

---

## 3. Thành phần thực tế trong source

### Runtime chính

- `Assets/_Project/Scripts/Combat/BattleManager.cs`
  - State và input combat.
  - Player/enemy plan.
  - Soul, inventory, damage, Daze.
  - Action execution, enemy AI, camera, audio và VFX orchestration.
- `Assets/_Project/Scripts/Combat/BattleUIManager.cs`
  - Runtime HUD construction.
  - Action/skill/item menu.
  - Player/enemy action bars, HP, Limit và Soul UI.
- `Assets/_Project/Scripts/Interaction/CharacterInteraction.cs`
  - Character stats, runtime skill fallback, selection, HP/Limit UI và animation bridge.
- `Assets/_Project/Scripts/Combat/SkillData.cs`
  - Plain C# runtime skill definition; chưa phải ScriptableObject và chưa có `[Serializable]`.
- `Assets/_Project/Scripts/Item/ItemData.cs`
  - ScriptableObject item definition; item demo hiện vẫn được tạo runtime trong `BattleManager`.
- `Assets/_Project/Scripts/Combat/PlannedAction.cs`
  - `PlannedAction` lưu provenance Red/Blue Soul; `BeatPlan` nhóm action của cả player và enemy theo từng nhịp.
- `Assets/_Project/Scripts/Combat/Domain`
  - Luật thuần C# cho Beat budget theo actor, Soul reservation/refund và Blue Soul reward.
- `Assets/_Project/Tests/EditMode`
  - EditMode tests cho Action Node, Soul economy và reward rules.

### Scene và editor tooling

- Scene build hiện hành: `Assets/_Project/Scenes/BattlePlaceholder.unity`.
- Scene generator thực tế: `BattleSceneGenerator.cs` với menu `Tools/Franken XIII/1. Generate 2.5D Battle Placeholder`.
- Không có `BattleSceneBuilder.cs`.
- Không có `BattleUIController.cs`.
- Không có `BattleSFXManager.cs`.
- Không có `MonoHelper.cs`.
- Các menu tên `Test...` trong thư mục Editor là công cụ chẩn đoán thủ công, không phải Integration Test suite.

---

## 4. Trạng thái tính năng

| Tính năng | Trạng thái | Ghi chú |
|---|---|---|
| Player planning và Execute | Implemented, chưa nghiệm thu PlayMode | Action được nhóm theo Beat; toàn bộ action trong Beat hiện tại resolve/cancel trước khi chuyển Beat tiếp theo. |
| Beat riêng theo actor | Implemented, chưa nghiệm thu PlayMode | Character = 2 Beat, Enemy thường = 1 Beat, Boss = 3 Beat; 6 Beat-rule cases pass, gồm latest-action lookup cho actor được chọn. Shared queue 6 node đã bị loại bỏ. |
| 3 Red + 6 Blue Soul | Implemented, chưa nghiệm thu PlayMode | Red-first reservation, hoàn đúng màu Soul và Blue cap có 5 domain tests pass. |
| Damage pipeline | Partial | Có công thức chính; `Nhập Hồn` multiplier vẫn TODO. |
| Limit / Daze | Implemented, chưa nghiệm thu PlayMode | Daze transition +2, Crit +1, Weakpoint non-Crit +1; 7 reward cases pass. |
| Bộ ba active skills | Partial | Skill được tạo bằng code dựa trên chuỗi tên nhân vật. |
| Instant Specials | Chưa có | Thiếu `Nhập Hồn`, `Toàn Thức`, `Bản Ngã Tái Sinh` và cooldown contract. |
| Boss ba phase + hai Hộc | Chưa có | Scene có ba enemy object nhưng thiếu phase, protection, feedback damage và top-down stun. |
| Combat items chuẩn Demo | Partial | Chưa có đủ bốn item và round restriction. |
| Narrative / Exploration | Chưa có | Không có five-area flow, dialogue, puzzle hoặc world state. |
| Playable Knowledge | Chưa có | Không có evidence-to-combat/narrative integration. |
| Automated tests | Có | 18/18 EditMode tests pass trên working tree hiện tại; chưa có PlayMode integration suite. |

---

## 5. Sai lệch/rủi ro đã biết

1. `BattleManager` và `BattleUIManager` đang là God classes, gộp domain, presentation, input, camera, audio và VFX.
2. Nhiều state gameplay để public mutable; data Inspector chưa theo chuẩn `[SerializeField] private`.
3. Có `Camera.main` và `GetComponent` trong `Update`/hot path.
4. Chuỗi tiếng Việt mojibake vẫn tồn tại ở một số UI/message legacy.
5. Skill/effect phụ thuộc string display name, không có stable ID.
6. `VFXSetup.cs` phụ thuộc đường dẫn tuyệt đối ngoài repository.
7. Default Volume Profile chứa component test của Render Pipeline với `m_Script: {fileID: 0}`.
8. Package AI Inference/Sentis được đưa vào player build dù chưa tìm thấy gameplay code sử dụng.
9. Chưa có active-asset BOM/license chain cho toàn bộ asset shipping.

---

## 6. Cách mở và kiểm tra hiện tại

1. Mở repository bằng Unity `6000.5.8f1`.
2. Mở `Assets/_Project/Scenes/BattlePlaceholder.unity`.
3. Chờ Console compile sạch trước khi vào Play Mode.
4. Kiểm tra tối thiểu:
   - Chọn từng đồng minh.
    - Xếp Attack/Skill/Item.
    - Xếp đúng 2 action cho từng Character và xác nhận action thứ 3 của actor đó bị từ chối.
    - Xác nhận Beat 1 của mọi actor hoàn tất trước khi Beat 2 bắt đầu.
   - Chuyển nhân vật bằng Q/E.
   - Xóa action và quan sát Soul được hoàn.
   - Execute toàn bộ plan.
   - Gây Daze và kiểm tra số Blue Soul nhận được.
   - Hoàn thành Victory/Defeat và Replay.
5. Không dùng các menu `Test...` trong `Tools` như bằng chứng automated test.

---

## 7. Production gate tiếp theo

Trước khi mở rộng Narrative hoặc thêm content, cần hoàn thành theo thứ tự:

1. Viết automated tests cho damage pipeline và các nhánh target chết/action cancel.
2. Data hóa character/skill/item bằng stable IDs.
3. Triển khai Instant Specials.
4. Triển khai boss multi-entity/phase contract.
5. Chạy full PlayMode smoke test và UI responsive test cho layout 3 hàng Character × 2 Beat.

Chỉ sau khi các gate trên có bằng chứng pass mới được nâng phân loại từ **battle-mechanics prototype** lên **verified combat slice**.

---

## 8. Quy tắc cập nhật bàn giao

- Mọi con số test phải kèm tên test, Unity version, Git revision, ngày chạy và result artifact.
- Mọi tuyên bố “hoàn thiện” phải trỏ đến acceptance criteria trong GDD và bằng chứng hiện hành.
- Nếu một rule LOCK thay đổi, ghi Director decision trước rồi mới sửa code và tài liệu.
- Cập nhật `PROJECT_STATUS.md` trước; đồng bộ file này sau.

*(Bản bàn giao cũ từng tuyên bố combat 100% và 24/24 tests đã được thay thế vì không được source hiện tại chứng minh.)*
