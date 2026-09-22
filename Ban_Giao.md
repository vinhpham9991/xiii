# BÁO CÁO BÀN GIAO HIỆN TRẠNG — FRANKEN XIII

**Ngày xác minh:** 21/09/2026

**Repository:** `D:\Work\UNity\git\xiii\xiii`

**Baseline:** `499f023` (`main`); working tree hiện chứa checkpoint Task 3 Instant Specials chưa commit.

**Unity:** `6000.5.8f1`

**Phân loại:** Battle-mechanics prototype — buildable, chưa phải Funding Demo vertical slice đã nghiệm thu

> Đọc `PROJECT_STATUS.md` trước. File đó quy định thứ bậc nguồn sự thật, các rule Funding Demo đang LOCK và ma trận IMPLEMENTED / PARTIAL / MISSING / VERIFIED.

---

## 1. Kết luận bàn giao

Repository hiện chứa một prototype trận đánh 2.5D có thể compile và tạo Windows player build. Prototype có luồng chọn nhân vật, xếp action theo Beat riêng của từng actor, Execute theo barrier Beat, enemy turn, Dual Soul, HP/Limit/Daze, skill/item demo, logic Instant Special, HUD, VFX và audio cơ bản. Presentation riêng của các Special và climax XIII chưa hoàn chỉnh.

Director decision ngày 20/09/2026 đã thay thế shared queue 6 node: mỗi Character có 2 Beat, Enemy thường có 1 Beat, Boss có 3 Beat. Beat 1 hoàn tất trước Beat 2; Beat 2 hoàn tất trước Beat 3. Tổng tối đa sáu action của phe người chơi là kết quả của `3 Character × 2 Beat`, không phải ngân sách dùng chung linh hoạt.

Thứ tự áp dụng effect giữa nhiều actor trong cùng một Beat vẫn là quyết định OPEN. Prototype hiện khởi chạy các action đó đồng thời với presentation offset nhỏ; không được coi đây là priority deterministic đã khóa.

Không có bằng chứng hiện hành để tuyên bố:

- Combat hoàn thiện 100%.
- Funding Demo đã playable end-to-end.
- Có 24/24 hoặc 18/18 Integration Test suite đang tồn tại và pass; bằng chứng hiện hành là 32/32 EditMode domain cases, không phải PlayMode integration suite.
- Narrative, Exploration hoặc Playable Knowledge đã được triển khai.
- Boss Bách Mệnh Quan ba phase và hai Hộc Tử Thi đã tuân thủ full contract.
- Kiến trúc đã data-driven hoặc production-ready.

Những tuyên bố PASS trong session summary/log lịch sử chỉ có giá trị tham khảo cho đến khi có test source và lần chạy mới trên revision hiện tại.

---

## 2. Bằng chứng đã xác minh

### Compile và build

- Fresh Unity batch compile trên `6000.5.8f1` ngày 2026-09-21: thành công, không có C# error.
- Fresh Windows player build trên working tree Task 3: thành công. Artifact: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-Task3-InstantSpecials-Build\FrankenXIII.exe`; log: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-Task3-InstantSpecials-Build\build.log`.
- Thư mục build hiện khoảng 234.4 MiB; đây là artifact tạm ngoài repository, chưa phải release package được version hóa.
- Build Settings bật `Assets/_Project/Scenes/BattlePlaceholder.unity`.
- Build có 29 compiler/analyzer warning duy nhất, chủ yếu từ object-search API và sprite/editor API obsolete, cùng các field chưa tương thích Unity serialization. Không có C# error.
- Working tree hiện chứa checkpoint Task 3 Instant Specials chưa commit. Không có thay đổi chủ đích trong `Packages`, `ProjectSettings` hoặc URP settings.

### Asset database và repository

- Fresh scan ngày 21/09/2026: 0 asset/thư mục thiếu `.meta`, 0 orphan `.meta`, 0 nhóm GUID trùng trên 357 metadata có GUID.
- `Library`, `Temp`, `Logs`, generated solution/project files và archive cục bộ đã được ignore.
- Test assembly `FrankenXIII.Combat.Domain.Tests` hiện có 32 EditMode cases. Unity Test Runner `6000.5.8f1` pass 32/32, fail 0, skip 0, inconclusive 0 trên working tree Task 3 dựa trên `499f023` ngày 2026-09-21. Artifact: `C:\Users\idola\AppData\Local\Temp\FrankenXIII-Task3-InstantSpecials-Validation\EditMode-results-retry.xml`.

Build thành công chỉ xác nhận pipeline tạo player hoạt động. Nó không thay thế PlayMode test, full battle playthrough, UI responsive check hoặc balance validation.

---

## 3. Thành phần thực tế trong source

### Runtime chính

- `Assets/_Project/Scripts/Combat/BattleManager.cs`
  - State và input combat.
  - Player/enemy plan.
  - Instant Special state, target selection, cooldown, Soul cost và Nhập Hồn consumption.
  - Soul, inventory, damage, Daze.
  - Action execution, enemy AI, camera, audio và VFX orchestration.
- `Assets/_Project/Scripts/Combat/BattleUIManager.cs`
  - Runtime HUD construction.
  - Action/skill/item/Special menu và trạng thái interactable/cooldown.
  - Player/enemy action bars, HP, Limit, Soul UI, `BattleMessage` và text `[WEAKPOINT]` placeholder.
- `Assets/_Project/Scripts/Interaction/CharacterInteraction.cs`
  - Character stats, runtime skill fallback, selection, HP/Limit UI, animation bridge và lọc enemy click-target khi chọn Toàn Thức.
- `Assets/_Project/Scripts/Combat/SkillData.cs`
  - Plain C# runtime skill definition; chưa phải ScriptableObject và chưa có `[Serializable]`.
- `Assets/_Project/Scripts/Item/ItemData.cs`
  - ScriptableObject item definition; item demo hiện vẫn được tạo runtime trong `BattleManager`.
- `Assets/_Project/Scripts/Combat/PlannedAction.cs`
  - `PlannedAction` lưu provenance Red/Blue Soul; `BeatPlan` nhóm action của cả player và enemy theo từng nhịp.
- `Assets/_Project/Scripts/Combat/Domain`
  - Luật thuần C# cho Beat budget theo actor, Soul reservation/refund, Blue Soul reward và Instant Special.
- `Assets/_Project/Tests/EditMode`
  - 32 EditMode cases: 6 Beat, 5 Soul economy, 7 reward và 14 Special rules.

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
| Damage pipeline | Partial | Có công thức chính; `Nhập Hồn` đã nối x2 Damage/Break cho kỹ năng tấn công kế tiếp của An, nhưng pipeline số học đầy đủ và các nhánh cancel/dead target chưa có automated coverage hoàn chỉnh. |
| Limit / Daze | Implemented, chưa nghiệm thu PlayMode | Daze transition +2, Crit +1, Weakpoint non-Crit +1; 7 reward cases pass. |
| Bộ ba active skills | Partial | Skill được tạo bằng code dựa trên chuỗi tên nhân vật. |
| Instant Specials | Partial, chưa nghiệm thu PlayMode | `Nhập Hồn` và `Toàn Thức` chạy ngoài Beat plan, cost 1 Soul Red-first và cooldown 2 round. Nhập Hồn giữ charge tới skill resolve và không stack; Toàn Thức chỉ thu phí sau khi xác nhận enemy chưa có Weakpoint. XIII có cost 0, unlock/refill/activation hook ở `<=20%` Boss HP. |
| Special presentation / climax | Partial | Có button state, battle message và text `[WEAKPOINT]`; thiếu Toàn Thức reticle, Nhập Hồn vignette/noise/audio, pre-charge trước Boss, impact freeze, Phase 3, DPS race, XIII finisher/cut-in. |
| Boss ba phase + hai Hộc | Chưa có | Scene có ba enemy object nhưng thiếu phase, protection, feedback damage và top-down stun. |
| Combat items chuẩn Demo | Partial | Chưa có đủ bốn item và round restriction. |
| Narrative / Exploration | Chưa có | Không có five-area flow, dialogue, puzzle hoặc world state. |
| Playable Knowledge | Chưa có | Không có evidence-to-combat/narrative integration. |
| Automated tests | Verified cho phạm vi EditMode domain | Unity Test Runner pass 32/32 domain cases, fail 0, skip 0, inconclusive 0; PlayMode integration suite vẫn còn thiếu. |

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
10. Runtime xác định An/Mặc/XIII/Boss bằng substring của display name, chưa có stable ID.
11. Boss chưa có HP gate/phase transition; đòn lethal có thể đi xuyên mốc 20% và bỏ qua unlock Bản Ngã Tái Sinh.
12. `ConfirmMenuSelection()` không kiểm tra `Button.interactable`; keyboard có thể dispatch Special đang disabled, và XIII đã `ACTIVE` vẫn có thể phát activation message lần nữa.

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
   - Chọn An, dùng `SPECIAL`, xác nhận không có Beat mới; kỹ năng tiếp theo nhận đúng Nhập Hồn và trạng thái không bị Attack thường tiêu mất.
   - Với Nhập Hồn, kiểm tra Hộ Thân Phù = 700, Dẫn Hồn Thuật = 800, action cancel/Item không tiêu charge và không thể cast chồng khi đang `NẠP SẴN`.
   - Chọn Mặc, dùng `SPECIAL`, xác nhận mục tiêu hai lần; kiểm tra trừ đúng 1 Soul, không có Beat mới, HP row hiện `[WEAKPOINT]` và state quay lại Player Turn.
   - Hủy Toàn Thức hoặc chọn enemy đã có Weakpoint; xác nhận không mất Soul và không bắt đầu cooldown.
   - Qua hai Player Phase tiếp theo và kiểm tra cooldown Special về 0 ở round thứ hai sau khi cast.
   - Hạ Boss xuống tối đa 20% HP; kiểm tra `???` mở thành `BẢN NGÃ TÁI SINH`, Soul được nạp 3 Red + 6 Blue và activation tốn 0 Soul/0 Beat.
   - Sau khi XIII đã `ACTIVE`, thử xác nhận lại bằng chuột và bàn phím. Keyboard dispatch lặp hiện là expected gap cần sửa, không phải acceptance pass.
   - Kiểm tra riêng trường hợp một hit đưa Boss từ trên 20% xuống 0; hiện chưa có HP gate nên đây là expected gap, không phải acceptance pass.
   - Hoàn thành Victory/Defeat và Replay.
5. Không dùng các menu `Test...` trong `Tools` như bằng chứng automated test.

---

## 7. Production gate tiếp theo

Trước khi mở rộng Narrative hoặc thêm content, cần hoàn thành theo thứ tự:

1. Viết automated tests cho damage pipeline và các nhánh target chết/action cancel.
2. Data hóa character/skill/item bằng stable IDs.
3. Hoàn tất nghiệm thu PlayMode cho Instant Specials và presentation của Nhập Hồn/Toàn Thức; phần Phase 3, HP gate và finisher/cut-in của XIII đi cùng gate Boss ở mục 4.
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


### Cập nhật 22/09/2026 (Patch GDD 2.1.0 Part 1)
- **Hoàn tất Boss HP Gates**: Đã code chuẩn `BossEncounterRules.cs` khóa HP tại 60%, 40%, 20% theo đúng tài liệu GDD 2.1.0 mới nhất.
- **Hoàn tất Cập nhật Bản Ngã Tái Sinh**: Fixed lỗi không nạp đầy Hồn Năng.
- **Hoàn tất ID Refactoring**: Chuyển đổi `SkillData` và `ItemData` sang dùng Enum `SkillId` và `ItemId`.
- **Cần làm tiếp**: 
  - Sửa lại Priority System bị revert (ExecutePlanRoutine).
  - Blue Soul Economy (+1 Break, 1 cap Crit, 1 cap Overkill).
  - Status Effects (Poison, Curse, Vulnerability, Berserk).
