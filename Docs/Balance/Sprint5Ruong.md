# Sprint 5 phần 1: giá và tỉ lệ ra bậc của 4 rương cơ bản (Claude 2)

Viết ngày 2026-11-24. Phần 1 của việc cân bằng Sprint 5: giá và tỉ lệ ra bậc của 4 rương cơ bản (Gỗ, Vải Niêm, Kim Khí, Đen), tỉ lệ rác, 4 ô tạo tác. **Bảng dưới đây là ĐỀ XUẤT, chưa chốt**: chờ chủ dự án duyệt, các điểm cần quyết ghi ở mục "Câu hỏi mở". Bàn giao giữa phiên trước đó: [BanGiaoSprint5Ruong.md](BanGiaoSprint5Ruong.md).

> **Tiền tệ:** mọi số tiền trong tài liệu này ghi bằng **đồng Việt Nam** (1 xu cũ = 10.000 đ, xem [bảng quy đổi](README.md#tiền-tệ)). Mô phỏng và `QA/chest_sim.py` vẫn tính theo đơn vị cân bằng 10.000 đ (Rương Đen `price=1200` là 12.000.000 đ); `QA/currency.py` đổi sang đồng khi in.

## Kết luận ngắn

1. **Nghiệm thu đạt** trên ván thường (n = 2.000): bot thường thắng **18,1% (thận trọng) và 19,4% (liều)**, người chơi tính toán hoàn hảo thắng **37,4% đến 44,0%** (trần 47%), không rương nào trên 38% lượt chọn, Rương Đen được chọn 17%.
2. **Ngày 1 không thua ngay** ở mọi cách chơi và mọi chế độ hộp sắp sẵn: 0 ván thua trước ngày 3 trên 2.000 ván (không sắp sẵn) và 1.500 ván mỗi chế độ sắp sẵn. Nợ tồn cuối ngày 1 xấu nhất 2.390.000 đ, xa ngưỡng thua 10.000.000 đ.
3. **Đường thắng vẫn tồn tại ở lần chơi đầu** (hộp sắp sẵn, spec mo-man: "chỉ cho riêng lần chơi đầu"), nhưng ván đó khó hơn rõ: bot thường 6,5% đến 15%, người chơi giỏi 19% đến 36% tùy cách sắp sẵn. Cách ít thiệt nhất: ngày 1 của lần chơi đầu chỉ bán Rương Gỗ cho 2 hộp đầu. Chủ dự án chọn.
4. **Bảng khởi đầu của spec là bẫy**: Kim Khí chỉ ra tạo tác thu về 0,46 lần giá, Đen 0,60 lần giá; bot thường (mua rương đắt nhất mua được) thắng 0,0% đến 0,2%. Bảng đề xuất sửa bằng cách cho Kim Khí ra cả đồ lẫn tạo tác (như bảng bộ sưu tập của `ruong-chu-de`), hạ Kim Khí xuống 4.500.000 đ và đẩy Rất hiếm của Đen lên 55%.
5. **4 ô tạo tác không phá cân bằng kinh tế**: 3, 4 hay 5 ô chênh dưới 1 điểm thắng. 4 ô làm bot combo lớn đạt CB3 gấp đôi (12% lên 25%), tức là vấn đề nằm ở **thiết kế** (combo thành mặc định, ít quyết định chọn hệ), không ở tiền. Giữ 4 ô được về số; việc có thêm tạo tác hay không là của Claude 1 và chủ dự án.
6. **Điểm nhạy nhất là giá nền 25 món mới**: ±15% làm bot thường đổi từ 10% lên 24%. Khi Claude 1 hoặc chủ dự án chốt giá món mới, phải đo lại (chạy `test_chest_balance`).

## 1. Bảng đề xuất

Tỉ lệ ra bậc: **R** Rác, **T** Thường, **H** Hiếm, **X** Rất hiếm, **L** Huyền thoại. Luật quay hai bước của `ruong-chu-de`: quay bậc, rồi quay đều một món trong bộ sưu tập của rương ở bậc đó. Rác dùng chung 9 món, bán 30.000 đ, không giả.

| Rương | Giá | R | T | H | X | L | Tạo tác | Hàng giả | Manh mối đúng |
|---|---|---|---|---|---|---|---|---|---|
| Gỗ | 1.400.000 đ | 15% | 83% | 1,5% | 0% | 0,5% | — | 10% | 2 trên 3 |
| Vải Niêm | 3.500.000 đ | 22% | 49% | 23% | 5% | 1% | — | 15% | 2 trên 3 |
| Kim Khí | 4.500.000 đ | 10% | 25% | 22% | 8% | 1% | **34%** (bậc 1: 60%, bậc 2: 35%, bậc 3: 5%) | 0% | 1 trên 3 |
| Đen | 12.000.000 đ | 25% | 5% | 10% | 55% | 5% | — | 30% | 1 trên 3 |

So với bảng khởi đầu của spec: Gỗ 1.200.000 đ lên **1.400.000 đ**, Kim Khí 5.000.000 đ xuống **4.500.000 đ** và ra cả đồ, Đen giữ **12.000.000 đ** nhưng Rất hiếm 25% lên **55%**, rác của Đen 30% xuống 25%, Vải Niêm rác 15% lên 22%. Huyền thoại có ở cả 4 rương nhưng hiếm (0,5% đến 5%), theo `ruong-chu-de`.

Rác theo thứ tự của `ruong-suu-tap`: Gỗ ít (15%), Vải Niêm vừa (22%), Kim Khí vài món máy móc hỏng (10%), Đen nhiều nhất (25%).

**Tạo tác từ Kim Khí** (giả định, spec chưa chốt): tạo tác chưa có thì vào ô trống; trùng hoặc hết ô thì đổi **50% giá Kho**. Tính theo giá Kho Tạo Tác.

**Giá nền dùng để đo** (đề xuất của Claude 2, nằm trong khoảng giá bậc của spec; món đã có giữ giá X110 đang dùng). Đầy đủ ở `chest_sim.NEW_ITEMS`. Huyền thoại cơ bản: Vé số độc đắc 40.000.000 đ, Bản đồ cổ vẽ tay 50.000.000 đ, Đồng hồ bỏ túi 50.000.000 đ, Két sắt chưa ai mở được 70.000.000 đ. Món mới bậc Thường 2.200.000 đến 3.500.000 đ, Hiếm 15.000.000 đến 19.000.000 đ, Rất hiếm 27.000.000 đến 30.000.000 đ.

### Giá trị kỳ vọng mỗi hộp (chưa tính dao động giá ±20%)

| Rương | Giá | Thu kỳ vọng | Lời kỳ vọng | Thu / giá |
|---|---|---|---|---|
| Gỗ | 1.400.000 đ | 3.200.000 đ | 1.800.000 đ | 2,28 |
| Vải Niêm | 3.500.000 đ | 5.350.000 đ | 1.850.000 đ | 1,53 |
| Kim Khí | 4.500.000 đ | 6.870.000 đ | 2.370.000 đ | 1,53 |
| Đen | 12.000.000 đ | 13.030.000 đ | 1.030.000 đ | 1,09 |

Gỗ an toàn nhất (tỉ lệ cao) nhưng không lời tuyệt đối nhiều nhất; Đen lời ít nhất so với giá nhưng phương sai lớn (Huyền thoại 5%, Rất hiếm 55%), đúng vai "đánh cược lớn". Mô hình cũ đã duyệt có lời mỗi hộp H1 1.500.000 đ, H2 2.080.000 đ, H3 1.900.000 đ, Rương Đen 150.000 đ.

## 2. Số đo trên ván thường (không sắp sẵn)

Lõi mô phỏng đã duyệt (nợ, phạt 20%, giá hộp dao động ±20%, đọc manh mối, giám định, combo), Rương Đen bán từ ngày 4. Bot thường mua rương đắt nhất mua được. Người chơi giỏi = chọn loại rương theo lời kỳ vọng, đọc manh mối, giám định (λ = 0,75). n = 2.000, sai số khoảng ±1,7 điểm.

| Nhóm | Thận trọng | Liều | Combo nhỏ | Combo lớn | Thận trọng + giám định chọn lọc |
|---|---|---|---|---|---|
| Bot thường | **18,1** | **19,4** | — | 20,4 | 21,6 |
| Người chơi giỏi | 37,4 | **44,0** | 38,8 | 37,5 | — |

Người chơi giỏi chọn rương: Gỗ 20%, Vải Niêm 25%, Kim Khí 38%, Đen 17%.

Khoảng cách kỹ năng (người chơi giỏi thận trọng trừ bot thận trọng) 19 điểm, gần mức của mô hình cũ (21 điểm).

## 3. Ngày 1 và lần chơi đầu (hộp sắp sẵn)

`mo-man`: hộp đầu ra rác hài, hộp hai ra đồ Thường bán được, **chỉ ở lần chơi đầu**. Mô phỏng có 4 chế độ:

- **không:** ván thường.
- **any:** hai hộp đầu sắp sẵn, trả giá rương đã chọn (đúng spec, không đổi gì khác).
- **free1:** hộp đầu tặng (hoàn tiền), hộp hai trả giá.
- **cheap:** ngày 1 của lần chơi đầu cửa hàng chỉ bán Gỗ cho 2 hộp đầu (đo như hai hộp đầu chỉ tốn giá Gỗ).

| Chế độ | Bot thận trọng | Bot liều | Người chơi giỏi (liều) | Thua trước ngày 3 | Ván có nợ tồn cuối ngày 1 | Nợ tồn ngày 1 xấu nhất |
|---|---|---|---|---|---|---|
| không (n = 2.000) | 18,1 | 19,4 | 44,0 | 0% | 9,9% (bot liều) | 2.380.000 đ |
| any (n = 1.500) | 6,5 | 7,1 | 25,5 | 0% | 16,9% (bot liều) | 2.390.000 đ |
| free1 | 8,5 | 10,3 | 33,9 | 0% | 1,2% | 2.080.000 đ |
| cheap | 10,1 | 14,9 | 35,7 | 0% | 0,1% | 1.340.000 đ |

Ngày thua sớm nhất ở mọi chế độ là cuối ngày 3. Nợ ngày 1 chỉ 2.000.000 đ, có phạt 20% thì nợ tồn tối đa 2.400.000 đ, dưới ngưỡng thua 10.000.000 đ, nên **theo luật ngày 1 không thể thua**, và mô phỏng xác nhận.

Hộp sắp sẵn tốn nhiều vì nó lấy 2 trên 3 lượt mua của ngày 1 và trả thấp hơn kỳ vọng. Nếu muốn lần chơi đầu cũng ở mức 15% đến 20% cho bot thường, cần thêm một điều mo-man chưa có (ví dụ "cheap" kèm vốn đầu ván cao hơn chút), đó là việc của chủ dự án.

## 4. Độ nhạy (n = 1.000, không sắp sẵn)

| Thay đổi | Bot thận trọng / liều | Người chơi giỏi (thận trọng / liều / combo nhỏ / combo lớn) | Chọn Đen |
|---|---|---|---|
| Bảng đề xuất (n = 2.000) | 18,1 / 19,4 | 37,4 / 44,0 / 38,8 / 37,5 | 17% |
| 3 ô | 16,6 / 16,9 | 35,7 / 42,2 / 36,5 / 35,5 | 17% |
| 5 ô | 16,3 / 17,0 | 35,7 / 42,1 / 36,4 / 35,4 | 17% |
| Tạo tác trùng không đổi tiền | 15,8 / 17,2 | 36,2 / 42,8 / 36,2 / 35,6 | 17% |
| Tạo tác trùng đổi đủ giá Kho | 17,5 / 18,2 | 36,7 / 43,3 / 37,5 / 37,3 | 18% |
| Tạo tác chỉ đáng nửa giá Kho | 16,3 / 17,0 | 37,9 / 44,3 / 39,5 / 38,9 | 18% |
| Giá món mới −15% | 10,2 / 10,2 | 33,8 / 41,4 / 34,8 / 34,2 | 10% |
| Giá món mới +15% | 20,6 / **24,1** | 39,6 / 44,7 / 39,4 / 38,7 | 22% |
| Kim Khí chỉ ra tạo tác, giá 5.000.000 đ (spec) | **0,3 / 0,1** | 36,5 / 43,6 / 43,3 / 41,6 | 18% (Kim Khí 0%) |
| Bảng khởi đầu của spec | **0,2 / 0,0** | 56,0 / **63,1** / 62,6 / 58,5 | 4% (Gỗ 63%) |

Số ô và tỉ lệ đạt combo (bot, n = 800; thắng / CB1 / CB3 / BK1):

| Số ô | Thận trọng | Combo nhỏ | Combo lớn |
|---|---|---|---|
| 3 | 17,0 / 12 / 0 / 19 | 18,6 / 55 / 0 / 56 | 18,9 / 55 / **12** / 56 |
| 4 | 16,6 / 12 / 0 / 19 | 18,2 / 55 / 0 / 56 | 18,5 / 55 / **25** / 56 |
| 5 | 16,6 / 12 / 0 / 19 | 18,2 / 55 / 0 / 56 | 18,9 / 55 / **27** / 56 |

Đọc bảng:

- **Bảng khởi đầu của spec vừa quá khó cho bot thường vừa quá dễ cho người chơi giỏi** (Gỗ 1.200.000 đ thu 3,02 lần giá, người chơi giỏi chỉ mua Gỗ và thắng 63%). Không dùng được.
- **Kim Khí chỉ ra tạo tác là bẫy**: không người chơi giỏi nào mua, bot mua thì gần như chắc thua. Kim Khí cần ra cả đồ.
- **Số ô và luật tạo tác trùng** chỉ lệch 1 đến 3 điểm: chọn theo thiết kế, không vướng cân bằng.
- **Giá món mới** là đòn bẩy lớn nhất. Giá chốt nên giữ trung bình theo bậc gần mức đo ở đây, hoặc đo lại.

## 5. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_chest_balance       # test khóa, khoảng 40 giây
nohup python3 chest_final.py final > final.log & # bảng cuối và hộp sắp sẵn, khoảng 10 phút
nohup python3 chest_final.py sens > sens.log &   # độ nhạy, khoảng 10 phút
python3 chest_final.py slots                     # số ô và combo, khoảng 2 phút
python3 chest_explore.py                         # dò cấu hình ứng viên G1..G4
```

Mã: `QA/chest_sim.py` (bộ chuyển đổi: nạp bảng 5 bậc, rác, tạo tác, 4 ô, hộp sắp sẵn vào lõi đã duyệt; `FINAL` là bảng đề xuất; `day1_stats` đo ngày 1; `restore()` trả lõi về như cũ để test khác không bị ảnh hưởng). Bộ chuyển đổi đã được kiểm chứng ở phiên trước: nạp lại bảng cũ cho kết quả khớp [GiaHopDaoDong.md](GiaHopDaoDong.md) trong sai số.

Đường dò: G1..G4 (Gỗ 1.400.000 đ, Kim Khí 5.000.000 đ, Đen 12.000.000 đ với Rất hiếm 46% đến 50%) cho bot 13% đến 16%, người chơi giỏi liều 46% đến 54%; Vải Niêm tốt hơn (G4) đẩy người chơi giỏi lên 54%, nên Vải Niêm là đòn bẩy của người chơi giỏi, còn Đen là đòn bẩy của bot. H1..H4 đẩy Đen lên Rất hiếm 55%; H2 (thêm Kim Khí 4.500.000 đ và Vải Niêm kém đi một chút) là bảng cuối.

## 6. Câu hỏi mở cho chủ dự án

1. **Duyệt bảng đề xuất** ở mục 1 (giá Gỗ 1.400.000 đ, Kim Khí 4.500.000 đ, Đen 12.000.000 đ; tỉ lệ bậc; Kim Khí ra cả đồ lẫn tạo tác 34%).
2. **Xung đột tỉ lệ rác giữa hai spec:** `tier-do-trong-ruong` ghi Gỗ 20%, Vải Niêm 15%; `ruong-suu-tap` ghi "Gỗ ít, Vải Niêm vừa". Bản đo theo `ruong-suu-tap`. Chọn spec nào thì sửa spec còn lại.
3. **Huyền thoại ở rương cơ bản:** `tier-do-trong-ruong` ghi 0%, `ruong-chu-de` nói có. Bản đo theo `ruong-chu-de` (0,5% đến 5%).
4. **Tạo tác trùng từ Kim Khí** đổi tiền không, bao nhiêu (bản đo 50% giá Kho; 0% hay 100% chỉ lệch 1 đến 2 điểm).
5. **Hộp sắp sẵn ở lần chơi đầu:** chấp nhận ván đầu khó hơn (đúng spec, bot 6,5%), hay ngày 1 của lần chơi đầu chỉ bán Gỗ (bot 10% đến 15%, đo tốt nhất), hay tặng hộp đầu.
6. **Giá nền 25 món mới và nhóm nguồn gốc** (Claude 1): bản đo dùng giá đề xuất trong `chest_sim.NEW_ITEMS`. Giá lệch trung bình quá ±5% thì cần đo lại. Nhóm nguồn gốc nên phân sao cho bậc cao trong một rương khác nhóm bậc thấp, để kỹ năng đọc manh mối còn giá trị.
7. **4 ô tạo tác:** về kinh tế không sao; về thiết kế, combo gần như mặc định (BanGiaoSprint5.md mục 2). Thêm tạo tác hay giữ 6 là quyết định của Claude 1 và chủ dự án.
8. **Rương Đen 12.000.000 đ hay 15.000.000 đ:** phương án C cũ giữ 15.000.000 đ trên bảng cũ. Với 5 bậc và rác, Đen 15.000.000 đ (Thường 5%, Hiếm 5%, Huyền thoại 5%) cần Rất hiếm 75% và rác chỉ 10% mới đạt thu 1,08 lần giá như mức 12.000.000 đ ở đây (Rất hiếm 70% mới hòa vốn), trái với "Đen nhiều rác nhất"; đề xuất 12.000.000 đ.

## 7. Chưa làm (phần sau của Sprint 5)

Rương mở khóa (Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý, chủ đề), mức phạt và lỗ khi soi sai thật giả, giá ép khi soi thiếu, điểm phát triển của cây. Dịch vụ chưa đo trên người chơi chọn loại (GiaHopDaoDong.md mục 5).
