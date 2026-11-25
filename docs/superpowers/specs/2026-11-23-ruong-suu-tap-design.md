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
- **Lớp 2, trong ván (không vĩnh viễn): cây phát triển (skill tree).** **Chủ dự án chốt 2026-11-25: cây kỹ năng chỉ dùng trong ván, không có tiến trình vĩnh viễn giữa các ván.** Mỗi ván bắt đầu với cây trống, điểm và nút mở được tính lại từ đầu, hết ván thì mất. Bản cũ ghi "giữa các ván (meta, vĩnh viễn)" đã bị thay. Giữ 4 mở khóa M1–M4 làm các nút đầu cây nếu còn phù hợp (Claude 2 xét lại).

**Cây có 4 nhánh**

| Nhánh | Mở ra | Ví dụ nút |
|---|---|---|
| Rương | Rương mới, rương xuất hiện sớm hơn | Rương Tổng Hợp (đủ **8** nhóm nguồn gốc, nay gồm cả Việt Nam và Nhại), Rương Cổ Phong / Thiên Cơ (đủ 1 hệ tạo tác), Rương Rác Quý (đủ 5 loại rác), **Rương Việt Nam, Rương Nhại** (đồ hai nhóm mới với tỉ lệ cao, xem phần 6), M3 Rương Đen sớm |
| Sưu tập | Trang Sổ Sưu Tập, bộ sưu tập mới | M1 chọn gia truyền, M2 vốn đầu ván, thưởng khi gom đủ một bộ |
| Thần lực | Thêm phản ứng dí dỏm của đồ cổ (khi cầm và ngoài đời thật) | Món thần thoại mở thêm lời thì thầm, đồ cổ tích phản ứng với mèo chợ |
| Câu chuyện | Cutscene, cảnh, NPC mới | Mở khóa chương mới khi sưu tập đủ một bộ đồ cổ |

**Luật mở nút**
- Nút cần **nút cha đã mở**, nên người chơi đi sâu một nhánh hoặc trải đều.
- Mở một nút cần **cả hai**: (1) đủ điều kiện sưu tập của nút đó, và (2) tốn **điểm phát triển**.
- Điểm phát triển **kiếm và tiêu ngay trong ván** (không mang sang ván sau). **Nguồn điểm trong ván (cuối mỗi ngày? khi ghi Sổ Sưu Tập? khi trả nợ đúng hạn?) chưa chốt**, xem câu hỏi mở dưới. Bản cũ (điểm cuối ván, tích lũy giữa các ván) không còn hiệu lực.
- Phần thưởng meta **không có buff chơi**: chỉ là rương mới, bộ cốt truyện, cảnh mới và phản ứng thần lực mới. (Riêng M2 vốn đầu ván và M4 hiện có là ngoại lệ đã có sẵn, Claude 2 xét có giữ không.)
- Đóng góp mới cho dữ liệu: nút cây gồm `id`, `branch`, `parent`, `cost`, `condition`, `reward`.

## 5. Thần lực

- Chỉ là **phản ứng dí dỏm**, không đổi cơ chế (xem mục không phải mục tiêu).
- Hai chỗ thể hiện, làm theo giai đoạn:
  - **Khi cầm và soi** (phím F): hiệu ứng, âm thanh, lời thoại riêng. Nối thẳng vào hệ cầm và soi đã có (`HeldItemViewmodel`).
  - **Ngoài đời thật:** chuông chợ rung, mèo của chợ phản ứng. Cần thêm nội dung và cảnh.
- Mỗi món có **một** phản ứng riêng, ưu tiên có hồn hơn có số lượng. Ví dụ: bình sứ thì thầm thơ; gương cổ tích khen hoặc chê; đèn dầu thờ bập bùng khi nói dối giá.

## Câu hỏi còn mở

1. Giá, tỉ lệ rơi, tỉ lệ rác và mức cộng hưởng khi có 4 ô: **chờ Claude 2**.
2. ~~Rác cần mô hình 3D hoặc icon~~ **Đã làm:** 9 món rác bằng Meshy, xem phần 6.
3. Rương mở khóa (Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý) cần mô hình 3D riêng không, hay dùng lại 4 mô hình hiện có đổi màu?
4. Khoảnh khắc thần lực "ngoài đời thật" cần cảnh chợ có NPC và mèo, đang chờ Antigravity dựng cảnh.
5. Giá điểm phát triển của từng nút và cách chia điểm cuối ván: **chờ Claude 2**; giao diện cây (màn hình) do Antigravity dựng.
6. Dữ liệu và hợp đồng: Codex cần thêm loại vật phẩm `junk`, trường `use`, loại rương mới và nút cây phát triển vào `Docs/Content/YeuCauDuLieu.md`.

## Đã chốt và câu hỏi mở sau khi cây chỉ trong ván (2026-11-25)

**Đã chốt với chủ dự án:**
1. **Nguồn điểm phát triển:** kiếm từ **từng công việc hằng ngày** trong ván, như mở rương và chơi mini game (không phải điểm cuối ván). Cách chia điểm cụ thể mỗi việc: Claude 2 đo lại (bản đo hiện dùng 3 điểm mỗi ngày là giả định).
2. **Rương mở khóa và rương chủ đề chỉ mở khóa trong ván**, không vĩnh viễn giữa các ván.
3. **Cây khó mở, thưởng không buff:** chọn phương án "cây khó" của báo cáo, tổng thưởng có tác động lên tiền hoặc rương không quá khoảng 1.000.000 đ quy đổi (chủ dự án trả lời "oke").

**Còn mở:**
1. Nút **Câu chuyện** (cutscene, cảnh, NPC mới): trong ván 7 ngày nên là gì? Đề xuất để chủ dự án chọn: giữ 1 đến 2 nút mở theo ngày (đoạn truyện ngắn, NPC ghé chợ), hoặc bỏ nhánh này khỏi cây và mở cảnh theo thành tựu.
2. **M2** (Vốn Khởi Nghiệp Nhỏ: +1.000.000 đ vốn đầu ván, mở bằng ghi 10 hiện vật khác nhau vào Sổ) và **M3** (Thử Thách Rương Đen: Rương Đen có từ ngày 1 thay vì ngày 4, mở bằng thắng 1 ván): giữ, đổi hay bỏ. Đề xuất của Claude 2: M3 bỏ khỏi cây (mở giữa ván vô nghĩa, chỉ là tùy chọn lúc vào ván); M2 bỏ phần cộng vốn hoặc thành khoản thưởng khi mở nút (đã đo: +1.000.000 đ từ ngày 1 đẩy người chơi hoàn hảo lên 47,6%, chạm trần).
