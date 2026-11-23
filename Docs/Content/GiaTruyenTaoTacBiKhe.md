# Gia truyền, tạo tác, combo và Bí Khế — CHỢ ĐỒ LẠ

Nội dung Sprint 2 của Claude 1, **đã cập nhật theo gói điều chỉnh được chủ dự án duyệt** (xem [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md)). Các con số vẫn là giá trị thử nghiệm và đã được Claude 2 đo ở [DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md). Luật nền theo [GDD mục 4](../GameDesign/README.md). Giá bán, giám định và chứng nhận xem [NoiDungMau.md](NoiDungMau.md) mục 4.

**Quy tắc chung áp dụng cho mọi hiệu ứng:**
- **Giảm phí giám định, giảm phí phục chế:** không cộng dồn, chỉ lấy mức tốt nhất.
- **Tăng giá bán:** cộng dồn theo phép cộng, gồm tạo tác, combo, biến cố và chứng nhận giám định. Ví dụ +5% và +10% thành +15%.
- **Manh mối miễn phí thêm:** cộng dồn nhưng mỗi hộp chỉ có tối đa 3 manh mối.
- "Trong ván" nghĩa là từ lúc đạt điều kiện đến hết ván. "Trong ngày" nghĩa là đến khi kết thúc ngày.
- Mọi hiệu ứng đều được ghi vào sổ tay kèm điều kiện và trạng thái.

## 1. Cổ vật gia truyền

Mỗi ván bắt đầu với **1 cổ vật gia truyền**, rút ngẫu nhiên từ 2 món mẫu (người chơi đã mở khóa M1 thì được chọn, xem [DichVuVaMeta.md](DichVuVaMeta.md)). Nội tại có hiệu lực trong suốt ván.

| Mã | Tên | Nội tại | Điều kiện kích hoạt | Hiệu lực |
| --- | --- | --- | --- | --- |
| GT1 | Sổ Ghi Nợ Cũ | **Hoãn nợ:** một lần mỗi ván, hoãn kỳ nợ của ngày đó sang ngày sau. | Người chơi chọn "Hoãn" khi thu nợ. | Một lần mỗi ván. |
| GT2 | Đèn Dầu Thờ | Khi xem hộp H1 (Hộp Gỗ Nhỏ) trước khi mua, thấy thêm 1 manh mối niêm phong miễn phí. | Xem hộp H1. | Trong ván. |

**Luật hoãn nợ của GT1:**
1. Kỳ hoãn thành khoản **kỳ × 1,15** (làm tròn lên), trả cùng kỳ của ngày kế tiếp.
2. Kỳ hoãn **không bị phạt 20%** và **không tính vào ngưỡng thua 1.000 xu**.
3. Chỉ hoãn **kỳ của ngày đó**, không hoãn nợ tồn. **Không hoãn được kỳ ngày 7.**
4. Nếu ngày kế tiếp không đủ tiền trả, phần thiếu thành nợ tồn như luật nợ thông thường.

**Ví dụ:**
- GT1: ngày 4 người chơi hoãn kỳ 750 xu. Cuối ngày 4 Anh Bảy không thu. Ngày 5 phải trả 1.050 cộng 863 (750 cộng 15%, làm tròn lên) là 1.913 xu, cộng nợ tồn nếu có.
- GT2: xem H1 có 3 manh mối, người chơi thấy 2 manh mối thay vì 1.

**Đánh đổi của GT1:** trả thêm 15% để có vốn sớm. Dùng sớm để đầu tư thường hợp hơn dùng khi đã thiếu tiền, vì hoãn lúc thiếu tiền chỉ đẩy khoản nợ lớn hơn sang ngày sau.

## 2. Tạo tác

Có **2 hệ**, mỗi hệ 3 tạo tác với 3 tier. Tạo tác mua ở Kho Tạo Tác (Kim Khuy), có số ô trang bị giới hạn và **không chiếm kho hiện vật**.

- **Hệ Cổ Phong** (gợi nhóm Quá khứ và Thần thoại).
- **Hệ Thiên Cơ** (gợi nhóm Hiện đại và Tương lai).

| Mã | Tên | Hệ | Tier | Giá mua | Hiệu ứng | Điều kiện kích hoạt | Hiệu lực |
| --- | --- | --- | --- | --- | --- | --- | --- |
| CP1 | Phiến khắc tre | Cổ Phong | 1 | 150 xu | **+5% giá bán mọi hiện vật.** | Bán hiện vật. | Khi trang bị. |
| CP2 | Hũ đựng đồ | Cổ Phong | 2 | 400 xu | Giảm 10% phí giám định. | Giám định hiện vật. | Khi trang bị. |
| CP3 | Ấn triện đồng | Cổ Phong | 3 | **500 xu** | Hiện vật nhóm Thần thoại bán thêm +10%. | Bán hiện vật nhóm Thần thoại. | Khi trang bị. |
| TC1 | Ốc vít dự phòng | Thiên Cơ | 1 | 120 xu | Giảm 10% phí phục chế. | Phục chế hiện vật. | Khi trang bị. |
| TC2 | Mắt máy soi | Thiên Cơ | 2 | 450 xu | Hiện vật nhóm Hiện đại hoặc Tương lai hiện thêm 1 dấu hiệu soi gần. | Soi hiện vật thuộc nhóm đó. | Khi trang bị. |
| TC3 | Lõi năng lượng | Thiên Cơ | 3 | **500 xu** | **+10% giá bán hiện vật nhóm Hiện đại, Tương lai và Ngoài hành tinh.** | Bán hiện vật thuộc các nhóm đó. | Khi trang bị. |

**Ô trang bị:** mặc định **3 ô**. Bí Khế BK1 có thể mở thêm ô thứ 4 (xem mục 5).

**Mốc cộng hưởng:**
- **Mốc 2:** trang bị 2 tạo tác cùng hệ. Hiệu ứng:
  - Cổ Phong: +5% giá bán mọi hiện vật nhóm Quá khứ và Thần thoại.
  - Thiên Cơ: +5% giá bán mọi hiện vật nhóm Hiện đại và Tương lai.
- **Mốc 3:** trang bị 3 tạo tác cùng hệ. Mốc 3 thay thế Mốc 2, không cộng dồn.
  - Cổ Phong: mỗi hộp xem trước khi mua có thêm 1 manh mối niêm phong miễn phí.
  - Thiên Cơ: +10% giá bán hiện vật đã phục chế.

**Ví dụ cộng hưởng:**
- Trang bị CP1 và CP2: đạt Mốc 2 Cổ Phong. Hiện vật Quá khứ và Thần thoại được +5% từ Mốc 2. Cộng với CP1 +5% cho mọi hiện vật (xem thêm CB1 ở mục 3).
- Trang bị CP1, CP2, CP3: đạt Mốc 3 Cổ Phong. Mốc 2 không còn áp dụng.

## 3. Combo

Combo kích hoạt khi trang bị **đủ các tạo tác trong danh sách**, không cần cùng tier. Mỗi combo có hiệu lực riêng và được ghi vào sổ tay.

| Mã | Tên | Điều kiện | Hiệu ứng | Hiệu lực |
| --- | --- | --- | --- | --- |
| CB1 | Bộ Trà Cổ (combo nhỏ) | CP1 và CP2 | **Mọi hiện vật bán trong ngày được cộng thêm 10% giá.** | Trong ngày bán, khi đang đủ 2 món. |
| CB2 | Tuần Tra Giới (combo nhỏ) | TC1 và TC2 | Mỗi hộp xem trước khi mua có thêm 1 manh mối miễn phí. | Trong ván. |
| CB3 | Chợ Đa Giới (combo lớn) | CP3, TC3 và gia truyền (luôn có sẵn) | **Giám định miễn phí không giới hạn và mọi hiện vật bán thêm +15% giá.** | Trong ván. |

**Ví dụ combo:**
- CB1: ngày 3 trang bị CP1 và CP2, bán Bình sứ (V01, thật, Nguyên, giá nền 230 xu). Cộng CP1 +5%, CB1 +10% và Mốc 2 Cổ Phong +5% (nhóm Quá khứ) là +20%, nên bán được 276 xu thay vì 230. Với hiện vật nhóm Hiện đại như V04 (giá nền 415 xu) chỉ được CP1 và CB1, tức +15%, bán 477 xu.
- CB3: khi trang bị CP3 và TC3, mọi lần giám định miễn phí. Giám định V10 (thật, Nguyên, giá nền 2.860 xu) được chứng nhận +25%, CP3 +10% cho Thần thoại, CB3 +15%, tổng +50% và bán được 4.290 xu.

## 4. Thứ tự áp dụng hiệu ứng

Khi có nhiều hiệu ứng trùng thời điểm, áp dụng theo thứ tự:
1. Gia truyền.
2. Mốc cộng hưởng.
3. Tạo tác riêng lẻ.
4. Combo.
5. Bí Khế.

Hiệu ứng giảm phí không cộng dồn, nên mức tốt nhất sẽ được chọn. Hiệu ứng tăng giá bán cộng với nhau.

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
| 3 | Có GT1. Ngày 4 hoãn kỳ 750 xu. | Ngày 5 phải trả 1.050 cộng 863. Không bị phạt 20% cho phần hoãn. |
| 4 | Có GT1. Hoãn lần thứ hai trong cùng ván. | Từ chối. |
| 5 | Có GT1. Hoãn kỳ ngày 7. | Từ chối. |
| 6 | Có GT1. Hoãn một khoản nợ tồn. | Từ chối. Chỉ hoãn được kỳ của ngày đó. |
| 7 | Ô trang bị 3 món đã đầy, BK1 đạt. | Mở ô thứ 4 ngay. |
| 8 | Tải lại ván sau khi đạt BK2. | Trạng thái BK2 và giảm phí giám định được khôi phục. |
| 9 | Trang bị CP1, TC3 và CB3 nhưng không có CP3. | CB3 không kích hoạt vì thiếu CP3. TC3 vẫn có hiệu ứng riêng. |
| 10 | Trang bị CP3 và TC3, giám định 2 hiện vật trong cùng ngày. | Cả hai lần đều miễn phí. Mọi hiện vật bán thêm +15%. |
| 11 | Trang bị CP1 và CP2, bán V01 (Quá khứ, giá nền 230) và V04 (Hiện đại, giá nền 415). | V01 được +20% (276 xu). V04 được +15% (477 xu). |

## 7. Kết quả đo của Claude 2

Gói này đã được Claude 2 đo ở [DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md) với bot thường thắng khoảng 15% đến 18%, bot đầu tư 26% đến 38%, combo nhỏ và combo lớn ngang nhau. Hai điểm cần theo dõi khi có bản chơi thật:
- Hiệu ứng thông tin (CB2, GT2, TC2, Mốc 3 Cổ Phong) không đo được bằng xu, cần thử người chơi.
- Tần suất đạt BK1 trước ngày 5 và tần suất người chơi chọn combo lớn thay vì combo nhỏ.
