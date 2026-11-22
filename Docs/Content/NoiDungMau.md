# Nội dung mẫu — CHỢ ĐỒ LẠ

Danh mục hộp, hiện vật mẫu, manh mối niêm phong và hướng dẫn soi của Claude 1 cho Sprint 1. Bản này **đã cập nhật theo gói điều chỉnh được chủ dự án duyệt** (xem [DieuChinhSauCanBang.md](DieuChinhSauCanBang.md)). Các con số vẫn là giá trị thử nghiệm và đã được Claude 2 đo ở [DoLaiDeXuat.md](../Balance/DoLaiDeXuat.md). Luật nợ và thắng/thua xem tại [GDD mục 2](../GameDesign/README.md).

## 1. Danh mục hộp

| Mã | Tên hộp | Giá | Nhóm nguồn gốc tiềm năng | Hàng giả | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| H1 | Hộp Gỗ Nhỏ | 100 xu | Quá khứ | 12% | Hộp thường, dễ mua vào đầu ván. |
| H2 | Hộp Kim Loại Mờ | 350 xu | Hiện đại, Tương lai | 15% | Hộp thường, có thể ra thiết bị hỏng. |
| H3 | Hộp Vải Dày | 800 xu | Thần thoại, Cổ tích | 22% | Hộp thường, rủi ro cao hơn. |
| RD | Rương Đen | 1.500 xu | Mọi nhóm | 30% | Hộp đặc biệt. Xuất hiện từ ngày 4. Gần như hòa vốn, chỉ đáng mua để lấy hiện vật hiếm nhất. |

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
| V01 | Bình sứ men xanh | Quá khứ | Thường | 230 xu | Vết rạn men gần miệng. Dấu triện dưới đáy. |
| V02 | Sách cổ bìa da | Quá khứ | Thường | 385 xu | Chữ mờ nhưng còn mục lục. Gáy sách có vết sửa. |
| V03 | Đồng hồ quả lắc cũ | Quá khứ | Thường | 495 xu | Kim bị gãy hoặc vẫn chạy. Mặt kính có vết mờ. |
| V04 | Mảnh kính đồng hồ thiên văn | Hiện đại | Thường | 415 xu | Khung kim loại in số. Mép kính có vết cắt thô. |
| V05 | Thiết bị liên lạc hỏng | Tương lai | Thường | 310 xu | Màn hình vỡ. Mã máy còn đọc được. Phục chế có thể đổi thành món đáng giá. |
| V06 | Bộ pin lượng tử | Tương lai | Hiếm | 1.845 xu | Nhãn in mờ. Linh kiện bạc còn sáng. |
| V07 | Tinh thể lam cầm tay | Ngoài hành tinh | Rất hiếm | 2.695 xu | Phát sáng yếu khi xoay. Bề mặt có vân. |
| V08 | Gương thủy tinh cổ tích | Cổ tích | Hiếm | 1.590 xu | Ảnh phản chiếu lệch góc so với người soi. |
| V09 | Lá bùa giấy vàng | Thần thoại | Thường | 895 xu | Mực đỏ dễ phai. Có thể là đồ giả. |
| V10 | Mảnh vảy long | Thần thoại | Rất hiếm | 2.860 xu | Vết cắt sắc. Ánh kim đổi màu theo góc nhìn. |

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
| H1 | 100 xu | 2,50 lần |
| H2 | 350 xu | 1,59 lần |
| H3 | 800 xu | 1,24 lần |
| RD | 1.500 xu | 1,01 lần |

Hộp càng đắt thì tỷ lệ lời càng thấp. Rương Đen gần như hòa vốn nên là canh bạc cần tính kỹ.

## 4. Giá bán, giám định và phục chế

**Giá bán thường (G)** của một hiện vật:

> G = giá nền × hệ số chất lượng × (0,2 nếu là hàng giả) × (1 + tổng hiệu ứng tăng giá bán)

Hiệu ứng tăng giá bán gồm tạo tác, combo, biến cố và chứng nhận giám định, cộng với nhau theo phép cộng (xem [GiaTruyenTaoTacBiKhe.md](GiaTruyenTaoTacBiKhe.md)).

**Giám định (Giáo Sư Lẫm).**
- Phí **15% giá nền**, tối thiểu 20 xu.
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

Ví dụ V09 Lá bùa (giá nền 895 xu, Nguyên): phí 134 xu. Nếu là hàng thật thì giá bán tăng thêm 224 xu. Nếu là hàng giả thì mất phí 134 xu vô ích. Người chơi phải đọc chất lượng và tỷ lệ giả của hộp để quyết định.

## 5. Manh mối niêm phong

Mỗi hộp có 3 manh mối trên niêm phong. Khi xem hộp **trước khi mua**, người chơi thấy **1 manh mối ngẫu nhiên miễn phí**. Hai manh mối còn lại mua thêm tại Quầy Niêm Phong của Dì Mận, **30 xu mỗi manh mối**. Các hiệu ứng "thêm manh mối miễn phí" của gia truyền, tạo tác và combo cộng dồn, nhưng mỗi hộp tối đa 3 manh mối. Manh mối chỉ có giá trị khi người chơi nhìn thấy nó trước lúc quyết định mua.

| Hộp | Manh mối | Gợi ý nhóm | Ghi chú |
| --- | --- | --- | --- |
| H1 | Sáp đỏ đã sậm màu | Quá khứ | Đúng. |
| H1 | Tem giấy in chữ mờ | Quá khứ | Đúng. |
| H1 | Dây buộc có mùi thuốc bắc | Cổ tích | Gây hiểu nhầm. Dùng để thử kỹ năng đọc manh mối. |
| H2 | Mã vạch bong một góc | Hiện đại | Đúng. |
| H2 | Tem điện tử nhấp nháy yếu | Tương lai | Đúng. |
| H2 | Hộp rỗng có tiếng kêu kim loại | Hiện đại | Gợi nhầm sang tiếng kêu của linh kiện. |
| H3 | Chỉ thêu vàng | Thần thoại | Đúng. |
| H3 | Lớp vải có hơi ẩm | Cổ tích | Đúng. |
| H3 | Dấu móng vuốt nhỏ | Thần thoại | Gợi nhầm sang Ngoài hành tinh. |
| RD | Ánh sáng lam lọt qua khe | Ngoài hành tinh | Đúng. |
| RD | Có tiếng vọng nhiều lớp | Mọi nhóm | Không gợi nhóm cụ thể. |
| RD | Ký hiệu khắc nghiêng | Thần thoại | Đúng. |

## 6. Hướng dẫn soi

Đây là vòng người chơi làm với một hộp, theo đúng [GDD mục 2](../GameDesign/README.md).

1. **Xem hộp và mua.** Trước khi mua, đọc 1 manh mối miễn phí, xem tỷ lệ hàng giả của hộp, và có thể trả thêm để xem manh mối khác. Sau khi mua, hộp nằm trên bàn mở và người chơi mở niêm phong.
2. **Nhấc và xoay.** Hiện vật được xoay theo ba trục. Người chơi cần xem ít nhất ba góc để soi xong.
3. **Soi gần.** Di chuyển gần để thấy vết nứt, dấu triện, linh kiện và chữ. Mỗi hiện vật có ít nhất một dấu hiệu chỉ thấy khi soi gần.
4. **Đọc thông tin.** Chất lượng và Độ hiếm đã biết. Tính xác thực chưa chắc chắn cho đến khi giám định.
5. **Quyết định.** Bán ngay, giữ trong kho, giám định, phục chế, cầm cố, đưa đấu giá hoặc dành tiền cho lượt sau. Giá bán tính theo mục 4.

**Nguyên tắc UI:** thông tin đã thấy không mất khi xoay. Dấu hiệu chỉ thấy khi soi gần phải có ghi chú "Soi gần để xem" để người chơi biết còn thông tin.

**Kho và bàn mở:** hộp chưa mở nằm ở bàn mở (tối đa 3 hộp), hiện vật đã mở nằm trong kho 8 ô. Luật chi tiết ở [GDD mục 3](../GameDesign/README.md).

## 7. Kiểm tra nội dung

- Dữ liệu đủ để dựng vòng mua → mở → soi → bán: có 4 loại hộp, 10 hiện vật, 12 manh mối, hướng dẫn soi 6 bước.
- Mọi hiện vật có nguồn trong ít nhất một hộp (kiểm tra mục 3).
- Mỗi nhóm nguồn gốc có ít nhất một hộp và một hiện vật.
- Mỗi hiện vật có độ hiếm. Chỉ độ hiếm Rất hiếm (V07, V10) ảnh hưởng cơ chế, qua biến cố E6.
- Có manh mối gây hiểu nhầm để thử kỹ năng đọc.
- Hộp càng đắt thì kỳ vọng càng thấp (2,50 > 1,59 > 1,24 > 1,01).
