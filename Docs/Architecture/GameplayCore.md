# Lõi gameplay vòng chơi 1 ngày — đề xuất kiến trúc để trao đổi với Codex

**Trạng thái:** đề xuất, chờ Codex xác nhận. Mã nằm ở `Assets/_Project/Scripts/Core/` (asmdef `ChoDoLa.Core`), test ở `Assets/_Project/Tests/EditMode/RunSessionTests.cs`.

**Lưu ý trung thực:** mã chưa được biên dịch hay chạy. Môi trường viết mã không có .NET SDK hay Unity. Cần mở trong Unity 6 và chạy Test Runner (EditMode) để xác minh. Các giá trị làm tròn trong test đã được kiểm lại bằng Python `decimal` (161, 269, 179, 81, 62).

**Quan hệ với bản của Codex:** `TASKS.md` ghi lõi Sprint 1 của Codex nằm ở nhánh local `codex/sprint-1`, chưa đẩy lên GitHub. Bản này viết độc lập theo `QA/*.py` và `content.json`. Cần Codex đẩy nhánh để đối chiếu và chọn một bản làm gốc, không giữ hai lõi song song.

## 1. Phạm vi đã làm

Mua, mở, soi, bán, kho 8 ô, bàn mở 3 hộp, giá hôm nay, kệ hộp theo lượt, manh mối, lời mách, kết thúc ngày và thu nợ (nợ tồn, phạt, thua ngay, thắng ngày 7).

**Chưa làm (cố ý):** giám định, phục chế, gia truyền (GT1), tạo tác, combo, Bí Khế, biến cố (E3), dịch vụ chợ, meta. Các điểm móc nối để thêm sau nằm ở mục 4.

## 2. Kiến trúc

| Lớp | Việc | Phụ thuộc Unity |
| --- | --- | --- |
| `ChoDoLa.Core` | Luật, trạng thái, lệnh, sự kiện | **Không** (`noEngineReferences`) |
| Cầu nối (MonoBehaviour, do Codex và Antigravity viết) | Gọi lệnh khi người chơi bấm, đọc view và sự kiện để cập nhật cảnh | Có |

- `RunSession` giữ `RunState` (dữ liệu thuần, serialize JSON được). Lưu ván chỉ cần chụp `RunState`.
- Lệnh trả `CommandResult` (đúng hoặc mã từ chối). **Lệnh bị từ chối không đổi trạng thái** (có test so sánh JSON trước và sau).
- Sự kiện đi qua hàng đợi `DrainEvents()`, cầu nối chuyển thành `OnBoxPurchased`, `OnRunStateChanged`... theo [DataContract.md](DataContract.md).
- View (`BoxOfferView`, `ItemView`, `RunSnapshot`) **không có** `IsFake`, hiện vật trong hộp, manh mối đúng hay sai, hay giá trị kỳ vọng. Giá hiển thị tính như hàng thật.
- Ngẫu nhiên: `DeterministicRng` (SplitMix64) khóa theo `(seed, ngày, lượt, loại hộp)`, giá hôm nay khóa theo `(seed, ngày, loại hộp)`. Không dùng `System.Random` vì kết quả khác nhau giữa runtime.
- Số liệu: mọi số lấy từ `content.json` qua `ContentData`. Tiền làm tròn bằng `decimal` để tránh lỗi kiểu 230 × 0,7 = 160,99999999999997.

## 3. Điểm cần Codex xác nhận

1. **Mã giao dịch (txId).** Lệnh tiền (`BuyClue`, `BuyBox`, `SellItem`, `EndDay`) nhận `txId`. Đề xuất cầu nối sinh một GUID mỗi lần bấm, core ghi vào `AppliedTx` chỉ khi lệnh **thành công**. Mã đã dùng thì trả `DUPLICATE_TRANSACTION`. **`DataContract.md` hiện chưa có `txId`**, cần sửa hợp đồng.
2. **Chặn D-01 (bấm đúp kết thúc ngày).** `EndDay(txId, forDay)`: bấm đúp thường sinh hai mã khác nhau nên chỉ chống trùng mã là chưa đủ. Core thêm tham số `forDay` (ngày UI đang hiển thị). Đã sang ngày mới thì trả `STALE_DAY`. Codex có đồng ý đổi chữ ký lệnh này không, hay đã chặn ở chỗ khác?
3. **Lệnh không cần txId.** `OpenBox` và `StopBuying` không đụng tiền nên không có txId. Mở hộp lặp thì hộp đã rời bàn nên tự an toàn (`BOX_ALREADY_OPENED`). Cần xác nhận quy tắc lưu: GDD mục 6 chỉ lưu sau giao dịch tiền. Nếu thoát ngay sau `OpenBox` thì tải lại hộp về bàn, mở lại ra **cùng hiện vật** (định sẵn theo seed), không lặp.
4. **Ai serialize.** Đề xuất core chỉ phơi `RunState`, cầu nối (hoặc lớp lưu của Codex) chịu trách nhiệm ghi file và ghi nguyên tử. Core không biết đường dẫn hay thời điểm lưu.
5. **Đồng bộ hay sự kiện.** Core đồng bộ, không có luồng riêng, không `Task`. Cầu nối gọi lệnh rồi `DrainEvents()` ngay. Có chấp nhận mô hình này không?
6. **Rương Đen.** Mã đang so `Id == "RD"` và đọc `rules.red_chest_from_day`. Nên chuyển thành trường `from_day` trong từng hộp của `content.json` (thuộc Claude 1 sở hữu nội dung). Cần Codex và Claude 1 đồng ý.
7. **Lời mách chỉ mua được lúc hộp còn trên kệ.** Hộp đã mua nằm trên bàn thì không mua lời mách nữa (luật nói "trả 15 xu để biết chắc... một lần mỗi hộp" nhưng không nói rõ thời điểm). Cần Claude 1 xác nhận.

## 4. Móc nối cho phần sau

| Việc sau | Chỗ gắn |
| --- | --- |
| Biến cố E3 (giảm 20% giá H2 **sau** dao động) | `Pricing.TodayPrice`, áp ngay sau bước dao động trong `StartDay` |
| Hiệu ứng tăng giá bán (tạo tác, combo, chứng nhận) | tham số `bonus` có sẵn trong `Pricing.SellPrice` và `ListedPrice`, cộng dồn theo phép cộng |
| Giám định | thêm `IsCertified` vào `ItemInstance`, lộ `IsFake` trong view chỉ khi đã giám định |
| GT1 hoãn nợ | thêm nhánh trong `EndDay` (`carry`, không phạt, không tính vào ngưỡng thua), mô hình ở `QA/economy_model.py` |
| Sổ tay ghi hiện vật đã gặp theo loại hộp (việc còn mở của Claude 1) | thêm sự kiện từ `OpenBox`, chờ quyết định có ghi hay không |

## 5. Ca kiểm thử đã có

Nợ: 8 ca của GDD mục 2. Giá: làm tròn xuống (V02 xước 269, V01 xước 161), hàng giả bán 20%. Kệ: xác định theo seed, giá hôm nay trong ±20%, Rương Đen từ ngày 4, số manh mối đúng theo loại hộp. Hộp: giới hạn 3 hộp mỗi ngày, bàn đầy, kho đầy giữ hộp ở bàn, mở hộp một lần. An toàn: mã giao dịch lặp, bấm đúp kết thúc ngày, lệnh bị từ chối không đổi trạng thái, tải lại bản lưu cho cùng kệ và giá.

**Chưa có:** đối chiếu số liệu với mô hình Python (tỷ lệ thắng bot). Các test hiện kiểm hành vi từng luật, không kiểm cân bằng. Muốn đối chiếu cần xuất kết quả `RunSession` ra file và so với `QA/` của Claude 2.
