# Bàn giao Sprint 5 cho phiên Claude 2 kế tiếp

Viết ngày 2026-11-24 khi kết thúc phiên đổi tiền tệ. Theo quy tắc trong [README](../../README.md): mỗi phiên làm một việc, nên Sprint 5 bắt đầu ở **phiên mới**. File này chứa mọi thứ cần nhớ.

## Việc đã làm trong phiên này

- **Đổi tiền sang đồng Việt Nam, 1 xu cũ = 10.000 đ** (chủ dự án chọn). Spec: [tien-te](../../docs/superpowers/specs/2026-11-24-tien-te-design.md), đã thêm vào [DaChot.md](../DaChot.md). Mô phỏng và test trong `QA/` giữ đơn vị cân bằng 10.000 đ; `QA/currency.py` (5 test) đổi sang đồng khi in. Tài liệu `Docs/Balance/*.md` có ghi chú quy đổi ở đầu và bảng quy đổi trong `Docs/Balance/README.md`.
- **Chưa đổi** (chủ của file khác): nhãn "xu" trong `Docs/Content`, GDD, `content.json` (Claude 1), định dạng tiền trong lõi gameplay (Codex), hiển thị (Antigravity).
- Gộp `main` (64 commit) và nhánh của Claude 1 vào nhánh `claude/eloquent-bell-5baoyc`. 175 test đạt.

## Phương án C đã đo xong (phiên cuối)

Kết quả đầy đủ ở [GiaHopDaoDong.md](GiaHopDaoDong.md), test khóa `QA/test_box_price_swing.py`. Kết luận: giữ **±20% và Rương Đen 1.500**; hạ Rương Đen xuống 1.250 làm bot thường thắng 24% đến 30% (quá dễ). Dịch vụ và biến cố không phá cân bằng. **Còn dở nhỏ:** dịch vụ chưa đo trên người chơi chọn loại hộp (`measure_turns` cần tham số `opt`), xem mục 5 của GiaHopDaoDong.md.

## Sprint 5 (cân bằng hệ mới): ghi chú phân tích ban đầu

Đọc 8 spec ở `docs/superpowers/specs/`. Việc của Claude 2 (TASKS.md): giá và tỉ lệ ra bậc của 4 rương cơ bản và rương mở khóa, tỉ lệ rác, 4 ô tạo tác (BK1 mở ô thứ 5), mức phạt và lỗ khi soi sai thật giả, giá ép khi soi thiếu, điểm phát triển của cây. **Nghiệm thu:** mô phỏng cho thấy **ngày 1 không thua ngay** và **đường thắng vẫn tồn tại**.

Phát hiện sớm, tính bằng số liệu hiện có (chưa cần mô phỏng):

1. **Rương Kim Khí giá 500 xu cũ (5.000.000 đ) là bẫy.** Rương ra tạo tác tier 1 (60%), tier 2 (35%), tier 3 (5%). Giá mua cùng tạo tác ở Kho Tạo Tác (`content.json`): tier 1 trung bình 135, tier 2 trung bình 425, tier 3 500. **Giá trị kỳ vọng của tạo tác từ Kim Khí chỉ 255 xu cũ**, bằng một nửa giá rương, nên mua tạo tác ở Kho luôn rẻ hơn. Chỉ hợp lý nếu rương có giá khoảng 250, hoặc tạo tác trùng đổi được tiền, hoặc Kim Khí còn ra thêm thứ gì khác.
2. **4 ô trang bị làm combo thành mặc định.** Với 6 tạo tác: 3 ô có 12 trên 20 bộ có ít nhất một combo (60%) và 4 trên 20 có CB3 (20%). **4 ô: 15 trên 15 bộ đều có combo (100%) và 6 trên 15 có CB3 (40%).** 5 ô (sau BK1): 6 trên 6 có combo, 4 trên 6 có CB3. Người chơi mang 4 trong 6 tạo tác, nên "chọn hệ hoặc pha hai hệ" gần như không còn là quyết định. Đề xuất: tăng số tạo tác lên khoảng 10 đến 12, hoặc giữ 3 ô. Cần Claude 1 và chủ dự án quyết.
3. **Xung đột giữa spec rương:** `tier-do-trong-ruong` ghi 4 rương cơ bản có Huyền thoại 0%, còn `ruong-chu-de` (mới hơn, mục 10 và 11) nói rương thường có Huyền thoại cơ bản (vé số, bản đồ cổ, đồng hồ bỏ túi, két sắt). Tỉ lệ Huyền thoại ở rương cơ bản chưa có. Khi mô phỏng cần đặt giả định (đề xuất 0,5% đến 2%).
4. **Giá nền món mới chưa có** (25 món mới, 38 món tổng). Tier chỉ có khoảng: Thường 200 đến 900, Hiếm 1.500 đến 1.900, Rất hiếm 2.700 trở lên, Huyền thoại cơ bản "rẻ hơn" huyền thoại chủ đề (chưa chốt). Cần chốt giá hoặc để Claude 2 hiệu chỉnh theo mục tiêu lời mỗi rương.
5. **Quay hai bước (bậc rồi món)** làm giá trị kỳ vọng của rương bằng `Σ tỉ lệ bậc x giá trung bình của bậc trong bộ sưu tập rương đó`; số món mỗi bậc mỗi rương khác nhau (Gỗ Thường 3 món, Rất hiếm 1 món...) nên phương sai theo rương cần tính.
6. **Ngày 1 không thua ngay:** `mo-man` ghi hộp đầu sắp sẵn (rác hài), hộp hai ra đồ thường bán được; vốn 10.000.000 đ, nợ ngày 1 chỉ 2.000.000 đ. Cần kiểm tra bằng mô phỏng rằng ngay cả khi hộp ngày 1 ra rác, người chơi không phải thua ngay (luật nợ tồn và ngưỡng thua 10.000.000 đ).

## Cách bắt đầu phiên mới

1. Đọc [DaChot.md](../DaChot.md) và file này.
2. Bật Ponytail mức `full`, dùng Superpowers (brainstorming trước khi xây) theo README.
3. Làm **cân bằng hệ mới Sprint 5** (lớn; nên chia phiên: một phiên cho giá và tỉ lệ rương, một phiên cho soi lỗi và trả giá, một phiên cho cây phát triển). Việc nhỏ có thể làm kèm: thêm `opt` cho `measure_turns` và đo dịch vụ trên người chơi chọn loại.
4. Mô phỏng nền: `QA/clue_sim.py`, `QA/proposal_sim.py`, `QA/swing_sim.py`, `QA/tier_sim.py`; hằng số kinh tế ở `QA/economy_model.py`. Sprint 5 cần mô hình hóa 5 bậc, hai bước quay, rác, soi lỗi và trả giá.
