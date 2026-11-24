# Sprint 5 phần 3: soi lỗi và trả giá (Claude 2)

> **Cần đo lại (2026-11-24):** chủ dự án đã chốt luật soi mới (tự tìm chỗ lỗi, bấm vào lỗi thì giá trừ, bảng cho chọn thật hay giả). Số đo dưới đây theo luật cũ. Kế hoạch đo lại: [BanGiaoSprint5SoiLoi.md](BanGiaoSprint5SoiLoi.md).

Viết ngày 2026-11-24. Phần 3 của việc cân bằng Sprint 5: lượt soi, phí soi thêm, mức phạt và mức lỗ khi soi sai thật giả, giá ép khi soi thiếu, bán nhanh, và giá trị của trả giá kiểu Ace Attorney theo 5 bậc (Thường bán nhanh, Hiếm trở lên trả giá đầy đủ). **Mọi con số dưới đây là ĐỀ XUẤT, chưa chốt.** Điều cần chủ dự án quyết nằm ở mục 7. Luật gốc: [soi-loi-tra-gia](../../docs/superpowers/specs/2026-11-23-soi-loi-tra-gia-design.md). Cách đo và giả định: [spec cách đo](../../docs/superpowers/specs/2026-11-24-can-bang-soi-loi-design.md). Nền: 4 rương cơ bản đã chốt ([Sprint5Ruong.md](Sprint5Ruong.md)).

> **Tiền tệ:** mọi số tiền ghi bằng **đồng Việt Nam**. Mô phỏng tính theo đơn vị 10.000 đ (`extra_fee` H=20 là 200.000 đ) và in qua `QA/currency.py`. Mọi mức là phần trăm của giá thẩm định, làm tròn về bội số 10.000 đ; phí cố định là bội số 10.000 đ.

## Kết luận ngắn

1. **Nghiệm thu đạt** trên ván thường (n = 2.000). Bot thường (soi kiểu thường, không mua lượt thêm) thắng **15,2% (thận trọng) và 16,2% (liều)**. Người chơi tính toán hoàn hảo thắng **35,4% đến 45,0%** (trần 47%).
2. **Kỹ năng soi có giá trị rõ.** Cùng cách mua rương (trung bình hai bot): soi giỏi **22,5%**, soi thường **15,7%**, bỏ qua soi **14,1%**, soi ẩu **12,0%**. Với người chơi chọn rương hoàn hảo, soi giỏi hơn soi thường **9 đến 12 điểm** (38,0% / 45,0% so với 28,6% / 33,2%).
3. **Soi sai có giá nhưng game vẫn chơi được.** Bot soi thường mất khoảng 1.600.000 đến 2.300.000 đ tiền phạt và 1.100.000 đến 1.700.000 đ tiền khai nhầm mỗi ván. Người soi ẩu mất 2.400.000 đến 3.600.000 đ tiền phạt mỗi ván và vẫn thắng 11,7% đến 12,2%.
4. **Ngày 1 lần chơi đầu không thua ngay**: 0 ván thua ở ngày 1 hay trước ngày 3, với mọi hồ sơ soi, có hay không miễn phạt ngày 1 (n = 1.500). Nợ tồn cuối ngày 1 xấu nhất 1.920.000 đ ở lần chơi đầu và 2.380.000 đ ở ván thường, xa ngưỡng thua 10.000.000 đ. Lần chơi đầu khó hơn phần 1 một chút: bot thường 6,9% / 7,5% (phần 1: 8,1% / 10,1%).
5. **Luật phạt gọn nhất là tịch thu.** Món giả mà khai thật thì cửa hàng giữ món và không trả đồng nào. Phạt nặng hơn (30% trở lên) làm bot thường tụt dưới 15%. Phạt nhẹ hơn (10% trở xuống) làm người chơi giỏi vượt 47% và làm việc bỏ qua soi gần như không thiệt (phạt 0%: bỏ qua soi ngang soi thường).
6. **Phần thưởng của trả giá phải có trần.** Trần là đòn bẩy mạnh nhất lên người chơi giỏi: trần 120% cho 49,2%, trần 112% cho 45,0% đến 46,6%, trần 110% cho 44,5% (n = 1.000). Bot thường gần như không chạm trần.
7. **Hai phát hiện thiết kế.** (a) Không soi gần bằng soi thường (kém 1,3 đến 1,9 điểm) và hơn soi ẩu. Nếu có tùy chọn bỏ qua hẳn soi lỗi, cần trả thấp hơn bán nhanh (ở 90%, người bỏ qua soi tụt còn 6,9%). (b) Dấu hiệu rõ hơn chưa chắc làm người chơi thường thắng hơn: họ tin một "dấu lạ" trên món thật, khai giả và lỗ 80%. Khai giả nhầm đắt hơn khai thật nhầm, nên khi còn nghi thì khai thật mới là cách đúng, và giao diện nên nói điều đó.

## 1. Bộ luật đề xuất

**Giá thẩm định (V)** = giá nền × hệ số tình trạng × hiệu ứng tạo tác, đúng bằng giá bán của lõi cũ. Mọi mức dưới đây tính theo V.

| Mục | Thường | Hiếm | Rất hiếm | Huyền thoại |
|---|---|---|---|---|
| Điểm lỗi có nghĩa trên món (tier-do-trong-ruong mục 4) | 2 | 3 | 3 | 4 |
| Lượt soi miễn phí | **1** (soi nhanh) | **3** | **4** | **4** |
| Phí mỗi lượt soi thêm (tối đa 2 lượt mỗi món) | — | **200.000 đ** | **300.000 đ** | **500.000 đ** |
| Cách bán khi khai thật | **bán nhanh 100% V** | trả giá đầy đủ, hoặc bán nhanh 100% V | như Hiếm | như Hiếm |

**Trả giá (Hiếm trở lên):**

| Mục | Đề xuất |
|---|---|
| Giá chào mở màn (giá ép khi chưa đưa bằng chứng nào) | **95% V** |
| Mỗi lần đưa đúng bằng chứng | **+8% V** |
| Mỗi lần đưa sai bằng chứng | **−5% V** |
| Số vòng | **3** |
| Trần, sàn giá chốt | **112% V**, **70% V** |
| Bỏ trả giá giữa chừng | chốt **giá chào hiện tại** (không quay về giá bán nhanh) |

Soi thiếu thì bị ép theo hai đường. Giá mở màn thấp hơn bán nhanh 5%, nên vào trả giá mà không có bằng chứng thì lỗ so với bán nhanh. Mỗi bằng chứng thiếu là một lần +8% không lấy được. Có 1 bằng chứng đúng thì đạt 103% V; có đủ 3 bằng chứng đúng thì đạt 112% V (trần).

**Soi sai thật giả:**

| Thực tế | Người chơi khai | Kết quả đề xuất |
|---|---|---|
| Thật | Giả | **Bán lỗ**: chỉ nhận giá hàng giả **20% V** (lỗ 80% so với giá thật) |
| Giả | Thật | **Tịch thu**: cửa hàng kiểm tra lại, giữ món, **không trả đồng nào** (phạt 20% V, đúng bằng giá hàng giả). Ngày 1: **miễn phạt**, vẫn nhận 20% V |
| Giả | Giả | Nhận 20% V (giá hàng giả, như cũ) |
| Thật | Thật | Bán nhanh hoặc trả giá như trên |
| Sai tình trạng | | Khai thấp hơn thật: trả theo mức khai. Khai cao hơn thật: bị bắt bẻ, mất 5% V |

**Gợi ý một lỗi của Giáo Sư Lẫm** (thay giám định bằng chữ): một bằng chứng chắc đúng, phí **5% giá nền, tối thiểu 200.000 đ**. Combo CB3 vẫn cho một lần miễn phí mỗi ngày như cũ. Chứng nhận +25% của giám định cũ **bỏ**, vì phần thưởng giá bây giờ nằm ở trả giá (mục 7, câu hỏi 3 và 4).

**Ví dụ bằng tiền.** Món Rất hiếm có V = 28.000.000 đ: bán nhanh 28.000.000 đ; trả giá mở màn 26.600.000 đ, mỗi bằng chứng đúng +2.240.000 đ, đưa sai −1.400.000 đ, trần 31.360.000 đ, sàn 19.600.000 đ; khai giả còn 5.600.000 đ; giả mà khai thật thì 0 đ. Lượt soi thêm 300.000 đ. Món Thường V = 3.000.000 đ: bán nhanh 3.000.000 đ, khai giả 600.000 đ, giả mà khai thật 0 đ.

Vì sao khi còn nghi nên khai thật: với món Rất hiếm và 3 bằng chứng, khai thật đáng hơn khai giả cho đến khi xác suất giả vượt khoảng 82%. Khai giả nhầm mất 80% V cộng phần trả giá; khai thật nhầm chỉ mất 20% V.

## 2. Số đo trên ván thường (n = 2.000, không sắp sẵn)

Lõi mô phỏng đã duyệt (nợ, phạt nợ 20%, giá rương dao động ±20%, manh mối, phục chế, tạo tác, combo), bảng 4 rương đã chốt, Rương Đen bán từ ngày 4. Sai số khoảng ±1,6 điểm ở mức 15% và ±2,2 điểm ở mức 45%.

**Bot thường** (mua rương đắt nhất mua được) theo hồ sơ soi:

| Hồ sơ soi | Thận trọng | Liều | Lượt soi mỗi món | Lượt thêm mỗi ván | Món đi trả giá | Thật khai giả | Giả khai thật | Tiền bị tịch thu mỗi ván (liều) | Lỗ khai nhầm mỗi ván (liều) | Thu so với giá thẩm định |
|---|---|---|---|---|---|---|---|---|---|---|
| giỏi | **21,6** | **23,4** | 2,7 đến 3,0 | 0,9 đến 1,1 | 50% đến 55% | 0,4% đến 0,6% | 6% | 1.650.000 đ | 1.030.000 đ | 1,07 |
| **thường (bot thường)** | **15,2** | **16,2** | 2,4 đến 2,7 | 0 | 31% đến 35% | 1,5% đến 1,6% | 7% | 2.250.000 đ | 1.700.000 đ | 0,98 |
| ẩu | 12,2 | 11,7 | 1 | 0 | 0% | 0% | 11% đến 12% | 3.560.000 đ | 0 đ | 0,94 |
| bỏ qua soi | 13,9 | 14,3 | 0 | 0 | 0% | 0% | 12% | 3.830.000 đ | 0 đ | 0,96 |

**Người chơi tính toán hoàn hảo** (chọn rương theo lời kỳ vọng, đọc manh mối, λ = 0,75, dùng gợi ý của Giáo Sư Lẫm khi đáng):

| Hồ sơ soi | Thận trọng | Liều | Combo nhỏ | Combo lớn | Gợi ý mỗi ván |
|---|---|---|---|---|---|
| giỏi | 38,0 | **45,0** | 38,4 | 35,4 | 1,1 đến 2,4 |
| thường | 28,6 | 33,2 | — | — | 2,3 đến 2,9 |

So với phần 1 (chưa có soi lỗi): bot 17,4% / 18,4% và người chơi giỏi 38,7% / 44,1% / 38,8% / 38,4%. Lớp soi lỗi lấy của bot thường khoảng 2 điểm (phạt và khai nhầm) và giữ người chơi giỏi gần như cũ. Phần chứng nhận +25% mất đi được trả giá bù lại.

**Tiền mất nằm ở đâu** (bot liều, n = 1.000, so với giá đúng: giá thẩm định nếu thật, giá hàng giả nếu giả). Người soi thường để lọt 56% món Rất hiếm giả (chủ yếu của Rương Đen), mất khoảng 1.760.000 đ mỗi ván. Món Thường tốn khoảng 290.000 đ mỗi ván, chủ yếu vì khai nhầm tình trạng. Trả giá của người soi thường chỉ đạt 1,00 đến 1,01 lần giá thẩm định; người soi giỏi đạt 1,09 đến 1,10 lần và để lọt 32% món Rất hiếm giả (có chủ ý: khi còn nghi thì khai thật vẫn lời hơn). Hệ quả là Rương Đen thành **rương kỹ năng**: người soi giỏi có lời, người soi kém bị lỗ.

## 3. Lần chơi đầu và ngày 1 (n = 1.500)

Lần chơi đầu đã chốt (`chest_sim.FIRST_RUN`): hộp ngày 1 sắp sẵn (hộp đầu rác, hộp hai đồ Thường thật), ngày 1 chỉ bán Gỗ và Vải Niêm, ngày 1 miễn phạt.

| Hồ sơ soi | Bot thận trọng | Bot liều | Thua ở ngày 1 | Thua trước ngày 3 | Nợ tồn cuối ngày 1 xấu nhất (liều) | Ngày thua sớm nhất |
|---|---|---|---|---|---|---|
| giỏi | 10,1 | 13,5 | 0 | 0% | 1.780.000 đ | 3 |
| thường | 6,9 | 7,5 | 0 | 0% | 1.920.000 đ | 3 |
| ẩu | 5,5 | 6,0 | 0 | 0% | 1.920.000 đ | 3 |
| bỏ qua soi | 5,7 | 7,7 | 0 | 0% | 1.820.000 đ | 3 |
| ẩu, ngày 1 phạt như ngày thường | | 5,9 | 0 | 0% | 2.210.000 đ | 3 |
| bỏ qua soi, ngày 1 phạt như ngày thường | | 7,5 | 0 | 0% | 2.030.000 đ | 3 |

Người chơi giỏi ở lần chơi đầu: 24,5% (thận trọng) và 33,6% (liều), phần 1 là 26,5% và 35,7%.

Ván thường, ngày 1 (bot liều): 0 ván thua trước ngày 3 ở mọi hồ sơ, nợ tồn ngày 1 xấu nhất 2.380.000 đ.

Theo luật nợ, ngày 1 không thể thua (nợ ngày 1 là 2.000.000 đ, có phạt nợ 20% thì nợ tồn tối đa 2.400.000 đ, dưới ngưỡng 10.000.000 đ), và mô phỏng xác nhận điều đó. Miễn phạt ngày 1 chỉ giảm nợ tồn ngày 1 xấu nhất 200.000 đến 300.000 đ. Nên giữ miễn phạt vì cảm giác ván đầu, không phải vì nguy cơ thua.

## 4. Độ nhạy (n = 1.000, ván thường)

*Đo trên bản ngay trước bản cuối: món Thường giả khai thật bị phạt 10% thay vì tịch thu, gợi ý của Giáo Sư Lẫm theo phí giám định cũ 15%. Lượt dò cho thấy hai điểm này chỉ lệch dưới 1 điểm.* Ở n = 1.000, hàng gốc (các luật không đổi hồ sơ tương ứng) là: soi thường 14,6% / 15,9%, bỏ qua soi 13,4% / 13,5%, bot soi giỏi thận trọng 21,5%, người chơi giỏi liều 46,6%.

| Thay đổi | Soi thường (thận trọng / liều) | Bỏ qua soi (thận trọng / liều) | Bot soi giỏi (thận trọng) | Người chơi giỏi (liều) | Tiền phạt mỗi ván (soi thường, liều) |
|---|---|---|---|---|---|
| Phạt 0% (giả khai thật vẫn nhận 20% V) | 15,9 / 18,5 | 16,4 / 16,6 | 23,5 | **49,5** | 0 đ |
| Phạt 10% | 15,3 / 17,3 | 14,8 / 15,3 | 22,3 | 47,2 | 1.070.000 đ |
| **Phạt 20% (tịch thu, đề xuất)** | 14,6 / 15,9 | 13,4 / 13,5 | 21,5 | 46,6 | 2.120.000 đ |
| Phạt 30% | 13,4 / 15,7 | 12,0 / 11,4 | 20,5 | 46,4 | 3.110.000 đ |
| Phạt 40% | 13,1 / 14,7 | 10,8 / 11,1 | 19,3 | 46,4 | 4.040.000 đ |
| Phạt 60% | **11,9 / 13,0** | 9,1 / 8,2 | 19,9 | 46,1 | 5.900.000 đ |
| Trần 110% | 13,9 / 15,9 | 13,4 / 13,5 | 18,3 | 44,5 | |
| Trần 115% | 15,0 / 16,2 | 13,4 / 13,5 | 21,1 | 47,9 | |
| Trần 120% | 15,3 / 16,3 | 13,4 / 13,5 | 23,3 | **49,2** | |
| 2 vòng thay vì 3 | 14,3 / 14,0 | 13,4 / 13,5 | 20,2 | 46,8 | |
| Giá chào mở màn 90% | 12,3 / 13,9 | 13,4 / 13,5 | 20,2 | 44,9 | |
| Lượt soi 1 / 3 / 3 / 3 | 13,8 / 12,4 | 13,4 / 13,5 | 20,7 | 46,6 | |
| Lượt soi 1 / 4 / 5 / 5 | 14,4 / 16,6 | 13,4 / 13,5 | 19,2 | 46,1 | |
| Không bán lượt soi thêm | 14,6 / 15,9 | 13,4 / 13,5 | 20,3 | 46,3 | |
| Lượt soi thêm miễn phí | 14,6 / 15,9 | 13,4 / 13,5 | 21,8 | 47,6 | |
| Giữ chứng nhận +25% | 14,6 / 15,9 | 13,4 / 13,5 | 21,5 | 46,6 | |
| Dấu hiệu rõ hơn (nhiễu 15%, dấu lạ trên món thật 5%) | **12,0 / 13,4** | 13,4 / 13,5 | 22,3 | 47,4 | 860.000 đ |
| Dấu hiệu mờ hơn (nhiễu 35%, dấu lạ 15%) | 15,3 / 14,3 | 13,4 / 13,5 | 19,4 | 46,5 | 1.810.000 đ |
| Bán không soi trả 90% V | 14,6 / 15,9 | **6,9 / 6,9** | 21,5 | 46,6 | |
| Người soi thường khá hơn (trúng 75%, đọc nhầm 7%) | 16,0 / 16,8 | | | | |
| Người soi thường kém hơn (trúng 55%, đọc nhầm 15%) | 11,4 / 12,7 | | | | |
| Người soi thường quyết theo giá trị kỳ vọng | 14,8 / 14,9 | | | | |

Gợi ý của Giáo Sư Lẫm (n = 1.000, người chơi giỏi thận trọng / liều): phí 15% gần như không ai mua (0,01 lần mỗi ván); 5% được mua 1,2 đến 1,5 lần mỗi ván, thắng 38,9% / 45,1%; 3% được mua 1,4 đến 1,7 lần mỗi ván, thắng 40,3% / **47,4%**.

Đọc bảng:

- **Mức phạt** chia hai phía. Phạt nhẹ thì người chơi giỏi vượt trần và bỏ qua soi không thiệt. Phạt nặng thì bot thường tụt dưới 15%. Tịch thu (20%) là điểm giữa, và người chơi dễ hiểu nhất.
- **Trần trả giá** điều chỉnh người chơi giỏi mà gần như không chạm bot thường. Nếu chủ dự án muốn biên rộng hơn dưới 47%, hạ trần xuống 110%; đổi lại soi giỏi bớt giá trị (bot soi giỏi 21,5% xuống 18,3%).
- **Lượt soi và lượt soi thêm** chỉ lệch 1 đến 2 điểm, nên chọn theo thời lượng chơi. Bán lượt soi thêm chủ yếu để tạo quyết định "trả tiền để chắc hơn", không phải để cân bằng.
- **Chứng nhận +25%** không đổi số (gợi ý có phí 15% gần như không ai mua), nên quyết theo thiết kế.
- **Dấu hiệu** (việc của Claude 1 và Antigravity) ảnh hưởng ngang mức phạt. Dấu lạ trên món thật càng hiếm thì mỗi dấu giả càng "đáng tin", nên người chơi thường càng dễ khai giả nhầm.
- **Giả định về người chơi thường** lệch ±1,5 điểm khi người đó khá hơn hoặc quyết khác. Người soi kém hẳn (trúng 55%) xuống 11% đến 13%.

## 5. Khuyến nghị cho nội dung và giao diện

1. **Mỗi món giả có ít nhất 2 dấu giả rõ; mỗi món thật có nhiều nhất 1 "dấu lạ".** Người chơi khi đó có lý do để soi tiếp thay vì kết luận từ một dấu (Claude 1, Antigravity).
2. **Giao diện nhắc trước khi khai giả**, kiểu "một dấu lạ chưa chắc là giả, khai giả mà sai thì chỉ được giá hàng giả". Hiện tỉ lệ hàng giả của rương lúc soi, như TASKS.md đã ghi cho lúc xem hộp.
3. **Món Thường chỉ soi 1 lượt nên gần như không bắt được hàng giả.** Tịch thu món Thường giả là một "thuế" nhỏ (khoảng 2% đến 3% giá trị đồ Thường, bằng tỉ lệ hàng giả nhân giá hàng giả). Muốn món Thường không phạt thì số đo gần như không đổi.
4. **Mục tiêu thời lượng:** mỗi món Hiếm trở lên 3 đến 4 lượt soi cộng tối đa 3 vòng trả giá; người soi giỏi đem 38% đến 55% số món (tính cả món Thường) đi trả giá, còn lại bán nhanh.

## 6. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_inspect            # test khóa, khoảng 45 giây
nohup python3 inspect_final.py final > f.log &  # bảng mục 2 và 3, n = 2.000 và 1.500, khoảng 6 phút
nohup python3 inspect_final.py sens > s.log &   # độ nhạy mục 4, n = 1.000, khoảng 10 phút
python3 inspect_explore.py 1000 q               # các nhóm dò: a, fine, look, hag, ft, cmp, q, r
```

Mã: `QA/inspect_sim.py` (luật `RULES`, hồ sơ người chơi `PROFILES`, `apply()` nạp bảng rương đã chốt rồi thay bước bán `P.handle5`, `restore()` trả lõi về như cũ). `QA/inspect_final.py` chạy bảng cuối, `QA/inspect_explore.py` chạy đường dò.

Đường dò: bản đầu (bán nhanh 95%, mở màn 85%, phạt 30%) cho bot thường 12% đến 13%. Nâng bán nhanh lên 100% và mở màn lên 90% đưa bot lên 14% đến 18%, nhưng người chơi giỏi lên 46% đến 48%. Đổi các bước trả giá (mở màn, bước cộng, bước trừ) gần như không nâng được bot thường, vì người soi thường hiếm khi trả giá vượt bán nhanh. Phần hụt của bot nằm ở hàng giả lọt qua. Đòn bẩy thật là mức phạt (lên bot) và trần (xuống người chơi giỏi). Q1 (phạt 20%, trần 115%) cho 15,3% / 16,3% và 46,6%; R1 (trần 112%) cho 15,3% / 15,8% và 45,3%, chọn R1. Cuối cùng gộp phạt món Thường vào luật tịch thu chung và hạ phí gợi ý xuống 5%.

## 7. Câu hỏi mở cho chủ dự án

1. **Duyệt bộ số đề xuất** ở mục 1 (lượt soi 1 / 3 / 4 / 4, soi thêm 200.000 / 300.000 / 500.000 đ, bán nhanh 100%, trả giá 95% + 8% / −5%, 3 vòng, trần 112%, khai giả 20%, tịch thu, ngày 1 miễn phạt).
2. **Có tùy chọn bỏ qua hẳn soi lỗi không** (spec câu hỏi 4)? Nếu có và trả như bán nhanh, người bỏ qua soi chỉ kém người soi thường 1,3 đến 1,9 điểm, và hơn người soi ẩu. Đề xuất: không có tùy chọn này, hoặc "bán không soi" chỉ trả 90% V (bỏ qua soi còn 6,9%).
3. **Gợi ý một lỗi của Giáo Sư Lẫm**: phí giám định cũ (15% giá nền) làm dịch vụ gần như không ai dùng. Đề xuất phí riêng 5% giá nền, tối thiểu 200.000 đ. Phí 3% đẩy người chơi giỏi lên 47,4%.
4. **Bỏ chứng nhận +25%** của giám định cũ (số đo không đổi; giữ thì có hai cơ chế thưởng giá chồng nhau). Claude 1 cập nhật `Docs/GameDesign` và `content.json` nếu đồng ý.
5. **Bỏ trả giá giữa chừng** chốt giá chào hiện tại (spec: "bỏ qua trả giá bất cứ lúc nào, chấp nhận giá thấp hơn"). Nếu được quay về giá bán nhanh, trả giá không còn rủi ro và bước trừ khi đưa sai mất ý nghĩa.
6. **Rương Đen thành rương kỹ năng**: người soi kém lỗ nặng nhất ở món Rất hiếm giả của Rương Đen. Có thể là điều chủ dự án muốn ("đánh cược lớn"), và không cần đổi bảng rương đã chốt.
7. **Lần chơi đầu khó hơn phần 1 một chút** (bot thường 6,9% / 7,5%, phần 1 là 8,1% / 10,1%). Đây là tiếp nối của câu hỏi còn mở ở phần 1: lần chơi đầu có cần dễ ngang ván thường không.
8. **Biên dưới trần 47%**: người chơi giỏi 45,0% (n = 2.000), 46,6% (n = 1.000). Muốn biên rộng hơn thì hạ trần xuống 110% (44,5%), đổi lại soi giỏi bớt giá trị.
9. **Rương mở khóa và rương chủ đề** (phần 2, chưa chốt) chưa được đo cùng soi lỗi. Khi phần 2 được duyệt, chạy lại với `inspect_sim.apply(design=...)`.
10. **Nội dung dấu hiệu** (Claude 1, Antigravity): mô hình đang giả định món giả hiện dấu giả 85%, món thật có dấu lạ 10%, chỗ nhiễu giống dấu giả 25%. Khi có hồ sơ lỗi thật của 38 món, đo lại bằng tỉ lệ thật.
