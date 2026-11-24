# Độ khó và đánh đổi — CHỢ ĐỒ LẠ (Claude 2)

> **Tiền tệ (quyết định 2026-11-24):** game dùng **đồng Việt Nam**. Các số "xu" trong tài liệu này là **đơn vị cân bằng cũ, 1 xu = 10.000 đ** (ví dụ vốn 1.000 xu = 10.000.000 đ). Tỉ lệ và phần trăm không đổi. Xem [bảng quy đổi](README.md#tiền-tệ).

> **Cập nhật:** Claude 1 đã đề xuất thêm TC3, CB3, GT1 mới và chứng nhận giám định. Claude 2 đã đo trong [DoLaiDeXuat.md](DoLaiDeXuat.md), tài liệu đó đưa ra gói số liệu mới. Các kết luận về độ khó, giá nền hệ số 0,85 và k đấu giá 1,16 vẫn khớp.

**Mục tiêu của chủ dự án:** người chơi không được thắng quá dễ. Ván phải tạo ra nhiều tình huống khó xử, có đánh đổi thật và cần tính toán kỹ.

Tài liệu này **thay thế các đề xuất giá nền và hiệu ứng ở [Sprint 3](Sprint3.md) và [Sprint 4](Sprint4.md)**, vì các đề xuất đó nhắm tỷ lệ thắng 42% đến 50% cho người chơi chưa biết soi, tức là quá dễ so với mục tiêu này. Các kết luận khác của hai tài liệu đó (nội dung gốc không thể thắng, giám định chưa có giá trị tiền, vòng xoáy thiếu tiền, tests chống giao dịch lặp) vẫn đúng.

## 1. Mục tiêu thành chỉ số đo được

Mô phỏng chỉ có ba chiến thuật máy (thận trọng, săn combo, liều), không biết soi, giám định hay thích nghi. Vì vậy chúng là **cận dưới**: người chơi thật có kỹ năng sẽ thắng nhiều hơn. Mục tiêu đặt cho cận dưới thấp để còn chỗ cho kỹ năng mà ván vẫn khó.

| Chỉ số | Mục tiêu | Lý do |
| --- | --- | --- |
| Tỷ lệ thắng của chiến thuật tốt nhất | Không quá 32% | Ván khó với người chưa có kỹ năng |
| Tỷ lệ thắng của chiến thuật kém nhất | Không dưới 12% | Vẫn thắng được, không bất khả thi |
| Chênh lệch giữa ba chiến thuật | Không quá 7 điểm | Không chiến thuật nào thống trị nên người chơi phải cân nhắc |
| Hộp càng đắt tỷ lệ lời càng thấp | H1 > H2 > H3 > Rương Đen, Rương Đen dưới 1,2 lần | Dồn tiền vào hộp đắt có giá phải trả |

Bốn chỉ số này nằm trong `QA/test_difficulty.py`. Mọi chỉnh sửa số liệu sau này mà làm game dễ đi hoặc để một chiến thuật thống trị sẽ làm test đỏ.

## 2. Cấu hình đề xuất cho ván khó

Dữ liệu nằm trong `QA/profiles.py`.

**Giá nền hiện vật** (so với nội dung gốc của Claude 1 và đề xuất dễ hơn ở Sprint 3):

| Hiện vật | Gốc | Sprint 3 (dễ) | **Ván khó** |
| --- | --- | --- | --- |
| V01 Bình sứ men xanh | 300 | 250 | **210** |
| V02 Sách cổ bìa da | 500 | 410 | **340** |
| V03 Đồng hồ quả lắc cũ | 650 | 530 | **450** |
| V04 Mảnh kính đồng hồ thiên văn | 200 | 440 | **390** |
| V05 Thiết bị liên lạc hỏng | 150 | 330 | **290** |
| V06 Bộ pin lượng tử | 900 | 1.970 | **1.750** |
| V07 Tinh thể lam cầm tay | 1.200 | 2.880 | **2.460** |
| V08 Gương thủy tinh cổ tích | 1.000 | 1.700 | **1.550** |
| V09 Lá bùa giấy vàng | 400 | 960 | **800** |
| V10 Mảnh vảy long | 1.800 | 3.060 | **2.790** |

**Giá trị kỳ vọng mỗi hộp** (đã tính chất lượng và hàng giả, theo bội số giá hộp):

| Hộp | Gốc | Sprint 3 (dễ) | **Ván khó** |
| --- | --- | --- | --- |
| H1 (100 xu) | 3,18 | 2,61 | **2,19** |
| H2 (350 xu) | 0,77 | 1,70 | **1,51** |
| H3 (800 xu) | 0,77 | 1,40 | **1,25** |
| RD Rương Đen (1.500 xu) | 0,62 | 1,25 | **1,10** |

**Tạo tác:** CP1 và CB1 áp dụng cho mọi hiện vật, hiệu ứng nhân **1,25** so với đề xuất của Claude 1 (CP1 +6,25%, CB1 +12,5%) thay vì nhân 2. Giá tạo tác giữ nguyên.

**Đấu giá:** giữ bảng k của Claude 1 (kỳ vọng **1,16**), **không** nâng lên 1,33 như đề xuất cũ. Ở ván khó, nâng k lên 1,33 làm tỷ lệ thắng tăng 8 đến 13 điểm, quá dễ.

## 3. Kết quả

10.000 seed ở Sprint 3, 1.500 đến 4.000 seed cho các phép đo dưới đây.

| Cấu hình | Thận trọng | Săn combo | Liều | Chênh |
| --- | --- | --- | --- | --- |
| Nội dung gốc | 0,0% | 0,0% | 0,0% | 0,0 |
| Sprint 3 (dễ) | 41,9% | 50,3% | 48,9% | 8,4 |
| **Ván khó** | **21,8%** | **21,8%** | **24,3%** | **2,5** |

Ngày hụt tiền trung bình mỗi ván tăng từ khoảng 1,0 lên **1,46 đến 1,62**: người chơi bị dồn nhiều hơn. Ba chiến thuật gần như ngang nhau, nghĩa là không có đáp án sẵn, quyết định phụ thuộc vào cách người chơi đọc tình huống.

## 4. Các lựa chọn: đâu là đánh đổi thật, đâu bị át

Một lựa chọn là đánh đổi thật khi hai hướng cho kết quả gần nhau (trong khoảng ±3 điểm thắng) và hướng nào tốt hơn tùy tình huống. Lựa chọn bị **át** là lựa chọn mà một hướng luôn tốt hơn, nên không còn gì để cân nhắc.

| Lựa chọn | Kết quả ở ván khó | Đánh giá |
| --- | --- | --- |
| Chừa tiền trả nợ hay dồn mua hộp đắt (thận trọng so với liều) | 21,8% so với 24,3% | **Đánh đổi thật.** Dồn tiền nhỉnh hơn một chút nhưng hụt tiền nhiều hơn (1,62 so với 1,46 ngày) |
| Đầu tư tạo tác hay mua hộp (săn combo so với thận trọng) | 21,8% so với 21,8% | **Đánh đổi thật.** Tạo tác là khoản lỗ ban đầu, chỉ lời nếu đủ ngày để thu hồi |
| Đấu giá (1 lô mỗi ngày, k 1,16) | −2 đến +3 điểm tùy ngưỡng và giá sàn | **Đánh đổi thật.** Lời với món đắt và sàn 1,0 G, lỗ nếu sàn cao hoặc món rẻ |
| Biến cố E1 đến E6 | Trung tính (±0,6 điểm) | Làm ván khác nhau chứ không đổi độ khó, cần đọc sổ để thích nghi |
| Cầm tạo tác (đang bị cấm) | 0 đến +1,6 điểm | Đánh đổi nhỏ nhưng có thật: giữ combo hay lấy tiền ngay |
| **Cầm hiện vật** (60% G, phí 15%) | Thua bán ngay 1 đến 5 điểm | **Bị át.** Bán ngay luôn tốt hơn, không có gì để cân nhắc |
| **Sòng bạc** | Tối đa +1,3 điểm | **Gần như không ảnh hưởng.** Cược nhỏ không cứu được khoản thiếu |
| **Giám định và phục chế** | Không đổi giá bán | **Vô nghĩa về kinh tế** vì giá bán không phụ thuộc đã giám định hay chưa |
| **Giảm phạt nợ** (GT1, TC3, Mốc 3 Thiên Cơ) | Không đổi tỷ lệ thắng | **Vô nghĩa** vì thiếu tiền một lần gần như chắc thua |
| **Chọn hạn mức nợ tồn, chừa tiền khi thu** | Ngưỡng thua 1.000 so với 1.500 không đổi, chừa 100 đến 200 xu đổi dưới 2 điểm | **Không phải quyết định** vì mọi lần hụt đều dẫn tới thua |

## 5. Đề xuất để có nhiều tình huống khó xử hơn

Bốn lựa chọn ở cuối bảng trên đang chết. Mỗi mục dưới đây biến một lựa chọn chết thành đánh đổi thật.

1. **Giám định có giá.** Đặt món chưa giám định bán với hệ số 0,70 so với giá thật. Phí giám định khoảng 10% giá nền. Khi đó người chơi phải chọn: trả phí để bán đủ giá và lộ hàng giả, hay bán vội mất 30% nhưng giữ vốn xoay vòng. Nếu thiếu tiền, chọn nào thì tùy kỳ nợ.
2. **Cầm cố có lý do.** Cho cầm cả tạo tác và gia truyền (hiện bị cấm), vì người chơi mất combo mới thấy đau, và có thể chuộc để mở lại. Có thể thêm **tin đồn biến cố ngày mai**: biết ngày mai món Quá khứ bán thêm 15% thì người chơi giữ món chờ, và dùng cầm cố để lấy tiền ngay. Đó là đánh đổi thật giữa thanh khoản hôm nay và giá cao ngày mai.
3. **Biến cố mạnh hơn nhưng có thông tin.** Biến cố hiện trung tính vì ±15% trung bình triệt tiêu. Nếu sổ báo trước một ngày và biên độ ±25% thì người chơi có thể lập kế hoạch, và ván sẽ hay mà vẫn khó. Cần đo lại khi có luật mới.
4. **Cho vòng xoáy thiếu tiền có lối ra tốn kém.** Hiện thiếu tiền một lần là thua, nên không còn quyết định "chấp nhận nợ tồn để đầu tư". Nếu có một cách gỡ đắt đỏ (ví dụ vay nóng của Anh Bảy với phí 40% và khóa mua hộp lớn một ngày) thì nợ tồn thành đánh đổi chứ không phải bản án. Mức phạt 20% hiện tại, ngưỡng thua 1.000 xu đều không ảnh hưởng gì vì mọi hụt tiền đều là cái chết, nên chúng cần được thiết kế lại cùng lối ra này.
5. **Sòng bạc là trò ham muốn, không phải cứu ván.** Giữ kỳ vọng −10% và đừng nâng tỷ lệ thắng. Nếu muốn nó thành lựa chọn khó, gắn phần thưởng không phải tiền (manh mối, mở Bí Khế) vào kết quả thắng.

Mọi đề xuất trên là việc của Claude 1 (thiết kế) và chủ dự án duyệt. Claude 2 sẽ đo lại từng đề xuất khi có luật cụ thể.

## 6. Giới hạn và bước tiếp theo

- Ba chiến thuật máy là cận dưới. Người chơi giỏi sẽ cao hơn nhiều, nên **chưa thể kết luận ván khó với người thật**. Cần chơi thử bản chạy rồi chỉnh. Nếu người thử giỏi thắng hơn 50% thì hạ tiếp giá nền (mỗi bước giảm khoảng 5% giá nền làm tỷ lệ thắng tụt vài điểm).
- Hiệu ứng chỉ có giá trị thông tin (manh mối, dấu hiệu soi) không đo được bằng mô phỏng.
- Cấu hình ván khó **chưa được chủ dự án duyệt**.

## 7. Cách tái hiện

```bash
cd QA
python3 difficulty.py                      # quét bộ giá, ~1 phút
python3 -m unittest -v test_difficulty     # 4 chỉ số mục tiêu
python3 sprint4_sim.py                     # ảnh hưởng của dịch vụ ở ván khó, ~5 phút
```
