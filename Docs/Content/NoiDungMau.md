# Nội dung mẫu — CHỢ ĐỒ LẠ

Danh mục hộp, hiện vật mẫu, manh mối niêm phong và hướng dẫn soi của Claude 1 cho Sprint 1. Bản này **đã cập nhật theo gói điều chỉnh được chủ dự án duyệt** (xem [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md)). Các con số vẫn là giá trị thử nghiệm và đã được Claude 2 đo ở [DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md). Luật nợ và thắng/thua xem tại [GDD mục 2](../GameDesign/README.md).

## 1. Danh mục hộp

| Mã | Tên hộp | Giá gốc | Nhóm nguồn gốc tiềm năng | Hàng giả | Manh mối đúng | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- |
| H1 | Hộp Gỗ Nhỏ | 1.000.000 đ | Quá khứ | 12% | 2 trên 3 | Hộp thường, dễ mua vào đầu ván. |
| H2 | Hộp Kim Loại Mờ | 3.500.000 đ | Hiện đại, Tương lai | 15% | 2 trên 3 | Hộp thường, có thể ra thiết bị hỏng. Manh mối đọc được nhiều nhất. |
| H3 | Hộp Vải Dày | 8.000.000 đ | Thần thoại, Cổ tích | 22% | 1 trên 3 | Hộp thường, rủi ro cao hơn, nhiều manh mối đánh lạc hướng. |
| RD | Rương Đen | 15.000.000 đ | Mọi nhóm | 30% | 1 trên 3 | Hộp đặc biệt. Xuất hiện từ ngày 4. Gần như hòa vốn, chỉ đáng mua để lấy hiện vật hiếm nhất. |

Mỗi lần mở hộp chỉ ra **1 hiện vật**. Tỷ lệ rơi nằm ở mục 3.

**Tỷ lệ hàng giả của hộp được hiển thị khi xem hộp.** Hộp càng đắt thì càng nhiều hàng giả. Đây là cái giá của việc dồn tiền vào hộp đắt, và người chơi tự cân nhắc dựa trên con số này.

## 2. Hiện vật mẫu

Mỗi hiện vật có ba thuộc tính:

| Thuộc tính | Giá trị | Ảnh hưởng đến giá bán |
| --- | --- | --- |
| **Chất lượng** | Nguyên (50%), Xước (35%), Hỏng (15%) | Nhân 1,0 / 0,7 / 0,3 |
| **Tính xác thực** | Thật, Giả. Người chơi chỉ biết chắc sau khi giám định. | Hàng giả bán chỉ 20% giá. Tỷ lệ giả theo hộp (mục 1), riêng V09 ít nhất 35%. |
| **Độ hiếm** | Thường, Hiếm, Rất hiếm | Chỉ ảnh hưởng biến cố E6 (xem [DichVuVaMeta.md](DichVuVaMeta.md)). |

Món chưa giám định **hiển thị giá như hàng thật**. Nếu là hàng giả, tiền thực nhận khi bán chỉ bằng 20%. Vì vậy phải giám định mới biết chắc món thật hay giả.

| Mã | Hiện vật | Nhóm nguồn gốc | Độ hiếm | Giá nền | Dấu hiệu khi soi |
| --- | --- | --- | --- | --- | --- |
| V01 | Bình sứ men xanh | Quá khứ | Thường | 2.300.000 đ | Vết rạn men gần miệng. Dấu triện dưới đáy. |
| V02 | Sách cổ bìa da | Quá khứ | Thường | 3.850.000 đ | Chữ mờ nhưng còn mục lục. Gáy sách có vết sửa. |
| V03 | Đồng hồ quả lắc cũ | Quá khứ | Thường | 4.950.000 đ | Kim bị gãy hoặc vẫn chạy. Mặt kính có vết mờ. |
| V04 | Mảnh kính đồng hồ thiên văn | Hiện đại | Thường | 4.150.000 đ | Khung kim loại in số. Mép kính có vết cắt thô. |
| V05 | Thiết bị liên lạc hỏng | Tương lai | Thường | 3.100.000 đ | Màn hình vỡ. Mã máy còn đọc được. Phục chế có thể đổi thành món đáng giá. |
| V06 | Bộ pin lượng tử | Tương lai | Hiếm | 18.450.000 đ | Nhãn in mờ. Linh kiện bạc còn sáng. |
| V07 | Tinh thể lam cầm tay | Ngoài hành tinh | Rất hiếm | 26.950.000 đ | Phát sáng yếu khi xoay. Bề mặt có vân. |
| V08 | Gương thủy tinh cổ tích | Cổ tích | Hiếm | 15.900.000 đ | Ảnh phản chiếu lệch góc so với người soi. |
| V09 | Lá bùa giấy vàng | Thần thoại | Thường | 8.950.000 đ | Mực đỏ dễ phai. Có thể là đồ giả. |
| V10 | Mảnh vảy long | Thần thoại | Rất hiếm | 28.600.000 đ | Vết cắt sắc. Ánh kim đổi màu theo góc nhìn. |

Giá nền là giá bán của hiện vật **thật, chất lượng Nguyên, chưa tính hiệu ứng nào**. Bảng giá này là bảng giá nền hệ số 0,85 của Claude 2 nhân thêm 1,10 để bù cho phạt rủi ro ở mục 1.

**Quy tắc khớp:** mỗi hiện vật có ít nhất một hộp có thể ra. Các hộp đều có ít nhất hai hiện vật khác nhau.

## 3. Tỷ lệ rơi và giá trị kỳ vọng

| Hộp | Hiện vật | Tỷ lệ |
| --- | --- | --- |
| H1 | V01 Bình sứ 40 · V02 Sách cổ 35 · V03 Đồng hồ 25 | 100 |
| H2 | V04 Mảnh kính 30 · V06 Bộ pin 30 · V05 Thiết bị hỏng 40 | 100 |
| H3 | V08 Gương 40 · V09 Lá bùa 40 · V10 Mảnh vảy 20 | 100 |
| RD | V07 Tinh thể 50 · V10 Mảnh vảy 30 · V08 Gương 20 | 100 |

**Giá trị kỳ vọng mỗi hộp** (đã tính chất lượng và hàng giả, theo bội số giá hộp, do Claude 2 đo):

| Hộp | Giá | Kỳ vọng |
| --- | --- | --- |
| H1 | 1.000.000 đ | 2,50 lần |
| H2 | 3.500.000 đ | 1,59 lần |
| H3 | 8.000.000 đ | 1,24 lần |
| RD | 15.000.000 đ | 1,01 lần |

Hộp càng đắt thì tỷ lệ lời càng thấp. Rương Đen gần như hòa vốn nên là canh bạc cần tính kỹ.

## 4. Giá bán, giám định và phục chế

**Giá bán thường (G)** của một hiện vật:

> G = giá nền × hệ số chất lượng × (0,2 nếu là hàng giả) × (1 + tổng hiệu ứng tăng giá bán)

Hiệu ứng tăng giá bán gồm tạo tác, combo, biến cố và chứng nhận giám định, cộng với nhau theo phép cộng (xem [GiaTruyenTaoTacBiKhe.md](GiaTruyenTaoTacBiKhe.md)).

**Làm tròn:** mọi giá bán **làm tròn xuống đến 10.000 đ**. Ví dụ V02 (giá nền 3.850.000 đ) chất lượng Xước: 3.850.000 × 0,7 = 2.695.000 nên bán 2.690.000 đ. Mô hình tham chiếu của Claude 2 dùng cách làm tròn này.

**Giá hiển thị và giá thực nhận.** Hai giá này khác nhau với hiện vật chưa giám định:
- **Giá hiển thị** (`ListedPrice`) tính như hàng **thật**, theo chất lượng đã thấy khi soi. Giá này **không bao giờ làm lộ hàng giả**.
- **Giá thực nhận** khi bán tính theo G thật. Nếu là hàng giả thì chỉ bằng 20%, và người chơi chỉ biết điều đó sau khi bán hoặc sau giám định.
- Sau giám định, món giả hiển thị giá thật (20%) và món thật hiển thị giá cộng 25% chứng nhận.

Nếu giao diện hiển thị trực tiếp giá thực nhận thì người chơi nhìn giá là biết hàng giả, và phạt rủi ro theo hộp (hộp đắt nhiều hàng giả hơn) mất tác dụng.

**Giám định (Giáo Sư Lẫm).**
- Phí **15% giá nền**, tối thiểu 200.000 đ.
- Cho biết món thật hay giả.
- Hàng **thật** đã giám định được **chứng nhận**: cộng **25%** vào giá bán. Hàng giả vẫn chỉ bán 20%.

**Phục chế (Chú Rỉ).**
- Phí **25% giá nền**, nâng chất lượng một bậc (Hỏng lên Xước, Xước lên Nguyên).
- Nên giám định trước khi phục chế, tránh trả phí cho hàng giả.

**Khi nào đáng giám định.** Phí cố định theo giá nền, còn lợi từ chứng nhận phụ thuộc chất lượng và khả năng là hàng thật:

| Chất lượng | Lợi kỳ vọng sau phí (so với giá nền) | Nhận xét |
| --- | --- | --- |
| Nguyên | Khoảng +2% đến +7% | Đáng giám định, nhất là với hộp ít hàng giả. |
| Xước | Khoảng −3% đến 0% | Hòa vốn hoặc lỗ nhẹ. Cần cân nhắc. |
| Hỏng | Khoảng −8% đến −10% | Luôn lỗ. |

Ví dụ V09 Lá bùa (giá nền 8.950.000 đ, Nguyên): phí 1.340.000 đ. Nếu là hàng thật thì giá bán tăng thêm 2.240.000 đ. Nếu là hàng giả thì mất phí 1.340.000 đ vô ích. Người chơi phải đọc chất lượng và tỷ lệ giả của hộp để quyết định.

## 5. Manh mối niêm phong và kệ hộp

### 5.1 Manh mối gắn với hiện vật bên trong

Mỗi hộp là một vật thể riêng. Hiện vật, chất lượng và tính xác thực bên trong được định sẵn khi bày hàng (theo seed). Mỗi hộp có **3 manh mối**, mỗi manh mối gợi một **nhóm nguồn gốc**:

- Manh mối **đúng** gợi nhóm nguồn gốc thật của hiện vật bên trong. H1 và H2 có **2 đúng, 1 sai**. H3 và Rương Đen chỉ có **1 đúng, 2 sai**, vì hộp đắt hay bị đánh lạc hướng.
- Manh mối **sai** gợi một nhóm khác, chọn ngẫu nhiên đều trong 5 nhóm còn lại.
- Thứ tự ba manh mối được xáo ngẫu nhiên. Người chơi không biết cái nào đúng.
- Manh mối chỉ cho biết **nhóm nguồn gốc**, không cho biết chất lượng hay hàng giả.
- H1 chỉ ra hiện vật Quá khứ nên manh mối không phân biệt được món nào, chỉ tạo bầu không khí.

**Từ điển manh mối theo nhóm:**

| Nhóm nguồn gốc | Manh mối |
| --- | --- |
| Quá khứ | Sáp đỏ đã sậm màu · Tem giấy in chữ mờ |
| Hiện đại | Mã vạch bong một góc · Hộp rỗng có tiếng kêu kim loại |
| Tương lai | Tem điện tử nhấp nháy yếu · Vỏ hộp ấm khi chạm |
| Ngoài hành tinh | Ánh sáng lam lọt qua khe · Có tiếng vọng nhiều lớp |
| Cổ tích | Dây buộc có mùi thuốc bắc · Lớp vải có hơi ẩm |
| Thần thoại | Chỉ thêu vàng · Ký hiệu khắc nghiêng · Dấu móng vuốt nhỏ |

### 5.2 Kệ hộp tại Quầy Niêm Phong

- Mỗi **lượt mua** (tối đa 3 lượt mỗi ngày), Dì Mận bày **1 hộp mỗi loại**: 3 loại, thêm Rương Đen từ ngày 4.
- Người chơi chọn mua **1 hộp** trong lượt, hoặc **dừng mua** trong ngày. Không đổi hộp trong cùng lượt. Hộp bị bỏ qua không quay lại, lượt sau bày hộp mới.
- Hộp của mỗi lượt xác định theo (seed, ngày, lượt, loại hộp), nên tải lại không đổi hộp.
- Mỗi hộp hiển thị: loại, tỷ lệ hàng giả của loại, giá niêm yết và **1 manh mối miễn phí**. CB2 và Mốc 3 Cổ Phong thêm manh mối miễn phí, tối đa 3 manh mối mỗi hộp.
- Giá mỗi loại hộp là **giá hôm nay**, dao động theo ngày (mục 5.3).
- **Lời mách của Dì Mận:** trả **150.000 đ** để biết **nhóm nguồn gốc thật** của hiện vật trong hộp đó. Lời mách **chắc đúng**, khác với manh mối miễn phí có thể sai. Mỗi hộp mua được một lần. Lời mách không cho biết chất lượng hay hàng giả. Giá niêm yết **không đổi** khi mua lời mách.

### 5.3 Giá hộp mỗi ngày dao động

Giá ghi ở bảng hộp (mục 1) là **giá gốc**. Mỗi ngày, giá **hôm nay** của từng loại hộp lệch khỏi giá gốc ngẫu nhiên đều trong khoảng **cộng trừ 20%**:

> giá hôm nay = giá gốc × (1 + u), u ngẫu nhiên đều trong khoảng từ −0,2 đến +0,2, làm tròn đến 10.000 đ gần nhất

- Giá hôm nay xác định theo (seed, ngày, loại hộp) nên tải lại không đổi. Mỗi loại hộp có độ lệch riêng, độc lập nhau. Áp dụng từ ngày 1.
- **Công bố đầu ngày, trước khi mua hộp**, như biến cố. Giao diện hiển thị bảng giá hôm nay của các loại hộp (Rương Đen từ ngày 4), và **không** hiển thị giá trị kỳ vọng vì người chơi tự tính.
- Biến cố E3 giảm thêm 20% giá H2, tính **sau** dao động: giá H2 hôm nay = giá gốc × (1 + u) × 0,8.
- Giá hôm nay chưa phải giá niêm yết cuối cùng: mỗi hộp còn được điều chỉnh theo manh mối (mục 5.4).

**Vì sao có luật này.** Lời tuyệt đối của một hộp là giá trị kỳ vọng trừ giá hộp, nên giá dao động làm loại hộp tốt nhất đổi theo ngày. Ví dụ H3 có giá trị kỳ vọng khoảng 9.900.000 đ: giá hôm nay 6.400.000 đ thì lời khoảng 3.500.000 đ, giá 9.600.000 đ thì lời khoảng 300.000 đ. Rương Đen có giá trị kỳ vọng khoảng 15.150.000 đ: giá 12.000.000 đ thì lời khoảng 3.150.000 đ, giá 18.000.000 đ thì lỗ khoảng 2.850.000 đ. Người chơi phải tính lại mỗi ngày thay vì có một đáp án cố định, và Rương Đen có những ngày đáng mua.

**Số liệu đo ban đầu** ([QA/box_price_swing.py](../../QA/box_price_swing.py), Claude 2 sẽ đo lại): với biên độ 20%, người chơi tối ưu thắng 40% / 46% / 46% / 47% (thận trọng, liều, combo nhỏ, combo lớn), bot thường 20% / 16%. Biên độ 30% làm game dễ hơn. Xem [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md) mục K.

### 5.4 Giá niêm yết đi theo manh mối

Dì Mận cũng đọc manh mối. Giá niêm yết của hộp là giá hôm nay nhân một hệ số theo **manh mối miễn phí đầu tiên**, kể cả khi manh mối đó là sai:

> giá niêm yết = giá hôm nay × hệ số, làm tròn đến 10.000 đ gần nhất
>
> hệ số = 1 + 0,75 × (giá trị kỳ vọng sau manh mối ÷ giá trị kỳ vọng trước manh mối − 1)

Bảng hệ số, theo độ tin cậy manh mối của từng loại hộp (nhóm của manh mối miễn phí đầu tiên):

| Hộp | Quá khứ | Hiện đại | Tương lai | Ngoài hành tinh | Cổ tích | Thần thoại |
| --- | --- | --- | --- | --- | --- | --- |
| H1 | 1,000 | 1,000 | 1,000 | 1,000 | 1,000 | 1,000 |
| H2 | 1,000 | 0,736 | 1,134 | 1,000 | 1,000 | 1,000 |
| H3 | 1,000 | 1,000 | 1,000 | 1,000 | 1,013 | 0,989 |
| RD | 1,000 | 1,000 | 1,000 | 1,022 | 0,936 | 1,031 |

- H1 không đổi giá vì manh mối không phân biệt được hiện vật.
- **H2 là hộp đọc manh mối rõ nhất.** Ở giá gốc 3.500.000 đ, nhóm Hiện đại chỉ ra V04 giá thấp (khoảng 2.580.000 đ), nhóm Tương lai chỉ ra V05 hoặc V06 (khoảng 3.970.000 đ).
- H3 và Rương Đen gần như không đổi giá vì chỉ 1 trên 3 manh mối đúng. Hai hộp đắt này là canh bạc, ít thông tin.
- Giá niêm yết là một thông tin: nếu thấp hơn giá hôm nay thì Dì Mận cho rằng hộp xấu. Người chơi tự cân xem có tin Dì Mận hay không.

**Số liệu đo (cận trên, bot đọc manh mối tối ưu):** thắng 34% / 38% / 44% (thận trọng cộng giám định, liều cộng giám định, combo lớn cộng giám định), so với 22% / 23% / 38% khi không đọc manh mối. Xem [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md) mục J.

## 6. Hướng dẫn soi

Đây là vòng người chơi làm với một hộp, theo đúng [GDD mục 2](../GameDesign/README.md).

1. **Xem kệ và mua.** Mỗi lượt mua, xem các hộp Dì Mận bày: đọc manh mối miễn phí, giá niêm yết và tỷ lệ hàng giả, có thể trả 150.000 đ hỏi lời mách của Dì Mận để biết chắc nhóm nguồn gốc thật. Chọn 1 hộp hoặc dừng mua. Sau khi mua, hộp nằm trên bàn mở và người chơi mở niêm phong.
2. **Nhấc và xoay.** Hiện vật được xoay theo ba trục. Người chơi cần xem ít nhất ba góc để soi xong.
3. **Soi gần.** Di chuyển gần để thấy vết nứt, dấu triện, linh kiện và chữ. Mỗi hiện vật có ít nhất một dấu hiệu chỉ thấy khi soi gần.
4. **Đọc thông tin.** Chất lượng và Độ hiếm đã biết. Tính xác thực chưa chắc chắn cho đến khi giám định.
5. **Quyết định.** Bán ngay, giữ trong kho, giám định, phục chế, cầm cố, đưa đấu giá hoặc dành tiền cho lượt sau. Giá bán tính theo mục 4.

**Nguyên tắc UI:** thông tin đã thấy không mất khi xoay. Dấu hiệu chỉ thấy khi soi gần phải có ghi chú "Soi gần để xem" để người chơi biết còn thông tin.

**Kho và bàn mở:** hộp chưa mở nằm ở bàn mở (tối đa 3 hộp), hiện vật đã mở nằm trong kho 8 ô. Luật chi tiết ở [GDD mục 3](../GameDesign/README.md).

## 7. Kiểm tra nội dung

- Dữ liệu đủ để dựng vòng mua → mở → soi → bán: có 4 loại hộp, 10 hiện vật, từ điển 13 manh mối theo nhóm, luật kệ hộp, hướng dẫn soi 6 bước.
- Mọi hiện vật có nguồn trong ít nhất một hộp (kiểm tra mục 3).
- Mỗi nhóm nguồn gốc có ít nhất một hộp và một hiện vật.
- Mỗi hiện vật có độ hiếm. Chỉ độ hiếm Rất hiếm (V07, V10) ảnh hưởng cơ chế, qua biến cố E6.
- Mỗi nhóm nguồn gốc có ít nhất 2 manh mối trong từ điển. Hộp H3 và Rương Đen có nhiều manh mối sai hơn để thử kỹ năng đọc.
- Hộp càng đắt thì kỳ vọng càng thấp (2,50 > 1,59 > 1,24 > 1,01).
