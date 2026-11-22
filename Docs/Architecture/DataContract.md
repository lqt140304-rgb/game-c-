# Hợp đồng dữ liệu & Thiết lập Sprint 0 (Antigravity ↔ Codex)

Tài liệu này xác định giao diện dữ liệu giữa **Antigravity** (cảnh 3D, prefab, tương tác, UI) và **Codex** (gameplay, dữ liệu, lưu ván, kiểm thử) để hai bên triển khai độc lập từ Sprint 0 mà không xung đột.

---

## 1. Dữ liệu Antigravity cần ĐỌC (Read / Events từ Codex)

### 1.1. Trạng thái ván & ngày (`RunStateSnapshot`)
- `CurrentDay` (`int`, 1–7)
- `Cash` (`int`, đơn vị xu)
- `DailyBoxesPurchased` (`int`, 0–3) và `MaxDailyBoxes` (`int`, mặc định 3)
- `DebtDueToday` (`int`) và `IsDebtPaidToday` (`bool`)
- `ActiveEvents` (danh sách biến cố đang hiệu lực để hiển thị trên sổ tay / cảnh chợ)

### 1.2. Hộp mù & Hiện vật (`BoxOfferView`, `ArtifactInstanceView`)
- **Hộp mù:** `OfferId`, `BoxTypeId`, `DisplayName`, `Price`, `PotentialCategory`, `ClueText`, `VisualVariantKey` (để Antigravity gắn đúng prefab/tem niêm phong 3D).
- **Hiện vật:** `InstanceId`, `ItemId`, `DisplayName`, `Quality` (tình trạng), `Authenticity` (xác thực), `Rarity` (độ hiếm), `BaseValue`, `ListedPrice`, `AppraisalState`, `VisualPrefabKey`.

### 1.3. Kho, Tạo tác & Bí Khế
- `InventoryItems`: danh sách hiện vật đang giữ.
- `EquippedArtifacts` / `ActiveCombos`: danh sách tạo tác theo tier/hệ và combo đang kích hoạt.
- `BikheProgressList`: danh sách Bí Khế kèm điều kiện hiện tại / mục tiêu / trạng thái đã mở.

### 1.4. Sự kiện thông báo thay đổi (Events)
- `OnRunStateChanged(RunStateSnapshot)` — cập nhật HUD, sổ tay.
- `OnBoxPurchased(BoxInstanceView)` — spawn hộp 3D tại quầy/bàn.
- `OnBoxOpened(ArtifactInstanceView)` — chạy animation mở hộp và spawn prefab hiện vật 3D.
- `OnInventoryChanged()` / `OnDebtSettled(bool success, int remainingCash)`
- `OnActionRejected(string reasonCode, string message)` — hiển thị cảnh báo UI (ví dụ: vượt 3 hộp/ngày, thiếu tiền, kho đầy, chưa tới kỳ trả nợ).

---

## 2. Hành động Antigravity GHI / GỬI LÊN (Commands tới Codex)

Scene và UI gọi các lệnh của `MarketRun` trong [mục 4](#4-giao-diện-c-sprint-1-cho-scene). Gameplay xử lý tiền và trạng thái ván. Tạo tác sẽ có giao diện riêng khi triển khai Sprint 2.

---

## 3. Hướng dẫn mở Sprint 0 trong Unity 6 (6000.6.9f1) URP

1. Mở **Unity Hub** → Add project from disk → chọn thư mục repo `game-c-` (phiên bản mục tiêu: **Unity 6 / 6000.6.9f1**, URP).
2. Trên thanh menu Unity, chọn **`Tools → Cho Do La → Create Sprint 0 Test Scene`**.
3. Script sẽ tự tạo [Market_Test.unity](../../Assets/_Project/Scenes/Market_Test.unity) gồm:
   - `Floor`, `Counter_NiemPhong_Placeholder`, `Table_OpenBox_Placeholder`, `TestBox_Placeholder`
   - `Player` (di chuyển WASD, xoay chuột, nhấn `E` để tương tác, `Esc` để khóa/mở chuột)
   - `HUD_Canvas` (thanh trạng thái ngày/tiền/nợ/hộp, tâm ngắm, gợi ý tương tác)
4. Nhấn **Play** để kiểm tra di chuyển First-Person và tương tác `[E]` với hộp mẫu.
5. Khi có mẫu 3D (`.fbx`/`.glb`), đặt vào `Assets/_Project/Art/` và thay thế các `*_Placeholder` trong scene/prefab.

## 4. Giao diện C# Sprint 1 cho scene

Gắn một `MarketRunHost` vào GameObject ở gốc của scene gameplay. Component khởi tạo `Run` trong `Awake` và dùng một ô tự lưu trong `Application.persistentDataPath`. Trong Editor, dữ liệu được đọc trực tiếp từ `Docs/Content/data/content.json`. Build desktop đưa cùng file nguồn vào StreamingAssets. Khi người chơi chọn chơi lại sau thắng hoặc thua, gọi `StartNextRun()` và gắn lại listener vào `Run` mới khi nhận `RunReplaced`.

| Scene đọc từ `Run` | Dùng để hiển thị |
| --- | --- |
| `Day`, `Cash`, `BoughtToday`, `DailyLimit` | Ngày, tiền và lượt mua |
| `TodayPrices` | Bảng giá theo loại hộp, đọc ngay đầu ngày và cả khi đã hết lượt mua |
| `Shelf`, `ExtraCluePrice` | ID offer, ID loại hộp, `TodayPrice` của loại hộp, `Price` là giá niêm yết theo manh mối đầu, tỷ lệ giả của loại và manh mối miễn phí. `CertainOrigin` là ID nhóm nguồn gốc thật, chỉ có giá trị sau khi mua lời mách |
| `Table`, `TableCapacity` | ID và loại hộp chưa mở, giữ qua ngày |
| `Inventory`, `InventoryCapacity`, `Inspect(itemId)` | Hiện vật đã mở, mã mẫu để chọn prefab, chất lượng, dấu hiệu soi và `ListedPrice` tính như hàng thật khi chưa giám định |
| `DueToday`, `Arrears`, `Status`, `EndReason` | Kỳ thu hiện tại gồm nợ tồn và lý do kết thúc ván |

| Lệnh Sprint 1 | Kết quả thành công |
| --- | --- |
| `BuyClue(transactionId, offerId)` | Trừ 15 xu và mở `CertainOrigin` trên offer. Chỉ mua một lần mỗi hộp, giá hộp giữ nguyên. |
| `BuyBox(transactionId, offerId)` | Trừ giá, thêm hộp lên bàn, tăng lượt và bày kệ mới. |
| `StopBuying(transactionId, expectedDay)` | Dừng mua đến ngày kế tiếp. |
| `OpenBox(transactionId, boxId)` | Chuyển hộp trên bàn thành một hiện vật trong kho. `EntityId` là ID hiện vật. |
| `Sell(transactionId, itemId)` | Xóa hiện vật khỏi kho và cộng tiền thực nhận. `Amount` là số xu thực nhận và `WasFake` cho biết món đã bán có phải hàng giả. |
| `EndDay(transactionId, expectedDay)` | Thu nợ, tính phạt, rồi sang ngày hoặc kết thúc ván. `Amount` là tiền đã thu, `Penalty` là tiền phạt mới. |

`EndDay` là lệnh duy nhất thu nợ. Truyền ngày đang hiển thị tại thời điểm người chơi xác nhận, để xác nhận lặp của ngày cũ nhận `STALE_DAY`. Một ID giao dịch dùng cho đúng một ý định, giữ nguyên ID khi gửi lại. Gọi từ luồng chính của Unity, đọc `Result.Code` để hiện lỗi và dùng `Result.EntityId` cho hiệu ứng theo cá thể.

Giá hôm nay dao động theo seed, ngày và loại hộp. E3 giảm giá sau khi tính dao động. Làm tròn đến xu gần nhất, rồi nhân giá hôm nay với hệ số của manh mối miễn phí đầu tiên và làm tròn lần nữa để được giá niêm yết. Lệnh mua hộp trừ đúng giá niêm yết.

Giá hiển thị và số xu thực nhận đều làm tròn xuống sau khi áp dụng các hệ số. Món chưa giám định chỉ lộ thật hay giả trong kết quả bán.

Đăng ký `Changed` để đọc lại trạng thái sau khi lưu thành công. Lệnh bị từ chối giữ nguyên tiền, lượt mua, bàn, kho và nợ. Gameplay lưu cả kết quả hộp và ID giao dịch. Tiền đã trả trừ vào tổng nợ. Phạt mới tính trên toàn bộ số còn lại, gồm cả phần phạt cũ còn thiếu.

Nếu bản lưu hỏng hoặc dùng schema không hỗ trợ, `MarketRunHost` giữ bản cũ cạnh ô lưu dưới tên `market-run.json.invalid-…`, ghi cảnh báo và bắt đầu ván mới. Một hộp đã mua manh mối trong bản lưu cũ được nhận lời mách chắc đúng khi tải lại, không thu thêm phí.

Chạy kiểm thử C# ở thư mục gốc repo:

```sh
dotnet run --project QA/Gameplay -- Docs/Content/data/content.json
```
