# Lời tăng theo giá hộp: đo thử cấu hình (Claude 2)

> **Tiền tệ:** số tiền trong tài liệu này đã đổi sang **đồng Việt Nam** ngày 2026-11-24 (1 xu cũ = 10.000 đ, xem [bảng quy đổi](README.md#tiền-tệ)). Mô phỏng vẫn tính theo đơn vị cân bằng 10.000 đ, `QA/currency.py` đổi sang đồng khi in.

Từ phát hiện ở [BienCoE3.md](BienCoE3.md): H2 chi phối việc chọn hộp (66% đến 71% lượt mua) và Rương Đen không bao giờ được mua. Mục tiêu của chủ dự án là ván có nhiều đánh đổi cần tính toán. Tài liệu này đo thử cấu hình **lời tuyệt đối mỗi hộp tăng theo giá hộp**, để người nghèo chọn hộp rẻ và người có vốn chọn hộp đắt.

Chỉ đổi **giá nền hiện vật** (giữ tỷ lệ rơi, hàng giả theo hộp G1, mọi luật khác). Cơ sở là cấu hình đã duyệt. Mô phỏng 1.500 seed, người chơi chọn loại hộp có lời kỳ vọng cao nhất mỗi lượt như luật chốt.

## Kết luận ngắn

1. **Cấu hình lời tăng theo giá hộp tạo ra đánh đổi theo số tiền đúng ý.** Ở cấu hình C3 người có dưới 6.000.000 đ chọn H1 và H2, người có 6.000.000 đến 15.000.000 đ chia H1, H2, H3, người có trên 15.000.000 đ chia H2, H3 và Rương Đen (30%). Hiện tại H2 chiếm 64% đến 67% mọi mức tiền và Rương Đen 0%.
2. **Không một loại hộp đơn lẻ nào thống trị:** chọn linh hoạt theo tiền thắng cao hơn ép một loại hộp khoảng 8 đến 18 điểm.
3. **Có cái giá phải trả: game dễ hơn nếu lời nâng cao.** Với lời 1.200.000 / 2.000.000 / 3.200.000 / 4.500.000 đ, ngay cả bot "mua hộp đắt nhất" cũng thắng 32% đến 43%. Phải hạ lời xuống cỡ **1.000.000 / 1.600.000 / 2.200.000 / 2.600.000 đ (C3)** để bot thường còn 19% đến 29% và người chơi tính toán hoàn hảo 22% đến 31%.
4. **C3 khớp mục tiêu của Claude 1** (bot thường khoảng 15% đến 20%, đầu tư khoảng 30%) tốt hơn cấu hình hiện tại, nhưng **thu hẹp khoảng cách** giữa bot thường và người tính toán (từ 15 đến 20 điểm xuống 3 đến 5 điểm), vì với lời tăng theo giá hộp thì "mua hộp đắt nhất mua được" đã là mẹo khá tốt.
5. **Khuyến nghị: dùng C3**, và để kỹ năng nằm ở đọc manh mối, canh biến cố, nhịp vốn và combo thay vì ở việc tránh hộp lỗ.

## Cách tái hiện

```bash
cd QA
python3 tier_sim.py            # 3 cấu hình (hiện tại, C1, C2), ~12 phút
```

C3 và C4 chạy bằng cách thay `TARGETS` trong `tier_sim.py`. Test khóa C3 nằm ở `QA/test_tier_profit.py` (đang tắt, chờ duyệt).

## Cấu hình thử

Lời kỳ vọng mỗi hộp (đồng) sau khi tính chất lượng và hàng giả theo hộp (G1: 12%, 15%, 22%, 30%):

| Cấu hình | H1 | H2 | H3 | RD | Lần giá hộp H1 / H2 / H3 / RD |
| --- | --- | --- | --- | --- | --- |
| Hiện tại (G1) | 1.500.000 đ | 2.080.000 đ | 1.900.000 đ | **150.000 đ** | 2,50 / 1,59 / 1,24 / 1,01 |
| C1 | 1.210.000 đ | 2.010.000 đ | 3.190.000 đ | 4.470.000 đ | 2,21 / 1,57 / 1,40 / 1,30 |
| C2 | 1.300.000 đ | 2.010.000 đ | 2.590.000 đ | 3.000.000 đ | 2,30 / 1,57 / 1,32 / 1,20 |
| **C3 (khuyến nghị)** | 980.000 đ | 1.600.000 đ | 2.200.000 đ | 2.590.000 đ | 1,98 / 1,46 / 1,27 / 1,17 |
| C4 | 890.000 đ | 1.400.000 đ | 1.910.000 đ | 2.180.000 đ | 1,89 / 1,40 / 1,24 / 1,15 |

Giá nền hiện vật (làm tròn):

| Hiện vật | Hiện tại (G1) | C1 | C2 | **C3** | C4 |
| --- | --- | --- | --- | --- | --- |
| V01 Bình sứ men xanh | 2.300.000 đ | 2.000.000 đ | 2.100.000 đ | **1.800.000 đ** | 1.700.000 đ |
| V02 Sách cổ bìa da | 3.850.000 đ | 3.400.000 đ | 3.500.000 đ | **3.000.000 đ** | 2.900.000 đ |
| V03 Đồng hồ quả lắc cũ | 4.950.000 đ | 4.400.000 đ | 4.600.000 đ | **4.000.000 đ** | 3.800.000 đ |
| V04 Mảnh kính đồng hồ thiên văn | 4.150.000 đ | 4.100.000 đ | 4.100.000 đ | **3.800.000 đ** | 3.600.000 đ |
| V05 Thiết bị liên lạc hỏng | 3.100.000 đ | 3.000.000 đ | 3.000.000 đ | **2.800.000 đ** | 2.700.000 đ |
| V06 Bộ pin lượng tử | 18.450.000 đ | 18.300.000 đ | 18.300.000 đ | **16.900.000 đ** | 16.300.000 đ |
| V07 Tinh thể lam cầm tay | 26.950.000 đ | 36.000.000 đ | 34.800.000 đ | **31.200.000 đ** | 30.600.000 đ |
| V08 Gương thủy tinh cổ tích | 15.900.000 đ | 19.500.000 đ | 17.000.000 đ | **18.500.000 đ** | 18.000.000 đ |
| V09 Lá bùa giấy vàng | 8.950.000 đ | 6.800.000 đ | 9.600.000 đ | **4.600.000 đ** | 4.400.000 đ |
| V10 Mảnh vảy long | 28.600.000 đ | 35.100.000 đ | 30.600.000 đ | **33.300.000 đ** | 32.400.000 đ |

Ghi chú: V09 Lá bùa rẻ hơn hiện nay (4.600.000 đ) vì H3 cần giá trị tổng thể thấp hơn trong khi V08 và V10 phải cao để Rương Đen lời. V07 (hiếm nhất của Rương Đen) và V10 đắt nhất. Claude 1 có thể điều chỉnh từng món miễn là lời mỗi hộp giữ nguyên.

## Kết quả

Thắng%, thứ tự thận trọng / liều / combo nhỏ / combo lớn (đều có giám định, trừ cột thận trọng thô).

**Mua hộp đắt nhất mua được (bậc thang cũ, không đọc manh mối):**

| Cấu hình | Thận trọng | Thận trọng + giám định | Liều | Liều + giám định | Combo nhỏ + gđ | Combo lớn + gđ |
| --- | --- | --- | --- | --- | --- | --- |
| Hiện tại (G1) | 16,2 | 22,0 | 15,2 | 23,2 | 35,1 | 37,9 |
| C1 | 31,8 | 33,7 | 40,4 | 43,4 | 40,3 | 40,0 |
| C2 | 26,5 | 31,1 | 33,5 | 37,8 | 39,5 | 40,2 |
| **C3** | **19,4** | **21,9** | **24,0** | **28,7** | **28,3** | **28,9** |
| C4 | 16,4 | 18,7 | 20,4 | 25,3 | 25,3 | 25,4 |

**Người chơi chọn loại hộp mỗi lượt và đọc manh mối** (luật chốt, λ = 0,75):

| Cấu hình | Thận trọng | Liều | Combo nhỏ | Combo lớn |
| --- | --- | --- | --- | --- |
| Hiện tại (G1) | 36,9 | 43,6 | 44,5 | 45,6 |
| C1 | 33,5 | 42,4 | 39,8 | 40,0 |
| C2 | 31,3 | 39,5 | 39,7 | 41,5 |
| **C3** | **22,3** | **29,1** | **28,8** | **30,8** |
| C4 | 18,9 | 24,9 | 24,7 | 25,2 |

**Chọn loại hộp theo số tiền đang có** (chiến thuật thận trọng + giám định, tỷ lệ các lần mua):

| Cấu hình | Số tiền | H1 | H2 | H3 | RD |
| --- | --- | --- | --- | --- | --- |
| Hiện tại | dưới 6.000.000 đ | 67% | 33% | 0% | 0% |
| | 6.000.000 đến 15.000.000 đ | 23% | **64%** | 14% | 0% |
| | trên 15.000.000 đ | 2% | **67%** | 31% | **0%** |
| C1 | dưới 6.000.000 đ | 69% | 31% | 0% | 0% |
| | 6.000.000 đến 15.000.000 đ | 23% | 44% | 32% | 0% |
| | trên 15.000.000 đ | 1% | 13% | 44% | 42% |
| **C3** | dưới 6.000.000 đ | 67% | 33% | 0% | 0% |
| | 6.000.000 đến 15.000.000 đ | 22% | 54% | 24% | 0% |
| | trên 15.000.000 đ | 2% | 28% | **40%** | **30%** |
| C4 | dưới 6.000.000 đ | 66% | 34% | 0% | 0% |
| | 6.000.000 đến 15.000.000 đ | 26% | 54% | 20% | 0% |
| | trên 15.000.000 đ | 8% | 32% | 34% | 26% |

**Ép một loại hộp** (thận trọng + giám định; H1 và RD đơn lẻ luôn 0% vì quá nghèo hoặc RD chỉ từ ngày 4):

| Cấu hình | Chỉ H2 | Chỉ H3 | Chọn theo tiền (C3 dòng "thận trọng" ở bảng trên) |
| --- | --- | --- | --- |
| Hiện tại | 30,3 | 14,9 | 36,9 |
| C1 | 28,8 | 18,7 | 33,5 |
| C2 | 28,8 | 19,9 | 31,3 |
| C3 | 12,1 | 14,1 | 22,3 |
| C4 | 9,5 | 12,6 | 18,9 |

## Phân tích

- **Hiện tại,** việc chọn hộp có một đáp án gần như cố định (H2) khi có từ 6.000.000 đ trở lên, và Rương Đen là nội dung chết. Chỉ khi nghèo (dưới 6.000.000 đ) mới có đánh đổi H1 và H2.
- **Lời tăng theo giá hộp** làm đáp án đổi theo vốn: nghèo H1, tầm trung H2 và H3, giàu H3 và Rương Đen. Đó là đánh đổi thật vì người chơi phải tính vốn hiện có, kỳ nợ sắp tới và rủi ro, không chọn "một loại tốt nhất".
- **Người chơi chọn theo tiền hơn chọn một loại đơn lẻ** 8 đến 18 điểm ở mọi cấu hình mới. Hiện tại chọn H2 mãi chỉ kém chọn theo tiền 6,6 điểm, còn ở C3 chọn H2 mãi kém tới 10 điểm.
- **C1 và C2 làm game dễ:** bot "mua đắt nhất" (không cần suy nghĩ) thắng 32% đến 43% (C1) và 27% đến 40% (C2). Tăng lời cho hộp đắt thì chiến lược "dồn vào hộp đắt" lại hiệu quả, trái mục tiêu ván khó.
- **C3 và C4 giữ độ khó:** bot thường 16% đến 29%, đầu tư 25% đến 29%. Người chơi tính toán hoàn hảo chỉ 19% đến 31%, khá thấp so với 37% đến 46% hiện nay. Cận trên thấp là phù hợp mục tiêu "không thắng dễ" nhưng cần xem người thử thật có thắng được không, vì người thật kém hơn bot tối ưu. Nếu thấy khó quá, C2 là mức giữa (bot thường 26% đến 40%).

## Đề xuất

| Hạng mục | Đề xuất |
| --- | --- |
| Cấu hình | **C3**: lời mỗi hộp khoảng 1.000.000 / 1.600.000 / 2.200.000 / 2.600.000 đ (H1 / H2 / H3 / RD), giá nền theo bảng trên |
| Rương Đen | Giữ làm lựa chọn của người đã có vốn (trên 15.000.000 đ). Hết là nội dung chết |
| M3 (Rương Đen từ ngày 1) | Vẫn là bẫy: người nghèo không thể mua. Xem lại nếu muốn thưởng |
| Khoảng cách kỹ năng | Với lời tăng theo giá hộp, "mua hộp đắt nhất" đã tốt. Muốn người biết tính thắng hơn rõ rệt cần thêm kỹ năng khác (manh mối, biến cố, nhịp vốn) |
| Kiểm tra | Chơi thử thật để xem C3 có thắng được không. Nếu khó quá lùi về C2 |

## Giả định và giới hạn

- Chọn loại hộp theo lời kỳ vọng tuyệt đối (bot). Người chơi thật có thể sợ rủi ro hơn hoặc chọn theo cảm tính.
- Chỉ đo kết hợp với cấu hình đã duyệt (G1, tạo tác đã chốt, luật dịch vụ chốt). Không đo kết hợp biến cố (đã đo riêng ở BienCoE3.md) hay giữ hàng.
- Giá nền V09 giảm mạnh ở C3 (8.950.000 đ xuống 4.600.000 đ) có thể làm Lá bùa mất vai "món Thần thoại rẻ": cần Claude 1 xem tên, mô tả và vị trí món này.
- Sai số khoảng ±1,3 điểm với 1.500 seed.

## Phát hiện

| Mã | Mức | Mô tả | Đề xuất | Chủ xử lý |
| --- | --- | --- | --- | --- |
| D-29 | Đã giải pháp | H2 chi phối việc chọn hộp | C3 | Claude 1, chủ dự án |
| D-30 | Đã giải pháp | Rương Đen không ai mua | C3 (30% lượt mua khi trên 15.000.000 đ) | Claude 1, chủ dự án |
| D-31 | Trung bình | Lời tăng theo giá hộp làm "mua hộp đắt nhất" thành mẹo tốt, thu hẹp khoảng cách bot và người tính toán | Thêm kỹ năng khác ngoài chọn loại hộp | Claude 1 |
| D-32 | Thấp | Giá nền V09 giảm 8.950.000 đ xuống 4.600.000 đ ở C3 | Claude 1 xem lại vai món này | Claude 1 |
