# Sprint 4 — Ảnh hưởng của dịch vụ và chống giao dịch lặp (Claude 2)

Đo cầm cố, đấu giá, sòng bạc, biến cố và mở khóa meta lên thanh khoản, nợ và tiến trình. Kèm bộ ca chống giao dịch lặp cho từng dịch vụ.

## Điều cần biết trước

Claude 1 **chưa chốt** luật các dịch vụ này (mục Sprint 4 của Claude 1 còn mở). Mọi luật dịch vụ trong tài liệu này là **giả định tham số hóa của Claude 2**, nhằm trả lời câu hỏi "tham số nằm trong khoảng nào thì kinh tế còn lành". Khi Claude 1 chốt luật, chỉ cần đổi hằng số trong `QA/services_model.py` và `QA/sprint4_sim.py` rồi chạy lại.

Kết quả đo dựa trên cấu hình Sprint 3 mà Claude 2 đề xuất: giá nền mới, CP1 và CB1 áp dụng mọi hiện vật với hiệu ứng nhân 2. Cấu hình này **chưa được chủ dự án duyệt**. Nội dung của Claude 1 sau đợt rà giấy (đổi TC3, quy tắc cộng dồn, manh mối thêm) cũng chưa được đưa vào mô phỏng.

## Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_services       # 29 test chống giao dịch lặp
python3 -m unittest discover -s .          # toàn bộ 81 test
python3 sprint4_sim.py                     # số liệu ảnh hưởng, khoảng 3 phút
```

| File | Nội dung |
| --- | --- |
| `QA/services_model.py` | Cầm cố, đấu giá, sòng bạc, biến cố, mã giao dịch, bất biến |
| `QA/test_services.py` | 29 test, gồm 1 test fuzz 300 ván ngẫu nhiên |
| `QA/sprint4_sim.py` | Quét tham số từng dịch vụ trên ba chiến thuật |

## 1. Ca chống giao dịch lặp

29 test đều **đạt**. Nguyên tắc: mỗi giao dịch đổi trạng thái **đúng một lần**, bị từ chối thì trạng thái giữ nguyên, kết quả ngẫu nhiên không đổi khi tải lại.

| Dịch vụ | Mã | Ca kiểm tra |
| --- | --- | --- |
| Cầm cố | PW-1 đến PW-8 | Nhận tiền vay và món rời kho. Cầm hai lần, chuộc hai lần bị từ chối. Chuộc thiếu tiền không đổi gì. Quá hạn thì mất món và không chuộc được. Chuộc đúng ngày hạn được. Món giả cầm theo giá giả. Món đã cầm không bán được |
| Đấu giá | AU-1 đến AU-6 | Tiền về ngày sau, đúng một lần. Đăng hai lần bị từ chối. Ngày 7 không đăng được. Kết quả chốt lúc đăng nên tải lại không đổi. Món đang đấu giá không bán hay cầm được. Tiền về sau kỳ thu nợ nên không trả nợ cùng ngày (D-13) |
| Sòng bạc | CS-1 đến CS-7 | Tiền đổi đúng bằng mức cược. Giới hạn mức cược và số lượt mỗi ngày, đặt lại khi sang ngày. Tải lại không đổi kết quả. Kết quả độc lập thứ tự thao tác khác. Phát lại mã giao dịch bị chặn. Nhà cái có lợi thế qua 4.000 seed |
| Biến cố | EV-1 đến EV-4 | Biến cố xác định theo (seed, ngày). Không phụ thuộc thao tác khác. Áp dụng cho giá bán một lần. Giữ nguyên sau khi tải |
| Lưu và tải | SV-1 đến SV-3 | Vé và lô đấu giá còn nguyên sau tải. Tải giữa lúc đấu giá vẫn chỉ nhận tiền một lần. Món quá hạn vẫn không chuộc được sau tải |
| Fuzz | 1 test | 300 ván ngẫu nhiên, 120 bước mỗi ván, xen lưu/tải và phát lại mã cũ. Sau mỗi bước kiểm tra tiền không âm, hiện vật không trùng, tổng tiền vay, chuộc và đấu giá khớp sổ cái. Phát lại mã cũ không đổi trạng thái |

Yêu cầu cho Codex rút ra từ các test:

- **Kết quả ngẫu nhiên phải xác định theo (seed, ngày, số thứ tự lượt) hoặc theo mã hiện vật**, không lấy từ một bộ sinh số chung. Nếu dùng bộ chung, thao tác khác xen vào làm đổi kết quả, và tải lại bản lưu cũ cho phép đổi kết quả cược (CS-4, CS-5).
- **Mỗi thao tác tiền cần mã giao dịch lưu cùng bản lưu**, kể cả giao dịch tự động như nhận tiền đấu giá.
- **Món hàng chỉ ở đúng một nơi**: kho, vé cầm, hoặc lô đấu giá.

## 2. Ảnh hưởng lên thanh khoản và nợ

Thắng% (chênh so với cơ sở) và số ngày hụt tiền trung bình mỗi ván. 4.000 seed mỗi ô, thứ tự thận trọng, săn combo, liều.

**Cơ sở (không dịch vụ): 42,0% (hụt 1,08), 51,1% (hụt 0,92), 48,9% (hụt 1,10).**

### Đấu giá (tiền về sau 1 ngày, phí 10%)

| Tham số | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| Đăng món từ 400 xu, kỳ vọng +0% | 11,5% (−30,5) | 18,9% (−32,2) | 4,0% (−44,9) |
| Từ 400 xu, +25% | 32,4% (−9,6) | 39,8% (−11,3) | 24,1% (−24,8) |
| Từ 400 xu, +25%, phí 20% | 20,0% (−22,0) | 29,4% (−21,7) | 10,4% (−38,5) |
| Từ 400 xu, +50% | 51,6% (+9,6) | 56,5% (+5,4) | 49,4% (+0,5) |
| Chỉ món từ 1.500 xu, +25% | 41,8% (−0,2) | 48,1% (−3,0) | 32,6% (−16,3) |
| Chỉ món từ 1.500 xu, +50% | 52,2% (+10,2) | 55,6% (+4,5) | 51,2% (+2,3) |
| Chỉ món từ 1.500 xu, +75% | 58,3% (+16,3) | 59,8% (+8,7) | 62,6% (+13,7) |

- **Tiền về chậm một ngày là chi phí rất lớn.** Vốn kẹt khiến những ngày đầu mua được ít hộp hơn. Đấu giá hòa vốn khi kỳ vọng cao hơn giá bán thường khoảng 40 đến 45%.
- **Đề xuất:** đăng đấu giá nhắm món đắt (từ 1.500 xu) với kỳ vọng khoảng +40%, dao động rộng. Khi đó thận trọng lời khoảng 5 điểm, còn liều chịu tác động mạnh nhất vì không chừa tiền (hụt 1,75 đến 2,15 ngày), đúng với vai trò kênh rủi ro.
- **Tiền đấu giá về sau kỳ thu nợ cùng ngày** (D-13). Người chơi cần đăng sớm một ngày. Cần nói rõ điều này trong UI, nếu không sẽ có cảm giác bị phạt oan.

### Sòng bạc (dùng làm đường gỡ lại khi thiếu tiền, cược 1 ăn 1, tối đa 2 lượt mỗi ngày)

| Tham số | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| Thắng 40%, cược tối đa 500 | +0,5 | +0,2 | +0,8 |
| Thắng 45%, cược tối đa 500 | +0,5 | +0,3 | +0,8 |
| Thắng 50%, cược tối đa 500 | +0,6 | +0,3 | +0,9 |
| Thắng 45%, cược tối đa 200 | +0,2 | +0,2 | +0,5 |
| Thắng 45%, cược tối đa 1.000 | +1,1 | +0,4 | +1,3 |

- **Sòng bạc gần như không đổi cân bằng**: tối đa +1,3 điểm. Khoản thiếu hụt thường vài trăm đến vài nghìn xu, vượt xa mức cược tối đa, nên sòng bạc không giải được vòng xoáy thiếu tiền (D-05).
- **Rủi ro cân bằng thấp.** Có thể đặt lợi thế nhà cái 5 đến 10% (thắng 45% đến 47,5%) mà không lo phá kinh tế. Ngược lại nó **không thay thế** được đường gỡ lại mà Claude 1 cần thiết kế cho D-05.
- Mô phỏng chỉ gồm trường hợp cược để gỡ nợ. Người chơi cược sớm để nhân vốn có kỳ vọng âm nên không có lợi.

### Cầm cố tạo tác (chỉ săn combo có tạo tác để cầm)

| Tham số | Săn combo |
| --- | --- |
| Vay 50%, phí chuộc 15%, hạn 2 ngày | +0,7 |
| Vay 70%, phí chuộc 15% | +1,5 |
| Vay 70%, phí chuộc 30% | +1,5 |

- Tác động nhỏ (≤ 1,5 điểm) và gần như không phụ thuộc phí chuộc. Cầm cố chưa tạo ra quyết định thú vị.
- **Điều kiện để cầm cố có ý nghĩa:** người chơi phải có món đáng giá chưa bán. Hiện mô phỏng bán ngay nên chỉ còn tạo tác để cầm. Cần luật cho giữ hiện vật (kho, chờ giá) hoặc món chỉ bán được qua một kênh.
- Giữ nguyên phí chuộc 15% là đủ. Hạn 2 ngày hợp lý với kỳ thu nợ mỗi ngày.

### Biến cố (giá bán dao động ± trong 50% số ngày, người chơi không đoán trước)

| Biên độ | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| ±10% | −1,1 | +0,4 | −0,5 |
| ±20% | −2,1 | −0,3 | −2,6 |
| ±30% | −3,9 | −4,1 | −6,7 |
| ±50% | −8,4 | −7,9 | −11,8 |

- Biến cố đối xứng vẫn **làm giảm** tỷ lệ thắng vì nợ là ngưỡng cứng: một ngày xấu gây hụt tiền mà một ngày tốt không bù lại.
- **Đề xuất biên độ tối đa ±20%** (giảm không quá 3 điểm). Từ ±30% trở lên người chơi mất kiểm soát.
- Mô phỏng chưa cho người chơi phản ứng. GDD nói biến cố được báo qua sổ nên người chơi có thể thích nghi, như mua nhiều vào ngày giá cao. Cần đo lại khi chiến thuật biết thích nghi. Biến cố tác động nguồn cung, nhu cầu hay phí dịch vụ cũng chưa mô phỏng.

### Mở khóa meta (thêm vốn đầu ván)

| Thêm vốn | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| +100 xu | +2,9 | +4,5 | +2,3 |
| +200 xu | +5,7 | +4,8 | +4,8 |
| +300 xu | +7,8 | +6,0 | +7,6 |
| +500 xu | +12,8 | +11,4 | +11,7 |

- Mỗi 100 xu vốn tăng khoảng **2,5 điểm** thắng, gần như tuyến tính.
- **Ngân sách meta đề xuất:** tổng các mở khóa có giá trị tiền không vượt khoảng **+400 xu vốn tương đương (≈ +10 điểm)**. Meta chỉ nên mở thêm lựa chọn chứ không làm game dễ hơn đáng kể, đúng với GDD ("mở thêm lựa chọn cho ván sau").

## 3. Lỗi và phát hiện mới

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-13 | Trung bình | Tiền đấu giá về sau kỳ thu nợ cùng ngày, nên không trả nợ ngày đăng (AU-6) | Ghi rõ trong luật và UI. Hoặc cho kỳ thu nợ diễn ra sau khi nhận tiền | Claude 1, Antigravity |
| D-14 | Cao | Nếu kết quả cược lấy từ bộ sinh số chung, người chơi tải lại bản lưu cũ để đổi kết quả | Kết quả xác định theo (seed, ngày, lượt). Đã có test CS-4, CS-5 | Codex |
| D-15 | Trung bình | Đấu giá làm chậm vốn nên nếu kỳ vọng thấp hơn khoảng +40% thì là cái bẫy | Dùng khoảng +40% cho món từ 1.500 xu | Claude 1 |
| D-16 | Trung bình | Sòng bạc và cầm cố gần như không giải được vòng xoáy thiếu tiền (D-05) | Thiết kế đường gỡ lại riêng, đừng trông vào sòng bạc | Claude 1, chủ dự án |
| D-17 | Thấp | Biến cố giá lớn hơn ±20% làm tỷ lệ thắng giảm hơn 3 điểm | Giới hạn biên độ ±20% | Claude 1 |
| T-02 | Thấp | Chưa đo biến cố khi người chơi thích nghi, nhiều biến cố cùng lúc, hoặc biến cố tác động nguồn cung | Đo khi có luật biến cố | Claude 2 |

## 4. Việc cần chốt

1. **Claude 1** chốt luật cầm cố, đấu giá, sòng bạc, biến cố và meta trong khoảng đã đo ở trên, rồi báo Claude 2 chạy lại.
2. **Chủ dự án** duyệt cấu hình giá nền của Sprint 3 vì số liệu mục 2 dựa trên đó.
3. **Codex** áp dụng ba yêu cầu ở cuối mục 1 khi nối dịch vụ và lưu giao dịch, rồi chuyển `QA/test_services.py` sang C#.
