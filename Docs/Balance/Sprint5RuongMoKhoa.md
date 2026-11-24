# Sprint 5 phần 2: rương mở khóa và khung rương chủ đề (Claude 2)

Viết ngày 2026-11-24. Phần 2 của việc cân bằng Sprint 5: giá và tỉ lệ ra bậc của 4 rương mở khóa (Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý) và **một khung chung** cho mọi rương chủ đề (Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot...), gồm tỉ lệ và giá nền Huyền thoại chủ đề. **Mọi bảng dưới đây là ĐỀ XUẤT, chưa chốt**; điều cần chủ dự án quyết ghi ở mục 7. Cách đo và giả định: [spec cách đo](../../docs/superpowers/specs/2026-11-24-can-bang-ruong-mo-khoa-design.md). Phần 1 (4 rương cơ bản, đã chốt): [Sprint5Ruong.md](Sprint5Ruong.md).

> **Tiền tệ:** mọi số tiền ghi bằng **đồng Việt Nam**. Mô phỏng tính theo đơn vị 10.000 đ (`price=600` là 6.000.000 đ), in qua `QA/currency.py`. Mọi giá đề xuất là bội số của 10.000 đ.

## Kết luận ngắn

@@KETLUAN@@

## 1. Tiêu chí và cách đo

Rương mở khóa và rương chủ đề có từ ngày hôm sau khi mở khóa và **không buff chơi** (ruong-suu-tap, ruong-chu-de), nên tiêu chí là **thêm lựa chọn và đánh đổi, không làm game dễ hơn rõ rệt**:

- bot thường (mua rương đắt nhất mua được) vẫn khoảng 15% đến 20%;
- người chơi tính toán hoàn hảo (chọn rương theo lời kỳ vọng, đọc manh mối, giám định; λ = 0,75) không quá 47%;
- không rương nào quá 50% lượt mua của người chơi giỏi;
- mỗi rương mới có lý do để mua (được chọn một phần đáng kể, hoặc có vai riêng).

Nền là **4 rương cơ bản đã chốt** (`chest_sim.FINAL`, có cả lần chỉnh Rương Đen rác 30%, Rất hiếm 58%), giữ nguyên. `QA/chest_unlock.py` thêm rương mới vào cùng lõi mô phỏng đã duyệt (nợ, phạt 20%, giá dao động ±20% theo từng loại rương, manh mối, giám định, combo, tạo tác trùng đổi 50% giá Kho). Chỉ đổi một chỗ trong lõi: bot và người chơi giỏi xem **mọi rương đang có trong shop**, xếp theo giá gốc, thay cho 4 mã cố định.

**Nhiễu đo cần biết.** Thêm một rương vào shop làm mỗi lượt rút thêm một hộp ứng viên, nên chuỗi ngẫu nhiên của cả ván bị xáo. Hai rương đối chứng **không ai mua** (một rương chỉ ra rác, một rương giá không ai trả nổi) vẫn làm người chơi giỏi (liều) đổi từ 43,4% lên **45,8% và 46,3%** dù không thể đổi kết quả thật. Vì vậy chênh lệch dưới khoảng 3 điểm giữa hai kịch bản là nhiễu (sai số mỗi số đo khoảng ±1,3 đến ±1,6 điểm ở n = 1.000 đến 1.500). Báo cáo so với cả nền lẫn đối chứng, và đề xuất chọn sao cho kịch bản tổ hợp nằm dưới trần 47% có biên.

## 2. Bảng đề xuất

@@BANG@@

## 3. Số đo theo kịch bản shop

@@SODO@@

## 4. Rương Cổ Phong và Thiên Cơ: vì sao khó nhất

@@HE@@

## 5. Khung rương chủ đề và Huyền thoại chủ đề

@@CHUDE@@

## 6. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_chest_unlock          # test khóa, khoảng 2 đến 3 phút
nohup python3 chest_unlock_final.py final > final.log &   # nền và từng kịch bản shop, n = 1.500 (khoảng 35 phút)
nohup python3 chest_unlock_final.py sens > sens.log &     # đối chứng, shop xoay vòng, mở khóa giữa ván, độ nhạy (khoảng 25 phút)
nohup python3 chest_unlock_final.py he > he.log &         # dò bảng Cổ Phong / Thiên Cơ (khoảng 10 phút)
```

Mã: `QA/chest_unlock.py` (`UNLOCK`, `THEME`, `THEME_LEG` là bảng đề xuất; `scenario()` dựng kịch bản shop; `shop_extra` cho shop xoay vòng; `from_day` cho mở khóa giữa ván; `hunt_strategy` là người chơi "săn hệ"; `restore()` trả lõi về như cũ).

## 7. Câu hỏi mở cho chủ dự án

@@CAUHOI@@
