# Yêu cầu dữ liệu từ các luật đã duyệt — CHỢ ĐỒ LẠ

**Tài liệu tham khảo của Claude 1 cho Codex và Antigravity.** Hợp đồng dữ liệu chính thức là [DataContract.md](../Architecture/DataContract.md), do Codex và Antigravity quản lý. Tài liệu này chỉ liệt kê những gì các luật đã duyệt đòi hỏi mà hợp đồng hiện tại chưa có, để hai bên bổ sung. Tên trường và lệnh là **gợi ý**, hai bên quyết định tên thật.

## 1. Dữ liệu nguồn

Toàn bộ số liệu đã duyệt nằm trong [data/content.json](data/content.json): luật chung, hộp, hiện vật, tỷ lệ rơi, manh mối, tạo tác, combo, gia truyền, Bí Khế, biến cố, dịch vụ và mở khóa meta.

- **Nạp từ file, không gán cứng số trong mã.** Claude 2 sẽ còn chỉnh số khi có bản chơi thật.
- [QA/test_content_data.py](../../QA/test_content_data.py) kiểm tra file này khớp với các tài liệu trong `Docs/Content` và với mô hình QA của Claude 2 (giá, tỷ lệ rơi, hàng giả, luật dịch vụ). Ai đổi số ở một nơi mà quên nơi khác thì test đỏ. Chạy: `cd QA && python3 -m unittest -v test_content_data`.

## 2. Nguyên tắc kỹ thuật bắt buộc

Các nguyên tắc này đến từ [DichVuVaMeta.md](DichVuVaMeta.md) mục 0 và yêu cầu của Claude 2 ([Sprint4.md](../Balance/Sprint4.md)):

1. **Mỗi thao tác tiền có mã giao dịch duy nhất**, lưu cùng bản lưu, kể cả giao dịch tự động như nhận tiền đấu giá. Phát lại mã cũ không đổi trạng thái.
2. **Kết quả ngẫu nhiên xác định theo (seed, ngày, số thứ tự lượt) hoặc theo mã hiện vật**, không lấy từ một bộ sinh số chung. Nhờ vậy tải lại không đổi kết quả cược hay đấu giá.
3. **Lưu tự động** vào đầu mỗi ngày và sau mỗi giao dịch tiền hoàn tất. Một ô lưu, không có lưu tay, không tải lại bản cũ trong cùng ván.
4. **Một hiện vật chỉ ở đúng một nơi:** kho, bàn mở (hộp), vé cầm, hoặc lô đấu giá.
5. **Thao tác bị từ chối không đổi trạng thái** và trả về mã lý do (mục 4).

## 3. Dữ liệu cần đọc và lệnh cần ghi theo từng luật

| Luật | Cần ĐỌC (thêm vào hợp đồng) | Cần GHI (lệnh gợi ý) |
| --- | --- | --- |
| **Kệ hộp, manh mối và bàn mở** | `ShelfBoxes` của lượt mua hiện tại (1 hộp mỗi loại, xác định theo seed, ngày, lượt, loại hộp), mỗi hộp có `TagPrice` (giá niêm yết theo manh mối), `FakeRate`, manh mối đã thấy; `ExtraCluePrice` (15 xu, mua một lần mỗi hộp, kết quả là **nhóm nguồn gốc thật**, chắc đúng); `TableBoxes` (tối đa 3 hộp chưa mở); `ShelfTurn` (1 đến 3). **Không** gửi cho UI manh mối nào đúng hay sai, và **không** hiển thị hay gửi giá trị kỳ vọng của loại hộp (chủ dự án chọn để người chơi tự tính). | `RequestBuyClue(offerId)` (trả về nhóm nguồn gốc thật của hiện vật trong hộp, không phải manh mối kế tiếp), `RequestStopBuying()` (dừng mua trong ngày, hoặc ngầm hiểu khi kết thúc ngày) |
| **Hiện vật và giá** | `ArtifactInstanceView.IsCertified`, `IsFake` (chỉ biết sau giám định), `ListedPrice` (giá hiển thị tính như hàng thật, **không làm lộ hàng giả**), tách các thành phần của giá (chất lượng, tổng hiệu ứng). **Không** gửi giá thực nhận của hàng chưa giám định cho giao diện. Giá làm tròn xuống đến xu. | không (tiền thực nhận trả về trong kết quả của lệnh bán) |
| **Giám định** | `AppraisalFee`, trạng thái dịch vụ mở hoặc đóng (biến cố E4) | `RequestAppraise(artifactInstanceId)` |
| **Phục chế** | `RestoreFee`, bậc chất lượng kế tiếp, dịch vụ mở hoặc đóng (E5) | `RequestRestore(artifactInstanceId)` |
| **Kho** | `InventoryCapacity` (8), `InventoryItems` | không (từ chối khi đầy) |
| **Tạo tác, combo, Bí Khế** | `ArtifactBag` (tạo tác đã mua chưa trang bị, không chiếm kho), `EquipSlotCount` (3 hoặc 4), danh sách hiệu ứng đang áp dụng kèm nguồn | `RequestUnequipTactArtifact(slotIndex)` |
| **Gia truyền GT1 (hoãn nợ)** | `PostponeAvailable`, `CarryDebt` (khoản hoãn sang ngày sau) | `RequestPostponeDebt()` hoặc cờ trong `RequestEndDay` |
| **Nợ** | `Arrears` (nợ tồn), `CarryDebt`, `AmountDueToday` gồm kỳ cộng nợ tồn cộng khoản hoãn | không |
| **Biến cố** | `ActiveEvents` (đã có): thêm mô tả hiệu ứng, thông báo trước khi mua hộp | không |
| **Đấu giá** | `AuctionListings` (lô đang chờ: hiện vật, giá sàn, ngày có kết quả) | `RequestAuctionSubmit(artifactInstanceId, floorOption)` |
| **Sòng bạc** | `BetsToday`, `CasinoOpen` (từ ngày 2) | `RequestPlaceBet(stake)` |
| **Cầm cố** | `PawnTickets` (hiện vật hoặc tạo tác, tiền cầm, giá chuộc, hạn) | `RequestPawn(instanceId)`, `RequestRedeem(ticketId)` |
| **Meta** | `MetaProgress` (hiện vật đã sưu tầm, mở khóa đã đạt) | tùy chọn khi bắt đầu ván: chọn gia truyền (M1), bật Rương Đen sớm (M3) |

## 4. Mã từ chối gợi ý (`OnActionRejected`)

Hợp đồng hiện có `OnActionRejected(reasonCode, message)`. Các mã gợi ý theo luật đã duyệt:

| Nhóm | Mã |
| --- | --- |
| Mua và mở hộp | `DAILY_BOX_LIMIT`, `TABLE_FULL`, `NOT_ENOUGH_CASH`, `INVENTORY_FULL`, `BOX_ALREADY_OPENED` |
| Giám định và phục chế | `APPRAISAL_CLOSED`, `RESTORATION_CLOSED`, `ALREADY_APPRAISED`, `QUALITY_AT_MAX`, `ITEM_LOCKED` |
| Tạo tác | `SLOT_FULL`, `ARTIFACT_NOT_OWNED`, `ARTIFACT_PAWNED` |
| Nợ và GT1 | `NOT_DUE_YET`, `POSTPONE_USED`, `POSTPONE_ON_DAY_7`, `POSTPONE_NOT_AVAILABLE` |
| Đấu giá | `AUCTION_BELOW_MIN_PRICE`, `AUCTION_DAILY_LIMIT`, `AUCTION_ON_DAY_7` |
| Sòng bạc | `CASINO_CLOSED`, `CASINO_DAILY_LIMIT`, `INVALID_STAKE` |
| Cầm cố | `PAWN_LIMIT`, `PAWN_NOT_ALLOWED` (gia truyền), `PAWN_EXPIRED` |
| Chung | `DUPLICATE_TRANSACTION`, `RUN_ENDED` |

## 5. Sự kiện thông báo thay đổi (gợi ý thêm)

`OnEventStarted(eventId)`, `OnEventEnded(eventId)`, `OnAuctionResolved(listingId, soldPrice)`, `OnBetResolved(won, delta)`, `OnPawnExpired(ticketId)`, `OnDebtPostponed(carryAmount)`, `OnBikheUnlocked(id)`, `OnComboChanged()`.

## 6. So với hợp đồng hiện tại

| Mục | Hợp đồng hiện tại | Còn thiếu |
| --- | --- | --- |
| Trạng thái ván | `CurrentDay`, `Cash`, `DailyBoxesPurchased`, `DebtDueToday`, `ActiveEvents` | `Arrears`, `CarryDebt`, `PostponeAvailable`, `BetsToday` |
| Hộp | `OfferId`, `Price`, `PotentialCategory`, `ClueText` | `FakeRate`, nhiều manh mối và giá manh mối, `TableBoxes` |
| Hiện vật | `Quality`, `Authenticity`, `Rarity`, `BaseValue`, `CurrentSellPrice`, `AppraisalState` | `IsCertified`, thành phần của giá bán |
| Lệnh | `RequestBuyBox`, `RequestOpenBox`, `RequestSellArtifact`, `RequestKeepArtifact`, `RequestPayDebt`, `RequestEndDay`, `RequestBuyTactArtifact`, `RequestEquipTactArtifact` | Giám định, phục chế, mua manh mối, tháo tạo tác, hoãn nợ, đấu giá, cược, cầm và chuộc |

**Một điểm cần Codex và Antigravity chốt:** luật nợ ([GDD mục 2](../GameDesign/README.md)) quy định Anh Bảy **tự thu vào cuối ngày** khi người chơi kết thúc ngày. Hợp đồng hiện có cả `RequestPayDebt` và `RequestEndDay`, dễ hiểu thành hai bước thu nợ. Đề xuất: việc thu nợ nằm trong `RequestEndDay`. Nếu giữ `RequestPayDebt` thì nó chỉ nên là bước xem trước hoặc xác nhận, không trừ tiền hai lần (đây là lỗi D-01 Claude 2 đã ghi ở [Sprint1.md](../Balance/Sprint1.md)).
