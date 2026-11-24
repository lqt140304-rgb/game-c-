# Sprint 3 — So sánh chiến thuật trên seed tái lập (Claude 2)

> **Tiền tệ:** số tiền trong tài liệu này đã đổi sang **đồng Việt Nam** ngày 2026-11-24 (1 xu cũ = 10.000 đ, xem [bảng quy đổi](README.md#tiền-tệ)). Mô phỏng vẫn tính theo đơn vị cân bằng 10.000 đ, `QA/currency.py` đổi sang đồng khi in.

> **Cập nhật theo mục tiêu độ khó:** chủ dự án muốn ván khó và nhiều đánh đổi. Đề xuất giá nền, hiệu ứng nhân 2 và các tỷ lệ thắng 42% đến 50% trong tài liệu này **quá dễ** và đã được thay bằng [DoKho.md](DoKho.md). Phát hiện về nội dung gốc, giám định và vòng xoáy thiếu tiền vẫn đúng.

Mô phỏng ba chiến thuật trên 10.000 seed (seed 0 đến 9999, cùng seed cho cả ba chiến thuật) bằng **nội dung thật của Claude 1** (hộp, hiện vật, gia truyền, tạo tác, combo, Bí Khế) và luật nợ tồn của Claude 1. Nội dung nằm trên nhánh `claude/relaxed-cerf-wgnu7l`, chưa gộp vào nhánh này.

## Kết luận ngắn

1. **Nội dung gốc không thể thắng: 0,0% ở cả ba chiến thuật.** Chỉ hộp H1 lời (3,18 lần giá hộp). H2, H3 và Rương Đen đều lỗ kỳ vọng (0,77 / 0,77 / 0,62 lần) sau khi tính chất lượng và hàng giả. Với giới hạn 3 hộp mỗi ngày, hộp rẻ không đủ kiếm 65.500.000 đ nợ.
2. **Giá nền cần tăng mạnh** ở các hiện vật của H2, H3 và Rương Đen. Bảng đề xuất ở mục 3 đưa tỷ lệ thắng của người chơi chưa biết soi lên 42% / 49% (thận trọng / liều).
3. **Tạo tác và combo hiện không đáng mua.** Chiến thuật săn combo thắng 19,3%, thua chiến thuật không mua tạo tác (41,9%). Hiệu ứng tăng giá chỉ áp dụng cho nhóm Quá khứ và Thần thoại, mà các nhóm này nằm ở hộp rẻ nhất, nên 9.960.000 đ chi cho tạo tác chỉ thu thêm 1.700.000 đ.
4. **Luật nợ khiến gia truyền giảm phạt vô nghĩa.** Thắng% với GT1 (phạt 10%) và GT2 đều bằng 41,9%: ván nào từng hụt tiền thì gần như chắc thua, nên giảm mức phạt không đổi kết quả.
5. **Giám định và phục chế chưa có giá trị kinh tế** vì giá bán không phụ thuộc đã giám định hay chưa. Mọi hiệu ứng giảm phí (CP2, TC1, Mốc 2, BK2, CB3) hiện không có tác dụng.

## Cách tái hiện

```bash
cd QA
python3 sprint3_report.py     # mất khoảng 1 phút, in toàn bộ bảng bên dưới
python3 sprint3_sim.py        # chỉ bảng nội dung gốc, 5.000 seed
```

| File | Nội dung |
| --- | --- |
| `QA/sprint3_sim.py` | Mô phỏng ba chiến thuật với nội dung của Claude 1 |
| `QA/sprint3_report.py` | Bảng giá đề xuất, các phương án và số liệu cho tài liệu này |

## Giả định của Claude 2

Luật nợ lấy từ Claude 1. Phần chưa có luật thì Claude 2 đặt giả định để mô phỏng chạy được. Các giả định này **chưa được chốt**:

- Chất lượng: Nguyên 50% (nhân 1,0), Xước 35% (nhân 0,7), Hỏng 15% (nhân 0,3).
- Hàng giả 15%, riêng V09 Lá bùa 35%. Bán chỉ được 20%.
- Phí giám định 15% giá nền (tối thiểu 200.000 đ). Phí phục chế 25% giá nền, đưa Hỏng lên Xước. Người chơi phục chế món Hỏng khi còn đủ tiền.
- Hiệu ứng thông tin (thêm manh mối, thêm dấu hiệu soi) không đo được bằng tiền nên không có trong mô phỏng. Đó là CB2, GT2, TC2 và Mốc 3 Cổ Phong.
- Người chơi bán ngay mọi hiện vật, không giữ chờ giá. Không mô phỏng kỹ năng soi nên kết quả là **sàn** cho người chơi mới, người chơi giỏi sẽ cao hơn.

## Ba chiến thuật

Cả ba đều mua mỗi lượt hộp đắt nhất còn mua được (Rương Đen từ ngày 4).

| Chiến thuật | Cách chơi |
| --- | --- |
| Thận trọng | Chỉ mua khi tiền còn đủ trả nợ kỳ này và nợ tồn. Không mua tạo tác |
| Săn combo | Như thận trọng nhưng ngày 1 đến 5 ưu tiên mua CP1, CP2 (CB1), rồi TC3, CP3 (CB3). Giám định hiện vật Thần thoại để mở BK2 |
| Liều | Không chừa tiền cho nợ, dồn vào hộp đắt nhất. Không mua tạo tác |

Khi đã không đủ tiền trả kỳ này, cả ba dồn hết tiền còn lại vào hộp để gỡ lại.

## 1. Kết quả với nội dung gốc

| Chiến thuật | Thắng | Từng hụt tiền | Ngày hụt trung bình mỗi ván | Ngày thua trung bình |
| --- | --- | --- | --- | --- |
| Thận trọng | 0,0% | 100% | 2,08 | 4,5 |
| Săn combo | 0,0% | 100% | 1,96 | 4,7 |
| Liều | 0,0% | 100% | 2,50 | 4,1 |

Giá trị kỳ vọng mỗi hộp (đã tính chất lượng và hàng giả):

| Hộp | Giá | Kỳ vọng | Lần giá hộp |
| --- | --- | --- | --- |
| H1 Hộp Gỗ Nhỏ | 1.000.000 đ | 3.180.000 đ | **3,18** |
| H2 Hộp Kim Loại Mờ | 3.500.000 đ | 2.710.000 đ | **0,77** |
| H3 Hộp Vải Dày | 8.000.000 đ | 6.190.000 đ | **0,77** |
| RD Rương Đen | 15.000.000 đ | 9.320.000 đ | **0,62** |

## 2. Đề xuất giá nền (Claude 2 sở hữu cân bằng)

Mục tiêu giá trị kỳ vọng: H1 2,6 lần, H2 1,7 lần, H3 1,4 lần, RD 1,25 lần. Hộp càng đắt thì tỷ lệ lời càng thấp nhưng số tiền lời tuyệt đối càng cao, vì chỉ được mua 3 hộp mỗi ngày.

| Hiện vật | Giá nền gốc | Giá nền đề xuất |
| --- | --- | --- |
| V01 Bình sứ men xanh | 3.000.000 đ | 2.500.000 đ |
| V02 Sách cổ bìa da | 5.000.000 đ | 4.100.000 đ |
| V03 Đồng hồ quả lắc cũ | 6.500.000 đ | 5.300.000 đ |
| V04 Mảnh kính đồng hồ thiên văn | 2.000.000 đ | 4.400.000 đ |
| V05 Thiết bị liên lạc hỏng | 1.500.000 đ | 3.300.000 đ |
| V06 Bộ pin lượng tử | 9.000.000 đ | 19.700.000 đ |
| V07 Tinh thể lam cầm tay | 12.000.000 đ | 28.800.000 đ |
| V08 Gương thủy tinh cổ tích | 10.000.000 đ | 17.000.000 đ |
| V09 Lá bùa giấy vàng | 4.000.000 đ | 9.600.000 đ |
| V10 Mảnh vảy long | 18.000.000 đ | 30.600.000 đ |

| Hộp | Giá | Kỳ vọng mới | Lần giá hộp |
| --- | --- | --- | --- |
| H1 | 1.000.000 đ | 2.610.000 đ | 2,61 |
| H2 | 3.500.000 đ | 5.940.000 đ | 1,70 |
| H3 | 8.000.000 đ | 11.170.000 đ | 1,40 |
| RD | 15.000.000 đ | 18.760.000 đ | 1,25 |

Nếu Claude 1 muốn giữ giá nền nhỏ cho đẹp, có thể chia mọi giá nền cho một hệ số rồi nhân lại ở hệ số giá bán chung (ví dụ giá nền nhân 2 lần khi bán). Mình chỉ kiểm hệ quả tiền, không đổi bản sắc từng món.

## 3. So sánh ba chiến thuật với giá nền đề xuất

| Chiến thuật | Thắng | Từng hụt tiền | Ngày hụt TB mỗi ván | Ngày thua TB | CB1 | CB3 | BK1 | BK2 | Chi tạo tác TB |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Thận trọng | 41,9% | 58,1% | 1,07 | 5,0 | — | — | — | — | 0 đ |
| Săn combo | 19,3% | 80,7% | 1,46 | 5,1 | 61,0% | 23,3% | 60,0% | 45,3% | 9.960.000 đ |
| Liều | 48,9% | 51,1% | 1,10 | 4,9 | — | — | — | — | 0 đ |

Ba điều thấy từ bảng:

- **Săn combo thua thận trọng 22,6 điểm.** Tiền bỏ vào tạo tác làm chậm vòng hộp những ngày đầu, trong khi hiệu ứng không bù lại.
- **Liều vẫn thắng cao nhất (48,9%).** Hộp có kỳ vọng lời nên rủi ro không bị phạt. Đây là vấn đề O-01 đã ghi từ Sprint 1. Muốn phạt liều thì cần hộp lỗ kỳ vọng thật hoặc chi phí rủi ro. Mình chưa đề xuất vì chưa có cách đo kỹ năng soi.
- **Combo xuất hiện thường:** CB1 xuất hiện ở 61,0% ván săn combo, BK1 (hoàn thành combo nhỏ trước ngày 5) đạt 60,0%. CB2 là 0% vì chiến thuật không mua TC1, TC2.

## 4. Chỉnh tạo tác và combo

Tất cả dùng giá nền đề xuất. Thắng% theo thứ tự thận trọng, săn combo, liều.

| Phương án | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| Luật gốc | 41,9% | 19,3% | 48,9% |
| Chừa lại 1.000.000 đ khi thu nợ | 42,4% | 20,4% | 51,0% |
| Giá tạo tác nhân 0,5 | 41,9% | 27,6% | 48,9% |
| Hiệu ứng tăng giá áp dụng mọi hiện vật (CP1 +5%, CB1 +10%) | 41,9% | 36,0% | 48,9% |
| Như trên và hiệu ứng nhân 2 (+10%, +20%) | 41,9% | **50,3%** | 48,9% |
| Như trên và giá tạo tác nhân 0,5 | 41,9% | 59,7% | 48,9% |

Với phương án "mọi hiện vật, hiệu ứng nhân 2", săn combo chi 13.640.000 đ và thu thêm 60.360.000 đ, thắng 50,3%, nhỉnh hơn thận trọng và liều. Đây là phương án Claude 2 đề xuất: tạo tác đáng mua nhưng không thống trị.

**Đề xuất cụ thể:** CP1 +10% giá bán mọi hiện vật. CB1 thêm +20% giá bán mọi hiện vật bán trong ngày. CP3 giữ +10% cho Thần thoại.

## 5. Trả lời câu hỏi của Claude 1

**Mức giảm phạt 10% đến 20% có đủ hấp dẫn không?** Không. GT1 và GT2 cho kết quả giống hệt (41,9%), và chừa lại 1.000.000 đ cũng như nhau (42,4%). Thiếu tiền một lần gần như là thua, nên giảm mức phạt không có giá trị. Giá trị chỉ xuất hiện nếu người chơi có đường gỡ lại sau khi thiếu tiền (D-05). TC3 giá 10.000.000 đ vì thế cũng không đáng mua cho hiệu ứng giảm phạt.

**CB3 có quá mạnh không?** Ngược lại, quá yếu. Hiệu ứng là một lần giám định miễn phí, trị giá tối đa vài triệu đồng, và giám định không đổi giá bán. Hai tạo tác đi kèm (CP3 9.000.000 đ, TC3 10.000.000 đ) là khoản đầu tư lớn nhất trong game.

**Tần suất BK1 đạt trước ngày 5?** 60,0% ở chiến thuật săn combo (mua CP1, CP2 ngày 1 đến 2). Hai chiến thuật còn lại không mua tạo tác nên 0%. BK2 (giám định 2 hiện vật Thần thoại) đạt 45,3%, nhưng phần thưởng giảm phí giám định không đáng giá nên không thúc đẩy ai làm.

**Phí giám định và phục chế ở mức nào?** Trước hết cần luật giám định đem lại gì. Mô hình hiện không có lợi ích giá, nên mọi mức phí đều lỗ. Tính trên giấy: nếu món chưa giám định bán được hệ số `u` so với món thật, món giám định thật bán 100% và món giả 20%, thì giám định có lợi khi `0,88 − u > phí / giá`. Với `u = 0,70` và phí 10% giá nền, mỗi lần giám định lời khoảng 8% giá nền, đủ để giảm phí 10% có ý nghĩa nhỏ. Cần chủ dự án chốt `u` trước.

## 6. Lỗi và phát hiện mới

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-09 | Cao | Nội dung gốc không thể thắng (0%). H2, H3, Rương Đen lỗ kỳ vọng | Áp dụng bảng giá nền mục 2 hoặc tương đương | Claude 1, chủ dự án |
| D-10 | Cao | Tạo tác và combo net âm cho người chơi: chi 9.960.000 đ, thu thêm 1.700.000 đ. Hiệu ứng tăng giá chỉ chạm hộp rẻ | CP1 và CB1 áp dụng mọi hiện vật, nhân 2 (mục 4) | Claude 1 |
| D-11 | Trung bình | Giám định không đổi giá bán nên CP2, TC1, Mốc 2, BK2, CB3 không có giá trị tiền | Chốt hệ số `u` cho món chưa giám định (mục 5) | Claude 1, chủ dự án |
| D-12 | Trung bình | Giảm phạt nợ (GT1, TC3, Mốc 3 Thiên Cơ) vô nghĩa do vòng xoáy thiếu tiền (D-05) | Giải D-05 trước | Claude 1, chủ dự án |
| O-03 | Cân bằng | Chiến thuật liều vẫn không bị phạt | Đo lại khi có kỹ năng soi trong bản chơi thật | Claude 2 |
| T-01 | Thấp | CB2, GT2, TC2, Mốc 3 Cổ Phong chỉ có hiệu ứng thông tin, mô phỏng không đo được | Đo bằng thử người chơi | Claude 1, Claude 2 |

## 7. Việc cần chốt

1. **Chủ dự án chọn:** áp dụng bảng giá nền mục 2 (hoặc đưa mục tiêu 1,25 đến 2,6 lần) và hệ số `u` của món chưa giám định.
2. **Claude 1 sửa:** hiệu ứng CP1 và CB1 theo mục 4, cập nhật nội dung theo giá nền mới, trả lời D-05 (đường gỡ lại sau thiếu tiền).
3. **Codex:** đưa bảng giá nền, hệ số chất lượng và xác suất giả vào dữ liệu có thể chỉnh, không gán cứng. Mô phỏng `QA/` có thể làm chuẩn đối chiếu.
4. **Sau khi chốt:** Claude 2 chạy lại `python3 sprint3_report.py` và cập nhật tài liệu này, sau đó thêm kỹ năng soi nếu có thể đo.
