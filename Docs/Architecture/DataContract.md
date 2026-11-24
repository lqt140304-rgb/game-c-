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

Scene và UI gọi các lệnh của `MarketRun` trong [mục 5](#5-giao-diện-c-sprint-1-cho-scene). Gameplay xử lý tiền và trạng thái ván. Tạo tác sẽ có giao diện riêng khi triển khai Sprint 2.

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

---

## 4. Thị trấn: Ô cửa hàng & Hướng dẫn thay Mesh

- **Menu dựng scene:** `Tools → Cho Do La → Build Market Town Blockout` (sinh `Assets/_Project/Scenes/Market_Town.unity`).
- **Hệ trục:** `+X` Đông, `+Z` Bắc, `+Y` lên (đơn vị mét). Gốc `(0,0,0)` ở mép Nam giữa trục phố.
- **Đọc ô cửa hàng (Codex):** Tìm các `ShopSlotMarker` trong scene; dùng `SlotId` (`ShopSlot_01`…`ShopSlot_08`), `SizeMeters` và `IsUpperFloor` để gắn NPC/quầy dịch vụ.

| `SlotId` | Vị trí | Kích thước | Tầng |
| --- | --- | --- | --- |
| `ShopSlot_01`, `ShopSlot_02` | Trục phố chính, gần phòng trọ | `4.0 × 3.0 m` | Trệt |
| `ShopSlot_03` | Hẻm vòm Tây | `4.0 × 3.0 m` | Trệt |
| `ShopSlot_04` | Ngõ Giếng Cổ Tự (Đông) | `5.0 × 4.0 m` | Trệt |
| `ShopSlot_05` | Ngõ khuất Tây | `4.0 × 3.0 m` | Trệt |
| `ShopSlot_06` | Sân hiên tầng 2 phía Đông | `5.0 × 4.0 m` | Tầng 2 (`+3.8m`) |
| `ShopSlot_07` | Ngõ khuất Tây Bắc (sau tháp) | `4.0 × 3.0 m` | Trệt |
| `ShopSlot_08` | Sân hiên tầng 2 phía Tây | `5.0 × 4.0 m` | Tầng 2 (`+3.8m`) |

- **Thay mesh 3D:** Mở Prefab tương ứng trong `Assets/_Project/Prefabs/Town/{Architecture,Verticality,Landmarks,AccentSockets,Slots}/`, thay `MeshFilter` bằng mesh Blender/Meshy/Hyper3D, giữ pivot tâm khối và tỷ lệ đơn vị `1×1×1`.

## 5. Giao diện C# Sprint 1 cho scene

Scene `Market_Town` dùng `Sprint1MarketService` để gọi `MarketRun`. Service tìm hoặc tạo một `MarketRunHost` trên cùng GameObject. Component khởi tạo `Run` trong `Awake` và dùng một ô tự lưu trong `Application.persistentDataPath`. Trong Editor, dữ liệu được đọc trực tiếp từ `Docs/Content/data/content.json`. Build desktop đưa cùng file nguồn vào StreamingAssets. Khi người chơi chọn chơi lại sau thắng hoặc thua, gọi `StartNextRun()` và gắn lại listener vào `Run` mới khi nhận `RunReplaced`.

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

Mở `Assets/_Project/Scenes/Market_Town.unity` để thử tích hợp. Mua hộp tại Nhà Niêm Phong, mang sang Tiệm Mở Hòm, mở và soi rồi bán. Dùng sổ ở phòng trọ hoặc [Tab] để kết thúc ngày, thu nợ và xem lượt mua về 0. Nạp lại scene để kiểm tra tiền, kho và hộp đã mua. Sprint 1 dùng một ô tự lưu; các nút tạo tác và Bí Khế chờ tích hợp Sprint 2.

Chạy kiểm thử C# ở thư mục gốc repo:

```sh
dotnet run --project QA/Gameplay -- Docs/Content/data/content.json
```

### Tạo Tác và Bí Khế BK1

Kho Kim Khuy có thể đọc `TactCatalog`, `EquippedTacts`, `EquipSlotCount`, `Resonance`, `ActiveCombos` và `Bikhe1Unlocked` từ `MarketRun`. Scene gọi `BuyTact(transactionId, tactId)`, `EquipTact(transactionId, tactId, slot)` và `UnequipTact(transactionId, slot)`. Ô đánh số từ 0. Gắn vào ô đang có món sẽ đưa món cũ về túi. Chuyển món sang ô khác chỉ giữ một bản trang bị.

Mua Tạo Tác dùng tiền ván và túi riêng. Sở hữu, trang bị và BK1 được ghi cùng bản tự lưu. Có 4 ô ban đầu. Khi trang bị đủ CB1 hoặc CB2 trước ngày 5, BK1 mở ô thứ 5 đến hết ván, kể cả sau khi tháo combo. Mốc 3 của cùng một hệ thay mốc 2. Hiệu ứng cộng giá bán và manh mối miễn phí chạy trong gameplay. `ActiveTactEffects` cung cấp loại hiệu ứng, nguồn và phạm vi cho scene và các dịch vụ sẽ tích hợp sau.

#### Màn Trang bị và Kho Kim Khuy

Nhấn **I** để mở Trang bị, hoặc tương tác với Kim Khuy để mở cùng màn và mua Tạo Tác. Chọn món rồi chọn ô. Chuột phải vào ô để tháo. Hệ và combo chỉ hiện trên màn Trang bị. Icon sáng khi đủ điều kiện. Rê vào combo chưa đủ để xem các icon món cần gắn.

Prefab hiển thị được tra bằng mã `CP1–CP3` hoặc `TC1–TC3` tại `Resources/TactEquipment/Models/{id}`, với đường dẫn tương thích `Resources/Blender/Models/Artifact_{id}`. Sprite tùy chọn nằm tại `Resources/TactEquipment/Icons/{id}`. Khi thiếu sprite, UI dùng mã món.

Đặt prefab nhân vật xem trước tại `Resources/TactEquipment/Character`. Dùng prefab chỉ phục vụ trình diễn, có điểm cầm `TactPreviewHand` hoặc xương tay phải Humanoid và trigger Animator `InspectTact` cho động tác cầm lên ngắm. Game gọi trigger sau khi giao dịch trang bị thành công. Nếu có model món mà thiếu prefab nhân vật, khung bên phải chỉ xoay món. Không gắn món lên người sau động tác.
