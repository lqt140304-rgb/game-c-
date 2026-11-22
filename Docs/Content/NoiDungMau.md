# Nội dung mẫu — CHỢ ĐỒ LẠ

Tài liệu này là bản nháp của Claude 1 cho Sprint 1: danh mục hộp, hiện vật mẫu, manh mối niêm phong và hướng dẫn soi. Các con số là **[ĐỀ XUẤT]** để thử nghiệm. Claude 2 sẽ cân bằng lại. Luật nợ và thắng/thua xem tại [GDD mục 2](../GameDesign/README.md).

## 1. Danh mục hộp

| Mã | Tên hộp | Giá | Nhóm nguồn gốc tiềm năng | Ghi chú |
| --- | --- | --- | --- | --- |
| H1 | Hộp Gỗ Nhỏ | 100 xu | Quá khứ | Hộp thường, dễ mua vào đầu ván. |
| H2 | Hộp Kim Loại Mờ | 350 xu | Hiện đại, Tương lai | Hộp thường, có thể ra thiết bị hỏng. |
| H3 | Hộp Vải Dày | 800 xu | Thần thoại, Cổ tích | Hộp thường, rủi ro cao hơn. |
| RD | Rương Đen | 1.500 xu | Mọi nhóm | Hộp đặc biệt. **[ĐỀ XUẤT]** Xuất hiện từ ngày 4. |

Mỗi lần mở hộp chỉ ra **1 hiện vật**. Tỷ lệ rơi nháp nằm ở mục 3.

## 2. Hiện vật mẫu

Mỗi hiện vật có ba thuộc tính. Giá trị hiện tại do Claude 2 định nghĩa, nên không nằm trong bảng này.

- **Chất lượng:** Nguyên, Xước, Hỏng.
- **Tính xác thực:** Thật, Chưa rõ, Giả. Người chơi có thể chưa biết khi mới soi. Giám định làm rõ.
- **Độ hiếm:** Thường, Hiếm, Rất hiếm.

| Mã | Hiện vật | Nhóm nguồn gốc | Giá nền | Dấu hiệu khi soi |
| --- | --- | --- | --- | --- |
| V01 | Bình sứ men xanh | Quá khứ | 300 xu | Vết rạn men gần miệng. Dấu triện dưới đáy. |
| V02 | Sách cổ bìa da | Quá khứ | 500 xu | Chữ mờ nhưng còn mục lục. Gáy sách có vết sửa. |
| V03 | Đồng hồ quả lắc cũ | Quá khứ | 650 xu | Kim bị gãy hoặc vẫn chạy. Mặt kính có vết mờ. |
| V04 | Mảnh kính đồng hồ thiên văn | Hiện đại | 200 xu | Khung kim loại in số. Mép kính có vết cắt thô. |
| V05 | Thiết bị liên lạc hỏng | Tương lai | 150 xu | Màn hình vỡ. Mã máy còn đọc được. Phục chế có thể đổi thành món đáng giá. |
| V06 | Bộ pin lượng tử | Tương lai | 900 xu | Nhãn in mờ. Linh kiện bạc còn sáng. |
| V07 | Tinh thể lam cầm tay | Ngoài hành tinh | 1.200 xu | Phát sáng yếu khi xoay. Bề mặt có vân. |
| V08 | Gương thủy tinh cổ tích | Cổ tích | 1.000 xu | Ảnh phản chiếu lệch góc so với người soi. |
| V09 | Lá bùa giấy vàng | Thần thoại | 400 xu | Mực đỏ dễ phai. Có thể là đồ giả. |
| V10 | Mảnh vảy long | Thần thoại | 1.800 xu | Vết cắt sắc. Ánh kim đổi màu theo góc nhìn. Độ hiếm cao. |

**Quy tắc khớp:** mỗi hiện vật có ít nhất một hộp thường có thể ra. Các hộp đều có ít nhất hai hiện vật khác nhau.

## 3. Tỷ lệ rơi nháp

| Hộp | Hiện vật | Tỷ lệ |
| --- | --- | --- |
| H1 | V01 Bình sứ 40 · V02 Sách cổ 35 · V03 Đồng hồ 25 | 100 |
| H2 | V04 Mảnh kính 30 · V06 Bộ pin 30 · V05 Thiết bị hỏng 40 | 100 |
| H3 | V08 Gương 40 · V09 Lá bùa 40 · V10 Mảnh vảy 20 | 100 |
| RD | V07 Tinh thể 50 · V10 Mảnh vảy 30 · V08 Gương 20 | 100 |

Tỷ lệ này chỉ để thử phân bố nội dung. Cần Claude 2 kiểm tra kinh tế trước khi chốt.

## 4. Manh mối niêm phong

Mỗi hộp có 3 manh mối trên niêm phong. Khi xem hộp **trước khi mua**, người chơi thấy **1 manh mối ngẫu nhiên miễn phí**. Hai manh mối còn lại mua thêm tại Quầy Niêm Phong của Dì Mận, **30 xu mỗi manh mối** **[ĐỀ XUẤT]**. Các hiệu ứng "thêm manh mối miễn phí" của gia truyền, tạo tác và combo cộng dồn, nhưng mỗi hộp tối đa 3 manh mối. Manh mối chỉ có giá trị khi người chơi nhìn thấy nó trước lúc quyết định mua.

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

## 5. Hướng dẫn soi

Đây là vòng người chơi làm với một hộp đã mở, theo đúng [GDD mục 2](../GameDesign/README.md).

1. **Xem hộp và mua.** Trước khi mua, đọc 1 manh mối miễn phí và có thể trả thêm để xem manh mối khác. Sau khi mua, đặt hộp lên bàn và mở niêm phong.
2. **Nhấc và xoay.** Hiện vật được xoay theo ba trục. Người chơi cần xem ít nhất ba góc để soi xong **[ĐỀ XUẤT]**.
3. **Soi gần.** Di chuyển gần để thấy vết nứt, dấu triện, linh kiện và chữ. Mỗi hiện vật có ít nhất một dấu hiệu chỉ thấy khi soi gần.
4. **Đọc ba dòng thông tin.** Chất lượng, Tính xác thực, Độ hiếm. Nếu Tính xác thực là "Chưa rõ" thì cần giám định.
5. **Quyết định.** Bán ngay, giữ, giám định, phục chế hoặc dành tiền cho lượt sau. Giá bán hiện tại do Claude 2 định nghĩa.

**Nguyên tắc UI:** thông tin đã thấy không mất khi xoay. Dấu hiệu chỉ thấy khi soi gần phải có ghi chú "Soi gần để xem" để người chơi biết còn thông tin.

## 6. Kiểm tra bản nháp

- Dữ liệu đủ để dựng vòng mua → mở → soi → bán: có 4 loại hộp, 10 hiện vật, 12 manh mối, hướng dẫn soi 5 bước.
- Mọi hiện vật có nguồn trong ít nhất một hộp (kiểm tra mục 3).
- Mỗi nhóm nguồn gốc có ít nhất một hộp và một hiện vật.
- Có manh mối gây hiểu nhầm để thử kỹ năng đọc. Tỷ lệ là bài toán cân bằng của Claude 2.
