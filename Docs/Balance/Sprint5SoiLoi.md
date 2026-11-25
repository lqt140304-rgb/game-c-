# Sprint 5 phần 3: soi lỗi và trả giá theo luật soi mới (Claude 2)

Viết ngày 2026-11-24, cập nhật 2026-11-25 sau khi chủ dự án trả lời các câu hỏi mở. **Bản này thay thế bản đo theo luật cũ** (lượt soi miễn phí, phí soi thêm, chỗ nhiễu, bằng chứng đẩy giá lên); bản cũ còn trong lịch sử Git. Luật soi: [spec](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md) mục 2; bàn giao: [BanGiaoSprint5SoiLoi.md](BanGiaoSprint5SoiLoi.md).

> **Tiền tệ:** số tiền ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ, in qua `QA/currency.py`.
> **Giả định:** xác suất tìm lỗi, phí vá, mức phạt và trần là đề xuất của Claude 2 (tham số `inspect_sim.RULES`), chưa kiểm tra bằng người thật.

## Chủ dự án đã chốt (2026-11-25)

| Câu hỏi | Quyết định |
|---|---|
| Giới hạn soi | **2 phút mỗi món.** Mô hình biểu diễn bằng xác suất tìm mỗi lỗi (giỏi 95%, thường 75%, ẩu 45%) |
| Nối bằng chứng với trả giá | Giữ: thắng mỗi vòng = kỹ năng trả giá × tỉ lệ lỗi đã tìm |
| Dấu lạ trên món thật | **15%**, cố ý giống dấu hàng giả |
| Bỏ bấm một lỗi đã thấy | Cho phép. Khi người thu mua soi thấy thì trừ tiền, cộng phạt (mô hình coi như lỗi sót, nên bỏ bấm không có lợi) |
| Nhà vá lỗi | **Thêm.** Lỗi đã tìm ra thì sửa được, mỗi lỗi một khoản phí. Lỗi sót không sửa được |
| "Khó hơn" | Là **cây khó mở hơn và phần thưởng hậu hĩnh hơn** (thuộc phần 4), không phải hạ tỉ lệ thắng. Mục tiêu tỉ lệ thắng của soi lỗi giữ như bản đầu |
| Lần chơi đầu | **Nới** |

Mục tiêu nghiệm thu: bot thường 15% đến 20%, người chơi tính toán hoàn hảo không quá 47%, soi giỏi hơn soi thường rõ, soi thường hơn soi ẩu và hơn bỏ qua soi, ngày 1 lần chơi đầu không thua.

## Kết luận ngắn

1. **Nghiệm thu đạt** (n = 2.000, ván thường). Bot thường (soi thường) thắng **16,8% (thận trọng) và 15,5% (liều)**. Người chơi tính toán hoàn hảo (soi giỏi, chọn rương tối ưu) thắng tối đa **45,9%**, dưới trần 47% nhưng sát (sai số khoảng 1 điểm).
2. **Kỹ năng soi có giá trị, đúng thứ tự.** Cùng cách mua rương (trung bình hai bot): soi giỏi **22,1%**, soi thường **16,2%**, soi ẩu **11,9%**, bỏ qua soi **9,3%**. Giỏi hơn thường 5,9 điểm.
3. **Lần chơi đầu đã nới và không thua ở ngày 1** với mọi hồ sơ soi (0 ván thua ngày 1 hay trước ngày 3, n = 1.500). Đề xuất `chest_sim.FIRST_RUN_LOOSE`: hai hộp đầu chỉ tốn giá rương rẻ nhất (bản cũ `FIRST_RUN`: hai hộp đầu sắp sẵn nhưng trả giá rương đã chọn). Bot thường thắng ván đầu **10,1% và 10,3%** (cũ 6,7% đến 7,1%); soi ẩu 7,2% và 8,2% (cũ 5,2% và 5,9%); soi giỏi 14,5% và 18,5% (cũ 11,3% và 12,3%).
4. **Trần trả giá là đòn bẩy mạnh nhất lên người chơi giỏi và là thứ phải chốt.** Người giỏi (liều + giám định, n = 1.000): trần 1,03 cho khoảng 41%, **1,08 cho 46,7%**, 1,10 cho 49,0% (vượt trần). Bot thường chỉ đổi 15% đến 17%.
5. **Nhà vá lỗi phải đắt.** Phí rẻ (6% lỗi nhẹ, 15% vết lớn) đẩy người giỏi lên 49,9%; phí đề xuất (12% và 30%) gần như không khác không có nhà vá (45,8% so với 45,4%). Ở phí đề xuất, bot thường vá 0,3 đến 0,7 lỗi mỗi ván và người giỏi 0,7 đến 0,8 lỗi.
6. **Dấu lạ, phạt sót và phạt giả khai thật chỉnh độ khó mịn** (mục 3). Phạt giả khai thật 0% đẩy người giỏi lên 49,8%, nên giữ tịch thu.

## 1. Bộ luật đề xuất (mặc định `inspect_sim.RULES`)

| Mục | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|
| Lỗi nhẹ của món Xước (trừ tổng 30%) | 1 | 2 | 3 | 3 |
| Món Hỏng | như Xước + 1 vết lớn; tổng trừ 70% (30% + 40%) | | | |
| Dấu hàng giả trên món giả | 1 | 1 | 2 | 2 |
| Cách bán khi khai thật | bán nhanh | trả giá | trả giá | trả giá |

- **Nguyên không lỗi.** Giá cuối vẫn là giá nền × (1; 0,7; 0,3) × hiệu ứng tạo tác, nên kinh tế cũ không đổi.
- **Dấu lạ:** món thật có **15%** xác suất mang một dấu giống dấu hàng giả.
- **Kỹ năng:** xác suất tìm ra mỗi lỗi hoặc dấu trong 2 phút mỗi món. Giỏi 95%, thường 75%, ẩu 45%, bỏ qua 0%.
- **Lỗi sót (kể cả lỗi thấy mà không bấm):** người thu mua kiểm tra lại, vẫn trừ giá, cộng phạt **10% giá thẩm định mỗi lỗi**.
- **Nhà vá lỗi:** phí mỗi lỗi nhẹ **12%** giá nền, vết hỏng lớn **30%**. Người chơi vá khi lời kỳ vọng ((1 − xác suất giả) × phần giá lỗi trừ) lớn hơn phí. Phục chế cũ (nâng một bậc tình trạng) vẫn có.
- **Chọn thật hay giả:** Bayes từ số dấu giả thấy được và tỉ lệ hàng giả của rương. Khai giả chỉ nhận giá hàng giả 20% (lỗ 80% nếu thật); giả mà khai thật bị tịch thu (phạt 20%, 0 đ); ngày 1 miễn phạt.
- **Trả giá (Hiếm trở lên):** mở màn ở giá bảng đã trừ lỗi (100%). 3 vòng; thắng một vòng với xác suất (kỹ năng trả giá) × (tỉ lệ lỗi đã tìm). Thắng +8%, thua −5%, giá chốt trong 70% đến **108%**.
- **Giáo Sư Lẫm:** chỉ ra một dấu giả chưa tìm thấy (nếu món có, không thì một lỗi chưa tìm thấy), phí 5% giá nền, tối thiểu 200.000 đ. Bỏ chứng nhận +25%.

## 2. Kết quả (n = 2.000)

**Ván thường, tỉ lệ thắng:**

| Hồ sơ soi | Thận trọng | Liều | Tìm lỗi | Sót/món | Vá/ván |
|---|---|---|---|---|---|
| Giỏi | 21,3% | 22,8% | 95% | 0,03 | 0,8 |
| Thường | 16,8% | 15,5% | 75% | 0,16 | 0,6 |
| Ẩu | 12,2% | 11,6% | 45% | 0,35 | 0,4 |
| Bỏ qua soi | 9,4% | 9,2% | 0% | 0,62 | 0 |

**Người chơi chọn rương tối ưu:**

| Chiến thuật | Soi giỏi | Soi thường |
|---|---|---|
| Thận trọng + giám định | 40,3% | 33,0% |
| Liều + giám định | **45,9%** | 37,4% |
| Combo nhỏ + giám định | 40,6% | |
| Combo lớn + giám định | 38,5% | |

**Lần chơi đầu nới (`FIRST_RUN_LOOSE`, n = 1.500):**

| Hồ sơ soi | Thận trọng | Liều |
|---|---|---|
| Thường | 10,3% | 10,1% |
| Giỏi | 14,5% | 18,5% |
| Ẩu | 7,2% | 8,2% |
| Bỏ qua soi | 6,4% | 8,5% |
| Giỏi chọn rương tối ưu (liều + giám định / thận trọng + giám định) | 40,0% | 31,9% |

Không ván nào thua ở ngày 1; ván thua sớm nhất là ngày 3. Phạt ngày 1 như ngày thường không đổi kết quả (ẩu 8,1%, bỏ qua soi 8,2%). Các cách nới khác đã thử (n = 1.500, bot thường thận trọng / liều): `free1` (hộp đầu tặng) 9,5% và 9,8%, `cheap` 10,3% và 10,1%, giữ `any` 6,7% và 7,1%.

**Soi sai có giá nhưng chơi được.** Soi thường đọc nhầm thật thành giả 2,9% đến 3,0% số món, giả thành thật 1,8% đến 2,5%. Soi ẩu: giả thành thật 4,5% đến 5,1%. Bỏ qua soi: giả thành thật 10,7% đến 10,8%. Doanh thu so với giá đúng: giỏi 1,06, thường 0,98, ẩu 0,94, bỏ qua soi 0,91.

## 3. Độ nhạy (n = 1.000; bot thường thận trọng / liều; giỏi liều + giám định)

Sai số khoảng 1 đến 1,5 điểm. Dòng cơ sở (n = 2.000): bot thường 16,8 / 15,5, giỏi 45,9.

| Thay đổi so với bộ luật cuối | Bot thường | Giỏi |
|---|---|---|
| Phạt sót 5% | 17,3 / 16,9 | 47,1 |
| Phạt sót 15% | 15,3 / 15,4 | 46,7 |
| Dấu lạ 10% | 16,9 / 17,5 | 47,6 |
| Dấu lạ 20% | 15,8 / 14,3 | 46,5 |
| Trần 1,10 | 17,0 / 17,1 | **49,0** |
| 2 vòng trả giá | 15,7 / 15,3 | 45,3 |
| Không có nhà vá lỗi | 16,4 / 15,9 | 45,4 |
| Nhà vá lỗi rẻ (6% / 15%) | 18,3 / 18,3 | **49,9** |
| Nhà vá lỗi đắt (18% / 40%) | 16,2 / 16,1 | 45,8 |
| Phạt giả khai thật 0% | 16,3 / 16,2 | **49,8** |
| Phạt giả khai thật 40% | 16,1 / 16,0 | 45,9 |
| Gợi ý 10% giá nền | 16,3 / 15,9 | 46,7 |
| 1 dấu giả ở mọi bậc | 15,7 / 15,4 | 44,7 |
| 3 dấu giả ở Rất hiếm và Huyền thoại | 17,6 / 18,9 | 46,2 |
| Người soi thường khá hơn (s 85%) | 20,6 / 21,0 | |
| Người soi thường kém hơn (s 60%) | 13,7 / 13,7 | |
| Người soi thường quyết theo giá trị kỳ vọng | 17,0 / 19,0 | |

In đậm là các dòng vượt trần 47%: trần 1,10, nhà vá rẻ và bỏ phạt giả khai thật. Giữ ba tham số đó như đề xuất.

## 4. Phát hiện còn đúng từ bản cũ

- **Khai giả nhầm đắt hơn khai thật nhầm.** Thật mà khai giả mất 80% giá trị, giả mà khai thật mất đúng giá hàng giả, nên người chơi phải khá chắc mới khai giả. Soi giỏi hạ tỉ lệ thật thành giả xuống 0%.
- **Cần có trần trả giá.** Trần cao đẩy người chơi giỏi qua 47%.
- **Ngày 1 miễn phạt đủ để không thua ngày 1**, và kết quả gần như không đổi khi bỏ miễn phạt.

## 5. Phát hiện mới theo luật soi mới

- **Soi lỗi chủ yếu bảo vệ khỏi phạt, ít kiếm thêm tiền.** Món đã trừ giá theo lỗi đúng bằng giá nền × hệ số tình trạng, nên soi giỏi chỉ tránh phạt sót, nhận ra hàng giả, thắng trả giá và vá lỗi có lời. Doanh thu so với giá đúng: giỏi 1,06.
- **Bỏ qua soi vẫn khả thi nhưng tệ nhất**, chủ yếu do bán nhầm hàng giả (11% số món). Muốn đau hơn thì tăng phạt sót hoặc tỉ lệ hàng giả.
- **Gợi ý của Giáo Sư Lẫm ít được dùng** với chiến thuật thường (khoảng 0,03 lần mỗi ván; combo lớn 2 lần nhờ miễn phí): soi giỏi đã đủ chắc nên gợi ý hiếm khi đáng 5% giá nền.
- **Chưa kiểm tra bằng người thật:** các mức tìm lỗi trong 2 phút (95%, 75%, 45%) là giả định. Nếu người thật tìm kém hơn, bot thường có thể xuống dưới 15% (s = 60% cho 13,7%).

## 6. Còn mở

1. **Người chơi giỏi sát trần (45,9% so với 47%).** Nếu chủ dự án muốn dư biên hơn, hạ trần xuống 1,05.
2. **Phần "khó mở hơn, thưởng hậu hĩnh hơn"** là của phần 4 (cây). Cây đo ở [Sprint5CayPhatTrien.md](Sprint5CayPhatTrien.md) dùng luật soi này.
3. **Chọn cách nới lần chơi đầu:** đề xuất `cheap`. Cần Codex chốt cách cài (hai hộp đầu giá rương rẻ nhất) và Claude 1 xác nhận khớp spec mở màn ("ngày 1 lần chơi đầu chỉ bán Rương Gỗ và Rương Vải Niêm").

## 7. Tái hiện

```bash
cd QA
python3 test_inspect.py                # khoảng 40 giây, n = 600
python3 inspect_explore.py 500 hard2   # dò; các nhóm khác: pen, hag, fine, hard, day1
python3 inspect_final.py final         # n = 2.000, ván thường và lần chơi đầu nới (khoảng 6 phút)
python3 inspect_final.py sens          # độ nhạy, n = 1.000
```
