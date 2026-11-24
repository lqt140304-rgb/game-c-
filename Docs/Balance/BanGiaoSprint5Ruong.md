# Bàn giao giữa phiên: Sprint 5 phần 1 (rương cơ bản)

> **Đã xong (2026-11-24).** Kết quả cuối ở [Sprint5Ruong.md](Sprint5Ruong.md). File này chỉ còn giá trị lịch sử.

Viết ngày 2026-11-24 khi chuyển phiên. Phiên trước đã dựng mô phỏng và có số liệu sơ bộ; phiên mới **làm tiếp đến khi xong phần 1**. Đọc trước: [DaChot.md](../DaChot.md), [BanGiaoSprint5.md](BanGiaoSprint5.md), mục Claude 2 của Sprint 5 trong [TASKS.md](../../TASKS.md).

> **Tiền tệ:** số dưới đây theo đơn vị cân bằng, **1 đơn vị = 10.000 đ** (Rương Đen 1.200 = 12.000.000 đ). In ra bằng `QA/currency.py`.

## Việc của phần 1 và nghiệm thu

Cân bằng giá và tỉ lệ ra bậc của 4 rương cơ bản (Gỗ, Vải Niêm, Kim Khí, Đen), tỉ lệ rác, 4 ô tạo tác. Nghiệm thu: ngày 1 không thua ngay (hộp đầu sắp sẵn ra rác, hộp hai ra đồ Thường), đường thắng vẫn tồn tại, **bot thường thắng khoảng 15% đến 20%**, **người chơi tính toán hoàn hảo không quá 47%**.

## Đã làm

- `QA/chest_sim.py`: nạp hệ mới vào lõi mô phỏng đã duyệt (`proposal_sim`, `clue_sim`, `swing_sim`: nợ, phạt 20%, giá dao động ±20%, đọc manh mối, giám định, combo). Mô hình hóa: 5 bậc, quay bậc rồi quay món (trọng số món = tỉ lệ bậc / số món của bậc trong rương), 9 món rác dùng chung (bán 3 đơn vị, không giả), Kim Khí ra cả đồ lẫn tạo tác, 4 ô (BK1 mở ô 5), hộp ngày 1 sắp sẵn với 4 chế độ (`script`: `False`, `"any"`, `"free1"`, `"cheap"`). Mã nội bộ H1..RD xếp theo giá tăng dần.
- **Đã kiểm chứng bộ chuyển đổi:** nạp lại bảng cũ (H1..RD, giá X110, hàng giả 12/15/22/30%, 3 ô, không sắp sẵn) cho bot thận trọng 19,6%, liều 15,6%, người chơi chọn loại 40,4 / 46,3 / 46,1 / 46,8%, khớp [GiaHopDaoDong.md](GiaHopDaoDong.md) (19,0 / 15,2 và 40,2 / 46,3 / 46,5 / 46,9) trong sai số.
- `QA/chest_explore.py`: chạy song song các cấu hình ứng viên.

## Số liệu sơ bộ (n = 800 trừ khi ghi khác)

Thứ tự "người chơi giỏi" = chọn loại + manh mối + giám định: thận trọng / liều / combo nhỏ / combo lớn.

| Cấu hình | Bot thường (thận trọng, liều) | Người chơi giỏi | Ghi chú |
|---|---|---|---|
| Đề xuất khởi đầu của spec (Gỗ 120, Vải 350, Kim Khí 500 chỉ tạo tác, Đen 1.200), 4 ô, n=1.200 | 0,1 / 0,0 | 44,8 / 52,9 / 50,4 / 45,9 | **Bẫy:** Kim Khí EV 230 (0,46 lần giá), Đen EV 716 (0,60 lần). Bot mua rương đắt nhất nên thua gần hết |
| E1/F4: Gỗ 120 [R25 T72,5 H2 L0,5], Vải 350 [R20 T49 H25 X5 L1], Kim Khí 500 [R10 T25 H22 X8 L1, 34% tạo tác], Đen 1.100 [R25 T5 H20 X40 L10], **không sắp sẵn** | 14,8 / 14,6 | 39,8 / 46,0 / 42,1 / 40,0 | Gần mục tiêu. Chọn rương: Gỗ 21%, Vải 30%, Kim Khí 32%, Đen 17% |
| Như trên, sắp sẵn `"any"` (hai hộp đầu trả giá rương đã chọn) | 7,0 / 6,0 | 20,9 / 25,2 / 20,4 / 20,6 | **Hộp sắp sẵn tốn 8 điểm bot, 20 điểm người chơi giỏi** |
| Như trên, sắp sẵn `"free1"` (hộp đầu tặng) | 6,0 / 7,5 | 26,2 / 35,1 / 30,2 / 29,4 | |
| Như trên, sắp sẵn `"cheap"` (ngày 1 chỉ tốn giá Gỗ cho 2 hộp đầu) | 9,5 / 12,0 | 29,6 / 36,2 / 36,5 / 34,5 | Tốt nhất trong 3 cách sắp sẵn |
| Như E1 nhưng Đen 1.200 | 5,6 / 5,1 (sắp sẵn "any") | 22,0 / 26,8 / 20,9 / 20,1 | |

Lời kỳ vọng mỗi hộp của E1 (chưa tính dao động): Gỗ 171, Vải 204, Kim Khí 183 (tính tạo tác theo giá Kho), Đen 87. Mô hình cũ đã duyệt: H1 150, H2 208, H3 190, RD 15.

**Đang đo dở (chưa có kết quả, chạy lại bằng `python3 QA/chest_explore.py`):** G1..G4, không sắp sẵn, n=1.500. Cơ sở G1: Gỗ **140** [R15 T83 H1,5 L0,5], Vải 350 [R20 T49 H25 X5 L1], Kim Khí 500 [R10 T25 H22 X8 L1], Đen **1.200** [R25 T5 H20 X46 L4]; Huyền thoại cơ bản: Vé số 4.000, Bản đồ cổ 5.000, Đồng hồ bỏ túi 5.000, Két sắt 7.000. Lý do đổi: rác theo thứ tự "Gỗ ít, Vải vừa, Kim Khí vài, Đen nhiều nhất" (spec phần 1), Gỗ phải lời ít hơn Vải để rương rẻ không thắng áp đảo, Huyền thoại ở rương cơ bản hiếm (0,5% đến 4%).

## Kết luận tạm (cần xác nhận bằng số đo cuối)

1. **Bảng đề xuất khởi đầu của spec là bẫy cả ở Kim Khí lẫn Đen**: với 5 bậc, rác 25% đến 30% và hàng giả 30%, Đen chỉ đạt khoảng 1 lần giá khi Rất hiếm khoảng 45% trở lên. Kim Khí chỉ hợp lý khi ra cả đồ (theo bảng của ruong-chu-de) lẫn tạo tác.
2. **Hộp ngày 1 sắp sẵn làm ván đầu khó hơn hẳn** (mất 2 trên 3 lượt mua của ngày 1). Không thua ngay vẫn giữ được theo luật (nợ tồn ngày 1 tối đa 240, dưới ngưỡng thua 1.000), nhưng cần cách giảm thiệt: ngày 1 chỉ bán Gỗ cho 2 hộp đầu (đo tốt nhất), hoặc tặng hộp đầu. Đây là quyết định của chủ dự án.
3. Hướng hiệu chỉnh: **ván thường (không sắp sẵn) đạt mục tiêu**, ván đầu có sắp sẵn báo riêng như ván hướng dẫn.

## Còn phải làm trong phần 1

1. Chạy `QA/chest_explore.py`, chọn cấu hình cuối sao cho ván không sắp sẵn có bot thận trọng và liều 15% đến 20%, người chơi giỏi không quá 45% (để có biên sai số với trần 47%), không rương nào trên 50% lượt chọn, Đen được chọn 5% đến 20%. Chạy lại cấu hình cuối với n = 2.000.
2. Đo ván có sắp sẵn theo 3 chế độ trên cấu hình cuối. Đo ngày 1: tỉ lệ thua trước ngày 3, nợ tồn cuối ngày 1, trường hợp xấu nhất.
3. Độ nhạy: 3 / 4 / 5 ô (hệ quả 4 ô lên cộng hưởng, so với bot "combo lớn"); tạo tác trùng đổi tiền `dup_frac` 0 / 0,5 / 1 và `art_value` 0,5 / 1; giá nền món mới ±15%; Kim Khí chỉ ra tạo tác (để chứng minh bẫy).
4. Viết `Docs/Balance/Sprint5Ruong.md` (bảng cuối, số đo, cách tái hiện, câu hỏi mở), test khóa `QA/test_chest_balance.py` (kiểm chứng bộ chuyển đổi tái hiện bảng cũ; mục tiêu thắng; ngày 1 không thua; giá trị kỳ vọng rương theo thứ tự mong muốn), ghi trạng thái vào TASKS.md. Không thêm vào DaChot.md vì chưa có gì chủ dự án chốt trong phần này.

## Câu hỏi mở cho chủ dự án (ghi vào báo cáo)

1. **Xung đột tỉ lệ rác giữa hai spec:** `tier-do-trong-ruong` ghi Gỗ 20%, Vải Niêm 15%; `ruong-suu-tap` ghi "Gỗ ít, Vải Niêm vừa". Bản đo dùng thứ tự của `ruong-suu-tap`.
2. **Xung đột Huyền thoại ở rương cơ bản** (đã nêu ở BanGiaoSprint5.md): bản đo theo `ruong-chu-de` (có, hiếm).
3. **Tạo tác trùng từ Kim Khí** đổi tiền không, đổi bao nhiêu. Bản đo giả định 50% giá Kho.
4. **Giá rương:** Gỗ 120 hay 140, Đen 1.200 (spec) hay 1.500 (phương án C cũ). Đen 1.500 cần Rất hiếm khoảng 50% trở lên mới đủ lời.
5. **Hộp ngày 1 sắp sẵn:** chọn cách nào để giảm thiệt (chỉ bán Gỗ, tặng hộp đầu, hay chấp nhận ván đầu khó).
6. **Giá nền 25 món mới và nhóm nguồn gốc:** bản đo dùng giá đề xuất trong `chest_sim.NEW_ITEMS` (theo khoảng giá bậc của spec). Nhóm nguồn gốc ảnh hưởng giá trị của kỹ năng đọc manh mối, Claude 1 nên phân nhóm sao cho bậc cao trong một rương có nhóm khác bậc thấp.
7. **4 ô tạo tác:** combo gần như mặc định (BanGiaoSprint5.md mục 2); chờ số đo độ nhạy.
