# Thiết kế: hệ rương, rác, sưu tập và thần lực (phần 1/4)

Trạng thái: **chờ chủ dự án duyệt**. Chưa có gì được xây theo bản này.

## Ý định

Làm cho việc mở rương bớt "đập hộp ra đồ cổ" và thêm may rủi, hài hước, tích lũy lâu dài. Rương có thể ra cả **rác vô giá trị**, tạo tác lấy được từ rương lẫn Kho, ô tạo tác nâng lên 4, và bộ sưu tập mở thưởng trong ván và giữa các ván. Đồ cổ có chút thần lực dí dỏm cho có hồn.

Đây là **phần 1 trong 4**. Ba phần còn lại có bản thiết kế riêng, làm sau:
2. Mở màn và cốt truyện (cutscene).
3. Một ngày trong game: mini-game buổi sáng (đánh răng, ăn sáng) và tình huống đời thường hài hước.
4. Nội dung thần lực chi tiết cho từng món.

## Mục tiêu

- 4 loại rương cơ bản, mỗi loại một lối chơi riêng, mua bằng tiền ở quầy.
- Rương có thể ra rác; rác có công dụng ẩn.
- Tạo tác có hai nguồn: rương và Kho Tạo Tác.
- 4 ô trang bị mặc định; Bí Khế BK1 mở ô thứ 5.
- Sưu tập hai lớp: thưởng nhỏ trong ván, thưởng lớn giữa các ván.
- Thần lực chỉ là phản ứng thú vị, không đổi cơ chế.

## Không phải mục tiêu

- Thần lực **không** đổi tiền, giá, xác suất, nợ hay luật thắng thua. Tắt hết thần lực thì cân bằng kinh tế vẫn y nguyên.
- Không viết nội dung cốt truyện, cutscene, mini-game buổi sáng trong bản này.
- Không chốt con số cuối cùng: giá, tỉ lệ rơi, tỉ lệ rác thuộc Claude 2 (cân bằng).

## Ràng buộc đã có

- Nợ ngày 1 chỉ 200 xu, vốn đầu ván 1000 xu, tối đa 3 hộp mỗi ngày, kho 8 ô, hàng giả bán còn 20% giá. (`Docs/Content/data/content.json`, mục `rules`.)
- Luật cộng hưởng, combo và thứ tự áp dụng hiệu ứng ở `Docs/Content/GiaTruyenTaoTacBiKhe.md`.
- Bản chơi thử hiện chỉ có một khu chợ và 7 ngày.

## 1. Bốn rương cơ bản (mua ở quầy)

Giá và tỉ lệ dưới đây chỉ là **đề xuất khởi đầu**.

| Rương | Lối chơi | Giá | Giả | Ra gì |
|---|---|---|---|---|
| Gỗ | An toàn, kiếm tiền đều | 120 | ~10% | 1 đồ cổ thường, 2 manh mối đúng |
| Vải Niêm | Gom theo bộ | 350 | ~15% | 1 đồ cổ thuộc một bộ in trên tem |
| Kim Khí | Ra tạo tác | 500 | không giả | 1 tạo tác tier 1–2, hiếm khi tier 3, có thể trùng |
| Đen | Đánh cược lớn | 1200 | ~30% | 1 đồ hiếm, đôi khi kèm 1 tạo tác |

Mô hình 3D đã có sẵn: Gỗ = H1, Vải Niêm = H3, Kim Khí = H2, Đen = RD.

## 2. Rác

- Mỗi rương có thêm tỉ lệ ra rác (Gỗ ít, Vải Niêm vừa, Kim Khí vài món máy móc hỏng, Đen nhiều nhất). Tỉ lệ do Claude 2 đo.
- Rác bán rất rẻ (vài xu) hoặc vứt được, **chiếm ô kho** như hiện vật.
- Mỗi món rác có **một công dụng ẩn**, chỉ lộ ra trong tình huống đúng. Ví dụ: trứng thối ném đuổi kẻ đòi nợ; xương cá cho mèo của chợ; giày cũ làm quà cho NPC; tất thối dọa kẻ trộm.
- Dữ liệu cần thêm trường `use` (công dụng). Nội dung từng công dụng điền ở phần 3 và 4.
- Rác làm người chơi mất lượt mua và ô kho, đó là cái giá của may rủi.

## 3. Ô tạo tác

- Mặc định **4 ô** (hiện 3). Bí Khế BK1 mở **ô thứ 5** (hiện mở ô thứ 4).
- Hai hệ Cổ Phong và Thiên Cơ có 6 tạo tác; 4 ô buộc người chơi chọn hệ hoặc pha hai hệ.
- Mốc cộng hưởng và combo hiện có giữ nguyên, nhưng "3 món cùng hệ" dễ đạt hơn nên **Claude 2 phải kiểm tra lại**.
- Cần đổi dữ liệu: `rules.equip_slots` từ 3 lên 4, `equip_slots_with_bk1` từ 4 lên 5.

## 4. Sưu tập hai lớp

**Cuốn Sổ Sưu Tập** ghi mọi thứ người chơi từng sở hữu: hiện vật, tạo tác và cả rác.

- **Lớp 1, trong ván:** gom đủ một bộ trong cùng ván (ví dụ 3 món cùng nhóm nguồn gốc) thì nhận hiệu ứng nhỏ đến hết ván, gần cơ chế Bí Khế.
- **Lớp 2, giữa các ván (meta, vĩnh viễn):** mở thưởng lớn. Giữ 4 mở khóa M1–M4 hiện có và thêm:

| Điều kiện | Phần thưởng (ví dụ) |
|---|---|
| Có đủ 6 nhóm nguồn gốc | Rương Tổng Hợp (ra đồ của mọi nhóm) |
| Gom đủ 1 hệ tạo tác | Rương Cổ Phong hoặc Rương Thiên Cơ |
| Sưu tập đủ 5 loại rác | Rương Rác Quý (đổi rác lấy đồ) |
| Sưu tập đủ một bộ đồ cổ | Cutscene và khoảnh khắc thần lực mới |

- Rương mở khóa nằm **ngoài 4 rương cơ bản**, là rương cấp cao chỉ có sau khi mở khóa, nên không làm lệch cân bằng 4 rương đầu.
- Phần thưởng meta **không có buff chơi**: chỉ là rương mới, bộ cốt truyện, cảnh mới và phản ứng thần lực mới.

## 5. Thần lực

- Chỉ là **phản ứng dí dỏm**, không đổi cơ chế (xem mục không phải mục tiêu).
- Hai chỗ thể hiện, làm theo giai đoạn:
  - **Khi cầm và soi** (phím F): hiệu ứng, âm thanh, lời thoại riêng. Nối thẳng vào hệ cầm và soi đã có (`HeldItemViewmodel`).
  - **Ngoài đời thật:** chuông chợ rung, mèo của chợ phản ứng. Cần thêm nội dung và cảnh.
- Mỗi món có **một** phản ứng riêng, ưu tiên có hồn hơn có số lượng. Ví dụ: bình sứ thì thầm thơ; gương cổ tích khen hoặc chê; đèn dầu thờ bập bùng khi nói dối giá.

## Câu hỏi còn mở

1. Giá, tỉ lệ rơi, tỉ lệ rác và mức cộng hưởng khi có 4 ô: **chờ Claude 2**.
2. Rác cần mô hình 3D hoặc icon: bao nhiêu món, làm bằng Meshy (gộp một ảnh như các lần trước)?
3. Rương mở khóa (Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý) cần mô hình 3D riêng không, hay dùng lại 4 mô hình hiện có đổi màu?
4. Khoảnh khắc thần lực "ngoài đời thật" cần cảnh chợ có NPC và mèo, đang chờ Antigravity dựng cảnh.
5. Dữ liệu và hợp đồng: Codex cần thêm loại vật phẩm `junk`, trường `use`, và loại rương mới vào `Docs/Content/YeuCauDuLieu.md`.
