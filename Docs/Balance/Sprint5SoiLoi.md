# Sprint 5 phần 3: soi lỗi và trả giá theo luật soi mới (Claude 2)

Viết ngày 2026-11-24, cập nhật 2026-11-25 sau khi chủ dự án trả lời các câu hỏi mở. **Bản này thay thế bản đo theo luật cũ** (lượt soi miễn phí, phí soi thêm, chỗ nhiễu, bằng chứng đẩy giá lên); bản cũ còn trong lịch sử Git. Luật soi: [spec](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md) mục 2; bàn giao: [BanGiaoSprint5SoiLoi.md](BanGiaoSprint5SoiLoi.md).

> **Tiền tệ:** số tiền ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ, in qua `QA/currency.py`.
> **Giả định:** mô hình (xác suất tìm lỗi, phí, mức phạt) là đề xuất của Claude 2. Chủ dự án đã chốt: giới hạn soi 2 phút, nối trả giá với lỗi đã tìm, dấu lạ 15%, cho bỏ bấm lỗi (thu mua soi thấy thì trừ tiền), có nhà vá lỗi, và **độ khó cao hơn bản đầu**. Các con số tiền, phạt, trần là tham số `inspect_sim.RULES`.

## Chủ dự án đã chốt (2026-11-25)

| Câu hỏi | Quyết định |
|---|---|
| Giới hạn soi | **2 phút.** Mô hình biểu diễn bằng xác suất tìm mỗi lỗi (giỏi 95%, thường 75%, ẩu 45%) |
| Nối bằng chứng với trả giá | Giữ: thắng mỗi vòng = kỹ năng trả giá × tỉ lệ lỗi đã tìm |
| Dấu lạ trên món thật | **15%**, cố ý giống dấu hàng giả |
| Bỏ bấm một lỗi đã thấy | Cho phép. Khi người thu mua soi thấy thì trừ tiền, cộng phạt (mô hình coi như lỗi sót, nên bỏ bấm không có lợi) |
| Nhà vá lỗi | **Thêm.** Lỗi đã tìm ra thì sửa được, mỗi lỗi một khoản phí. Lỗi sót không sửa được |
| Độ khó | **Khó hơn** bản đầu |

## Kết luận ngắn

1. **Nghiệm thu đạt theo mục tiêu "khó hơn"** (n = 2.000, ván thường). Bot thường (soi thường) thắng **12,7% (thận trọng) và 14,2% (liều)**, trong 12% đến 16% (bản đầu 18,5% đến 19,2%). Người chơi tính toán hoàn hảo (soi giỏi, chọn rương tối ưu) thắng tối đa **40,8%** (bản đầu 46,9%). Mục tiêu 12% đến 16% và không quá 42% do tôi đặt khi chủ dự án chỉ nói "khó hơn"; nếu muốn mức khác, đổi bốn tham số ở mục 3.
2. **Kỹ năng soi vẫn có giá trị, đúng thứ tự.** Cùng cách mua rương (trung bình hai bot): soi giỏi **18,6%**, soi thường **13,5%**, soi ẩu **10,3%**, bỏ qua soi **9,3%**. Khoảng cách giỏi và thường 5,1 điểm.
3. **Lần chơi đầu không thua ở ngày 1** với mọi hồ sơ (0 ván thua ngày 1 hay trước ngày 3, n = 1.500). Nhưng ván đầu rất chặt: bot thường thắng 5,3% đến 5,7%, soi ẩu 4,0% đến 5,0%, bỏ qua soi 3,6% đến 5,3%, soi giỏi 8,7% đến 9,0%. Nếu thấy lần chơi đầu quá khó, nới ở bảng rương lần chơi đầu chứ không phải luật soi.
4. **Trần trả giá là đòn bẩy mạnh nhất lên người chơi giỏi.** Người giỏi (liều + giám định): trần 1,03 cho 40,8%; 1,08 cho 46,1%; 1,10 cho 48,0% (n = 1.000, cùng các tham số khác). Trần 1,03 nghĩa là trả giá chỉ thêm tối đa 3%: trả giá còn tác dụng chủ yếu là tránh bị trừ, ít khi lời thêm. Nếu muốn trả giá có lợi rõ hơn, nâng trần lên 1,05 và chấp nhận người giỏi khoảng 43%.
5. **Nhà vá lỗi cần đắt mới không làm game dễ đi.** Phí vá thấp (6% lỗi nhẹ, 15% vết lớn) kéo bot thường +2 và người giỏi +4,4 điểm; phí 15% và 35% gần như không đổi so với không có nhà vá (42,3% so với 42,3%, n = 1.000). Ở phí đó người chơi vẫn vá khi lời lớn (Thường có 1 lỗi nhẹ trừ cả 30%, vết hỏng lớn trừ 40%), bot thường vá khoảng 0,3 lần mỗi ván.
6. **Phạt lỗi sót và dấu lạ chỉnh độ khó mịn.** Dấu lạ càng nhiều, soi thật giả càng rủi ro: ở 10% bot thường 13,8% và 15,2%, ở 20% là 11,7% và 13,0%. Phạt sót 5% đến 15% đổi bot thường khoảng 1 điểm nhưng kéo bỏ qua soi từ 10% đến 12% xuống 7,5%.

## 1. Bộ luật đề xuất (mặc định `inspect_sim.RULES`)

| Mục | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|
| Lỗi nhẹ của món Xước (trừ tổng 30%) | 1 | 2 | 3 | 3 |
| Món Hỏng | như Xước + 1 vết lớn; tổng trừ 70% (30% + 40%) | | | |
| Dấu hàng giả trên món giả | 1 | 1 | 2 | 2 |
| Cách bán khi khai thật | bán nhanh | trả giá | trả giá | trả giá |

- **Nguyên không lỗi.** Giá cuối vẫn là giá nền × (1; 0,7; 0,3) × hiệu ứng tạo tác, nên kinh tế cũ không đổi.
- **Dấu lạ:** món thật có **15%** xác suất mang một dấu giống dấu hàng giả.
- **Kỹ năng:** xác suất tìm ra mỗi lỗi hoặc dấu. Giỏi 95%, thường 75%, ẩu 45%, bỏ qua 0%, trong 2 phút soi.
- **Lỗi sót (kể cả lỗi thấy mà không bấm):** người thu mua kiểm tra lại, vẫn trừ giá, cộng phạt **10% giá thẩm định mỗi lỗi**.
- **Nhà vá lỗi:** phí mỗi lỗi nhẹ **15%** giá nền, vết hỏng lớn **35%**. Người chơi vá khi lời kỳ vọng ((1 − xác suất giả) × phần giá lỗi trừ) lớn hơn phí. Phục chế cũ (nâng một bậc tình trạng) vẫn có.
- **Chọn thật hay giả:** Bayes từ số dấu giả thấy được và tỉ lệ hàng giả của rương. Khai giả chỉ nhận giá hàng giả 20% (lỗ 80% nếu thật); giả mà khai thật bị tịch thu (phạt 20%, 0 đ); ngày 1 miễn phạt.
- **Trả giá (Hiếm trở lên):** mở màn ở giá bảng đã trừ lỗi (100%). 3 vòng; thắng một vòng với xác suất (kỹ năng trả giá) × (tỉ lệ lỗi đã tìm). Thắng +8%, thua −5%, giá chốt trong 70% đến **103%**.
- **Giáo Sư Lẫm:** chỉ ra một dấu giả chưa tìm thấy (nếu món có, không thì một lỗi chưa tìm thấy), phí 5% giá nền, tối thiểu 200.000 đ. Bỏ chứng nhận +25%.

## 2. Kết quả (n = 2.000, bộ luật cuối)

**Ván thường, tỉ lệ thắng:**

| Hồ sơ soi | Thận trọng | Liều | Tìm lỗi | Sót/món | Vá/ván |
|---|---|---|---|---|---|
| Giỏi | 17,9% | 19,3% | 95% | 0,03 | 0,4 |
| Thường | 12,7% | 14,2% | 75% | 0,16 | 0,3 |
| Ẩu | 10,3% | 10,3% | 45% | 0,35 | 0,2 |
| Bỏ qua soi | 9,4% | 9,2% | 0% | 0,62 | 0 |

**Người chơi chọn rương tối ưu:**

| Chiến thuật | Soi giỏi | Soi thường |
|---|---|---|
| Thận trọng + giám định | 35,4% | 29,8% |
| Liều + giám định | **40,8%** | 34,2% |
| Combo nhỏ + giám định | 36,6% | |
| Combo lớn + giám định | 34,6% | |

**Lần chơi đầu (bảng rương FIRST_RUN, n = 1.500):** thường 5,3% và 5,7%, giỏi 8,7% và 9,0%, ẩu 4,0% và 5,0%, bỏ qua soi 3,6% và 5,3%. Giỏi chọn rương tối ưu: 24,5% và 32,1%. Không ván nào thua ở ngày 1; ván thua sớm nhất là ngày 3. Phạt ngày 1 như ngày thường không đổi kết quả (ẩu 4,9%, bỏ qua soi 5,2%).

**Soi sai có giá nhưng chơi được.** Soi thường đọc nhầm thật thành giả 2,8% đến 3,0% số món, giả thành thật 1,8% đến 2,5%. Soi ẩu: giả thành thật 4,4% đến 5,1%. Bỏ qua soi: giả thành thật 10,7% đến 10,8% số món. Doanh thu so với giá đúng: giỏi 1,01, thường 0,95, ẩu 0,92, bỏ qua soi 0,91.

## 3. Độ nhạy (n = 1.000; bot thường thận trọng / liều; giỏi liều + giám định)

Số đo ở n = 1.000 có sai số khoảng 1 đến 1,5 điểm (dòng bỏ qua soi dao động 7,5% đến 12%).

| Thay đổi so với bộ luật cuối | Bot thường | Giỏi (liều + giám định) |
|---|---|---|
| Phạt sót 5% | 13,0 / 14,2 | 42,9 |
| Phạt sót 15% | 11,8 / 13,2 | 43,1 |
| Dấu lạ 10% | 13,8 / 15,2 | 44,3 |
| Dấu lạ 20% | 11,7 / 13,0 | 42,2 |
| Trần 1,08 | 16,1 / 15,7 | 46,1 |
| Trần 1,10 | 16,5 / 16,5 | 48,0 |
| 2 vòng trả giá | 12,1 / 12,7 | 40,4 |
| Không có nhà vá lỗi | 12,5 / 14,1 | 42,3 |
| Nhà vá lỗi rẻ (6% / 15%) | 14,7 / 16,1 | 46,7 |
| Nhà vá lỗi đắt (18% / 40%) | 12,7 / 14,3 | 42,3 |
| Phạt giả khai thật 0% | 13,0 / 14,5 | 45,9 |
| Phạt giả khai thật 40% | 12,6 / 14,3 | 42,7 |
| Gợi ý 10% giá nền | 12,6 / 14,4 | 42,8 |
| 1 dấu giả ở mọi bậc | 12,5 / 12,4 | 40,9 |
| 3 dấu giả ở Rất hiếm và Huyền thoại | 15,8 / 17,3 | 42,4 |
| Người soi thường khá hơn (s 85%) | 15,8 / 18,7 | |
| Người soi thường kém hơn (s 60%) | 11,7 / 11,8 | |
| Người soi thường quyết theo giá trị kỳ vọng | 13,2 / 15,4 | |

## 4. Phát hiện còn đúng từ bản cũ

- **Khai giả nhầm đắt hơn khai thật nhầm.** Thật mà khai giả mất 80% giá trị, giả mà khai thật mất đúng giá hàng giả, nên người chơi phải khá chắc mới khai giả. Soi giỏi hạ tỉ lệ thật thành giả xuống 0%.
- **Cần có trần trả giá.** Trần cao đẩy người chơi giỏi qua 47%; trần 1,03 giữ dưới 42%.
- **Ngày 1 miễn phạt đủ để không thua ngày 1**, và kết quả gần như không đổi khi bỏ miễn phạt.

## 5. Phát hiện mới theo luật soi mới

- **Soi lỗi chủ yếu bảo vệ khỏi phạt, ít kiếm thêm tiền.** Món đã trừ giá theo lỗi đúng bằng giá nền × hệ số tình trạng, nên soi giỏi chỉ tránh phạt sót, nhận ra hàng giả, thắng trả giá. Với trần 1,03 phần lời thêm từ trả giá còn rất nhỏ.
- **Bỏ qua soi chỉ kém soi ẩu 1 điểm.** Khoảng cách thấp vì bot không soi vẫn thắng 9% nhờ chọn rương. Nếu muốn bỏ qua soi đau hơn nữa, tăng phạt sót hoặc tỉ lệ hàng giả.
- **Gợi ý của Giáo Sư Lẫm ít được dùng** với chiến thuật thường (khoảng 0,03 lần mỗi ván; combo lớn 1,9 lần nhờ miễn phí): soi giỏi đã đủ chắc nên gợi ý hiếm khi đáng 5% giá nền.
- **Chưa kiểm tra bằng người thật:** các mức tìm lỗi trong 2 phút (95%, 75%, 45%) là giả định. Nếu người thật tìm kém hơn, bot thường sẽ dưới 12%.

## 6. Còn mở

1. 2 phút tính mỗi món hay mỗi buổi mở rương? Số đo giả định mỗi món.
2. Mục tiêu độ khó (bot thường 12% đến 16%, người giỏi không quá 42%) có đúng ý chủ dự án không? Nếu muốn dễ hơn một chút, nâng trần trả giá lên 1,05.
3. Lần chơi đầu chỉ 4% đến 9% tùy hồ sơ soi, thấp hơn trước (6% đến 12%). Có nới bảng rương lần chơi đầu không?

## 7. Tái hiện

```bash
cd QA
python3 test_inspect.py                # khoảng 35 giây, n = 600
python3 inspect_explore.py 500 hard2   # dò độ khó; các nhóm khác: pen, hag, fine, hard, day1
python3 inspect_final.py final         # n = 2.000, ván thường và lần chơi đầu (khoảng 6 phút)
python3 inspect_final.py sens          # độ nhạy, n = 1.000
```
