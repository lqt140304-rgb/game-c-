# Game Design — CHỢ ĐỒ LẠ

## Trạng thái thiết kế

- **[ĐÃ CHỐT]** Quyết định nền tảng được xác nhận trong yêu cầu hoặc cuộc thảo luận.
- **[ĐỀ XUẤT]** Cách thể hiện, phạm vi chơi thử hoặc giá trị ban đầu để kiểm nghiệm.
- **[MỞ]** Cần người phụ trách thiết kế trình phương án để chủ dự án duyệt.

## 1. Nhân vật và thế giới

**[ĐÃ CHỐT]** Người chơi là người mua và săn đồ. Bối cảnh là khu chợ giao thương cổ vật và hàng hóa đa vũ trụ, từ quá khứ, hiện đại, tương lai đến ngoài hành tinh, thần thoại, cổ tích và các thực tại khác. Một ván xoay quanh áp lực tiền mặt của 7 ngày nợ giang hồ và những cơ hội bất ngờ từ hộp mù.
**[ĐÃ CHỐT]** Unity 6 (6000.6.9f1), URP, Blender và 3D First-Person. **[ĐỀ XUẤT]** Bản đầu chơi đơn trên Windows PC, dùng C# và Unity Input System. Khu chợ stylized có ánh đèn ấm, quầy sát nhau và đường đi ngắn. Vật phẩm được ưu tiên chi tiết ở khoảng cách gần để đọc vết nứt, dấu ấn và linh kiện.

## 2. Một ngày và một ván

1. Mở sổ ở phòng trọ để xem ngày, tiền mặt, khoản đến hạn và biến cố.
2. Đến Quầy Niêm Phong, đọc manh mối trên hộp và cân nhắc ngân sách. **[ĐÃ CHỐT]** Tối đa 3 hộp mua trong mỗi ngày. Lượt mua của ngày tiếp theo bắt đầu khi sang ngày.
3. Đặt hộp lên bàn, tháo niêm phong, mở nắp, nhấc và xoay hiện vật để xem thông tin. **[ĐỀ XUẤT]** Có mở nhanh cho thao tác lặp và hiển thị chỉ số cơ bản ngay khi soi. Manh mối bổ sung có thể dẫn tới giám định.
4. Quyết định bán, giữ, giám định, phục chế hoặc dành tiền mua tạo tác. Khi cân nhắc combo, đối chiếu tiền mặt với khoản đến hạn.
5. Kết thúc ngày ở phòng trọ. **[ĐÃ CHỐT]** Anh Bảy thu khoản đến hạn vào cuối từng ngày trong 7 ngày. Khoản tương lai chưa thể thanh toán. **[ĐỀ XUẤT]** Sau kỳ cuối, hiển thị kết quả và tổng tài sản.

**[ĐỀ XUẤT] Thiếu tiền khi đến hạn.** Cuối mỗi ngày, Anh Bảy thu đúng khoản của kỳ đó. Nếu tiền mặt không đủ:
- Người chơi trả bao nhiêu thì trả bấy nhiêu. Phần còn thiếu thành **nợ tồn**.
- Sau mỗi lần thu nợ, nếu còn nợ tồn thì cộng thêm **phạt 20%** của **toàn bộ nợ còn lại** (làm tròn lên). Tức là nợ tồn mới bằng nợ còn lại nhân 1,2. Nợ tồn là **một con số duy nhất**, không tách phần gốc và phần phạt. Tiền trả luôn trừ vào tổng nợ, nên phần phạt cũ cũng chịu phạt ở lần thu sau.
- Kỳ mới vẫn đến đúng hạn. Nợ tồn cộng dồn vào kỳ mới và phải trả cùng lúc.
- Không trả trước kỳ chưa đến hạn. Khoản đã thu không hoàn lại.
- **Ngoại lệ duy nhất:** gia truyền GT1 (Sổ Ghi Nợ Cũ) cho hoãn kỳ nợ của một ngày, một lần mỗi ván, theo luật riêng ở [Docs/Content/GiaTruyenTaoTacBiKhe.md](../Content/GiaTruyenTaoTacBiKhe.md). Kỳ hoãn không bị phạt 20%.
- Tài sản chưa đổi thành tiền (hiện vật, tạo tác) không được tính để trả nợ. Muốn dùng phải bán hoặc cầm cố trước khi thu nợ trong cùng ngày.

**[ĐỀ XUẤT] Thắng và thua.**
- **Thua ngay:** nợ tồn sau khi thu nợ của bất kỳ ngày nào vượt **10.000.000 đ** (ngưỡng thử nghiệm).
- **Thua cuối ngày 7:** sau khi thu nợ kỳ 7 còn nợ tồn lớn hơn 0.
- **Thắng:** sau khi thu nợ kỳ 7, nợ tồn bằng 0 và tất cả kỳ 1–7 đã thanh toán.

**[ĐỀ XUẤT] Ca kiểm thử luật nợ.**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 1 | Ngày 1 có 10.000.000 đ, trả 2.000.000 đ | Còn 8.000.000 đ, không có nợ tồn, tiếp tục. |
| 2 | Ngày 1 có 1.500.000 đ, trả 1.500.000 đ | Nợ tồn 500.000 đ, phạt 100.000 đ, nợ tồn còn 600.000 đ. |
| 3 | Ngày 2 phải trả 4.100.000 đ (kỳ 3.500.000 đ cộng nợ tồn 600.000 đ), có 1.000.000 đ | Trả 1.000.000 đ, còn 3.100.000 đ, phạt 620.000 đ, nợ tồn 3.720.000 đ, tiếp tục. |
| 4 | Cuối ngày bất kỳ, nợ tồn sau thu là 11.000.000 đ | Thua ngay. |
| 5 | Ngày 7 tổng phải trả 22.500.000 đ (kỳ 22.000.000 đ cộng nợ tồn 500.000 đ), người chơi chỉ có 22.000.000 đ | Còn nợ tồn 500.000 đ sau khi thu. Thua cuối ngày 7. |
| 6 | Ngày 7 trả đủ kỳ và không còn nợ tồn | Thắng. |
| 7 | Hiện vật trị giá 30.000.000 đ nhưng tiền mặt 0, chưa bán trước khi thu nợ | Không tính để trả. Ngày 1 thiếu cả 2.000.000 đ, nợ tồn thành 2.400.000 đ (2.000.000 đ cộng phạt 400.000 đ). |
| 8 | Ngày 2 phải trả 4.100.000 đ (kỳ 3.500.000 đ cộng nợ tồn 600.000 đ, trong đó 100.000 đ là phạt cũ), người chơi chỉ trả 50.000 đ | Còn 4.050.000 đ, phạt 810.000 đ, nợ tồn 4.860.000 đ. Phạt tính trên tổng còn lại, kể cả phần phạt cũ. |

Các con số phạt 20% và ngưỡng 10.000.000 đ là giá trị thử nghiệm. Claude 2 sẽ kiểm tra lại khi cân bằng.

## 3. Hộp mù, hiện vật và định giá

**[ĐÃ CHỐT]** Hàng hóa có chất lượng, tính xác thực và độ hiếm. **[ĐỀ XUẤT]** Chất lượng thể hiện qua độ nguyên vẹn và độ chuẩn đẹp. Tính xác thực phản ánh nguồn gốc thật, giả hoặc mức độ đã xác minh. Độ hiếm là mức phổ biến của món trong nhóm hàng phù hợp. Một món hiếm vẫn có thể bị hỏng hoặc cần kiểm chứng.
Hộp có giá và nhóm hàng tiềm năng. Hình dáng, tem niêm phong hoặc lời người bán cung cấp manh mối về nguồn hàng. Mẫu vật có giá nền và dấu hiệu nhận diện. Hiện vật thu được có tình trạng và kết quả xác thực riêng. Người chơi cân nhắc giá bán hiện tại, phí giám định hoặc phục chế và lợi ích giữ vật phẩm. **[ĐỀ XUẤT]** Bảng rơi, công thức định giá, giám định, phục chế và tỷ lệ hàng giả theo hộp nằm trong [Docs/Content/NoiDungMau.md](../Content/NoiDungMau.md). Tỷ lệ hàng giả của hộp được hiển thị khi xem hộp.

**[ĐỀ XUẤT] Kho và bàn mở.**
- **Bàn mở** chứa tối đa 3 hộp chưa mở. Hộp chưa mở không chiếm kho và giữ được qua ngày. Bàn đầy thì không mua thêm hộp.
- **Kho** có 8 ô cho hiện vật đã mở. Tạo tác không chiếm kho.
- **Kho đầy:** không mở được hộp mới cho đến khi bán, cầm hoặc đưa đấu giá bớt. Hộp vẫn ở lại bàn, không mất.
- Hiện vật đang cầm hoặc đang đấu giá không chiếm kho vì đang ở nơi khác. Mỗi món chỉ ở đúng một nơi.
- Hết ván, hiện vật còn lại không tính vào kết quả.

**[ĐỀ XUẤT] Kệ hộp và manh mối.** Chi tiết và bảng giá ở [Docs/Content/NoiDungMau.md](../Content/NoiDungMau.md) mục 5.
- Mỗi **lượt mua** (tối đa 3 lượt mỗi ngày), Dì Mận bày **1 hộp mỗi loại**. Người chơi chọn mua **1 hộp** hoặc **dừng mua** trong ngày. Không đổi hộp trong cùng lượt, hộp bị bỏ qua không quay lại.
- Mỗi hộp là vật thể riêng, hiện vật bên trong định sẵn theo seed. Hộp có **3 manh mối** gợi nhóm nguồn gốc: H1 và H2 có 2 đúng 1 sai, H3 và Rương Đen chỉ 1 đúng 2 sai.
- Người chơi thấy **1 manh mối miễn phí** mỗi hộp (có thể sai), và trả **150.000 đ** hỏi **lời mách của Dì Mận** để biết chắc nhóm nguồn gốc thật, một lần mỗi hộp.
- **Giá hộp dao động theo ngày:** giá gốc của mỗi loại hộp lệch ngẫu nhiên đều trong khoảng cộng trừ 20% mỗi ngày, công bố đầu ngày trước khi mua, nên loại hộp tốt nhất đổi theo ngày và người chơi phải tính lại mỗi ngày.
- **Giá niêm yết đi theo manh mối đầu tiên** (hệ số 0,75): Dì Mận cũng đọc manh mối nên hộp được cho là tốt bị nâng giá. Giá niêm yết bằng giá hôm nay nhân hệ số theo manh mối.
- Giao diện **không hiển thị giá trị kỳ vọng** của từng loại hộp. Người chơi tự tính từ giá niêm yết, tỷ lệ hàng giả của loại và manh mối.

## 4. Gia truyền, tạo tác và Bí Khế

- **[ĐÃ CHỐT] Cổ vật gia truyền.** Có từ đầu ván và đem lại nội tại bị động. **[ĐỀ XUẤT]** Mỗi ván rút ngẫu nhiên 1 trong 2 món mẫu, danh sách nội tại nằm trong [Docs/Content/GiaTruyenTaoTacBiKhe.md](../Content/GiaTruyenTaoTacBiKhe.md). Gia truyền GT1 cho hoãn một kỳ nợ, là ngoại lệ duy nhất của luật thiếu tiền ở mục 2.
- **[ĐÃ CHỐT] Tạo tác.** Có tier và hệ. Các món đáp ứng luật cộng hưởng tạo combo, giúp mỗi ván có hướng đầu tư riêng. **[ĐỀ XUẤT]** 3 ô trang bị (4 ô nếu đạt BK1), mốc cộng hưởng ở 2 và 3 món cùng hệ, quy tắc cộng dồn và danh sách hiệu ứng nằm trong [Docs/Content/GiaTruyenTaoTacBiKhe.md](../Content/GiaTruyenTaoTacBiKhe.md).
- **[ĐÃ CHỐT] Bí Khế.** Mở khóa theo điều kiện đạt được trong chính ván đang chơi. **[ĐỀ XUẤT]** Sổ tay hiển thị tiến độ điều kiện và thông báo khi đạt. Ví dụ để thử là hoàn thành một combo hoặc giám định hiện vật thuộc nhóm nguồn gốc yêu cầu. **[ĐỀ XUẤT]** Hai Bí Khế mẫu (BK1, BK2) với điều kiện, phần thưởng và thời điểm hiệu lực nằm trong [Docs/Content/GiaTruyenTaoTacBiKhe.md](../Content/GiaTruyenTaoTacBiKhe.md).

## 5. NPC, biến cố và dịch vụ chợ

| NPC hoặc dịch vụ | Vai trò trong vòng chơi |
| --- | --- |
| Dì Mận · Quầy Niêm Phong | Bán hộp và cung cấp manh mối nguồn hàng. |
| Kim Khuy · Kho Tạo Tác | Cho xem và mua tạo tác phục vụ combo. |
| Anh Bảy · Thu nợ | Báo khoản phải trả, nhận tiền đúng kỳ và phản hồi kết quả. |
| Giáo Sư Lẫm · Giám định | Kiểm tra sâu và làm rõ dấu hiệu xác thực. |
| Chú Rỉ · Phục chế | Xử lý tình trạng hiện vật để đổi lấy phí. |
| Đấu giá | Kênh giao dịch cạnh tranh cho hiện vật. Cần chốt thời điểm nhận tiền và xử lý món chưa bán. |
| Sòng bạc | Lựa chọn rủi ro trong nền kinh tế tiền của ván. Cần chốt luật cược, giới hạn và tác động đến khả năng trả nợ. |
| Cầm cố | Đổi hiện vật lấy thanh khoản. Cần chốt giá cầm, phí, hạn chuộc và điều kiện mất món. |

**[ĐÃ CHỐT]** NPC, biến cố, đấu giá, sòng bạc và cầm cố thuộc thiết kế. **[ĐỀ XUẤT]** Biến cố làm thay đổi nguồn cung, nhu cầu hoặc khả năng tiếp cận dịch vụ trong một khoảng thời gian và được báo qua sổ. Chốt điều kiện phát sinh, thời lượng và tác động trước khi đưa vào bảng cân bằng. Luật đề xuất cho biến cố, đấu giá, sòng bạc và cầm cố nằm trong [Docs/Content/DichVuVaMeta.md](../Content/DichVuVaMeta.md).

## 6. Sổ tay, lưu ván và meta progression

- **Sổ tay trong ván.** Theo dõi ngày, tiền, các kỳ nợ, lượt mua hộp, giao dịch, hiện vật đã xem, combo, Bí Khế và biến cố.
- **Lưu ván.** **[ĐỀ XUẤT]** Khi tải lại, khôi phục tiền, ngày, số lượt hộp, kho, kết quả hộp đã xác định, trạng thái nợ, trang bị và Bí Khế. Kiểm thử tình huống tải lại sau khi mở hộp, bán món hoặc trả nợ để tránh lặp giao dịch. **[ĐỀ XUẤT]** Chỉ lưu tự động, một ô duy nhất, không có lưu tay và không tải lại bản cũ trong cùng ván. Lưu vào đầu mỗi ngày và sau mỗi giao dịch tiền hoàn tất. Thoát giữa thao tác thì khôi phục về trạng thái ngay sau giao dịch hoàn tất gần nhất. Kết quả ngẫu nhiên xác định theo (seed, ngày, lượt) hoặc theo mã hiện vật, nên tải lại không đổi kết quả.
- **Meta progression.** **[ĐÃ CHỐT]** Có tiến trình giữa các ván. **[ĐỀ XUẤT]** Ghi nhận bộ sưu tập và mở thêm lựa chọn cho ván sau. Danh sách mở khóa, nguồn phần thưởng và mức ảnh hưởng đến kinh tế đầu ván được đề xuất trong [Docs/Content/DichVuVaMeta.md](../Content/DichVuVaMeta.md).

## 7. Phạm vi chơi thử và cân bằng

**[ĐỀ XUẤT]** Bản thử gồm phòng trọ, một khu chợ nhỏ, bàn mở/soi, Dì Mận, Kim Khuy và Anh Bảy. Mục tiêu là chơi từ mua hộp đầu tiên đến kết quả ngày 7. Dự kiến 8–12 hiện vật, 2 nội tại gia truyền, 3 loại hộp thường và 1 loại đặc biệt, 6 tạo tác thuộc 2 hệ, 2 combo nhỏ và 1 combo lớn, cùng 2 điều kiện Bí Khế mẫu. Đấu giá, sòng bạc, cầm cố, biến cố phong phú và meta progression đầy đủ theo sau khi vòng chơi cốt lõi đạt nghiệm thu.

| Tham số thử nghiệm | Giá trị tham khảo |
| --- | --- |
| Vốn đầu ván | 10.000.000 đ |
| Nợ ngày 1–7 | 2.000.000 / 3.500.000 / 5.000.000 / 7.500.000 / 10.500.000 / 15.000.000 / 22.000.000 đ |
| Giá gốc các hộp thường (dao động cộng trừ 20% mỗi ngày) | 1.000.000 / 3.500.000 / 8.000.000 đ |

Các mức tiền, giá bán, xác suất, quy mô danh mục và mục tiêu tỷ lệ thắng vẫn đang thử nghiệm. Claude 2 so sánh chiến thuật thận trọng, săn combo và liều lĩnh bằng các lượt chạy với seed tái lập. Ghi tỷ lệ thắng, ngày hụt tiền, tần suất hoàn thành combo và giả định trước khi đề xuất thay số. Giới hạn 3 hộp/ngày, thời hạn 7 ngày và quy tắc thanh toán đúng kỳ là luật cốt lõi.
Xem [checklist công việc](../../TASKS.md).
