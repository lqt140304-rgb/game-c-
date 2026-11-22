# Sprint 4 — Ảnh hưởng của dịch vụ và chống giao dịch lặp (Claude 2)

Đo biến cố, đấu giá, sòng bạc, cầm cố và mở khóa meta theo **đúng luật Claude 1 đã soạn** trong [DichVuVaMeta.md](https://github.com/lqt140304-rgb/game-c-/blob/claude/relaxed-cerf-wgnu7l/Docs/Content/DichVuVaMeta.md) (commit `d6b597e`, nhánh `claude/relaxed-cerf-wgnu7l`, chưa gộp vào nhánh này). Mỗi "Ca" trong tài liệu của Claude 1 thành một test. Mục 7 của tài liệu đó đặt năm câu hỏi cho Claude 2, được trả lời ở mục 3.

Bản đầu của tài liệu này dùng luật giả định của Claude 2 khi Claude 1 chưa soạn. Bản này **thay thế** bản đó.

Mô phỏng chạy trên cấu hình Sprint 3 do Claude 2 đề xuất (giá nền mới, CP1 và CB1 áp dụng mọi hiện vật, nhân 2), **chưa được chủ dự án duyệt**. Hiệu ứng giá bán cộng theo phép cộng như quy tắc mới của Claude 1, nên mốc cơ sở của săn combo là 49,6% thay vì 51,1% ở Sprint 3.

## Cách tái hiện

```bash
cd QA
python3 -m unittest discover -s .          # toàn bộ 97 test
python3 sprint4_sim.py                     # số liệu mục 2, khoảng 4 phút
```

| File | Nội dung |
| --- | --- |
| `QA/services_model.py` | Biến cố, đấu giá, sòng bạc, cầm cố, meta theo luật Claude 1, mã giao dịch, bất biến |
| `QA/test_services.py` | 45 test, gồm các ca của Claude 1 và một test fuzz 300 ván |
| `QA/sprint4_sim.py` | Mô phỏng ảnh hưởng, 4.000 seed mỗi ô |

## 1. Kiểm thử luật và chống giao dịch lặp

45 test đều **đạt**.

| Nhóm | Mã | Nội dung |
| --- | --- | --- |
| Biến cố | EV-1 đến EV-6 | Ngày 1 không có biến cố. E4 chặn giám định, hôm sau giám định được. E1 +15% cho Quá khứ, E2 −15%, E6 +10% cho Rất hiếm. E3 giảm giá H2 còn 280 xu và giữ nguyên sau tải lại. Biến cố không phụ thuộc thao tác khác. Tần suất xấp xỉ 50% |
| Cầm cố | PW-1 đến PW-10 | V10 cầm nhận 1.080, chuộc ngày 4 tốn 1.242. Quá hạn ngày 5 mất món. Món thứ 3 bị từ chối. Cầm trước kỳ nợ dùng trả nợ cùng ngày. Tải lại không nhận tiền lần hai. Chuộc hai lần, chuộc thiếu tiền bị từ chối. Hết ván mất món chưa chuộc. Món đang cầm bị khóa với mọi dịch vụ |
| Đấu giá | AU-1 đến AU-10 | Phí 90 và nhận 2.340 khi k = 1,3. Sàn 1,3 mà k = 1,1 thì trả món, mất phí. Ngày 7 bị từ chối. Tiền chưa về không trả nợ. Tải lại không trừ phí lần hai. Một lô mỗi ngày. Tiền về đầu ngày sau. Kết quả không đổi khi tải lại. Sàn không hợp lệ bị từ chối. Kỳ vọng k đúng 1,16 |
| Sòng bạc | CS-1 đến CS-8 | Thắng cộng đúng mức cược, thua trừ đúng mức cược. Lượt thứ 4 bị từ chối, sang ngày đặt lại. Thiếu tiền hoặc sai mức cược bị từ chối. Tải lại không đổi kết quả. Không mở ngày 1. Phát lại mã giao dịch bị chặn. Kỳ vọng −10% |
| Meta | MT-1 đến MT-6 | Sưu tầm một lần dù soi nhiều lần. 5 món mở M1. Thua ván không mất mở khóa. Thắng chỉ tính một lần mỗi ván dù tải lại. Vốn đầu ván không vượt 1.100. M4 cần đủ cả hai Bí Khế |
| Lưu và tải | SV-1 đến SV-3 | Vé và lô đấu giá còn nguyên. Tải giữa lúc đấu giá vẫn nhận tiền một lần. Món quá hạn vẫn không chuộc được sau tải |
| Fuzz | 1 test | 300 ván, 150 bước mỗi ván, xen lưu/tải và phát lại mã cũ. Sau mỗi bước: tiền không âm, hiện vật không nằm ở hai nơi, tổng tiền vay, chuộc, phí và bán đấu giá khớp sổ cái. Phát lại mã cũ không đổi trạng thái |

Luật của Claude 1 đã giải các vấn đề mà bản đầu của tài liệu này nêu: tiền đấu giá về **đầu ngày** nên trả được nợ cuối ngày đó (D-13 đóng), và kết quả ngẫu nhiên **chốt theo seed** (D-14 đóng, test CS-5 và AU-8 xác nhận).

## 2. Ảnh hưởng lên thanh khoản và nợ

Thắng% (chênh so với cơ sở) và số ngày hụt tiền trung bình mỗi ván. Thứ tự thận trọng, săn combo, liều.

**Cơ sở (không dịch vụ): 42,0% (hụt 1,08), 49,6% (hụt 0,94), 48,8% (hụt 1,10).**

### Biến cố E1 đến E6, 50% số ngày từ ngày 2

| | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| Có biến cố | 42,0% (+0,0) | 50,0% (+0,4) | 49,5% (+0,7) |

Tác động trung bình gần như bằng 0. Biến cố làm ván khác nhau chứ không đổi độ khó. Một ván 6 ngày có trung bình 3 ngày biến cố và chỉ 1,6% ván không có biến cố nào.

### Đấu giá (1 lô mỗi ngày, phí 5%, k kỳ vọng 1,16, tiền về đầu ngày sau)

Chiến thuật đăng **món đầu tiên đạt ngưỡng giá** trong ngày.

| Ngưỡng đăng | Giá sàn | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- | --- |
| 400 xu | không | −3,5 | −5,8 | −4,4 |
| 400 xu | 1,0 G | −1,6 | −5,0 | −2,3 |
| 400 xu | 1,3 G | −4,9 | −8,3 | −6,4 |
| 1.000 xu | không | +0,3 | −1,7 | −3,2 |
| 1.000 xu | 1,0 G | +2,0 | −0,8 | −1,1 |
| 1.000 xu | 1,3 G | −1,6 | −3,8 | −4,3 |
| 1.500 xu | 1,0 G | +1,6 | +0,4 | −0,9 |

Tăng kỳ vọng k (món từ 1.000 xu, sàn 1,0 G):

| k kỳ vọng | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| 1,16 (đề xuất của Claude 1) | +2,0 | −0,8 | −1,1 |
| 1,33 | **+9,7** | **+5,0** | **+9,2** |
| 1,51 | +16,9 | +10,0 | +17,7 |

### Sòng bạc (dùng khi thiếu tiền, cược 50/100/200, 3 lượt)

| Xác suất thắng | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| 45% (đề xuất) | +0,3 | +0,3 | +0,8 |
| 50% | +0,4 | +0,4 | +0,8 |
| 55% | +0,4 | +0,4 | +1,0 |
| 45%, cược tối đa 500 | +1,0 | +0,8 | +1,4 |

### Cầm cố so với bán (giữ một món từ 600 xu làm đệm mỗi ngày, khi thiếu tiền thì dùng)

| Cách dùng đệm | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| Bán | 30,5% (−11,5) | 41,0% (−8,6) | 42,1% (−6,7) |
| Cầm 60% G | 27,5% (−14,5) | 35,0% (−14,6) | 29,1% (−19,7) |

Cả hai cách đều thua cơ sở vì món giữ lại làm tiền chưa về tay. Điểm cần so là **cầm so với bán**: cầm thua bán 3 điểm (thận trọng), 6 điểm (săn combo) và 13 điểm (liều).

### Mở khóa meta

| Mở khóa | Thận trọng | Săn combo | Liều |
| --- | --- | --- | --- |
| M2 vốn +100 xu | +2,9 | +4,0 | +2,4 |
| M3 Rương Đen từ ngày 1 | −0,4 | +2,5 | +0,6 |

## 3. Trả lời câu hỏi của Claude 1

**Đấu giá có đủ hấp dẫn không?** Chưa. Kỳ vọng 1,16 G trừ phí 5% còn +11%, đổi lấy chậm một ngày và rủi ro, kết quả là **trung tính đến lỗ** (−8 đến +2 điểm tùy ngưỡng và giá sàn). Bán ngay luôn an toàn hơn. Đề xuất đổi bảng k thành 1,0 / 1,25 / 1,5 / 1,75 với xác suất 20% / 40% / 30% / 10% (kỳ vọng 1,33). Khi đó đấu giá món từ 1.000 xu với sàn 1,0 G lời 5 đến 10 điểm, vẫn có rủi ro cho chiến thuật liều vì tiền về chậm.

**Sòng bạc kỳ vọng −10% có đủ rủi ro không, hay nên tăng tỷ lệ thắng?** Đủ, đừng tăng. Dù nâng xác suất thắng lên 55% hay cược tối đa lên 500 xu, tác động lên tỷ lệ thắng ván vẫn dưới 1,5 điểm. Sòng bạc là trò giải trí rủi ro, không phải công cụ cứu ván. Cược tối đa 200 xu quá nhỏ so với khoản thiếu (vài trăm đến vài nghìn xu), nên sòng bạc không giải vòng xoáy thiếu tiền (D-05).

**Cầm cố 60% G, phí 15%, hạn 2 ngày có cứu ván mà không quá rẻ không?** Nó không cứu ván tốt hơn việc **bán ngay**, vì người chơi luôn bán được món với G trước kỳ nợ, còn cầm chỉ được 0,6 G và phải trả thêm 0,09 G nếu chuộc. Cầm thua bán 3 đến 13 điểm (mục 2). Không ai có lý do chọn cầm, trừ khi muốn giữ món vì lý do ngoài tiền. Đề xuất một trong các hướng:

1. Cho cầm **tạo tác** (hiện bị cấm), để người chơi lấy tiền trả nợ mà vẫn giữ combo. Với luật cũ của Claude 2 tác động nhỏ (+0,7 đến +1,5 điểm) nhưng có quyết định thật.
2. Làm cho bán ngay kém hơn trong lúc thiếu tiền, ví dụ giá bán vội 85% G hoặc hạn mức bán mỗi ngày.
3. Chấp nhận cầm là công cụ phụ, nhưng cần lý do giữ món: Sổ Sưu Tầm, chờ biến cố giá cao, hoặc đấu giá hôm sau.

**Vốn +100 xu của M2 có làm ván sau quá dễ không?** Không, nếu bảng giá nền đã được cân bằng. M2 cộng thêm 2,4 đến 4,0 điểm. Điều kiện là H1 không còn lời 4,6 lần như nội dung gốc: ở nội dung gốc cả ván đều bất khả thi hoặc đều dễ tùy hộp. Giới hạn +100 xu tổng cộng của Claude 1 nằm trong ngân sách đã đo ở bản trước (khoảng 2,5 điểm mỗi 100 xu, tổng meta không quá khoảng +10 điểm).

**Tần suất biến cố 50% mỗi ngày có hợp lý không?** Có. Tác động trung bình lên tỷ lệ thắng dưới 1 điểm, nên không làm ván khó hơn hay dễ hơn. Trung bình 3 ngày mỗi ván có biến cố, tức khác biệt rõ giữa các ván nhưng không dày. Có thể tăng độ ảnh hưởng của biến cố: ±15% hiện chỉ đổi ván khi người chơi thích nghi theo thông tin sổ, mà mô phỏng chưa đo.

## 4. Lỗi và phát hiện

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-18 | Trung bình | Cầm cố bị bán ngay át hoàn toàn (0,6 G và phí 15% so với 1,0 G) | Một trong ba hướng ở mục 3 | Claude 1 |
| D-19 | Trung bình | Đấu giá k = 1,16 là trung tính đến lỗ | Bảng k kỳ vọng 1,33 | Claude 1 |
| D-16 | Trung bình | Sòng bạc và cầm cố không giải vòng xoáy thiếu tiền (D-05) | Thiết kế đường gỡ lại riêng | Claude 1, chủ dự án |
| T-02 | Thấp | Chưa đo người chơi thích nghi với biến cố (mua nhiều H2 ngày E3, tránh bán ngày E2) | Đo khi có bản chơi thật | Claude 2 |
| T-03 | Thấp | Chưa đo ảnh hưởng của E4, E5 với chiến thuật hay giám định, phục chế | Đo khi giám định có giá trị tiền (D-11) | Claude 2 |

Đã đóng nhờ luật của Claude 1: **D-13** (tiền đấu giá về đầu ngày), **D-14** (kết quả chốt theo seed), **D-17** (biến cố nhẹ, không cần giới hạn thêm).

## 5. Việc cần chốt

1. **Claude 1** sửa bảng k đấu giá và cách dùng cầm cố, rồi báo Claude 2 chạy lại `sprint4_sim.py`.
2. **Chủ dự án** duyệt cấu hình giá nền Sprint 3, vì mọi số liệu mục 2 dựa trên đó.
3. **Codex** chuyển `QA/test_services.py` sang C# khi nối dịch vụ. Hai yêu cầu cần giữ: kết quả ngẫu nhiên xác định theo (seed, ngày, lượt hoặc mã hiện vật), và mọi thao tác tiền có mã giao dịch lưu cùng bản lưu.
