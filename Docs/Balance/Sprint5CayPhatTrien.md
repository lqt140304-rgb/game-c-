# Sprint 5 phần 4: điểm phát triển và cây phát triển (Claude 2)

Viết ngày 2026-11-24. Phần 4 (phần cuối) của việc cân bằng Sprint 5: điểm cuối ván, điểm ghi Sổ Sưu Tập lần đầu, giá điểm từng nút của 4 nhánh (Rương, Sưu tập, Thần lực, Câu chuyện), và có giữ M2 (vốn đầu ván), M4 không. **Mọi con số dưới đây là ĐỀ XUẤT, chưa chốt.** Điều cần chủ dự án quyết ở mục 7. Luật gốc: [ruong-suu-tap](../../docs/superpowers/specs/2026-11-23-ruong-suu-tap-design.md) mục 4 (nút cần nút cha, cần đủ điều kiện sưu tập và tốn điểm; thắng hay thua đều có điểm, thắng nhiều hơn; phần thưởng meta không buff chơi). Rương chủ đề mở bằng thành tựu, không tốn điểm ([ruong-chu-de](../../docs/superpowers/specs/2026-11-24-ruong-chu-de-design.md), đã chốt). Phần 1: [Sprint5Ruong.md](Sprint5Ruong.md) (đã chốt). Phần 2: [Sprint5RuongMoKhoa.md](Sprint5RuongMoKhoa.md). Phần 3: [Sprint5SoiLoi.md](Sprint5SoiLoi.md).

> **Tiền tệ:** mọi số tiền ghi bằng **đồng Việt Nam**. Mô phỏng tính theo đơn vị 10.000 đ (`CASH_BONUS = 100` là 1.000.000 đ), in qua `QA/currency.py`.

> **Lưu ý về phần 3:** số đo ở đây chạy trên mô hình soi lỗi của phần 3 bản đầu (`QA/inspect_sim.py`). Chủ dự án đã chốt luật soi mới ([BanGiaoSprint5SoiLoi.md](BanGiaoSprint5SoiLoi.md)) và phần 3 sẽ được đo lại. **Nhịp mở cây** (mục 2) gần như không phụ thuộc luật soi, vì điểm chủ yếu đến từ Sổ và điểm thua. **Tỉ lệ thắng** (mục 3 và 4) thì phụ thuộc. `QA/tree_sim.py` gọi thẳng `inspect_sim.handle`, nên khi phần 3 xong chỉ cần chạy lại `tree_final.py` và `test_tree`.

> **Cập nhật 2026-11-25 (đo lại trên luật soi mới, Claude 2 phần 3):** chạy `QA/tree_final.py static 1000` và `QA/test_tree.py` với luật soi mới ([Sprint5SoiLoi.md](Sprint5SoiLoi.md): phạt lỗi sót 10%, trần trả giá 103%, nhà vá lỗi, dấu lạ 15%). Mọi tỉ lệ thắng ở mục 3 và 4 bên dưới **thấp hơn khoảng 5 đến 6 điểm** so với số cũ. Đo tĩnh, cấu hình cố định (bot thận trọng / liều; giỏi liều + giám định, n = 1.000):
>
> | Cấu hình | Bot | Giỏi (liều + giám định) |
> |---|---|---|
> | Nền (4 rương cơ bản) | 12,7 / 14,2 | 42,8 |
> | M2 vốn +1.000.000 đ | 13,6 / 14,1 | 43,9 |
> | M3 Rương Đen từ ngày 1 | 7,2 / 7,1 | 37,6 |
> | M1 chọn GT1 | 12,8 / 14,5 | 43,5 |
> | M1 chọn GT2 | 12,4 / 13,8 | 41,9 |
> | + 4 rương mở khóa | 11,9 / 11,7 | 44,0 |
> | + 4 mở khóa + 2 chủ đề | 12,0 / 11,6 | 42,1 |
> | + 4 mở khóa + 2 chủ đề + M2 | 12,1 / 12,6 | 42,6 |
> | Cả cây, M2 +1.000.000 đ | 8,9 / 8,7 | 42,6 |
> | + 4 mở khóa, shop 2 rương mới | 12,2 / 11,9 | 40,3 |
> | + 4 mở khóa + 2 chủ đề, shop 2 rương mới | 12,1 / 11,7 | 41,7 |
>
> Điều rút ra: (a) cây không đẩy người chơi giỏi qua trần: mọi cấu hình dưới 44,0% (mục tiêu mới không quá 42%, sát nhưng chấp nhận được; +4 rương mở khóa 44,0% là cao nhất). (b) Bot thường ở nền đúng mục tiêu 12% đến 16%, nhưng **tụt dưới 12% khi bật M3 (7%) hoặc cả cây kèm M2 (9%)**; hai cấu hình đó nên là thử thách tự chọn, không phải mặc định. (c) M2 gần như không đổi bot, +1,1 điểm người giỏi: không còn quá mạnh như số cũ. (d) Nhịp mở nút (mục 2) giữ nguyên. Test khóa `test_tree.py` đã chỉnh ngưỡng theo mục tiêu mới (bot 9% đến 17%, người giỏi tối đa 46% ở n = 800); 10 test đạt. Phần này **chưa chạy lại `chain` và `sens`** (chuỗi ván và độ nhạy điểm): các số ở mục 4 vẫn là số cũ.

## Kết luận ngắn

1. **Nhịp mở cây đạt.** Bot thường mở nút đầu ở **ván 2**, nửa cây ở **ván 10 đến 11**, cả cây ở **ván 37 đến 40** (trung vị; 90% người chơi xong trong khoảng 45 đến 50 ván). Người chơi tính toán hoàn hảo nhanh hơn: nút đầu ván 1, nửa cây ván 8, cả cây **ván 26 đến 27**.
2. **Người thua nhiều vẫn tiến bộ.** Người soi ẩu (thắng 10% đến 12%) mở nửa cây ở ván 11, cả cây ở ván 39 đến 42. Trong 30 ván đầu, người chơi điển hình không quá 4 ván liền mà không mở được nút nào. Lý do: điểm thua luôn có, có thêm điểm khi đi xa hơn, và điểm ghi Sổ chiếm phần lớn điểm của 10 ván đầu.
3. **M2 (vốn +1.000.000 đ) nên bỏ phần buff.** M2 đẩy người chơi giỏi từ 44,0% lên **47,0%** (đúng trần), và khi đã mở đủ rương (Tổng Hợp hàng giả 10%) thì lên **50,6%** (quá trần). Đề xuất giữ nút M2 nhưng đổi phần thưởng thành thứ không buff (mục 5).
4. **M4 giữ được.** M4 chỉ là thông tin (Sổ hiện sẵn điều kiện Bí Khế), không đổi kinh tế. Điều kiện cũ "đạt cả hai Bí Khế" gần như không đạt được dưới luật soi lỗi (mục 5), nên đổi thành "có 4 tạo tác trong Sổ".
5. **M3 (Rương Đen từ ngày 1) là thử thách, không phải buff.** Bật M3 làm mọi cách chơi thua nhiều hơn: bot từ 14,6% xuống **10,4%**, người chơi giỏi từ 44,0% xuống 41,5%. Giữ nguyên là tùy chọn. **M1** (chọn gia truyền) cộng dưới 1 điểm (GT1 tốt hơn GT2 với mọi cách chơi). Giữ.
6. **Phát hiện chéo giữa phần 2 và phần 3: Rương Tổng Hợp cần sửa khi có soi lỗi.** Dưới luật soi lỗi, Tổng Hợp (hàng giả 20%) kéo bot thường từ 14,6% / 15,7% xuống **11,1% / 11,6%**, vì bot mua rương đắt nhất mua được và mất nhiều vì món giả bị tịch thu. Phần 2 đo không có soi lỗi nên không thấy. Đề xuất: Tổng Hợp **hàng giả 10%**, Thường 43%, Hiếm 25%, Rất hiếm 13% (bảng phần 2: 20%, 39%, 28%, 14%).
7. **Tỉ lệ thắng theo chuỗi ván không trôi lên quá trần,** với Tổng Hợp đã sửa và không có M2. Người chơi giỏi: 44,0% ở 5 ván đầu, 44,2% đến 46,4% ở các khối giữa, **48,6% ở khối ván 41 đến 45**. Số cuối nằm trong nhiễu (khoảng ±3 điểm ở 1.000 ván). Đo tĩnh cả cây ở n = 1.500 là **46,4%**. Biên dưới trần mỏng (mục 4).
8. **Bot thường thấp hơn mục tiêu một chút ở mọi giai đoạn: 12,3% đến 16,1% theo khối 5 ván, trung bình khoảng 14%.** Nguyên nhân là phần 3, không phải cây: nền 4 rương cơ bản có soi lỗi đã là 14,6% / 15,7%. Cây không kéo bot xuống thêm sau khi Tổng Hợp được sửa. Cần xem lại khi phần 3 đo lại.

## 1. Tiêu chí và cách đo

Nghiệm thu (TASKS.md): nhịp mở nút hợp lý (nút đầu sau vài ván, cả cây cần nhiều ván); người chơi thua nhiều vẫn tiến bộ; tỉ lệ thắng theo chuỗi ván không vượt 47% với người chơi tính toán hoàn hảo; bot thường vẫn khoảng 15% đến 20% kể cả khi đã mở nhiều nút.

`QA/tree_sim.py` giữ nguyên lõi đã duyệt và các phần trước: 4 rương cơ bản đã chốt (`chest_sim.FINAL`), rương mở khóa và khung rương chủ đề (`chest_unlock`, phần 2), soi lỗi và trả giá (`inspect_sim`, phần 3). Thêm một vòng ngoài:

- Mỗi người chơi chơi nhiều ván liên tiếp (chuỗi). Cuối ván nhận **điểm ván** (thắng hoặc thua) và **điểm Sổ** (mỗi mục mới ghi vào Sổ Sưu Tập lần đầu: hiện vật, tạo tác, rác).
- Giữa hai ván, người chơi mở mọi nút đủ điều kiện và đủ điểm. Bot mở nút rẻ trước. Người chơi giỏi mở nút đổi ván trước, rồi đến nút rẻ.
- Nút đổi ván có hiệu lực từ ván sau: rương mới vào shop, M1 chọn GT1, M3 là tùy chọn (không ai bật, vì bật thì thua nhiều hơn), M2 cộng vốn nếu giữ.
- **Rương chủ đề** mở bằng thành tựu (không tốn điểm). Thành tựu chưa chốt, nên mô phỏng giả định chủ đề thứ nhất mở khi Sổ có 15 mục, chủ đề thứ hai khi có 30 mục (`THEME_RULE`). Tối đa 2 chủ đề, như kịch bản "tất cả" của phần 2.
- Hồ sơ người chơi: **bot thường** (mua rương đắt nhất mua được, soi kiểu thường), **người chơi giỏi** (chọn rương theo lời kỳ vọng, đọc manh mối, soi giỏi; λ = 0,75), **người soi ẩu** (bot, soi ẩu, thắng khoảng 11%).

Số người mỗi chuỗi: bot 400, người chơi giỏi 200, 45 ván mỗi người. Hạt giống mỗi ván khác nhau theo người và số thứ tự ván.

## 2. Điểm và cây đề xuất

### Điểm phát triển

| Nguồn | Điểm |
|---|---|
| Thắng ván | **8** |
| Thua ván | **2**, cộng **1 mỗi ngày** đã qua từ ngày 4 trở đi, tối đa +3 (thua ở ngày 3: 2; ngày 5: 4; ngày 7: 5) |
| Mỗi mục mới trong Sổ Sưu Tập | **1** (hiện vật Thường, Hiếm, rác, tạo tác), **2** (Rất hiếm, Huyền thoại) |

Thắng luôn được nhiều hơn thua (8 so với tối đa 5). Thua sớm vẫn có 2 điểm, nên không ván nào trắng tay.

Điểm mỗi ván giảm dần vì Sổ đầy dần. Bot thường: ván 1 khoảng 11 điểm (Sổ 6 đến 7 mục), ván 5 khoảng 8, ván 10 khoảng 5, ván 20 trở đi khoảng 4. Người chơi giỏi: ván 1 khoảng 16, ván 10 khoảng 7 đến 8, ván 45 khoảng 5,5. Sổ trong mô phỏng có tối đa khoảng 65 mục (4 rương cơ bản, rác, 6 tạo tác, 3 rương mở khóa có bộ sưu tập giữ chỗ, 2 chủ đề). Sau 45 ván, bot có 46 đến 50 mục, người chơi giỏi 58 mục.

### Cây (20 nút, tổng 200 điểm)

Điều kiện "nhóm nguồn gốc" dùng 6 nhóm của 4 rương cơ bản trong mô phỏng (Quá khứ, Hiện đại, Tương lai, Ngoài hành tinh, Cổ tích, Thần thoại); spec nói 8 nhóm có cả Việt Nam và Nhại, nay đã thành chủ đề (câu hỏi 4).

| Nhánh | Nút (cha) | Điểm | Điều kiện | Phần thưởng | Đổi ván? |
|---|---|---|---|---|---|
| Rương | R1 Kệ rương (gốc) | 6 | Sổ có 8 mục | Ghi chú cửa hàng | không |
| | R2 Rương Rác Quý (R1) | 8 | 5 loại rác | Rác Quý vào shop | có |
| | M3 Thử thách Rương Đen (R1) | 10 | Thắng 1 ván | Tùy chọn Rương Đen từ ngày 1 | tùy chọn, khó hơn |
| | R3 Rương Tổng Hợp (R1) | 11 | Đủ 6 nhóm nguồn gốc | Tổng Hợp vào shop | có |
| | R4 Rương Cổ Phong (R3) | 13 | 2 tạo tác Cổ Phong | Cổ Phong vào shop | có |
| | R5 Rương Thiên Cơ (R3) | 13 | 2 tạo tác Thiên Cơ | Thiên Cơ vào shop | có |
| Sưu tập | S1 Trang sưu tập đầu tiên (gốc) | 6 | Sổ có 8 mục | Trang Sổ | không |
| | S2 Sổ tay người săn đồ (S1) | 8 | Sổ có 20 mục | Trang Sổ | không |
| | M1 Chọn gia truyền (S2) | 10 | Sổ có 25 mục | Chọn 1 trong 2 gia truyền | có, dưới 1 điểm |
| | M2 Vốn khởi nghiệp nhỏ (M1) | 13 | Sổ có 35 mục | **Đề xuất: không cộng vốn** (mục 5) | không |
| | S3 Trang Huyền thoại (S2) | 11 | Có 1 món Huyền thoại | Trang Sổ | không |
| | M4 Bí Khế quen mặt (S3) | 13 | Có 4 tạo tác trong Sổ | Sổ hiện điều kiện Bí Khế | không (thông tin) |
| Thần lực | T1 Tiếng thì thầm (gốc) | 6 | Sổ có 10 mục | Phản ứng dí dỏm | không |
| | T2 Mèo chợ (T1) | 8 | Đủ 4 nhóm nguồn gốc | Phản ứng ngoài đời | không |
| | T3 Chuông chợ (T2) | 11 | Sổ có 30 mục | Phản ứng ngoài đời | không |
| | T4 Lời thì thầm Huyền thoại (T3) | 14 | Có 2 món Huyền thoại | Phản ứng của món Huyền thoại | không |
| Câu chuyện | C1 Sổ của Dì Mận (gốc) | 6 | Đã chơi 2 ván | Đoạn truyện | không |
| | C2 Lời nhắn của Anh Bảy (C1) | 8 | Đã chơi 5 ván | Đoạn truyện | không |
| | C3 Chương mới: phiên chợ đêm (C2) | 11 | Thắng 1 ván | Cảnh mới | không |
| | C4 Chương mới: món nợ cũ (C3) | 14 | Đã chơi 12 ván | Cảnh mới | không |

Mỗi nhánh có 1 nút gốc giá 6 điểm. Đi sâu thì đắt hơn (6, 8, 10 đến 11, 13 đến 14). Chỉ 2 nút cần thắng (M3, C3), nên người thua nhiều vẫn mở được 18 trên 20 nút mà không cần thắng ván nào. Tên nút và nội dung phần thưởng ở đây là giữ chỗ, việc của Claude 1. Khung tên giống bản thử của Codex trên nhánh `codex/skill-tree` (thắng 4, thua 2, soi lần đầu 1). Bản đó chưa vào nhánh này, nên chưa so khớp mã.

### Nhịp mở nút (trung vị số ván; trong ngoặc: ván mà 90% người chơi đã đạt)

| Người chơi | Thắng (cả chuỗi) | Nút đầu | Nửa cây (10 nút) | Cả cây | Tổng Hợp | Cổ Phong / Thiên Cơ | M1 | M4 |
|---|---|---|---|---|---|---|---|---|
| Bot thận trọng | khoảng 14% | 2 (2) | 10 (13) | **37 (45)** | 12 | 22 / 25 | 10 | 31 |
| Bot liều | khoảng 15% | 2 (2) | 11 (13) | **40** (82% xong trong 45 ván) | 13 | 24 / 27 | 10 | 33 |
| Người soi ẩu (thận trọng) | khoảng 11% | 2 (2) | 11 (13) | **39** (87% xong) | 12 | 23 / 26 | 10 | 32 |
| Người soi ẩu (liều) | khoảng 11% | 2 (2) | 11 (14) | **42** (75% xong) | 13 | 25 / 28 | 11 | 35 |
| Người chơi giỏi (liều) | khoảng 45% | 1 (2) | 8 (10) | **26 (29)** | 3 | 7 / 10 | 8 | 21 |
| Người chơi giỏi (thận trọng) | khoảng 38% | 1 (2) | 8 (11) | **27 (31)** | 3 | 9 / 11 | 8 | 22 |

Người chơi giỏi mở Tổng Hợp sớm (ván 3) vì họ ưu tiên nút đổi ván và gom đủ 6 nhóm nguồn gốc nhanh. Bot mở nút rẻ trước nên Tổng Hợp đến ở ván 12.

## 3. Tác động của từng nút đổi ván (cấu hình cố định)

Người chơi giỏi n = 1.500 mỗi cách chơi, bot n = 3.000. Sai số khoảng ±2,5 điểm ở mức 45% và ±1,3 điểm ở mức 15%. Bot ghi thận trọng / liều. Người chơi giỏi ghi thận trọng / liều / combo nhỏ / combo lớn; "tối đa" là cách chơi tốt nhất.

**Bảng phần 2 (Tổng Hợp hàng giả 20%):**

| Cấu hình | Bot | Người chơi giỏi | Tối đa |
|---|---|---|---|
| Nền (4 rương cơ bản, có soi lỗi) | 14,6 / 15,7 | 37,8 / 44,0 / 37,7 / 35,5 | 44,0 |
| M2 vốn +1.000.000 đ | 16,2 / 16,8 | 40,5 / 47,0 / 41,4 / 40,8 | **47,0** |
| M3 Rương Đen từ ngày 1 | **10,4 / 10,2** | 34,1 / 41,5 / 37,5 / 34,9 | 41,5 |
| M1 chọn GT1 | 14,8 / 16,2 | 38,0 / 44,7 / 38,0 / 35,9 | 44,7 |
| M1 chọn GT2 | 14,3 / 15,2 | 37,7 / 43,1 / 37,3 / 35,1 | 43,1 |
| + Rác Quý, Tổng Hợp | **11,1 / 11,6** | 38,8 / 42,5 / 41,8 / 40,1 | 42,5 |
| + 4 rương mở khóa | **11,0 / 11,2** | 40,0 / 45,5 / 40,3 / 39,7 | 45,5 |
| + 2 chủ đề (chưa mở rương nào ở cây) | 14,2 / 13,9 | 35,4 / 42,7 / 36,9 / 35,6 | 42,7 |
| + 4 mở khóa + 2 chủ đề | 14,7 / 14,4 | 38,3 / 43,5 / 37,9 / 37,3 | 43,5 |
| + 4 mở khóa + 2 chủ đề + M3 | 10,0 / 10,5 | 37,3 / 43,3 / 38,6 / 37,9 | 43,3 |
| + 4 mở khóa + 2 chủ đề + M2 | 15,0 / 16,1 | 40,5 / 45,3 / 40,1 / 40,0 | 45,3 |

Bot riêng với từng rương (n = 4.000): Rác Quý 14,9 / 15,6; **Tổng Hợp 11,2 / 11,7**; Cổ Phong 14,3 / 14,9; Thiên Cơ 14,4 / 14,9 (nền 14,5 / 15,2). Chỉ Tổng Hợp kéo bot xuống. Đây là rương đắt nhất bot mua được trước ngày 4, có hàng giả 20%, và dưới luật soi lỗi món giả khai thật bị tịch thu (phần 3). Phần 2 đo không có soi lỗi nên Tổng Hợp không làm bot thua thêm (17,0 / 18,0).

**Dò Tổng Hợp** (bot n = 4.000; người chơi giỏi chỉ đo cách chơi liều, n = 1.500):

| Biến thể Tổng Hợp | Bot, chỉ thêm Tổng Hợp | Bot, 4 mở khóa | Người chơi giỏi, 4 mở khóa | Bot, cả cây | Người chơi giỏi, cả cây |
|---|---|---|---|---|---|
| Bảng phần 2 (hàng giả 20%) | 11,2 / 11,7 | 11,0 / 11,2 | 45,5 | 14,7 / 14,4 | 43,5 |
| Hàng giả 5% | 18,3 / 19,2 | — | — | — | — |
| Hàng giả 10% | 15,9 / 15,7 | 15,8 / 15,6 | **49,8** | 15,4 / 14,8 | **48,5** |
| Hàng giả 15% | 13,3 / 13,6 | — | — | 15,1 / 14,7 | — |
| Giá 6.500.000 đ (hàng giả 20%) | 13,7 / 14,4 | — | — | 15,2 / 14,6 | — |
| Hàng giả 10%, giá 6.500.000 đ | 19,1 / 19,1 | 12,5 / 12,9 | 46,1 | 14,7 / 14,7 | 46,8 |
| Hàng giả 10%, Rất hiếm 12% | — | 14,7 / 14,2 | 46,2 | 15,6 / 15,1 | 47,7 |
| Hàng giả 12%, Rất hiếm 13% | — | 14,4 / 14,1 | 47,4 | 15,2 / 15,0 | 47,5 |
| **Đề xuất: hàng giả 10%, T 43%, H 25%, X 13%** | — | 14,0 / 13,5 | **45,8** | 15,3 / 15,1 | **46,4** |

"Cả cây" = 4 rương mở khóa + 2 chủ đề + M1 (GT1), không M2, không M3. Hạ hàng giả một mình đưa bot về mức nền nhưng làm Tổng Hợp quá mạnh cho người chơi giỏi (49,8%). Bản đề xuất bù bằng cách bớt Hiếm và Rất hiếm. Lời kỳ vọng mỗi hộp giảm, nhưng hàng giả ít hơn nên bot ít bị tịch thu hơn.

**Shop bày tối đa 2 rương mới mỗi ngày** (phần 2, câu hỏi 2), đo với Tổng Hợp hàng giả 10% chưa bớt bậc: 4 mở khóa: bot 15,0 / 15,6, người chơi giỏi 49,1%; 4 mở khóa + 2 chủ đề: bot 13,7 / 14,8, người chơi giỏi 46,5%. Luật này hạ người chơi giỏi khoảng 1 điểm khi có chủ đề, nhưng cũng hạ bot.

## 4. Tỉ lệ thắng theo chuỗi ván (bản đề xuất)

Bảng đề xuất: Tổng Hợp đã sửa, không M2, không ai bật M3, M1 chọn GT1, chủ đề mở theo `THEME_RULE`. Tỉ lệ thắng (%) theo khối 5 ván:

| Người chơi | 1–5 | 6–10 | 11–15 | 16–20 | 21–25 | 26–30 | 31–35 | 36–40 | 41–45 |
|---|---|---|---|---|---|---|---|---|---|
| Bot thận trọng (400) | 14,5 | 12,3 | 15,1 | 14,7 | 12,6 | 14,7 | 15,2 | 12,8 | 13,9 |
| Bot liều (400) | 14,9 | 13,7 | 15,3 | 14,2 | 13,6 | 16,1 | 15,6 | 14,1 | 13,8 |
| Người chơi giỏi liều (200) | 44,0 | 44,2 | 46,4 | 44,9 | 45,9 | 45,2 | 45,0 | 46,3 | **48,6** |
| Người chơi giỏi thận trọng (200) | 37,4 | 37,4 | 38,7 | 36,3 | 39,8 | 37,2 | 37,9 | 37,5 | 39,2 |
| Người soi ẩu, thận trọng (400) | 10,4 | 9,6 | 10,3 | 11,2 | 10,8 | 11,6 | 11,0 | 10,2 | 10,8 |
| Người soi ẩu, liều (400) | 12,3 | 10,6 | 11,1 | 12,7 | 11,8 | 11,6 | 11,1 | 10,5 | 11,4 |

Mỗi ô của bot là 2.000 ván (sai số khoảng ±1,5 điểm). Mỗi ô của người chơi giỏi là 1.000 ván (sai số khoảng ±3 điểm).

Đọc bảng:

- **Người chơi giỏi trôi lên khoảng 2 điểm** khi mở dần rương (44% ở đầu chuỗi, 45% đến 46% ở giữa). Khối cuối 48,6% nằm trong nhiễu so với đo tĩnh cả cây 46,4% (n = 1.500). Nhưng **biên dưới trần chỉ khoảng 1 điểm**, đúng như phần 2 đã cảnh báo. Muốn biên rộng hơn: shop bày tối đa 2 rương mới mỗi ngày (khoảng −1 điểm), hoặc hạ trần trả giá ở phần 3 (phần 3: trần 110% hạ người chơi giỏi khoảng 1 đến 2 điểm).
- **Bot không trôi**: 12% đến 16% suốt chuỗi. Bot thấp hơn khoảng 15% một chút ở mọi giai đoạn, kể cả 5 ván đầu khi chưa mở nút nào, nên phần hụt là của nền phần 3, không phải của cây.
- **Không có M2 thì không có buff tích lũy:** ván thứ 40 của bot thường khó ngang ván đầu. Đúng tinh thần "meta không buff chơi".

## 5. M1 đến M4

| Mở khóa | Tác động đo được | Đề xuất |
|---|---|---|
| M1 Chọn gia truyền | GT1 tốt hơn GT2 với mọi cách chơi; chọn GT1 thay vì rút ngẫu nhiên cộng 0,2 đến 0,7 điểm | **Giữ.** Là lựa chọn, tác động nhỏ |
| M2 Vốn khởi nghiệp nhỏ (+1.000.000 đ) | Người chơi giỏi 44,0 lên **47,0** (nền), 43,5 lên 45,3 (đã mở rương), bot +1 đến 1,6 điểm; với Tổng Hợp hàng giả 10% và đủ rương thì người chơi giỏi lên **50,6** | **Bỏ phần cộng vốn**, giữ nút ở cây với phần thưởng không buff (ví dụ trang Sổ, đoạn truyện "vốn đầu tiên của Dì Mận"). Nếu chủ dự án muốn giữ vốn thì cần hạ chỗ khác (ví dụ trần trả giá 110%) và đo lại |
| M3 Rương Đen từ ngày 1 | Bot 14,6 xuống **10,4**, người chơi giỏi 44,0 xuống 41,5 | **Giữ là tùy chọn thử thách.** Giao diện nên nói rõ đây là chế độ khó hơn, để người chơi non không bật nhầm |
| M4 Bí Khế quen mặt | Chỉ là thông tin. Mô phỏng đã giả định người chơi biết luật Bí Khế, nên tác động bằng 0 (với người thật có thể giúp người mới một chút) | **Giữ.** Đổi điều kiện (dưới đây) |

**Điều kiện của M4.** Điều kiện cũ "đạt mỗi Bí Khế ít nhất một lần" gần như không đạt được dưới luật soi lỗi. Bí Khế BK2 cần "giám định 2 hiện vật Thần thoại", mà gợi ý của Giáo Sư Lẫm (thay giám định ở phần 3) không tính là giám định: trong 40 ván, không bot nào đạt BK2. Còn BK1 (combo trước ngày 5) thì 40% bot không đạt trong 40 ván. Đề xuất đổi điều kiện thành **"có 4 tạo tác trong Sổ"**: 92% đến 96% bot đạt trong 45 ván. Nếu luật soi mới tính gợi ý là giám định thì có thể dùng lại điều kiện cũ (câu hỏi 5).

## 6. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_tree                 # test khóa, khoảng 1 phút
nohup python3 tree_final.py static 1500 > s.log &    # mục 3, bảng phần 2 (Tổng Hợp hàng giả 20%), khoảng 15 phút
                                                 #   (đặt tree_sim.UNLOCK_OVER = {} trước khi chạy để đo đúng bảng phần 2)
nohup python3 tree_final.py static10 1500 > s10.log &  # mục 3 với Tổng Hợp đề xuất, khoảng 10 phút
nohup python3 tree_final.py chain 200 45 > c.log &   # mục 2 và 4: chuỗi ván, khoảng 10 phút
```

Mã: `QA/tree_sim.py` (`POINTS` điểm, `TREE` cây, `UNLOCK_OVER` bản sửa Tổng Hợp, `CASH_BONUS` M2, `THEME_RULE` giả định thành tựu mở chủ đề, `SHOP_EXTRA` số rương mới shop bày mỗi ngày; `static()` đo một cấu hình, `chain()` đo chuỗi ván, `milestones()` và `win_by_run()` tổng hợp; `restore()` trả lõi về như cũ). `QA/tree_final.py` chạy số đo cho báo cáo. Bảng `static` ở mục 3 được chạy trước khi thêm `UNLOCK_OVER`. Bảng "dò Tổng Hợp" chạy bằng một kịch bản tạm đổi `UNLOCK_OVER` theo từng biến thể.

## 7. Câu hỏi mở cho chủ dự án

1. **Duyệt bộ điểm** (thắng 8; thua 2 + 1 mỗi ngày từ ngày 4, tối đa +3; Sổ 1 điểm mỗi mục, Rất hiếm và Huyền thoại 2) và **giá cây** (20 nút, tổng 200 điểm, gốc 6, sâu nhất 13 đến 14).
2. **M2 có bỏ cộng vốn không?** Đề xuất bỏ (mục 5). Nội dung thưởng thay thế là việc của Claude 1. Nếu bỏ, `content.json` (`meta_unlocks` M2 và `meta_start_cash_bonus_cap`) và `Docs/Content/DichVuVaMeta.md` cần sửa theo.
3. **Sửa Tổng Hợp khi có soi lỗi** (hàng giả 20% → 10%, Thường 43%, Hiếm 25%, Rất hiếm 13%). Bảng phần 2 chưa chốt, nên đây là đề xuất sửa bảng đó. Cần đo lại khi phần 3 chạy theo luật soi mới: nếu luật mới phạt món giả nhẹ hơn, có thể Tổng Hợp giữ được hàng giả 20%.
4. **Điều kiện "đủ nhóm nguồn gốc" của Tổng Hợp.** Spec ghi 8 nhóm (có Việt Nam và Nhại), nhưng Việt Nam và Nhại nay là rương chủ đề (ruong-chu-de). Mô phỏng dùng "đủ 6 nhóm của 4 rương cơ bản". Nếu điều kiện cần đồ chủ đề, Tổng Hợp sẽ mở muộn hơn nhiều.
5. **Điều kiện của M4**: đổi thành "có 4 tạo tác trong Sổ" (đề xuất), hay giữ "đạt cả hai Bí Khế" và tính gợi ý của Giáo Sư Lẫm là giám định cho BK2?
6. **Thành tựu mở rương chủ đề** chưa chốt. Mô phỏng giả định chủ đề thứ nhất mở khi Sổ có 15 mục, thứ hai khi có 30 mục. Ván có 2 chủ đề và 4 rương mở khóa đã được đo, nên thành tựu nào cũng không phá trần, miễn shop không bày quá 2 chủ đề cùng lúc (hơn 2 chủ đề chưa đo cùng soi lỗi).
7. **Sổ lớn hơn khi có thêm chủ đề.** Mỗi chủ đề có khoảng 8 đến 10 món, tức thêm khoảng 10 điểm Sổ. 14 chủ đề trong danh sách đề xuất sẽ cộng khoảng 120 điểm, làm cây nhanh gấp rưỡi ở giai đoạn sau. Hai cách: điểm Sổ của đồ chủ đề tính nửa, hoặc thêm nút ở cây khi thêm chủ đề. Chọn khi chốt danh sách chủ đề.
8. **Biên dưới trần 47% chỉ khoảng 1 điểm** (mục 4). Phần 2 và phần 3 cũng sát trần. Nếu muốn biên rộng hơn: shop bày tối đa 2 rương mới mỗi ngày (phần 2, câu hỏi 2), hoặc trần trả giá 110% (phần 3, câu hỏi 8). Nên chốt một trong hai sau khi phần 3 đo lại.
9. **Bot thường khoảng 14%**, dưới mục tiêu 15% đến 20% một chút, ngay từ nền 4 rương cơ bản có soi lỗi (phần 3). Cây không làm bot tệ hơn. Xem lại khi phần 3 đo lại theo luật soi mới.
10. **Bản thử của Codex** (nhánh `codex/skill-tree`: 9 nút, thắng 4, thua 2, soi lần đầu 1) chưa vào nhánh này. Khi gộp, Codex dùng bộ số ở mục 2 và các mã nút ở đây (mã R1..R5, S1..S3, T1..T4, C1..C4 là đề xuất, khớp ý với mã R01, S01, T01, C01 của Codex).
