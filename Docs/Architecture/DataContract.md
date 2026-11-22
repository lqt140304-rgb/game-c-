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
- **Hiện vật:** `InstanceId`, `ItemId`, `DisplayName`, `Quality` (tình trạng), `Authenticity` (xác thực), `Rarity` (độ hiếm), `BaseValue`, `CurrentSellPrice`, `AppraisalState`, `VisualPrefabKey`.

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

Mọi thao tác 3D/UI của người chơi chỉ gửi yêu cầu (không tự trừ tiền hay tự đổi trạng thái):
- `RequestBuyBox(string offerId)`
- `RequestOpenBox(string boxInstanceId)`
- `RequestSellArtifact(string artifactInstanceId)`
- `RequestKeepArtifact(string artifactInstanceId)`
- `RequestPayDebt()`
- `RequestEndDay()`
- `RequestBuyTactArtifact(string tactArtifactId)` / `RequestEquipTactArtifact(int slotIndex, string tactArtifactId)`

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
