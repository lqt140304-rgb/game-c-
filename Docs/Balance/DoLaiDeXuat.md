# Đo lại đề xuất của Claude 1: TC3, CB3, GT1, chứng nhận giám định (Claude 2)

Claude 1 đề xuất điều chỉnh để ván khó và nhiều đánh đổi hơn ([DieuChinhSauCanBang.md](https://github.com/lqt140304-rgb/game-c-/blob/claude/relaxed-cerf-wgnu7l/Docs/Content/DieuChinhSauCanBang.md), commit `6dc386a`, nhánh `claude/relaxed-cerf-wgnu7l`) và nhờ Claude 2 đo bốn thay đổi chưa được đo: **TC3, CB3, GT1 mới và chứng nhận giám định +25%**. Tài liệu này đo chúng, đo luôn đấu giá và cầm tạo tác theo luật dịch vụ bản mới, rồi đề xuất một gói số liệu.

Nhánh của Claude 1 chưa gộp vào nhánh này nên Claude 2 đo theo mô tả trong tài liệu của Claude 1.

## Kết luận ngắn

1. **Chứng nhận giám định là đòn bẩy rất mạnh.** Nguyên văn đề xuất (phí 10%, +25%) nâng bot từ 15% lên 30% (thận trọng) và 42% (liều). Giám định gần như luôn có lời nên không còn là quyết định. Đề xuất **phí 15%**, khi đó giám định có lời nhưng phải chọn món.
2. **CP1 +10% và CB1 +20% cho mọi hiện vật quá mạnh khi cộng cùng chứng nhận.** Combo nhỏ thắng 40% so với 15% của bot không đầu tư, nên đầu tư thành bắt buộc. Đề xuất **CP1 +5%, CB1 +10%**.
3. **TC3 và CB3 hiện là bẫy.** Đi tới combo lớn tốn thêm 1.900 xu cho CP3 và TC3 nhưng thắng ít hơn combo nhỏ ở mọi mức thưởng đã thử. Cần **hạ giá tier 3 xuống khoảng 500 xu** và cho CB3 thưởng giá bán, như mục 3.
4. **GT1 mới gần như không đổi tỷ lệ thắng** (+0,1 đến +1,8 điểm), trừ khi săn combo dùng sớm để có vốn mua tạo tác (+3,8). Đây là lựa chọn nhẹ chứ không áp đảo, ổn với mục tiêu.
5. **Đấu giá k kỳ vọng 1,40 quá mạnh** (+17 điểm). Đề xuất **k kỳ vọng 1,16** (+3 đến +6 điểm), khớp với kết luận ở [DoKho.md](DoKho.md).
6. **Cầm tạo tác vô hại** (−0,1 đến +0,9 điểm). **Trả nợ tồn sớm không đổi gì** về kinh tế (mục 5).

Gói đề xuất ở mục 6 cho: bot thận trọng 15%, bot liều 17%, bot đầu tư 26%, người giám định chọn lọc 24% đến 32%, combo nhỏ và combo lớn ngang nhau (32%).

## Cách tái hiện

```bash
cd QA
python3 proposal_sim.py            # mục 1, ~5 phút
python3 proposal_sweeps.py         # mục 2 và 4, ~8 phút
python3 sweeps2.py                 # mục 3
python3 -m unittest -v test_proposal   # khóa gói đề xuất
```

## Giả định của Claude 2

Luật chưa nói rõ nên Claude 2 đặt giả định. Mọi giả định cần Claude 1 xác nhận.

- **GT1:** chỉ hoãn **kỳ của ngày đó** (không hoãn nợ tồn). Kỳ hoãn thành khoản `kỳ x 1,15` trả cùng kỳ ngày sau, **không bị phạt 20%** và **không tính vào ngưỡng thua 1.000 xu**. Bot dùng khi sắp thiếu tiền và chưa dùng.
- **Chứng nhận:** hiện vật thật đã giám định cộng 25% giá bán, cộng theo phép cộng cùng các hiệu ứng giá bán. Hàng giả vẫn bán 20%.
- **Phục chế:** nâng một bậc chất lượng (0,3, 0,7, 1,0), phí 25% giá nền. Bot phục chế khi lợi kỳ vọng lớn hơn phí.
- **Giám định chọn lọc:** bot giám định khi `thưởng chứng nhận x giá nền x chất lượng x xác suất thật > phí`. Chỉ cần đọc chất lượng (đã thấy khi soi), không cần đọc manh mối, nên đây là cận dưới của "người biết tính toán".
- **CB3 (bản của Claude 1):** giám định miễn phí 1 hiện vật mỗi ngày. Mốc cộng hưởng theo bản mới trong GiaTruyenTaoTacBiKhe.md.
- Giá nền hệ số 0,85 theo bảng của Claude 1. Kho 8 ô, bàn mở 3 hộp và lưu tự động **không mô phỏng** (bot bán ngay nên không giữ hàng).
- 3.000 đến 4.000 seed, nên sai số khoảng ±0,7 điểm.

## 1. Đề xuất của Claude 1 nguyên văn (giá nền hệ số 0,85)

| Chiến thuật | Thắng | Ngày hụt tiền TB | CB1 | CB3 | Giám định TB mỗi ván |
| --- | --- | --- | --- | --- | --- |
| Thận trọng | 15,2% | 1,61 | | | |
| Thận trọng + giám định chọn lọc | 31,1% | 1,35 | | | 10,2 |
| Liều | 17,4% | 1,78 | | | |
| Liều + giám định chọn lọc | **42,4%** | 1,33 | | | 13,1 |
| Combo nhỏ (CB1) | **40,0%** | 1,19 | 57% | | |
| Combo nhỏ + giám định | **48,6%** | 1,01 | 62% | | 13,2 |
| CB1 + TC3 (không CB3) | 35,6% | 1,27 | 57% | | |
| Combo lớn (CB1 + CB3) | 33,6% | 1,30 | 57% | 31% | |
| Combo lớn + giám định | 44,2% | 1,09 | 62% | 39% | 12,8 (miễn phí 1,5) |

Bot thường đúng mục tiêu 15%. Nhưng **đầu tư tạo tác không còn là lựa chọn**: combo nhỏ thắng gấp 2,7 lần bot thận trọng, và người biết giám định chạm 42% đến 49%, chưa cần đọc manh mối. Mục tiêu "khoảng 30% nếu biết đầu tư" của Claude 1 bị vượt 10 đến 19 điểm.

## 2. Chứng nhận giám định

Thắng% của bốn chiến thuật khi đổi phí giám định và thưởng chứng nhận. Các cột combo dùng CP1 +10%, CB1 +20% (đề xuất nguyên văn).

| Phí | Thưởng | Thận trọng | + giám định | Liều + giám định | Combo nhỏ + giám định |
| --- | --- | --- | --- | --- | --- |
| 10% | +25% (nguyên văn) | 14,7% | 30,2% | 42,3% | 49,0% |
| 15% | +25% | 14,7% | **24,1%** | **31,7%** | 43,5% |
| 20% | +25% | 14,7% | 17,6% | 22,8% | 39,2% |
| 15% | +35% | 14,7% | 32,9% | 44,4% | 50,0% |
| 20% | +35% | 14,7% | 25,9% | 35,1% | 43,9% |

- Với phí 10% và +25%, bot giám định tất cả món từ Xước trở lên và được lời gần như chắc chắn. Không có đánh đổi.
- Điểm giám định bắt đầu là **quyết định** khi lợi ích kỳ vọng ngang phí: phí 15% làm bot chỉ giám định khoảng 7,7 món mỗi ván thay vì 10,2 (nguyên văn) và chỉ chọn món chất lượng cao hoặc ít rủi ro hàng giả.
- Phí 20% và +25% làm giám định gần như vô nghĩa (+3 điểm). Phí 15% là điểm ngọt.
- Liều + giám định luôn cao hơn thận trọng + giám định 8 đến 12 điểm. Rủi ro vẫn chưa bị phạt đủ. Đây là vấn đề O-01 còn tồn tại (xem mục 7).

## 3. TC3 và CB3

Với giá tier 3 gốc (CP3 900, TC3 1.000) và CP1 +5%, CB1 +10%, phí 15%:

| Phương án | Thắng |
| --- | --- |
| Combo nhỏ + giám định (CP1, CP2) | **32,1%** |
| CB1 + TC3, không CB3 (ba món) | 23,2% (không giám định) so với combo nhỏ 26,2%: TC3 lỗ 3 điểm |
| Combo lớn + giám định (CP1, CP2, TC3, CP3) | 27,1% |

Thử làm TC3 và CB3 mạnh hơn (đo trên cùng cấu hình, combo lớn + giám định):

| Thay đổi | Combo lớn + giám định | So với combo nhỏ + giám định (32,1%) |
| --- | --- | --- |
| Gốc | 27,1% | −5,0 |
| CB3 giám định miễn phí không giới hạn | 28,3% | −3,8 |
| CB3 thêm +10% giá bán mọi hiện vật | 27,9% | −4,2 |
| CB3 +15% giá bán | 28,5% | −3,6 |
| CB3 +30% giá bán, miễn phí không giới hạn, TC3 +20% | 30,2% | −1,9 |
| CB3 +40% giá bán, miễn phí không giới hạn, TC3 +20% | 30,2% | −1,9 |

**Dù nâng thưởng thế nào, combo lớn vẫn thua combo nhỏ**, và thưởng +30% đến +40% đã bão hòa. Nguyên nhân là **thời gian hoàn vốn**: CP3 và TC3 cộng 1.900 xu, bot chỉ mua được vào khoảng ngày 4 đến 5, nên chỉ còn 2 đến 3 ngày để thu lại. Tăng hiệu ứng không đủ, cần hạ giá.

| Giá CP3 / TC3 | Combo lớn + giám định |
| --- | --- |
| 900 / 1.000 (gốc) | 27,1% |
| **500 / 500 và CB3 +15% giá bán, miễn phí không giới hạn** | **32,4%** (ngang combo nhỏ 32,1%) |

Với giá 500 và CB3 như trên, đi combo nhỏ hay combo lớn cho kết quả ngang nhau. Đây là đánh đổi thật: combo nhỏ rẻ và chắc, combo lớn tốn thêm nhưng mở ô thứ 4 qua BK1 và có thêm phần thưởng cuối ván.

**TC3 +10% cho nhóm Hiện đại, Tương lai, Ngoài hành tinh:** tăng giá chỉ chạm ba trong sáu nhóm nguồn gốc, nên tác động nhỏ. Nâng lên +20% hoặc +30% chỉ thêm 0,5 đến 1,1 điểm. TC3 vẫn là món kém nhất. Đề xuất giữ +10%, hạ giá xuống 500 và coi nó là bước đệm đến CB3.

## 4. GT1 mới (hoãn nợ)

| | Thận trọng | Liều | Combo nhỏ | Combo lớn |
| --- | --- | --- | --- | --- |
| GT1 (bot dùng khi sắp thiếu tiền) | 15,3% | 17,8% | 40,5% | 34,5% |
| GT2 (không hiệu ứng tiền) | 15,2% | 17,0% | 39,3% | 32,7% |
| Chênh | +0,1 | +0,8 | +1,2 | +1,8 |

Dùng GT1 sớm thay vì chờ thiếu tiền:

| Ngày dùng | Thận trọng | Combo nhỏ |
| --- | --- | --- |
| Khi thiếu tiền | 14,7% | 40,5% |
| Ngày 2 | 15,4% | **43,4%** |
| Ngày 3 | 14,8% | 41,0% |
| Ngày 4 | 14,3% | 40,6% |
| Ngày 5 | 14,1% | 40,5% |
| GT2 | 14,6% | 39,6% |

- GT1 hoãn kỳ khi đã thiếu tiền **không cứu được ván**, vì khoản hoãn quay lại cộng 15% và người chơi vẫn thiếu tiền hôm sau.
- GT1 mạnh nhất khi dùng **sớm để có vốn đầu tư** (ngày 2, săn combo +3,8 điểm). Đó là đánh đổi đúng ý Claude 1: trả thêm 15% để có vốn sớm.
- Mức ảnh hưởng nhẹ, ổn với mục tiêu "gia truyền là lựa chọn, không áp đảo". Có thể mạnh hơn nếu muốn GT1 hấp dẫn hơn GT2.
- **Cần Claude 1 chốt:** kỳ hoãn có bị phạt, có tính vào ngưỡng thua không (mô phỏng giả định không), có hoãn được nợ tồn không (mô phỏng giả định không).

## 5. Luật phụ

- **Trả nợ tồn sớm (D-08):** về kinh tế **không đổi gì**. Anh Bảy cuối ngày đã thu toàn bộ tiền mặt cho kỳ, kể cả nợ tồn, nên trả sớm hay trả cuối ngày đều mất cùng số tiền. Tác dụng duy nhất là thao tác, không phải cân bằng. Giữ hay bỏ đều được.
- **Cầm tạo tác:** combo nhỏ + giám định 48,5% (−0,1), combo lớn + giám định 45,1% (+0,9). Vô hại, nhưng cũng không thành lựa chọn lớn vì cầm tạo tác chỉ có ích khi sắp thiếu tiền mà lúc đó thường đã quá muộn.
- **Kho 8 ô, bàn mở 3 hộp, lưu tự động:** không mô phỏng được (bot bán ngay). Tác động thật chỉ đo được khi người chơi giữ hàng.

## 6. Đấu giá bản mới (k kỳ vọng 1,40, món từ 1.300 xu, sàn 1,2 G)

| k kỳ vọng | Thận trọng | Liều | Combo nhỏ + giám định |
| --- | --- | --- | --- |
| Không có đấu giá | 14,7% | 17,4% | 49,0% |
| 1,16 | 20,9% (+6,2) | 19,7% (+3,0) | 48,4% (−0,6) |
| 1,26 | 23,5% (+8,8) | 23,2% (+6,5) | 50,0% (+1,0) |
| 1,40 (bản mới của Claude 1) | 32,1% (+17,4) | 33,5% (+16,8) | 54,0% (+5,0) |

Ở ván khó với món từ 1.300 xu, k = 1,40 gần như tăng gấp đôi tỷ lệ thắng của bot thường. Bản mới này dựa trên kết quả Sprint 4 đo ở cấu hình dễ, trước khi chủ dự án đặt mục tiêu ván khó. Đề xuất **k kỳ vọng 1,16** (ví dụ k = 0,7 / 1,0 / 1,3 / 1,6 với xác suất 15% / 35% / 35% / 15% cho kỳ vọng khoảng 1,16).

## 7. Gói đề xuất của Claude 2

| Hạng mục | Đề xuất của Claude 1 | Đề xuất của Claude 2 |
| --- | --- | --- |
| Giá nền | Hệ số 0,85 | **Giữ** (bot 15%) |
| Phí giám định | 10% giá nền | **15%** |
| Chứng nhận | +25% hiện vật thật | **Giữ** |
| CP1 | +10% mọi hiện vật | **+5%** |
| CB1 | +20% mọi hiện vật | **+10%** |
| CP3 và TC3 | Giá 900 và 1.000 | **Giá 500 và 500** |
| TC3 | +10% ba nhóm | Giữ |
| CB3 | Giám định miễn phí 1 hiện vật mỗi ngày | **Miễn phí không giới hạn, thêm +15% giá bán mọi hiện vật** |
| GT1 hoãn nợ | Theo đề xuất | **Giữ** (cần chốt ba điểm ở mục 4) |
| Đấu giá | k 1,40, món từ 1.300 | **k 1,16**, món từ 1.300 |
| Cầm tạo tác | Cho phép | Giữ |
| Trả nợ tồn sớm | Cho phép | Giữ (tiện thao tác, không đổi cân bằng) |

Kết quả của gói này (4.000 seed):

| Chiến thuật | Thắng | Hụt TB |
| --- | --- | --- |
| Thận trọng | 15,2% | 1,61 |
| Thận trọng + giám định chọn lọc | 24,4% | 1,48 |
| Liều | 17,4% | 1,78 |
| Liều + giám định chọn lọc | 32,4% | 1,53 |
| Combo nhỏ (CB1) | 26,2% | 1,43 |
| Combo nhỏ + giám định | 32,1% | 1,33 |
| CB1 + TC3 | 25,5% | 1,46 |
| Combo lớn (CB1 + CB3) | 26,9% | 1,45 |
| Combo lớn + giám định | 32,4% | 1,34 |

So với mục tiêu của Claude 1 và chủ dự án:

- **Bot thường khoảng 15%:** thận trọng 15,2%, liều 17,4%. Đạt.
- **Bot đầu tư khoảng 30%:** 26% không giám định, 32% có giám định. Đạt.
- **Người biết tính cao hơn bot:** giám định chọn lọc thêm 9 đến 15 điểm. Đạt.
- **Đầu tư đáng tiền nhưng không bắt buộc:** combo nhỏ hơn thận trọng 11 điểm, không quá 16. Đạt.
- **Có lựa chọn thật giữa các hướng đầu tư:** combo nhỏ và combo lớn ngang nhau. Đạt.
- **Còn thiếu:** liều + giám định (32,4%) ngang các hướng đầu tư và cao hơn thận trọng + giám định (24,4%) 8 điểm. Rủi ro chưa bị phạt khi có giám định. Để sửa cần cơ chế phạt rủi ro (hộp lỗ kỳ vọng, hoặc hàng giả nhiều hơn ở hộp đắt) mà Claude 1 và chủ dự án quyết.

Các tiêu chí này được khóa trong `QA/test_proposal.py`.

## 8. Lỗi và phát hiện

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-20 | Cao | Phí giám định 10% làm giám định gần như luôn có lời, bot thắng 30% đến 42% chưa cần đọc manh mối | Phí 15% | Claude 1, chủ dự án |
| D-21 | Cao | CP1 +10% và CB1 +20% cùng chứng nhận làm combo nhỏ thắng 40%, đầu tư thành bắt buộc | CP1 +5%, CB1 +10% | Claude 1 |
| D-22 | Trung bình | Tier 3 (CP3, TC3, CB3) không hoàn vốn trong 7 ngày, combo lớn thua combo nhỏ ở mọi mức thưởng đã thử | Hạ giá còn 500, CB3 thưởng giá bán +15% | Claude 1 |
| D-23 | Trung bình | Đấu giá k 1,40 làm bot thường thắng gấp đôi | k kỳ vọng 1,16 | Claude 1 |
| D-24 | Thấp | GT1 chưa nói rõ: kỳ hoãn có bị phạt, có vào ngưỡng thua, có hoãn được nợ tồn không | Chốt ba điểm | Claude 1 |
| O-01 | Cân bằng | Liều vẫn không bị phạt khi có giám định | Cơ chế phạt rủi ro | Claude 1, chủ dự án |

## 9. Việc cần chốt

1. **Chủ dự án** duyệt gói ở mục 7, đặc biệt phí giám định 15% và hạ giá tier 3.
2. **Claude 1** cập nhật nội dung theo gói và chốt ba điểm GT1, rồi báo Claude 2 chạy lại.
3. **Sau khi có bản chơi thật,** đo lại bằng người thử vì bot không đọc manh mối, không canh biến cố và không thích nghi.
