# Gia truyền, tạo tác, combo và Bí Khế — CHỢ ĐỒ LẠ

Bản nháp của Claude 1 cho Sprint 2. Mọi con số là **[ĐỀ XUẤT]** để thử nghiệm và sẽ do Claude 2 cân bằng lại. Luật nền theo [GDD mục 4](../GameDesign/README.md).

**Quy tắc chung áp dụng cho mọi hiệu ứng:**
- Các hiệu ứng cùng loại không cộng dồn. Nếu nhiều hiệu ứng cùng giảm phạt nợ thì chỉ lấy mức tốt nhất.
- "Trong ván" nghĩa là từ lúc đạt điều kiện đến hết ván. "Trong ngày" nghĩa là đến khi kết thúc ngày.
- Mọi hiệu ứng đều được ghi vào sổ tay kèm điều kiện và trạng thái.

## 1. Cổ vật gia truyền

Mỗi ván bắt đầu với **1 cổ vật gia truyền**, rút ngẫu nhiên từ 2 món mẫu. Nội tại là bị động và có hiệu lực trong suốt ván. Đây là cách chọn món đầu ván **[ĐỀ XUẤT]**.

| Mã | Tên | Nội tại bị động | Điều kiện kích hoạt | Hiệu lực |
| --- | --- | --- | --- | --- |
| GT1 | Sổ Ghi Nợ Cũ | Phạt khi thu nợ giảm từ 20% xuống 10%. | Mỗi lần thu nợ còn nợ tồn. | Trong ván. |
| GT2 | Đèn Dầu Thờ | Khi mở hộp H1 (Hộp Gỗ Nhỏ), được xem thêm 1 manh mối niêm phong miễn phí. | Mở hộp H1. | Trong ván. |

**Ví dụ:**
- GT1: ngày 2 còn nợ tồn 310 xu, nếu không có GT1 thì phạt 62, có GT1 thì phạt 31. Nợ tồn thành 341 thay vì 372.
- GT2: mở H1 có 3 manh mối, người chơi thấy 2 manh mối thay vì 1.

## 2. Tạo tác

Có **2 hệ**, mỗi hệ 3 tạo tác với 3 tier. Tạo tác mua ở Kho Tạo Tác (Kim Khuy) và có số ô trang bị giới hạn.

- **Hệ Cổ Phong** (gợi nhóm Quá khứ và Thần thoại).
- **Hệ Thiên Cơ** (gợi nhóm Hiện đại và Tương lai).

| Mã | Tên | Hệ | Tier | Giá mua | Hiệu ứng | Điều kiện kích hoạt | Hiệu lực |
| --- | --- | --- | --- | --- | --- | --- | --- |
| CP1 | Phiến khắc tre | Cổ Phong | 1 | 150 xu | +5% giá bán hiện vật nhóm Quá khứ. | Bán hiện vật nhóm Quá khứ. | Khi trang bị. |
| CP2 | Hũ đựng đồ | Cổ Phong | 2 | 400 xu | Giảm 10% phí giám định. | Giám định hiện vật. | Khi trang bị. |
| CP3 | Ấn triện đồng | Cổ Phong | 3 | 900 xu | Hiện vật nhóm Thần thoại bán thêm +10%. | Bán hiện vật nhóm Thần thoại. | Khi trang bị. |
| TC1 | Ốc vít dự phòng | Thiên Cơ | 1 | 120 xu | Giảm 10% phí phục chế. | Phục chế hiện vật. | Khi trang bị. |
| TC2 | Mắt máy soi | Thiên Cơ | 2 | 450 xu | Hiện vật nhóm Hiện đại hoặc Tương lai hiện thêm 1 dấu hiệu soi gần. | Soi hiện vật thuộc nhóm đó. | Khi trang bị. |
| TC3 | Lõi năng lượng | Thiên Cơ | 3 | 1.000 xu | Phạt thu nợ giảm từ 20% xuống 10%. | Mỗi lần thu nợ còn nợ tồn. | Khi trang bị. |

**Ô trang bị:** mặc định **3 ô**. Bí Khế BK1 có thể mở thêm ô thứ 4 (xem mục 5).

**Mốc cộng hưởng [ĐỀ XUẤT]:**
- **Mốc 2:** trang bị 2 tạo tác cùng hệ. Hiệu ứng:
  - Cổ Phong: giảm 10% phí giám định cho mọi hiện vật.
  - Thiên Cơ: giảm 10% phí phục chế cho mọi hiện vật.
- **Mốc 3:** trang bị 3 tạo tác cùng hệ. Mốc 3 thay thế Mốc 2, không cộng dồn.
  - Cổ Phong: mỗi hộp có thêm 1 manh mối niêm phong miễn phí.
  - Thiên Cơ: phạt thu nợ giảm xuống 10%. Cùng loại với GT1 và TC3 nên không cộng dồn.

**Ví dụ cộng hưởng:**
- Trang bị CP1 và CP2: đạt Mốc 2 Cổ Phong, mọi lần giám định giảm 10% phí.
- Trang bị CP1, CP2, CP3: đạt Mốc 3 Cổ Phong. Mốc 2 không còn áp dụng.

## 3. Combo

Combo kích hoạt khi trang bị **đủ các tạo tác trong danh sách**, không cần cùng tier. Mỗi combo có hiệu lực riêng và được ghi vào sổ tay.

| Mã | Tên | Điều kiện | Hiệu ứng | Hiệu lực |
| --- | --- | --- | --- | --- |
| CB1 | Bộ Trà Cổ (combo nhỏ) | CP1 và CP2 | Mỗi hiện vật nhóm Quá khứ bán trong ngày được cộng thêm 10% giá. | Trong ngày. Nếu đủ 2 món thì áp dụng ngay. |
| CB2 | Tuần Tra Giới (combo nhỏ) | TC1 và TC2 | Mỗi lần mở hộp có thêm 1 manh mối miễn phí. | Trong ván. |
| CB3 | Chợ Đa Giới (combo lớn) | CP3, TC3 và 1 gia truyền bất kỳ | Mỗi ván được giám định miễn phí 1 hiện vật. | Trong ván, dùng 1 lần. |

**Ví dụ combo:**
- CB1: ngày 3 trang bị CP1 và CP2, bán 2 Bình sứ (V01) nhóm Quá khứ với giá nền 300 xu. Mỗi món được cộng thêm 10%.
- CB3: khi trang bị CP3 và TC3, lần giám định đầu tiên trong ván được miễn phí (cổ vật gia truyền luôn có sẵn nên không cần kiểm tra thêm). Các lần sau tính phí bình thường.

## 4. Thứ tự áp dụng hiệu ứng

Khi có nhiều hiệu ứng trùng thời điểm, áp dụng theo thứ tự:
1. Gia truyền.
2. Mốc cộng hưởng.
3. Tạo tác riêng lẻ.
4. Combo.
5. Bí Khế.

Hiệu ứng giảm phạt đã nói ở trên không cộng dồn, nên mức tốt nhất sẽ được chọn.

## 5. Bí Khế mẫu

Bí Khế mở theo điều kiện đạt được **trong chính ván đang chơi**. Sổ tay hiển thị tiến độ và thông báo khi đạt.

| Mã | Tên | Điều kiện | Phần thưởng | Thời điểm hiệu lực |
| --- | --- | --- | --- | --- |
| BK1 | Lời Thề Bảy Ngày | Hoàn thành bất kỳ combo nhỏ (CB1 hoặc CB2) trước ngày 5. | Mở thêm ô trang bị thứ 4 đến hết ván. | Ngay khi đạt. |
| BK2 | Mắt Người Sành | Giám định thành công 2 hiện vật nhóm Thần thoại. | Giảm 50% phí giám định cho mọi hiện vật còn lại trong ván. | Ngay khi đạt. |

**Ví dụ:**
- BK1: ngày 3 hoàn thành CB1, BK1 đạt. Từ lúc đó người chơi có 4 ô trang bị đến hết ván. Nếu hoàn thành CB1 sau ngày 5 thì BK1 không đạt.
- BK2: giám định 1 Mảnh vảy long (V10) và 1 Bình sứ (V01) thì chưa đạt. Giám định thêm 1 Lá bùa (V09) thì đạt, vì cả 2 món thuộc nhóm Thần thoại. Bình sứ không tính vì thuộc nhóm Quá khứ.

## 6. Ca kiểm thử

| Ca | Tình huống | Kết quả mong đợi |
| --- | --- | --- |
| 1 | Trang bị CP1, CP2 và TC1. | Đạt Mốc 2 Cổ Phong. CB1 kích hoạt. TC1 giữ hiệu ứng riêng là giảm phí phục chế. Không đạt mốc Thiên Cơ vì chỉ có 1 món. |
| 2 | Trang bị CP1, CP2, CP3. | Mốc 3 Cổ Phong. Mốc 2 không còn áp dụng. |
| 3 | Thu nợ còn nợ tồn khi có GT1 và TC3. | Phạt giảm còn 10%, không cộng dồn thành 5%. |
| 4 | Ô trang bị 3 món đã đầy, BK1 đạt. | Mở ô thứ 4 ngay. |
| 5 | Tải lại ván sau khi đạt BK2. | Trạng thái BK2 và giảm phí giám định được khôi phục. |
| 6 | Trang bị CP1, TC3 và CB3 nhưng không có CP3. | CB3 không kích hoạt vì thiếu CP3. TC3 vẫn có hiệu ứng riêng. |

## 7. Cần Claude 2 kiểm tra

- Mức giảm 10% và 20% có đủ hấp dẫn để người chơi muốn đầu tư không.
- Combo lớn CB3 có quá mạnh không, vì lần giám định miễn phí đầu tiên có thể đáng kể.
- Tần suất BK1 đạt trước ngày 5 là bao nhiêu trong các lượt chạy có seed.
- Phí giám định và phục chế ở mức nào để giảm 10% có ý nghĩa.
