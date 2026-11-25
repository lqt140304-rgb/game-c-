# Sprint 5 phần 2: rương mở khóa và khung rương chủ đề (Claude 2)

Viết ngày 2026-11-24. Phần 2 của việc cân bằng Sprint 5: giá và tỉ lệ ra bậc của 4 rương mở khóa (Tổng Hợp, Cổ Phong, Thiên Cơ, Rác Quý) và **một khung chung** cho mọi rương chủ đề (Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot...), gồm tỉ lệ và giá nền Huyền thoại chủ đề. **Mọi bảng dưới đây là ĐỀ XUẤT, chưa chốt**; điều cần chủ dự án quyết ghi ở mục 7. Cách đo và giả định: [spec cách đo](../../docs/superpowers/specs/2026-11-24-can-bang-ruong-mo-khoa-design.md). Phần 1 (4 rương cơ bản, đã chốt): [Sprint5Ruong.md](Sprint5Ruong.md).

> **Tiền tệ:** mọi số tiền ghi bằng **đồng Việt Nam**. Mô phỏng tính theo đơn vị 10.000 đ (`price=600` là 6.000.000 đ), in qua `QA/currency.py`. Mọi giá đề xuất là bội số của 10.000 đ.

## Kết luận ngắn

1. **Nghiệm thu đạt trên các kịch bản shop đã đo**, với một trường hợp sát trần. Shop có cả 4 rương mở khóa và 2 rương chủ đề (n = 1.500): bot thường **17,2% (thận trọng) và 19,2% (liều)**, người chơi tính toán hoàn hảo tối đa **44,0%**, không rương nào quá 19% lượt mua. Nền chỉ 4 rương cơ bản: 17,5% / 18,3% và 43,4%.
2. **Trường hợp sát trần:** shop có đủ 4 rương mở khóa nhưng chưa có rương chủ đề nào: người chơi giỏi **47,1%** (trần 47%), bot 14,7% / 16,7%. Một phần là nhiễu đo (khoảng 2 đến 3 điểm, xem mục 1), nhưng không có biên. Hai cách giữ biên: shop mỗi ngày chỉ bày 2 rương mở khóa (đo 44,1%), hoặc hạ Tổng Hợp thêm một nấc. Chủ dự án chọn (câu hỏi 2).
3. **Tổng Hợp** là rương mở khóa có sức nặng nhất: được chọn 17% đến 25% lượt mua, lời mỗi hộp 1.780.000 đ, bằng Gỗ và Vải Niêm. Lý do mua: Huyền thoại cơ bản 3% (cao nhất ngoài Rương Đen) và đủ 4 loại đồ của rương cơ bản.
4. **Cổ Phong và Thiên Cơ là khó nhất**: tạo tác đúng hệ đáng giá hơn giá Kho vì có combo (CB1 cộng giá bán). Đắt hơn Kim Khí thì bot thường mua chúng thay Kim Khí và tụt còn 7% đến 11%; ngang Kim Khí mà nhiều đồ thì người chơi giỏi lên 48% đến 53%. Bảng đề xuất giữ giá **4.500.000 đ** (bằng Kim Khí) và cho lời thấp hơn Kim Khí: người chơi giỏi chọn 2% đến 5%. **Vai của hai rương này là "Kim Khí theo hệ"**, không phải đường tắt lên combo: mua rương hệ để săn combo vẫn kém hơn mua tạo tác ở Kho (36% đến 45% so với 44%). Nếu chủ dự án muốn chúng hấp dẫn hơn, cần đổi thiết kế, không chỉ đổi giá (câu hỏi 4).
5. **Rác Quý** là vé số rẻ 600.000 đ: 86% rác, hiếm khi ra đồ thật (trông như rác nên manh mối không tách được). Lời mỗi hộp nhỏ (270.000 đ), người chơi giỏi mua 1,5% đến 3,5% lượt, chủ yếu khi thiếu tiền. Bỏ hẳn Huyền thoại của Rác Quý không đổi kết quả (47,1% cả hai), nên giữ 0,5% cho vui.
6. **Khung rương chủ đề ổn định khi thêm chủ đề**: 1, 2, 3 rương chủ đề cùng lúc cho bot 16% đến 18%, người chơi giỏi 42% đến 44%, mỗi rương chủ đề 16% đến 29% lượt mua. Đề xuất giá **8.000.000 đ**, Huyền thoại chủ đề **3%**, giá nền Huyền thoại chủ đề **90.000.000 đ** (Huyền thoại cơ bản 40 đến 70 triệu đ). Huyền thoại 5% hoặc 120 triệu đ ở 3% làm rương chủ đề quá hút (38% đến 44% lượt mua, bot lên 21%).
7. Mở khóa giữa ván (rương có từ ngày 3) không đổi kết luận: kết quả ngang với có từ ngày 1.

## 1. Tiêu chí và cách đo

Rương mở khóa và rương chủ đề có từ ngày hôm sau khi mở khóa và **không buff chơi** (ruong-suu-tap, ruong-chu-de), nên tiêu chí là **thêm lựa chọn và đánh đổi, không làm game dễ hơn rõ rệt**:

- bot thường (mua rương đắt nhất mua được) vẫn khoảng 15% đến 20%;
- người chơi tính toán hoàn hảo (chọn rương theo lời kỳ vọng, đọc manh mối, giám định; λ = 0,75) không quá 47%;
- không rương nào quá 50% lượt mua của người chơi giỏi;
- mỗi rương mới có lý do để mua (được chọn một phần đáng kể, hoặc có vai riêng).

Nền là **4 rương cơ bản đã chốt** (`chest_sim.FINAL`, có cả lần chỉnh Rương Đen rác 30%, Rất hiếm 58%), giữ nguyên. `QA/chest_unlock.py` thêm rương mới vào cùng lõi mô phỏng đã duyệt (nợ, phạt 20%, giá dao động ±20% theo từng loại rương, manh mối, giám định, combo, tạo tác trùng đổi 50% giá Kho). Chỉ đổi một chỗ trong lõi: bot và người chơi giỏi xem **mọi rương đang có trong shop**, xếp theo giá gốc, thay cho 4 mã cố định.

**Nhiễu đo cần biết.** Thêm một rương vào shop làm mỗi lượt rút thêm một hộp ứng viên, nên chuỗi ngẫu nhiên của cả ván bị xáo. Hai rương đối chứng **không ai mua** (một rương chỉ ra rác, một rương giá không ai trả nổi) vẫn làm người chơi giỏi (liều) đổi từ 43,4% lên **45,8% và 46,3%** dù không thể đổi kết quả thật. Vì vậy chênh lệch dưới khoảng 3 điểm giữa hai kịch bản là nhiễu (sai số mỗi số đo khoảng ±1,3 đến ±1,6 điểm ở n = 1.000 đến 1.500). Báo cáo so với cả nền lẫn đối chứng, và đề xuất chọn sao cho kịch bản tổ hợp nằm dưới trần 47% có biên.

## 2. Bảng đề xuất

Tỉ lệ ra bậc: **R** Rác, **T** Thường, **H** Hiếm, **X** Rất hiếm, **L** Huyền thoại. Luật quay hai bước của `ruong-chu-de`. Rác dùng chung 9 món, bán 30.000 đ.

| Rương | Giá | R | T | H | X | L | Tạo tác | Hàng giả | Manh mối đúng |
|---|---|---|---|---|---|---|---|---|---|
| Tổng Hợp | 6.000.000 đ | 16% | 39% | 28% | 14% | 3% | — | 20% | 2 trên 3 |
| Cổ Phong | 4.500.000 đ | 14% | 30% | 15% | 7% | 0% | **34%**, chỉ hệ Cổ Phong (bậc 1: 60%, bậc 2: 35%, bậc 3: 5%) | 10% | 2 trên 3 |
| Thiên Cơ | 4.500.000 đ | 12% | 29% | 17% | 8% | 0% | **34%**, chỉ hệ Thiên Cơ (như trên) | 10% | 2 trên 3 |
| Rác Quý | 600.000 đ | 86% | 11% | 2,5% | 0% | 0,5% | — | 5% | 2 trên 3 |
| **Mỗi rương chủ đề** | 8.000.000 đ | 13% | 30% | 30% | 24% | **3%** (Huyền thoại chủ đề) | — | 20% | **1 trên 3** |

**Giá nền Huyền thoại chủ đề đề xuất: 90.000.000 đ** (Huyền thoại cơ bản: Vé số 40.000.000 đ, Bản đồ cổ và Đồng hồ bỏ túi 50.000.000 đ, Két sắt 70.000.000 đ). Chủ đề có nhiều Huyền thoại (Việt Nam 4 món) thì tỉ lệ 3% chia đều cho các món, giá trung bình các món nên giữ quanh 90.000.000 đ; món biểu tượng nhất có thể đắt hơn nếu món khác rẻ hơn.

Tổng Hợp ra **đúng các món của 4 rương cơ bản** (ruong-chu-de mục 9), quay đều trong hợp bộ sưu tập ở bậc vừa quay. Cổ Phong, Thiên Cơ, Rác Quý và rương chủ đề **chưa có bộ sưu tập**: bản đo dùng món giữ chỗ có giá bằng giá trung bình bậc của rương cơ bản (Thường 3.960.000 đ, Hiếm 16.870.000 đ, Rất hiếm 28.140.000 đ). Khi Claude 1 chốt món, giá trung bình theo bậc nên giữ gần mức này, lệch quá khoảng ±5% thì đo lại.

### Giá trị kỳ vọng mỗi hộp (chưa tính dao động giá ±20%; tạo tác tính theo giá Kho)

| Rương | Giá | Thu / giá | Lời kỳ vọng | Huyền thoại |
|---|---|---|---|---|
| Gỗ (đã chốt) | 1.400.000 đ | 2,29 | 1.810.000 đ | 0,5% |
| Vải Niêm (đã chốt) | 3.500.000 đ | 1,51 | 1.800.000 đ | 1% |
| Kim Khí (đã chốt) | 4.500.000 đ | 1,53 | 2.370.000 đ | 1% |
| Đen (đã chốt) | 12.000.000 đ | 1,08 | 980.000 đ | 5% |
| Tổng Hợp | 6.000.000 đ | 1,30 | 1.780.000 đ | 3% |
| Cổ Phong | 4.500.000 đ | 1,11 | 500.000 đ | 0% |
| Thiên Cơ | 4.500.000 đ | 1,21 | 920.000 đ | 0% |
| Rác Quý | 600.000 đ | 1,45 | 270.000 đ | 0,5% |
| Mỗi rương chủ đề | 8.000.000 đ | 1,30 | 2.420.000 đ | 3% |

Cổ Phong lời thấp hơn Thiên Cơ có chủ ý: combo Cổ Phong (CB1) cộng giá bán mọi món, nên tạo tác Cổ Phong đáng giá hơn giá Kho nhiều hơn tạo tác Thiên Cơ.

## 3. Số đo theo kịch bản shop

n = 1.500 (nền và tổ hợp), sai số mỗi số khoảng ±1,3 điểm, cộng nhiễu xáo hạt giống ở mục 1. "Người chơi giỏi" ghi 4 số: thận trọng / liều / combo nhỏ / combo lớn. Lượt mua: phần trăm lượt mua của người chơi giỏi thận trọng.

| Shop có | Bot thận trọng / liều | Người chơi giỏi | Lượt mua của rương mới |
|---|---|---|---|
| Chỉ 4 rương cơ bản (nền) | 17,5 / 18,3 | 38,3 / 43,4 / 38,4 / 38,0 | — |
| Đối chứng: + rương không ai mua (n = 1.000) | 16,4 / 16,6 | 38,7 / 45,8 / 37,1 / 37,1 | 0% |
| + Cổ Phong | 16,9 / 17,5 | 41,5 / 46,9 / 40,3 / 39,5 | 3,7% |
| + Thiên Cơ (n = 1.000) | 15,6 / 15,8 | 39,1 / 45,8 / 39,8 / 39,8 | 4,9% |
| + Rác Quý | 18,1 / 18,5 | 39,8 / 46,7 / 42,8 / 42,1 | 3,5% |
| + 1 rương chủ đề | 16,5 / 18,4 | 35,7 / 43,7 / 37,5 / 37,6 | 26,3% |
| + 3 rương chủ đề | 16,2 / 18,0 | 34,5 / 42,3 / 35,2 / 35,9 | 16% đến 18% mỗi rương |
| + 4 rương mở khóa | 14,7 / 16,7 | 41,6 / **47,1** / 41,7 / 41,5 | Tổng Hợp 17,5%, Cổ Phong 2,3%, Thiên Cơ 2,9%, Rác Quý 3,3% |
| + 4 rương mở khóa, Rác Quý không Huyền thoại | 14,6 / 16,7 | 41,7 / 47,1 / 41,4 / 41,0 | như trên |
| **+ 4 rương mở khóa + 2 chủ đề (tất cả)** | **17,2 / 19,2** | 37,3 / **44,0** / 37,5 / 38,1 | Tổng Hợp 12,4%, Cổ Phong 0,9%, Thiên Cơ 1,2%, Rác Quý 4,5%, chủ đề 16,1% mỗi rương; Gỗ 16,8%, Kim Khí 16,6% |
| Tất cả, shop bày 2 rương mới mỗi ngày | 15,8 / 16,6 | 38,9 / 44,1 / 39,1 / 38,9 | chủ đề 8% mỗi rương, Tổng Hợp 5% |

Mở khóa giữa ván (rương mới có từ ngày 3) được đo trên bảng Cổ Phong / Thiên Cơ trước (bản mạnh hơn): kết quả ngang với có từ ngày 1 (48,5% so với 49,1%), nên kết luận không phụ thuộc ngày rương xuất hiện.

Tổng Hợp riêng (bảng trước, Rất hiếm 16%): bot 17,0 / 18,0, người chơi giỏi 46,9%, chọn 24,5%. Bảng cuối hạ Rất hiếm xuống 14% vì tổ hợp 4 rương mở khóa lên 49,3% với bản 16%.

Bot thường mua Tổng Hợp nhiều nhất khi nó có trong shop (khoảng 49% lượt mua của bot), vì nó là rương đắt nhất bot mua được trước ngày 4. Lời mỗi hộp của Tổng Hợp thấp hơn Kim Khí một chút nên bot thận trọng tụt nhẹ (14,7% khi có đủ 4 rương mở khóa). Có rương chủ đề thì bot mua rương chủ đề thay vào và lên lại 17%.

## 4. Rương Cổ Phong và Thiên Cơ: vì sao khó nhất

Dò trên kịch bản "4 cơ bản + một rương hệ" (n = 1.000 đến 1.500). "Săn hệ" là người chơi giỏi ưu tiên mua rương hệ ở ngày 1 đến 5 cho đến khi có đủ cặp combo (CB1 hoặc CB2), rồi chơi bình thường.

| Bảng thử | Bot thận trọng / liều | Người chơi giỏi (liều) | Săn hệ (liều) | Chọn |
|---|---|---|---|---|
| Cổ Phong 3.500.000 đ, 50% tạo tác (bảng cũ, n = 600) | 15,2 / 16,3 | 50,5 | — | 3% |
| Cổ Phong 4.000.000 đ, 50% tạo tác, đồ yếu | 14,8 / 15,4 | 45,1 | 37,3 | 0,7% |
| Cổ Phong 4.500.000 đ, bảng như Kim Khí (R10 T25 H22 X9) | 17,0 / 17,7 | **53,3** | 54,1 | 20% |
| Thiên Cơ 4.500.000 đ, bảng như Kim Khí | 17,0 / 17,6 | 47,9 | 47,2 | 18% |
| Cổ Phong 5.000.000 đ, bảng như Kim Khí | **10,1 / 11,5** | 50,2 | 46,8 | 13% |
| Cổ Phong 5.500.000 đ, bảng như Kim Khí | **7,5 / 10,0** | 47,5 | 42,6 | 6% |
| Cổ Phong 4.500.000 đ, R15 T30 H15 X6 | 15,4 / 15,8 | 46,0 | 41,3 | 2% |
| Cổ Phong 4.500.000 đ, R12 T29 H17 X8 | 15,6 / 15,8 | 48,8 | 45,2 | 8% |
| **Cổ Phong 4.500.000 đ, R14 T30 H15 X7 (đề xuất, n = 1.500)** | 16,9 / 17,5 | 46,9 | 42,8 | 4% |
| Thiên Cơ 4.500.000 đ, R15 T30 H15 X6 | 15,4 / 15,8 | 45,2 | 36,5 | 0,7% |
| **Thiên Cơ 4.500.000 đ, R12 T29 H17 X8 (đề xuất)** | 15,6 / 15,8 | 45,8 | 41,5 | 5% |

Đọc bảng:

- **Giá trên Kim Khí làm bot thường tụt mạnh** vì bot mua rương đắt nhất mua được, nên rương hệ chiếm chỗ của Kim Khí trong thứ tự mua. Vì vậy giá rương hệ nên **bằng Kim Khí (4.500.000 đ)**.
- **Ngang giá, lời bằng Kim Khí thì quá mạnh** cho người chơi giỏi: rương hệ cho tạo tác đúng hệ mà Kim Khí chia đôi cho hai hệ, cộng combo.
- Bảng đề xuất cho rương hệ lời ít hơn Kim Khí: người chơi giỏi chỉ mua khi đang cần hệ đó, khoảng 4% đến 5% lượt mua. Đó là **"lý do để mua" yếu nhất** trong các rương mới; nếu muốn mạnh hơn, xem câu hỏi 4.

## 5. Khung rương chủ đề và Huyền thoại chủ đề

Một khung dùng cho mọi chủ đề (Việt Nam, Tuổi thơ Việt Nam, Wibu, Truyền thuyết Châu Âu, Robot, Không gian...). Bộ sưu tập giữ chỗ mỗi rương: 2 Thường, 2 Hiếm, 2 Rất hiếm, 1 Huyền thoại chủ đề, cùng một nhóm nguồn gốc. Đo với 1 rương chủ đề (n = 1.000, trừ dòng đề xuất).

| Biến thể | Thu / giá | Bot thận trọng / liều | Người chơi giỏi (liều) | Chọn |
|---|---|---|---|---|
| 8.000.000 đ, R15 T40 H27 X15 L3 (bảng đầu, n = 600) | 1,08 | **9,7 / 9,0** | 42,2 | 2% |
| **Đề xuất: 8.000.000 đ, R13 T30 H30 X24 L3, Huyền thoại 90 triệu đ (n = 1.500)** | 1,30 | 16,5 / 18,4 | 43,7 | 26% |
| Như đề xuất, manh mối đúng 2 trên 3 | 1,30 | 16,5 / 18,7 | 43,0 | 28% |
| Huyền thoại 70 triệu đ | 1,25 | 15,5 / 17,6 | 45,2 | 18% |
| Huyền thoại 120 triệu đ | 1,38 | 17,5 / **20,6** | 39,0 | **38%** |
| Huyền thoại 120 triệu đ, tỉ lệ 2% | 1,28 | 15,3 / 17,9 | 43,3 | 22% |
| Huyền thoại 5% (90 triệu đ) | 1,41 | 18,4 / **21,6** | 40,8 | **44%** |

Đọc bảng:

- Rương chủ đề **đắt hơn Kim Khí** nên chiếm chỗ của Kim Khí trong thứ tự mua của bot; thu chỉ 1,08 lần giá thì bot tụt còn 9%. Cần thu khoảng **1,3 lần giá** (lời khoảng 2.400.000 đ, ngang Kim Khí).
- **Manh mối đúng 1 trên 3** (như Rương Đen) hợp với rương chủ đề: đồ chủ đề cùng một nhóm nguồn gốc nên manh mối chỉ phân biệt rác với đồ chủ đề; với 2 trên 3 kết quả gần như nhau.
- **Huyền thoại chủ đề đắt hơn hoặc dễ ra hơn làm game dễ hơn cho bot** (lên 21%) và rương chủ đề hút gần nửa lượt mua. Giữ 3% và 90 triệu đ. Nếu muốn món biểu tượng đắt hơn (120 triệu đ) thì hạ tỉ lệ xuống 2%: kết quả vẫn trong mục tiêu.
- **Thêm chủ đề không làm game dễ hơn**: 1, 2, 3 rương chủ đề cho người chơi giỏi 43,7%, 40,4% (n = 1.000, bảng trước khi chốt Rương Đen), 42,3%. Lượt mua chia đều giữa các chủ đề.

## 6. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_chest_unlock          # test khóa, khoảng 2 đến 3 phút
nohup python3 chest_unlock_final.py final > final.log &   # nền và từng kịch bản shop, n = 1.500 (khoảng 35 phút; số của "Tổng Hợp", "mo_khoa", "tat_ca" ở đây là trên bảng trước, số cuối lấy từ "xacnhan")
nohup python3 chest_unlock_final.py sens > sens.log &     # đối chứng, shop xoay vòng, mở khóa giữa ván, độ nhạy (khoảng 25 phút)
nohup python3 chest_unlock_final.py he > he.log &         # dò bảng Cổ Phong / Thiên Cơ (khoảng 10 phút)
nohup python3 chest_unlock_final.py xacnhan > xn.log &    # xác nhận bảng cuối: đủ 4 rương mở khóa, tất cả, shop 2 rương mới mỗi ngày (khoảng 20 phút)
```

Mã: `QA/chest_unlock.py` (`UNLOCK`, `THEME`, `THEME_LEG` là bảng đề xuất; `scenario()` dựng kịch bản shop; `shop_extra` cho shop xoay vòng; `from_day` cho mở khóa giữa ván; `hunt_strategy` là người chơi "săn hệ"; `restore()` trả lõi về như cũ).

## 7. Câu hỏi mở cho chủ dự án

1. **Duyệt bảng đề xuất** ở mục 2 (Tổng Hợp 6.000.000 đ, Cổ Phong và Thiên Cơ 4.500.000 đ, Rác Quý 600.000 đ, khung rương chủ đề 8.000.000 đ với Huyền thoại chủ đề 3% giá nền 90.000.000 đ).
2. **Shop bày bao nhiêu rương một ngày?** Spec chưa nói. Bày tất cả rương đã mở thì trường hợp "đủ 4 rương mở khóa, chưa có chủ đề" sát trần (47,1%). Đề xuất: mỗi ngày shop bày 4 rương cơ bản và **tối đa 2 rương mở khóa hoặc chủ đề**, xoay vòng theo hạt giống ván (đo 44,1%, bot 15,8% / 16,6%). Cách khác: hạ Tổng Hợp thêm (ví dụ Hiếm 26%).
3. **Bộ sưu tập của Cổ Phong, Thiên Cơ, Rác Quý** chưa có món nào (spec chỉ có tên rương). Cần Claude 1 đề xuất món; giá trung bình theo bậc nên giữ gần giá giữ chỗ ở mục 2.
4. **Vai của Rương Cổ Phong / Thiên Cơ.** Mở khóa khi "đủ 1 hệ tạo tác", nhưng về kinh tế rương hệ không thể vừa rẻ vừa ra tạo tác đúng hệ mà không làm game dễ hơn. Bản đề xuất là "Kim Khí theo hệ", lời thấp, ít người mua. Nếu muốn rương có vai rõ hơn mà vẫn không buff, có thể đổi thiết kế: ví dụ tạo tác từ rương hệ **không cho đổi tiền khi trùng** (chỉ là món sưu tập), hoặc rương hệ ra **phiên bản sưu tập** của tạo tác (không có hiệu ứng). Việc của chủ dự án và Claude 1.
5. **Rác Quý ra gì:** bản đo giả định "rác quý" là đồ trông như rác (cùng nhóm nguồn gốc với rác) nên manh mối không tách được. Nếu đồ của Rác Quý có nhóm riêng dễ nhận ra, người chơi giỏi lọc được hộp tốt và cần đo lại (bảng đầu với nhóm khác cho người chơi giỏi 50%).
6. **Tạo tác của Cổ Phong / Thiên Cơ có tính trùng như Kim Khí** (đổi 50% giá Kho, đã chốt cho Kim Khí) hay không: bản đo giả định có.
7. **Chủ đề có nhiều Huyền thoại** (Việt Nam 4 món, Truyền thuyết Châu Âu 4 món): bản đo dùng 1 món 90 triệu đ. Đề xuất giữ tỉ lệ bậc 3% chung và giá trung bình 90 triệu đ; chủ dự án quyết món nào đắt nhất.
8. **Ngưỡng nhiễu đo:** trần 47% là tuyệt đối, nhưng chênh 2 đến 3 điểm giữa hai kịch bản là nhiễu. Nếu chủ dự án muốn chắc hơn, chạy lại kịch bản tổ hợp với n = 5.000 (khoảng 1 giờ).
