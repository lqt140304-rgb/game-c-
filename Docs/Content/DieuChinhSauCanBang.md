# Đề xuất điều chỉnh sau cân bằng — CHỢ ĐỒ LẠ

Claude 1 tổng hợp báo cáo của Claude 2 ([Sprint 2](../Balance/Sprint2.md), [Sprint 3](../Balance/Sprint3.md), [Sprint 4](../Balance/Sprint4.md)) và đề xuất cách sửa nội dung.

**Mục tiêu của chủ dự án:** không để người chơi thắng quá dễ. Game phải tạo nhiều tình huống khó, có đánh đổi và buộc tính toán kỹ.

Từ mục tiêu đó, mục tiêu độ khó **[ĐỀ XUẤT]** là:

- Bot không đọc manh mối, không canh biến cố, không tính kỹ thắng khoảng **15%** nếu chơi an toàn hoặc liều, khoảng **30%** nếu biết đầu tư tạo tác.
- Người chơi thật biết đọc manh mối, chọn thời điểm giám định, canh biến cố và đầu tư đúng chỗ nên thắng cao hơn bot. Mức này cần đo bằng chơi thử thật.
- Đầu tư tạo tác phải **đáng tiền nhưng không bắt buộc**, để có lựa chọn thật giữa giữ tiền và đầu tư.

**Cách duyệt.** Nội dung Sprint 0–2 đã nghiệm thu nên tôi chưa sửa vào file gốc. Chủ dự án duyệt hoặc từ chối **từng mục A–F**. Sau khi duyệt, Claude 1 áp dụng vào file gốc rồi báo Claude 2 chạy lại.

| Mục | Vấn đề của Claude 2 | Đề xuất tóm tắt |
| --- | --- | --- |
| A | D-09 nội dung gốc thắng 0% | Dùng giá nền của Claude 2 nhân 0,85 (khó hơn đề xuất của Claude 2) |
| B | D-11 giám định không có giá trị tiền | Giám định thật được cộng 25% giá bán, phí 10% giá nền |
| C | D-10, D-12 tạo tác net âm, giảm phạt vô nghĩa | Đổi hiệu ứng để tạo tác đáng đầu tư và có đánh đổi |
| D | D-05, D-07, D-08 luật nợ | Giữ luật nợ khắt khe, cho trả nợ tồn sớm |
| E | D-02, BT-14 kho | Kho 8 ô, bàn mở 3 hộp |
| F | D-06 lưu ván | Chỉ lưu tự động, không tải lại bản cũ |

## A. Giá nền và độ khó (D-09)

Claude 2 đề xuất bảng giá nền đưa bot thắng 42–49%, dễ hơn mục tiêu. Tôi đo lại bằng [QA/difficulty_sweep.py](../../QA/difficulty_sweep.py), nhân bảng của Claude 2 với một hệ số.

Cấu hình đo: CP1 và CB1 áp dụng mọi hiện vật, hiệu ứng nhân 2 (xem mục C). Thắng% theo thứ tự thận trọng, săn combo, liều. 3.000 seed mỗi dòng.

| Hệ số | Lần giá hộp H1 / H2 / H3 / RD | Thắng% (thận trọng / săn combo / liều) |
| --- | --- | --- |
| 1,00 (đề xuất của Claude 2) | 2,61 / 1,70 / 1,40 / 1,25 | 41,5 / 51,1 / 48,5 |
| 0,90 | 2,35 / 1,53 / 1,26 / 1,13 | 23,3 / 36,2 / 26,9 |
| **0,85** | **2,22 / 1,44 / 1,19 / 1,06** | **14,9 / 27,6 / 15,2** |
| 0,80 | 2,09 / 1,36 / 1,12 / 1,00 | 6,4 / 20,0 / 7,1 |
| 0,70 | 1,83 / 1,19 / 0,98 / 0,88 | 0,9 / 6,3 / 0,5 |

**Đề xuất: hệ số 0,85.** Với hệ số này, săn combo thắng gần gấp đôi hai chiến thuật còn lại, nên đầu tư đúng được thưởng. Hộp H3 chỉ lời khoảng 19% và Rương Đen lời khoảng 6%, nên mua hộp đắt thành một canh bạc phải cân nhắc.

| Hiện vật | Giá nền hiện tại | Giá của Claude 2 | **Giá đề xuất (0,85)** |
| --- | --- | --- | --- |
| V01 Bình sứ men xanh | 300 | 250 | **210** |
| V02 Sách cổ bìa da | 500 | 410 | **350** |
| V03 Đồng hồ quả lắc cũ | 650 | 530 | **450** |
| V04 Mảnh kính đồng hồ thiên văn | 200 | 440 | **375** |
| V05 Thiết bị liên lạc hỏng | 150 | 330 | **280** |
| V06 Bộ pin lượng tử | 900 | 1.970 | **1.675** |
| V07 Tinh thể lam cầm tay | 1.200 | 2.880 | **2.450** |
| V08 Gương thủy tinh cổ tích | 1.000 | 1.700 | **1.445** |
| V09 Lá bùa giấy vàng | 400 | 960 | **815** |
| V10 Mảnh vảy long | 1.800 | 3.060 | **2.600** |

Các giá đã làm tròn đến 5 xu, nên Claude 2 cần chạy lại để xác nhận. Hệ số chất lượng (Nguyên 1,0, Xước 0,7, Hỏng 0,3, tỷ lệ 50/35/15) và hàng giả (15%, riêng V09 là 35%, bán còn 20%) lấy theo giả định của Claude 2.

## B. Giám định và phục chế (D-11)

Hiện tại giám định không đổi giá bán, nên mọi hiệu ứng giảm phí không có tác dụng.

- **Phí giám định:** 10% giá nền, tối thiểu 20 xu.
- **Phí phục chế:** 25% giá nền, nâng chất lượng một bậc (Hỏng lên Xước, Xước lên Nguyên).
- **Chứng nhận giám định:** hiện vật **thật** đã giám định được cộng **25%** giá bán. Hàng giả vẫn bán còn 20%, giám định không đổi giá.

**Đánh đổi thật:** giám định tốn tiền và tốn thời gian, nhưng chỉ có lời nếu món là hàng thật. Kỳ vọng mỗi lần giám định xấp xỉ +7% giá nền sau phí, nên lợi lớn với món đắt và chất lượng cao, hầu như không đáng với món rẻ. Người chơi phải dùng manh mối để đoán món nào đáng giám định. Ví dụ V09 Lá bùa (giá 815, có 35% là giả): phí 82 xu, nếu là hàng thật nguyên vẹn thì giá bán tăng thêm khoảng 204 xu, nếu giả thì mất phí vô ích.

Đây là hệ số `u` mà Claude 2 yêu cầu chốt, dưới dạng cộng thêm cho món đã giám định thay vì trừ món chưa giám định. Cách này không làm bảng giá ở mục A thấp đi.

## C. Tạo tác, combo và gia truyền (D-10, D-12)

Theo Claude 2, tạo tác hiện chi 996 xu và chỉ thu thêm 170 xu. Chiến thuật đầu tư còn thua chiến thuật không đầu tư. GT1, TC3 và Mốc 3 Thiên Cơ cùng giảm phạt nợ nhưng vô nghĩa.

| Mã | Hiện tại | Đề xuất |
| --- | --- | --- |
| CP1 | +5% giá bán hiện vật nhóm Quá khứ | **+10% giá bán mọi hiện vật** |
| CB1 | +10% hiện vật Quá khứ bán trong ngày | **+20% mọi hiện vật bán trong ngày** |
| CP3 | +10% hiện vật Thần thoại | Giữ nguyên |
| TC3 | Miễn phạt khi nợ tồn không quá 300 xu | **+10% giá bán hiện vật nhóm Hiện đại, Tương lai và Ngoài hành tinh** |
| CB3 | Giám định miễn phí 1 lần mỗi ván | **Giám định miễn phí 1 hiện vật mỗi ngày** |
| GT1 | Phạt thu nợ giảm xuống 10% | **Hoãn nợ (xem dưới)** |

Dòng CP1 và CB1 được Claude 2 đo ở mục A: săn combo thắng cao hơn hai chiến thuật còn lại. Các dòng TC3, CB3 và GT1 **chưa được đo**.

**GT1 mới: Sổ Ghi Nợ Cũ.** Một lần mỗi ván, hoãn nguyên một kỳ nợ sang ngày sau. Kỳ hoãn bị cộng thêm 15% và không được hoãn kỳ 7. Đây là đánh đổi thật: hoãn kỳ ngày 4 (750 xu) để mua thêm hộp, đổi lại phải trả thêm 113 xu. Dùng sai thì ngày sau nợ chồng nặng hơn.

Hiệu ứng thông tin (CB2, GT2, TC2, Mốc 3 Cổ Phong) giữ nguyên vì không đo được bằng xu, cần thử người chơi.

## D. Luật nợ (D-05, D-07, D-08)

**D-05, vòng xoáy thiếu tiền.** Claude 2 đo: sau khi thiếu tiền, không ván nào thắng. Tôi đo thêm đổi ngưỡng thua (1.000, 1.500, 2.000) và mức phạt (0%, 10%, 20%) trong cấu hình giá nền của Claude 2. **Kết quả giống hệt nhau ở mọi dòng**, nên ngưỡng và phạt không phải đòn bẩy.

Đòn bẩy duy nhất là chừa lại tiền mặt khi thu nợ, đo bằng [QA/d05_recovery.py](../../QA/d05_recovery.py) (hệ số giá 1,0). Với hệ số 0,85 mà tôi đề xuất ở mục A:

| Anh Bảy chừa lại | Thắng% (thận trọng / săn combo / liều) | Gỡ lại được sau khi thiếu tiền |
| --- | --- | --- |
| 0 xu (luật hiện tại) | 14,9 / 27,6 / 15,2 | 0% |
| 100 xu | 14,1 / 29,9 / 15,5 | 0,3% / 3,9% / 0,8% |
| 200 xu | 14,4 / 30,4 / 15,6 | 1,0% / 5,2% / 1,6% |

**Đề xuất: giữ luật hiện tại, không chừa tiền.** Theo mục tiêu độ khó, thiếu tiền nên là sai lầm rất đắt. Chừa tiền làm tỷ lệ thắng gần như không đổi nhưng đánh mất cảm giác "một bước sai là thua". Phạt 20% và ngưỡng 1.000 xu giữ nguyên.

**D-07, phạt tối thiểu:** chấp nhận, giữ nguyên phạt làm tròn lên.

**D-08, trả nợ tồn sớm:** cho phép. Tại Anh Bảy, người chơi trả phần **nợ tồn** bất kỳ lúc nào trong ngày, không phạt thêm. Không trả trước kỳ mới. Đánh đổi: tiền trả nợ tồn sớm thì không dùng để mua hộp, nhưng tránh được phạt 20% ở lần thu nợ kế tiếp.

**Ca kiểm thử bổ sung:**

| Ca | Tình huống | Kết quả |
| --- | --- | --- |
| 8 | Ngày 3 còn nợ tồn 240 xu, có 700 xu. Giữa ngày trả 240 tại Anh Bảy. | Nợ tồn về 0, tiền còn 460 xu. Không bị phạt ở kỳ thu cuối ngày. |
| 9 | Ngày 2 thử trả trước kỳ ngày 3. | Từ chối. Chỉ nhận trả nợ tồn. |
| 10 | Dùng GT1 hoãn kỳ ngày 4 (750 xu). | Ngày 5 phải trả 1.050 cộng 863 (750 cộng 15%, làm tròn lên), cộng nợ tồn nếu có. |
| 11 | Dùng GT1 vào kỳ ngày 7. | Từ chối. |

## E. Kho và bàn mở (D-02, BT-14)

Kho là nguồn đánh đổi tự nhiên: giữ hàng chờ giá hay bán ngay để có chỗ.

- **Bàn mở:** chứa tối đa 3 hộp chưa mở. Hộp chưa mở không chiếm kho và giữ được qua ngày. Bàn đầy thì không mua thêm hộp.
- **Kho:** 8 ô cho hiện vật đã mở. Tạo tác không chiếm kho.
- **Kho đầy:** không mở được hộp mới cho đến khi bán, cầm hoặc đưa đấu giá bớt. Hộp vẫn ở lại bàn, không mất.
- Hiện vật đang cầm hoặc đang đấu giá không chiếm kho vì đang ở nơi khác. Mỗi món chỉ ở đúng một nơi.
- Hết ván, hiện vật còn lại không tính vào kết quả.

## F. Lưu ván (D-06)

Lưu ván **chỉ tự động**, một ô duy nhất, không có lưu tay và không tải lại bản cũ trong cùng ván.

- Lưu vào đầu mỗi ngày và sau mỗi giao dịch tiền hoàn tất.
- Thoát giữa thao tác thì khôi phục về trạng thái ngay sau giao dịch hoàn tất gần nhất.
- Kết quả ngẫu nhiên xác định theo (seed, ngày, lượt) hoặc theo mã hiện vật, theo yêu cầu của Claude 2 cho Codex.

Cách này vừa chặn lặp giao dịch, vừa đúng với roguelite: mỗi quyết định có hậu quả không quay lại được.

## G. Việc cần Claude 2 đo lại

1. Giá nền hệ số 0,85 và bảng giá làm tròn ở mục A.
2. Các thay đổi chưa được đo: TC3, CB3, GT1 mới và chứng nhận giám định +25%.
3. Tỷ lệ thắng của một chiến thuật biết giám định có chọn lọc, để kiểm tra mục tiêu "người biết tính toán thắng cao hơn bot".
4. Ba luật dịch vụ mới ở [DichVuVaMeta.md](DichVuVaMeta.md) với hệ số giá 0,85.

## H. Chủ dự án chọn

| Mục | Khuyến nghị của Claude 1 |
| --- | --- |
| A. Giá nền hệ số 0,85 | Duyệt. Nếu thấy khó quá, thử 0,9 (bot thắng khoảng 23–36%). |
| B. Chứng nhận giám định +25% | Duyệt. |
| C. Đổi hiệu ứng tạo tác, combo, gia truyền | Duyệt dòng CP1 và CB1 (đã đo). Dòng TC3, CB3, GT1 duyệt có điều kiện, chờ Claude 2 đo. |
| D. Giữ luật nợ, cho trả nợ tồn sớm | Duyệt. |
| E. Kho 8 ô, bàn mở 3 hộp | Duyệt. |
| F. Chỉ lưu tự động | Duyệt. |
