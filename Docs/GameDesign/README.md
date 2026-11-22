# Game Design — CHỢ ĐỒ LẠ

## Trạng thái thiết kế

- **[ĐÃ CHỐT]** Quyết định nền tảng được xác nhận trong yêu cầu hoặc cuộc thảo luận.
- **[ĐỀ XUẤT]** Cách thể hiện, phạm vi chơi thử hoặc giá trị ban đầu để kiểm nghiệm.
- **[MỞ]** Cần người phụ trách thiết kế trình phương án để chủ dự án duyệt.

## 1. Nhân vật và thế giới

**[ĐÃ CHỐT]** Người chơi là người mua và săn đồ. Bối cảnh là khu chợ giao thương cổ vật và hàng hóa đa vũ trụ, từ quá khứ, hiện đại, tương lai đến ngoài hành tinh, thần thoại, cổ tích và các thực tại khác. Một ván xoay quanh áp lực tiền mặt của 7 ngày nợ giang hồ và những cơ hội bất ngờ từ hộp mù.
**[ĐÃ CHỐT]** Unity 6.3 LTS (6000.3.x), URP, Blender và 3D First-Person. **[ĐỀ XUẤT]** Bản đầu chơi đơn trên Windows PC, dùng C# và Unity Input System. Khu chợ stylized có ánh đèn ấm, quầy sát nhau và đường đi ngắn. Vật phẩm được ưu tiên chi tiết ở khoảng cách gần để đọc vết nứt, dấu ấn và linh kiện.

## 2. Một ngày và một ván

1. Mở sổ ở phòng trọ để xem ngày, tiền mặt, khoản đến hạn và biến cố.
2. Đến Quầy Niêm Phong, đọc manh mối trên hộp và cân nhắc ngân sách. **[ĐÃ CHỐT]** Tối đa 3 hộp mua trong mỗi ngày. Lượt mua của ngày tiếp theo bắt đầu khi sang ngày.
3. Đặt hộp lên bàn, tháo niêm phong, mở nắp, nhấc và xoay hiện vật để xem thông tin. **[ĐỀ XUẤT]** Có mở nhanh cho thao tác lặp và hiển thị chỉ số cơ bản ngay khi soi. Manh mối bổ sung có thể dẫn tới giám định.
4. Quyết định bán, giữ, giám định, phục chế hoặc dành tiền mua tạo tác. Khi cân nhắc combo, đối chiếu tiền mặt với khoản đến hạn.
5. Kết thúc ngày ở phòng trọ. **[ĐÃ CHỐT]** Anh Bảy thu khoản đến hạn vào cuối từng ngày trong 7 ngày. Khoản tương lai chưa thể thanh toán. **[ĐỀ XUẤT]** Sau kỳ cuối, hiển thị kết quả và tổng tài sản.

**[MỞ]** Hậu quả khi thiếu tiền, số kỳ được phép trễ, xử lý nợ tồn và tiêu chí thua cụ thể. Thắng và thua phải có điều kiện rõ trước khi nghiệm thu bản chơi đủ 7 ngày.

## 3. Hộp mù, hiện vật và định giá

**[ĐÃ CHỐT]** Hàng hóa có chất lượng, tính xác thực và độ hiếm. **[ĐỀ XUẤT]** Chất lượng thể hiện qua độ nguyên vẹn và độ chuẩn đẹp. Tính xác thực phản ánh nguồn gốc thật, giả hoặc mức độ đã xác minh. Độ hiếm là mức phổ biến của món trong nhóm hàng phù hợp. Một món hiếm vẫn có thể bị hỏng hoặc cần kiểm chứng.
Hộp có giá và nhóm hàng tiềm năng. Hình dáng, tem niêm phong hoặc lời người bán cung cấp manh mối về nguồn hàng. Mẫu vật có giá nền và dấu hiệu nhận diện. Hiện vật thu được có tình trạng và kết quả xác thực riêng. Người chơi cân nhắc giá bán hiện tại, phí giám định hoặc phục chế và lợi ích giữ vật phẩm. **[MỞ]** Bảng rơi, công thức định giá, sức chứa kho, mức thông tin được công bố trước khi mua và tác động của phục chế.

## 4. Gia truyền, tạo tác và Bí Khế

- **[ĐÃ CHỐT] Cổ vật gia truyền.** Có từ đầu ván và đem lại nội tại bị động. **[MỞ]** Cách chọn hoặc cấp món đầu ván và danh sách nội tại.
- **[ĐÃ CHỐT] Tạo tác.** Có tier và hệ. Các món đáp ứng luật cộng hưởng tạo combo, giúp mỗi ván có hướng đầu tư riêng. **[MỞ]** Số ô trang bị, các mốc cộng hưởng, quy tắc cộng dồn, thay món và danh sách hiệu ứng.
- **[ĐÃ CHỐT] Bí Khế.** Mở khóa theo điều kiện đạt được trong chính ván đang chơi. **[ĐỀ XUẤT]** Sổ tay hiển thị tiến độ điều kiện và thông báo khi đạt. Ví dụ để thử là hoàn thành một combo hoặc giám định hiện vật thuộc nhóm nguồn gốc yêu cầu. **[MỞ]** Điều kiện, phần thưởng và thời điểm hiệu lực của từng Bí Khế.

## 5. NPC, biến cố và dịch vụ chợ

| NPC hoặc dịch vụ | Vai trò trong vòng chơi |
| --- | --- |
| Dì Mận · Quầy Niêm Phong | Bán hộp và cung cấp manh mối nguồn hàng. |
| Kim Khuy · Kho Tạo Tác | Cho xem và mua tạo tác phục vụ combo. |
| Anh Bảy · Thu nợ | Báo khoản phải trả, nhận tiền đúng kỳ và phản hồi kết quả. |
| Giáo Sư Lẫm · Giám định | Kiểm tra sâu và làm rõ dấu hiệu xác thực. |
| Chú Rỉ · Phục chế | Xử lý tình trạng hiện vật để đổi lấy phí. |
| Đấu giá | Kênh giao dịch cạnh tranh cho hiện vật. Cần chốt thời điểm nhận tiền và xử lý món chưa bán. |
| Sòng bạc | Lựa chọn rủi ro trong nền kinh tế xu của ván. Cần chốt luật cược, giới hạn và tác động đến khả năng trả nợ. |
| Cầm cố | Đổi hiện vật lấy thanh khoản. Cần chốt giá cầm, phí, hạn chuộc và điều kiện mất món. |

**[ĐÃ CHỐT]** NPC, biến cố, đấu giá, sòng bạc và cầm cố thuộc thiết kế. **[ĐỀ XUẤT]** Biến cố làm thay đổi nguồn cung, nhu cầu hoặc khả năng tiếp cận dịch vụ trong một khoảng thời gian và được báo qua sổ. Chốt điều kiện phát sinh, thời lượng và tác động trước khi đưa vào bảng cân bằng.

## 6. Sổ tay, lưu ván và meta progression

- **Sổ tay trong ván.** Theo dõi ngày, tiền, các kỳ nợ, lượt mua hộp, giao dịch, hiện vật đã xem, combo, Bí Khế và biến cố.
- **Lưu ván.** **[ĐỀ XUẤT]** Khi tải lại, khôi phục tiền, ngày, số lượt hộp, kho, kết quả hộp đã xác định, trạng thái nợ, trang bị và Bí Khế. Kiểm thử tình huống tải lại sau khi mở hộp, bán món hoặc trả nợ để tránh lặp giao dịch. **[MỞ]** Thời điểm lưu và chính sách khôi phục khi thoát giữa thao tác.
- **Meta progression.** **[ĐÃ CHỐT]** Có tiến trình giữa các ván. **[ĐỀ XUẤT]** Ghi nhận bộ sưu tập và mở thêm lựa chọn cho ván sau. **[MỞ]** Danh sách mở khóa, nguồn phần thưởng và mức ảnh hưởng đến kinh tế đầu ván.

## 7. Phạm vi chơi thử và cân bằng

**[ĐỀ XUẤT]** Bản thử gồm phòng trọ, một khu chợ nhỏ, bàn mở/soi, Dì Mận, Kim Khuy và Anh Bảy. Mục tiêu là chơi từ mua hộp đầu tiên đến kết quả ngày 7. Dự kiến 8–12 hiện vật, 2 nội tại gia truyền, 3 loại hộp thường và 1 loại đặc biệt, 6 tạo tác thuộc 2 hệ, 2 combo nhỏ và 1 combo lớn, cùng 2 điều kiện Bí Khế mẫu. Đấu giá, sòng bạc, cầm cố, biến cố phong phú và meta progression đầy đủ theo sau khi vòng chơi cốt lõi đạt nghiệm thu.

| Tham số thử nghiệm | Giá trị tham khảo |
| --- | --- |
| Vốn đầu ván | 1.000 xu |
| Nợ ngày 1–7 | 200 / 350 / 500 / 750 / 1.050 / 1.500 / 2.200 xu |
| Giá các hộp thường | 100 / 350 / 800 xu |

Các mức xu, giá bán, xác suất, quy mô danh mục và mục tiêu tỷ lệ thắng vẫn đang thử nghiệm. Claude 2 so sánh chiến thuật thận trọng, săn combo và liều lĩnh bằng các lượt chạy với seed tái lập. Ghi tỷ lệ thắng, ngày hụt tiền, tần suất hoàn thành combo và giả định trước khi đề xuất thay số. Giới hạn 3 hộp/ngày, thời hạn 7 ngày và quy tắc thanh toán đúng kỳ là luật cốt lõi.
Xem [checklist công việc](../../TASKS.md).
