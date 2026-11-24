# Giá hộp dao động theo ngày (phương án C), đo lại đầy đủ (Claude 2)

> **Tiền tệ:** số trong tài liệu này tính theo đơn vị cân bằng cũ, **1 đơn vị = 10.000 đ** (Rương Đen 1.500 = 15.000.000 đ). Xem [bảng quy đổi](README.md#tiền-tệ).

Chủ dự án chọn phương án C ([DieuChinhSauCanBang.md](../Content/DieuChinhSauCanBang.md) mục K): mỗi ngày giá mỗi loại hộp lệch ngẫu nhiên đều trong ±a (công bố đầu ngày, xác định theo seed, ngày, loại hộp, áp dụng từ ngày 1), E3 giảm thêm 20% giá H2 sau dao động, giá niêm yết đi theo manh mối đầu (λ = 0,75), lời mách Dì Mận 15 xu chắc đúng. Claude 1 nhờ đo lại bằng bộ chiến thuật đầy đủ, luật dịch vụ, biên độ ±15% và ±20%, và kết hợp hạ giá Rương Đen xuống 1.250. Số đo ban đầu của Claude 1 chỉ là sơ bộ.

Cấu hình cơ sở: gói đã duyệt (giá nền G1 nhân 1,10, hàng giả theo hộp, tạo tác và combo đã chốt). 1.500 seed mỗi ô, sai số khoảng ±1,3 điểm.

## Kết luận ngắn

1. **Giữ phương án C ±20% với Rương Đen 1.500.** Bot thường vẫn thắng 15% đến 19%, khoảng cách kỹ năng (người chơi chọn loại hộp hơn bot thường khoảng 21 điểm) được giữ, và người chơi tính hoàn hảo thắng 40% đến 47%, nằm trong mức đã chấp nhận.
2. **Dao động giá làm việc chọn hộp đa dạng hơn, nhưng vừa phải:** H2 giảm từ 63% xuống 52%, Rương Đen từ 0% lên 6% (±20%), 4% (±15%). Chưa giải hẳn D-30 nhưng Rương Đen không còn là nội dung chết.
3. **±20% nhỉnh hơn ±15% 1 đến 3 điểm** cho người chơi chọn loại hộp (vì ngày giá thấp có lợi nhiều hơn ngày giá cao thiệt), không đổi bot thường. Đề xuất ±20% như Claude 1 đã chọn.
4. **Hạ giá Rương Đen xuống 1.250 làm game dễ hơn rõ rệt** (bot thường 24% đến 30% thay vì 15% đến 19%, người chơi chọn loại 50% ở ±20%), đổi lấy Rương Đen được chọn 19% đến 23%. Đề xuất **không hạ**, trừ khi chủ dự án chủ ý muốn dễ hơn.
5. **Các luật dịch vụ và biến cố không phá cân bằng** khi đo trên bot bậc thang: đấu giá +1 đến +6 điểm, cầm tạo tác ±0,5, sòng bạc +0,5 đến +2,7, biến cố −0,7 đến +0,5. **Hạn chế:** dịch vụ chưa đo trên người chơi chọn loại (mục "Còn dở").

## Cách tái hiện

```bash
cd QA
python3 swing_sim.py                        # toàn bộ, khoảng 40 phút (nên chạy nền)
python3 -m unittest -v test_box_price_swing # test khóa ±20%, Rương Đen 1.500
```

## 1. Biên độ và giá Rương Đen

Thắng% (1.500 seed). **Người chơi chọn loại hộp** = chọn hộp có lời kỳ vọng cao nhất mỗi lượt, đọc manh mối miễn phí; cột "lời mách" thêm lời mách Dì Mận 15 xu. Thứ tự thận trọng / liều / combo nhỏ / combo lớn (đều có giám định).

| Biên độ | Rương Đen | Người chơi chọn loại | + lời mách Dì Mận | Chọn hộp (thận trọng + gđ) |
| --- | --- | --- | --- | --- |
| ±0% | 1.500 | 36,9 / 43,6 / 44,5 / 45,6 | 38,9 / 45,9 / 47,2 / 47,4 | H1 14%, H2 63%, H3 23%, RD 0% |
| ±15% | 1.500 | 38,3 / 43,4 / 45,4 / 47,3 | 39,5 / 45,5 / 48,3 / 48,9 | H1 18%, H2 56%, H3 22%, RD 4% |
| **±20%** | **1.500** | **40,2 / 46,3 / 46,5 / 46,9** | **42,2 / 48,7 / 49,1 / 48,5** | **H1 19%, H2 52%, H3 23%, RD 6%** |
| ±0% | 1.250 | 34,7 / 40,7 / 44,1 / 45,3 | 40,3 / 45,7 / 47,5 / 48,1 | H1 14%, H2 49%, H3 14%, RD 23% |
| ±15% | 1.250 | 38,8 / 46,3 / 46,1 / 47,8 | 41,5 / 50,3 / 50,1 / 49,8 | H1 16%, H2 47%, H3 16%, RD 20% |
| ±20% | 1.250 | 41,6 / 50,0 / 47,0 / 47,1 | 44,3 / 52,3 / 49,4 / 49,7 | H1 17%, H2 45%, H3 19%, RD 19% |

**Bot thường** (mua hộp đắt nhất mua được, không đọc manh mối), thắng%:

| Biên độ | Rương Đen | Thận trọng | + gđ | Liều | + gđ | Combo nhỏ | + gđ | CB1 + TC3 | Combo lớn | + gđ |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ±0% | 1.500 | 16,2 | 22,0 | 15,2 | 23,2 | 31,1 | 35,1 | 29,1 | 31,6 | 37,9 |
| ±15% | 1.500 | 18,2 | 24,5 | 14,9 | 24,2 | 31,0 | 37,3 | 30,5 | 32,7 | 38,8 |
| **±20%** | **1.500** | **19,0** | **25,1** | **15,2** | **24,7** | **32,3** | **38,1** | **32,9** | **35,1** | **39,9** |
| ±0% | 1.250 | 23,9 | 28,4 | 28,7 | 36,0 | 37,4 | 40,1 | 34,7 | 34,7 | 40,0 |
| ±15% | 1.250 | 27,0 | 31,1 | 28,9 | 36,1 | 37,9 | 40,6 | 36,1 | 37,1 | 42,9 |
| ±20% | 1.250 | 26,3 | 31,9 | 29,7 | 36,2 | 37,8 | 41,0 | 37,1 | 38,2 | 42,3 |

Nhận xét:

- **Giá cố định đến ±20% (Rương Đen 1.500):** bot thận trọng và liều thô 15% đến 19%, đúng mục tiêu "bot thường khoảng 15%". Người chơi chọn loại hộp 40% đến 47%. Khoảng cách kỹ năng khoảng 21 điểm, như Claude 1 đo.
- **Rương Đen 1.250** làm "mua hộp đắt nhất" thành mẹo tốt: bot thường 24% đến 30%, tức là Rương Đen vừa được chọn hơn vừa làm game dễ hơn. Đánh đổi này **không** đáng với mục tiêu ván khó.
- **Lời mách Dì Mận** cộng 1 đến 3 điểm ở mọi cấu hình, như đã đo ở KyNangManhMoi.md.

## 2. Luật dịch vụ và biến cố (±20%, Rương Đen 1.500)

Đo trên **bot bậc thang** (mua hộp đắt nhất). Thắng%, chênh so với không dịch vụ (19,0 / 25,1 / 15,2 / 24,7 / 32,3 / 38,1 / 32,9 / 35,1 / 39,9).

| Luật | Thận trọng | + gđ | Liều | + gđ | Combo nhỏ | + gđ | Combo lớn | + gđ |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Đấu giá k 1,16, món từ 1.300, sàn 1,0 | 24,6 (+5,6) | 29,7 (+4,6) | 17,3 (+2,1) | 24,2 (−0,5) | 36,9 (+4,6) | 39,7 (+1,6) | 36,1 (+1,0) | 41,3 (+1,4) |
| Cầm tạo tác | 19,0 (0,0) | 25,1 (0,0) | 15,2 (0,0) | 24,7 (0,0) | 31,9 (−0,4) | 38,4 (+0,3) | 35,6 (+0,5) | 39,7 (−0,2) |
| Sòng bạc 100/250/500, 2 lượt | 19,9 (+0,9) | 26,1 (+1,0) | 17,9 (+2,7) | 26,6 (+1,9) | 32,9 (+0,6) | 38,6 (+0,5) | 35,9 (+0,8) | 40,6 (+0,7) |
| Biến cố E1 đến E6 | 19,2 (+0,2) | 25,6 (+0,5) | 16,1 (+0,9) | 24,2 (−0,5) | 32,6 (+0,3) | 37,2 (−0,9) | 34,7 (−0,4) | 39,1 (−0,8) |

- **Đấu giá k 1,16:** lời nhẹ (+1 đến +6 điểm), cao nhất cho thận trọng. Không phá cân bằng. Giữ.
- **Cầm tạo tác:** vô hại (không quá ±0,5).
- **Sòng bạc:** +0,5 đến +2,7 điểm, nhỏ, đúng "canh bạc không phải đường cứu".
- **Biến cố:** trung tính (không quá ±0,9), như BienCoE3.md. Với Rương Đen 1.250 và biến cố (±20%): bot thường 27,5 / 32,7 / 30,6 / 36,1, người chơi chọn loại 41,8 / 48,1 / 46,8 / 46,3, Rương Đen được chọn 19%, cùng kết luận: dễ hơn 1.500 khoảng 8 đến 12 điểm cho bot thường.

## 3. Test khóa

`QA/test_box_price_swing.py` (700 seed, ±20%, Rương Đen 1.500) kiểm 5 điều: bot thường 10% đến 26%, khoảng cách kỹ năng ít nhất 12 điểm, người chơi hoàn hảo không quá 52%, Rương Đen được chọn 2% đến 15%, H2 dưới 62% lượt mua. Cả 5 đạt.

## 4. Đề xuất

| Hạng mục | Đề xuất |
| --- | --- |
| Biên độ giá hộp | **±20%** (phương án C đã chọn) |
| Giá gốc Rương Đen | **Giữ 1.500** (15.000.000 đ). Hạ xuống 1.250 làm bot thường thắng 24% đến 30%, quá dễ so với 15% đến 19% |
| Luật dịch vụ | Giữ nguyên (đấu giá k 1,16, cầm tạo tác, sòng bạc), không phá cân bằng |
| Biến cố | Giữ nguyên |
| Rương Đen chưa là lựa chọn đầy đủ | Chỉ được chọn 6%. Muốn nhiều hơn mà không dễ hơn, cần thay đổi khác (ví dụ tăng phương sai hoặc phần thưởng riêng của Rương Đen), chưa đo |

## 5. Còn dở

- **Dịch vụ chưa đo trên người chơi chọn loại hộp:** mô phỏng `run_config` truyền luật dịch vụ cho bot bậc thang mà chưa truyền cho chiến thuật chọn loại (`measure_turns` chưa nhận `opt`). Cần thêm tham số `opt` rồi đo lại bảng mục 2 cho bốn chiến thuật chọn loại.
- Bot chọn hộp theo lời kỳ vọng tuyệt đối, người thật có thể khác.
