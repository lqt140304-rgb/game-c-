# Sprint 5 phần 3: soi lỗi và trả giá theo luật soi mới (Claude 2)

Viết ngày 2026-11-24. **Bản này thay thế bản đo theo luật cũ** (lượt soi miễn phí, phí soi thêm, chỗ nhiễu, bằng chứng đẩy giá lên). Bản cũ còn trong lịch sử Git. Bản này đo lại theo luật soi mới mà chủ dự án chốt ngày 2026-11-24 ([spec](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md) mục 2, bàn giao ở [BanGiaoSprint5SoiLoi.md](BanGiaoSprint5SoiLoi.md)).

> **Tiền tệ:** số tiền ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ và in qua `QA/currency.py`.
> **Giả định:** mô hình dưới đây là **giả định đề xuất của Claude 2, chủ dự án chưa chốt**. Mọi con số là tham số `inspect_sim.RULES`.

## Kết luận ngắn

1. **Nghiệm thu đạt** trên ván thường (n = 2.000). Bot thường (soi thường) thắng **18,5% (thận trọng) và 19,2% (liều)**, nằm trong 15% đến 20%. Người chơi tính toán hoàn hảo (soi giỏi, chọn rương tối ưu) thắng tối đa **46,9%**, dưới trần 47%. Mức này sát trần, sai số khoảng 1 điểm.
2. **Kỹ năng soi có giá trị, đúng thứ tự.** Cùng cách mua rương (trung bình hai bot): soi giỏi **23,4%**, soi thường **18,9%**, soi ẩu **14,5%**, bỏ qua soi **11,1%**. Soi giỏi hơn soi thường 4,5 điểm; bỏ qua soi là cách chơi tệ nhất.
3. **Lần chơi đầu không thua ở ngày 1** với mọi hồ sơ soi: 0 ván thua ngày 1 hay trước ngày 3, có hay không miễn phạt ngày 1 (n = 1.500). Bot thường thắng 7,5% đến 7,7% ván đầu, soi giỏi 10,5% đến 11,9%.
4. **Trần trả giá là đòn bẩy mạnh nhất lên người chơi giỏi** và là thứ phải chốt. Người chơi giỏi (liều + giám định, n = 2.000): trần 1,05 cho 45,3%; 1,08 cho 46,0%; **1,10 cho 46,9%**; 1,12 cho 47,9%; 1,20 cho 53,1%. Bot thường đổi ít hơn (14,6% đến 21,8% từ trần 1,05 đến 1,20). Đề xuất trần **110%**.
5. **Phạt lỗi sót ít ảnh hưởng thắng thua** (bot thường 20,7% ở 0%, 17,3% đến 18,6% ở 10%) nhưng quyết định giá trị của kỹ năng soi: bỏ qua soi tụt từ 14,2% xuống 8,9%. Giữ **5% giá thẩm định mỗi lỗi sót**.
6. **Tỉ lệ dấu lạ trên món thật quyết định soi thật giả có rủi ro hay không.** Ở 0%, thấy một dấu giả là chắc giả, soi trở nên dễ (bot thường 21,5% đến 22,0%, người giỏi 49,6%, quá trần). Ở 10% bot thường 18,5% đến 19,2%; ở 20% bot thường 16,5% đến 17,3%. Đề xuất **10%**.

## 1. Bộ luật đề xuất

| Mục | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|
| Lỗi nhẹ của món Xước (trừ tổng 30%) | 1 | 2 | 3 | 3 |
| Món Hỏng | như Xước + 1 vết lớn (tổng trừ 70%) | | | |
| Dấu hàng giả trên món giả | 1 | 1 | 2 | 2 |
| Cách bán khi khai thật | bán nhanh | trả giá | trả giá | trả giá |

- **Nguyên không lỗi.** Giá cuối vẫn là giá nền × (1, 0,7 hoặc 0,3) × hiệu ứng tạo tác, nên kinh tế cũ không đổi.
- **Dấu lạ:** món thật có 10% xác suất mang một dấu giống dấu hàng giả.
- **Kỹ năng:** xác suất tìm ra mỗi lỗi hoặc dấu. Giỏi 95%, thường 75%, ẩu 45%, bỏ qua 0%. Soi không giới hạn thời gian.
- **Lỗi sót:** khi cửa hàng kiểm tra lại, lỗi sót vẫn bị trừ giá, cộng phạt 5% giá thẩm định mỗi lỗi.
- **Chọn thật hay giả:** người chơi suy ra xác suất giả theo Bayes từ số dấu thấy được và tỉ lệ hàng giả của rương. Khai giả chỉ nhận giá hàng giả 20% (lỗ 80% nếu món thật); giả mà khai thật bị tịch thu (phạt 20%, 0 đ); ngày 1 miễn phạt.
- **Trả giá (Hiếm trở lên):** mở màn ở giá bảng đã trừ lỗi (100%). 3 vòng; mỗi vòng thắng với xác suất (kỹ năng trả giá) × (tỉ lệ lỗi đã tìm). Thắng +8%, thua −5%, giá chốt trong 70% đến **110%**. Dừng lúc nào cũng được.
- **Giáo Sư Lẫm:** chỉ ra một dấu giả chưa tìm thấy (nếu món có, không thì một lỗi chưa tìm thấy), phí 5% giá nền, tối thiểu 200.000 đ. Bỏ chứng nhận +25%.

## 2. Kết quả (n = 2.000, trần 1,10)

**Ván thường, tỉ lệ thắng:**

| Hồ sơ soi | Thận trọng | Liều | Tìm lỗi | Sót/món |
|---|---|---|---|---|
| Giỏi | 22,5% | 24,2% | 95% | 0,03 |
| Thường | 18,5% | 19,2% | 74% | 0,17 |
| Ẩu | 14,3% | 14,7% | 45% | 0,35 |
| Bỏ qua soi | 10,9% | 11,2% | 0% | 0,62 |

**Người chơi chọn rương tối ưu:**

| Chiến thuật | Soi giỏi | Soi thường |
|---|---|---|
| Thận trọng + giám định | 40,0% | 34,2% |
| Liều + giám định | **46,9%** | 41,1% |
| Combo nhỏ + giám định | 40,2% | |
| Combo lớn + giám định | 37,9% | |

**Lần chơi đầu (bảng rương FIRST_RUN, n = 1.500):** thường 7,5% và 7,7%, giỏi 10,5% và 11,9%, ẩu 6,1% và 6,4%, bỏ qua soi 4,5% và 6,3%. Giỏi chọn rương tối ưu: 27,0% và 35,5%. Không có ván nào thua ở ngày 1; ván thua sớm nhất là ngày 3. Phạt ngày 1 như ngày thường không đổi kết quả (ẩu 6,3%, bỏ qua soi 6,1%).

**Soi sai có giá nhưng chơi được.** Soi thường đọc nhầm thật thành giả 2,4% đến 2,7% số món, giả thành thật 1,3% đến 1,6%. Soi ẩu: giả thành thật 4,4%, tức khoảng 1.000.000 đồng phạt mỗi ván. Bỏ qua soi: giả thành thật 11,1% (khoảng 2.600.000 đến 3.500.000 đồng tịch thu mỗi ván) cộng phạt sót.

## 3. Độ nhạy (n = 1.000 đến 2.000, thận trọng / liều của bot thường; người giỏi liều + giám định)

| Thay đổi | Bot thường | Bỏ qua soi | Giỏi liều + giám định |
|---|---|---|---|
| Đề xuất (phạt sót 5%, dấu lạ 10%, trần 1,10) | 18,5 / 19,2 | 10,9 / 11,2 | 46,9 |
| Phạt sót 0% | 20,7 / 20,0 | 11,9 / 14,2 | 48,7 |
| Phạt sót 10% | 17,3 / 18,6 | 8,8 / 8,9 | 48,7 |
| Dấu lạ 0% | 21,5 / 22,0 | 11,6 / 10,9 | 49,6 |
| Dấu lạ 20% | 17,3 / 16,5 | 10,7 / 12,0 | 47,9 |
| Phạt giả khai thật 0% | 18,5 / 19,0 | 12,3 / 13,8 | 50,5 |
| Phạt giả khai thật 40% | 17,9 / 18,8 | 7,9 / 8,9 | 47,7 |
| Trần 1,05 | 14,6 / 16,7 | 10,2 / 11,3 | 45,3 |
| Trần 1,20 | 20,0 / 21,8 | 10,2 / 11,3 | 53,1 |
| 2 vòng trả giá | 18,0 / 17,1 | 10,2 / 11,3 | 48,2 |
| Thắng +12% | 19,2 / 20,1 | 10,2 / 11,3 | 48,7 |
| Thua −10% | 16,5 / 17,7 | 10,2 / 11,3 | 47,9 |
| 1 dấu giả ở mọi bậc | 17,2 / 17,9 | 10,2 / 10,7 | 46,4 |
| 3 dấu giả ở Rất hiếm và Huyền thoại | 19,9 / 21,9 | 10,7 / 10,8 | 48,7 |
| Người soi thường khá hơn (s 85%) | 21,0 / 21,1 | | |
| Người soi thường kém hơn (s 60%) | 16,4 / 17,8 | | |
| Người soi thường quyết theo giá trị kỳ vọng | 18,8 / 21,5 | | |

Các dòng độ nhạy trong bảng này chạy ở trần 1,12 (bộ đo ban đầu, trước khi hạ trần), nên cột người giỏi ở dòng "đề xuất" chỉ so được với dòng này. Cột đó cao hơn 46,9% ở mọi dòng trừ trần 1,05 và dấu giả 1 mỗi món, cho thấy trần là tham số cần giữ.

## 4. Phát hiện còn đúng từ bản cũ

- **Khai giả nhầm đắt hơn khai thật nhầm.** Thật mà khai giả mất 80% giá trị, giả mà khai thật mất đúng giá hàng giả. Vì vậy người chơi phải khá chắc mới khai giả. Soi giỏi hạ tỉ lệ thật thành giả từ 2,5% xuống 0,3%.
- **Cần có trần trả giá.** Không trần (hoặc trần cao) đẩy người chơi giỏi qua 50%. Trần 1,10 giữ dưới 47%.
- **Ngày 1 miễn phạt đủ để không thua ngày 1**, và kết quả gần như không đổi khi bỏ miễn phạt, vì ngày 1 của lần chơi đầu chỉ bán rương rẻ.

## 5. Phát hiện mới theo luật soi mới

- **Soi lỗi giờ chủ yếu bảo vệ khỏi phạt, ít kiếm thêm tiền.** Món đã bị trừ giá theo lỗi đúng bằng giá nền × hệ số tình trạng, nên soi giỏi chỉ tránh phạt sót, nhận ra hàng giả, và thắng trả giá nhờ biết đủ lỗi. Doanh thu so với giá đúng: giỏi 1,06 đến 1,08, thường 1,01, ẩu 0,97, bỏ qua soi 0,93.
- **Bỏ qua soi vẫn khả thi nhưng tệ nhất**, chủ yếu do bán nhầm hàng giả (11% số món). Nếu muốn nhấn mạnh soi lỗi hơn nữa, tăng phạt sót hoặc tăng tỉ lệ hàng giả; nếu muốn dễ hơn, giảm một trong hai.
- **Không giới hạn thời gian soi** khiến người kiên nhẫn tìm được hết lỗi (mô hình chỉ có xác suất tìm mỗi lỗi, không có đồng hồ). Kỹ năng soi chỉ khác nhau ở độ kỹ, cần chốt giới hạn thời gian ở bước thiết kế.
- **Gợi ý của Giáo Sư Lẫm ít được dùng** (0,03 đến 0,04 lần mỗi ván với chiến thuật thường, 1,9 lần với combo lớn nhờ miễn phí). Soi giỏi đã đủ chắc nên gợi ý hiếm khi đáng 5% giá nền.

## 6. Câu hỏi mở cho chủ dự án (từ bàn giao, chưa chốt)

1. Có giới hạn lượt hoặc thời gian soi không? Không giới hạn thì kỹ năng soi chỉ còn là sự kiên nhẫn.
2. Bằng chứng ở bước soi nối với trả giá thế nào? Mô hình này nối qua tỉ lệ lỗi đã tìm.
3. Dấu lạ trên món thật có cố ý giống dấu hàng giả không? Tỉ lệ dấu lạ quyết định soi thật giả có rủi ro hay không.
4. Người chơi có được bỏ bấm một lỗi đã thấy để giữ giá cao không? Mô hình này coi đó là lỗi sót (cùng khoản phạt), nên không có lợi.
5. Mức phạt lỗi sót (5%), trần 110%, và tỉ lệ dấu lạ (10%) có chấp nhận được không? Bốn tham số này quyết định độ khó.

## 7. Tái hiện

```bash
cd QA
python3 test_inspect.py                # khoảng 35 giây, n = 600
python3 inspect_explore.py 500 pen     # dò phạt sót x dấu lạ; thêm hag, fine, day1
python3 inspect_final.py final         # n = 2.000, ván thường và lần chơi đầu
python3 inspect_final.py sens          # độ nhạy, n = 1.000
```
