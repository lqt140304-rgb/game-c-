# Thiết kế: rương chủ đề (bổ sung cho phần tier)

Trạng thái: **nháp theo lời chủ dự án, chờ duyệt danh sách chủ đề**. Chưa có gì được xây. Bản này **thay đổi** cách mở khóa Rương Việt Nam và Rương Nhại ở [tier-do-trong-ruong](2026-11-23-tier-do-trong-ruong-design.md) (mục "Rương riêng mở khóa bằng cây phát triển").

## Ý định

Mỗi chủ đề là một **hệ sinh thái riêng**: một rương, một nhóm đồ, một món Huyền thoại của riêng chủ đề đó. Người chơi săn thành tựu để mở rương, rồi theo đuổi món Huyền thoại ở cuối chủ đề.

## Đã chốt với chủ dự án

1. **Chỉ cần thành tựu để mở khóa**, không tốn điểm phát triển.
2. **Sau khi mở khóa, rương xuất hiện trong shop rương vào ngày hôm sau.**
3. **Rương Nhại tách ra, không gộp chung.** Nhóm Nhại cũ chia thành các chủ đề rõ ràng.
4. **Mỗi chủ đề có món Huyền thoại riêng**, chỉ ra từ rương chủ đề đó.
5. **Mọi loại rương đều có bộ sưu tập riêng** (cả 4 rương cơ bản, rương mở khóa và rương chủ đề).
6. **Cơ chế mở rương: chọn bậc trước, rồi mới quay ra món** (xem mục dưới).
7. Các chủ đề chủ dự án nêu: **Việt Nam, Wibu, truyền thuyết Châu Âu, mô hình, robot**. **Việt Nam vẫn là một chủ đề.** Danh sách sẽ còn thêm, chủ dự án chưa chọn.
8. **Tiêu chí món Huyền thoại:** là **biểu tượng (icon)** của chủ đề, **có câu chuyện** (thần thoại, truyền thuyết, sự tích), và **người trong nền văn hóa đó ai cũng biết**, đủ tầm ảnh hưởng và phẩm giá để đứng ở bậc cao nhất. Ví dụ mẫu: **Nỏ thần** của Việt Nam (truyện An Dương Vương, người Việt nào cũng biết).
9. **Rương Tổng Hợp** ra ngẫu nhiên các món trong **bộ sưu tập của các rương thường** (4 rương cơ bản), **không** ra món của rương chủ đề. Điều này **thay** ghi chú cũ "Rương Tổng Hợp là rương cho Huyền thoại" ở spec tier.
10. **Rương thường cũng có món Huyền thoại**, nhưng **cơ bản hơn, giá thấp hơn** Huyền thoại của rương chủ đề và rương đặc biệt. Rương thường để **sinh tồn**: mở để kiếm tiền. Đồ ở đây tự phát, không cần nhiều câu chuyện; người chơi tự dệt câu chuyện cho nó. Tiêu chí "biểu tượng, có truyền thuyết" ở mục 8 chỉ áp dụng cho Huyền thoại của rương chủ đề.
11. **Vé số độc đắc giữ ở Huyền thoại** (Huyền thoại của rương thường). Sau này có thể thêm cơ chế mang vé đến **quầy vé số để rút thưởng** (chưa thiết kế).
12. **Bộ sưu tập khóa chặt theo rương:** mỗi món chỉ thuộc **một** rương. Món của chủ đề chỉ ra ở rương chủ đề đó; món của từng rương cơ bản chỉ ra ở rương đó. **Ngoại lệ duy nhất:** Rương Tổng Hợp ra được món của các rương cơ bản (mục 9).

## Cơ chế mở rương (đã chốt)

1. **Bước 1, quay bậc:** mỗi rương có bảng tỉ lệ bậc riêng (Rác, Thường, Hiếm, Rất hiếm, Huyền thoại). Quay ra một bậc.
2. **Bước 2, quay món:** trong bộ sưu tập của **chính rương đó**, lấy các món thuộc bậc vừa quay, quay ra một món.
3. Mỗi món chỉ thuộc bộ sưu tập của một rương (mục 12), trừ Rương Tổng Hợp.
4. Món Huyền thoại riêng của chủ đề chỉ nằm trong bộ sưu tập của rương chủ đề đó.
5. Cả hai bước dùng hạt giống ván, để lưu và tải lại cho kết quả không đổi.

Tỉ lệ bậc của từng rương và trọng số từng món: Claude 2.

## Khung chung của một rương chủ đề

| Mục | Nội dung |
|---|---|
| Ra gì | Ưu tiên đồ của chủ đề, có thể lẫn rác. Món Huyền thoại riêng chỉ ra ở rương này |
| Mở khóa | Một **thành tựu** gắn với chủ đề (đề xuất, chưa chốt từng cái) |
| Xuất hiện | Mở khóa hôm nay thì có trong shop rương **từ ngày mai** |
| Giá, tỉ lệ ra | Claude 2 cân bằng |
| Tác dụng khác | Không buff chơi, giữ nguyên nguyên tắc ở phần 1 |

## Chủ đề dự kiến (chờ duyệt)

| Chủ đề | Rương | Huyền thoại riêng | Thành tựu mở | Đồ có sẵn có thể thuộc về |
|---|---|---|---|---|
| Wibu | Rương Wibu | chưa chọn | chưa chọn | kiếm sáng (X03), quả cầu ngọc (X04), mũ phù thủy (H05)? |
| Truyền thuyết Châu Âu | Rương Truyền thuyết Châu Âu | chưa chọn | chưa chọn | giày thủy tinh (L03), quả táo vàng (L02)? |
| Mô hình | Rương Mô hình | chưa chọn | chưa chọn | chưa có |
| Robot | Rương Robot | chưa chọn | chưa chọn | trái tim robot (X05), phi thuyền mini (X06)? |
| Việt Nam | Rương Việt Nam | chưa chọn | chưa chọn | nỏ thần (L05), gậy sắt (L06), ấn ngọc tỷ (L01), vé số (L04)? |

Dấu "?" là gợi ý ghép đồ đã có mô hình, **chưa chốt**. Món Huyền thoại riêng của từng chủ đề có thể là món sẵn có hoặc món làm mới; mô hình mới sẽ gom vào một lượt Meshy chung để đỡ tốn credit.

## Bảng chia 38 món vào rương (đề xuất, chờ duyệt)

Mỗi món một rương. "—" là ô **còn thiếu**, sẽ bổ sung cùng chủ dự án. Kim Khí ngoài bảng này còn ra **tạo tác**.

**Rương cơ bản**

| Rương | Rác | Thường | Hiếm | Rất hiếm | Huyền thoại (cơ bản) |
|---|---|---|---|---|---|
| Gỗ (an toàn, đồ đời thường) | Giày cũ R01, Tất thối R02, Trứng thối R03, Xương cá R04 | Bình sứ V01, Đồng hồ quả lắc V03, Lá bùa V09 | — | — | Vé số độc đắc L04 |
| Vải Niêm (đồ xưa, gom bộ) | — | Sách cổ V02, Mảnh kính thiên văn V04 | — | Mảnh vảy long V10 | — |
| Kim Khí (máy móc, tạo tác) | Lon bẹp R06 | Thiết bị liên lạc V05 | Pin lượng tử V06 | Tinh thể lam V07 | — |
| Đen (cờ bạc, cá độ) | Áo đấu cũ R07, Vé cược nhàu R09 | — | — | — | — |

**Rương chủ đề**

| Rương | Rác | Thường | Hiếm | Rất hiếm | Huyền thoại (biểu tượng) |
|---|---|---|---|---|---|
| Việt Nam | Dép tổ ong R05, Bao thuốc rồng R08 | Quạt nan V11 | Cassette H01, Nón lá H02 | Đàn thần X01, Trống đồng X02 | Ấn Ngọc Tỷ L01, Nỏ thần L05, Gậy sắt làng Gióng L06 |
| Wibu | — | — | Túi thần kỳ H03, Cục sạc chuột H04 | Quả cầu ngọc X04 | Quyển sổ định mệnh (chưa có mô hình) |
| Truyền thuyết Châu Âu | — | — | Gương cổ tích V08, Mũ phù thủy H05 | — | Táo vàng L02, Giày thủy tinh L03, Chiếc nhẫn duy nhất (chưa có mô hình) |
| Robot (và khoa học viễn tưởng) | — | — | — | Kiếm sáng X03, Trái tim robot X05, Phi thuyền mini X06 | — |
| Mô hình | — | Máy game 8-bit V12 | — | — | — |

**Thiếu nhiều nhất:** Huyền thoại của Vải Niêm, Kim Khí, Đen, Robot, Mô hình; gần như cả rương Mô hình; bậc thấp của Wibu, Châu Âu, Robot; Hiếm và Rất hiếm của Gỗ, bậc giữa của Đen.

**Lý do vài chỗ gán:** Áo đấu và vé cược vào Đen vì gắn chuyện cá độ của nhân vật chính. Gương cổ tích chuyển từ đồ thường sang Châu Âu (Bạch Tuyết). Kiếm sáng xếp vào Robot vì là khoa học viễn tưởng, không phải anime. Máy game 8-bit tạm để ở Mô hình.

## Hệ quả cần xử lý

- Đồ nhại hiện có (áo đấu, máy game 8-bit, túi thần kỳ, cục sạc chuột, mũ phù thủy, kiếm sáng, quả cầu ngọc) cần phân vào các chủ đề mới.
- Số món Huyền thoại sẽ **tăng**: hiện 8 (2 chưa có mô hình), cộng món riêng của từng chủ đề. Rương Tổng Hợp có thể không còn là nơi duy nhất ra Huyền thoại.
- Cây phát triển: nhánh Rương bỏ nút Rương Nhại, Rương Việt Nam chuyển sang điều kiện thành tựu nếu chủ dự án đồng ý.
- Cần hệ **thành tựu** trong dữ liệu (`id`, `điều kiện`, `rương mở`), và shop rương có danh sách hôm nay (Codex).
- Mô hình 3D cho từng rương chủ đề (Antigravity và Blender): chưa có.

## Câu hỏi còn mở

1. Chủ đề bổ sung ngoài 5 chủ đề trên (chủ dự án sẽ nghĩ thêm).
2. Món Huyền thoại cụ thể của từng chủ đề (theo tiêu chí ở mục 8), và đồ hiện có thuộc chủ đề nào. Vé số đã chốt giữ (mục 11). Cần chia 38 món hiện có vào từng rương (mỗi món một rương), và tách Huyền thoại hiện có thành loại của rương thường và loại của rương chủ đề.
3. Thành tựu mở khóa từng rương.
4. ~~Rương Tổng Hợp còn ra Huyền thoại không~~ Đã chốt ở mục 9. Rương thường có Huyền thoại cơ bản (mục 10).
6. Tỉ lệ và giá: Claude 2.
