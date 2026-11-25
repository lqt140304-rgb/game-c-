# Sprint 5 phần 4: cây kỹ năng trong ván (Claude 2)

Viết lại ngày 2026-11-25 sau hai thay đổi của chủ dự án: (1) **cây kỹ năng chỉ dùng trong ván, không có tiến trình vĩnh viễn** ([ruong-suu-tap](../../docs/superpowers/specs/2026-11-23-ruong-suu-tap-design.md) mục 4, [DaChot](../DaChot.md)); (2) luật soi lỗi mới và khó hơn (phạt sót 10%, trần trả giá 1,05, nhà vá lỗi 12% / 30%, dấu lạ 15%; [Sprint5SoiLoi.md](Sprint5SoiLoi.md)). **Mọi con số dưới đây là ĐỀ XUẤT hoặc GIẢ ĐỊNH của Claude 2, chưa chốt.** Điều cần chủ dự án quyết ở mục 6.

> **Phần đã bỏ.** Bản trước đo *chuỗi 45 ván* (điểm cuối ván tích lũy, "nút đầu ván 2, cả cây ván 37", tỉ lệ thắng theo khối 5 ván, M2 đẩy người chơi giỏi lên 50,6%). Toàn bộ không còn ý nghĩa vì không có tiến trình giữa các ván, và đo trên luật soi cũ. Mã chuỗi vẫn nằm ở `QA/tree_sim.py` (`chain`, đã ghi chú là bỏ), không còn test. Điểm thắng 8 / thua 2 và giá cây 200 điểm của bản trước bị thay bằng mục 2.

> **Tiền tệ:** số tiền ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ (`CASH` 100 = 1.000.000 đ), in qua `QA/currency.py`. "Điểm" là điểm cây, không phải tiền.

## Kết luận ngắn

1. **Trong một ván 7 ngày cây chỉ mở được một phần: khoảng 5 đến 6 nút trên 19** (giá một nửa bản cũ, 3 điểm mỗi ngày). Nút đầu mở ở **ngày 2** (84% đến 95% ván), nút thứ hai đến thứ tư ở ngày 3 đến 4. Rương mở khóa (Rác Quý, Tổng Hợp) mở ở **ngày 4 đến 5**, Cổ Phong và Thiên Cơ ngày 4 đến 5 nhưng chỉ khoảng 20% ván (cần có tạo tác đúng hệ). M2 (ngày 6) chỉ 3% ván. Nếu giữ giá bản cũ (6 đến 14 điểm), chỉ mở 3 đến 4 nút, rương mở khóa ở ngày 7 hoặc không bao giờ: **cây không có nghĩa**. Vì thế đề xuất cây nhỏ hơn và rẻ hơn (mục 2).
2. **Cây trong ván không phá cân bằng, vì rương mở muộn.** Kịch bản "rương chỉ mở qua cây, chưa có rương chủ đề" (n = 3.000 mỗi cách chơi): bot thường **14,7% / 15,3%**, người chơi tính toán hoàn hảo **46,0%** (liều + giám định), 38,6% (thận trọng + giám định). Sát trần 47% nhưng dưới, trong khoảng ±0,9 điểm.
3. **Cận trên của "vĩnh viễn" (mọi rương mở khóa và chủ đề có từ ngày 1) cũng dưới trần:** người chơi giỏi tối đa **44,3%** (4 mở khóa), **42,8%** (4 mở khóa + 2 chủ đề), 42,7% khi shop bày tối đa 4. Vậy dù chủ dự án chọn rương vĩnh viễn hay trong ván, trần 47% giữ được ở luật soi mới.
4. **Bot thường 12% đến 15%, thấp hơn 15% đến 20% một chút khi có nhiều rương** (4 mở khóa + 2 chủ đề, mọi rương từ ngày 1: 12,0% / 12,2%). Nền 4 rương cơ bản dưới luật soi mới đã chỉ **14,3% / 14,3%**: phần hụt là của luật soi, không phải cây. Rương mở muộn trong ván làm bot gần 15% hơn rương có từ ngày 1.
5. **M2 (vốn +1.000.000 đ) không còn vượt trần dưới luật soi mới.** Cộng vốn từ ngày 1: nền 43,9% lên 45,5%; cả cây 42,8% lên 45,2% (người chơi giỏi), bot +1 điểm. Trong ván M2 gần như không mở (3% ván), nên tác động thực bằng 0. Bản cũ kết luận "bỏ cộng vốn" vì 50,6%, nay không còn lý do về trần; quyết định giữ hay bỏ là của chủ dự án (mục 6, câu 4).
6. **Đề xuất "Tổng Hợp hàng giả 10% (Thường 43%, Hiếm 25%, Rất hiếm 13%)" vẫn nên giữ nhưng cần đo lại với luật soi mới ở mục 4:** dưới luật mới, với bảng đó 4 mở khóa cho người chơi giỏi 44,3% và bot 12% đến 14%. Chưa đo lại bảng gốc 20% trên luật mới (tồn việc, mục 6 câu 6).
7. **Luật shop "tối đa 4 rương mở khóa hoặc chủ đề mỗi ngày" (chủ dự án chốt 2026-11-25)** gần như không đổi so với bày hết (4 mở khóa: 44,3% cả hai; thêm 2 chủ đề: 42,8% so với 42,7%). Phương án "tối đa 2" của phần 2 không còn cần, vì trần đã giữ được.

## 1. Cách đo (mô hình GIẢ ĐỊNH, chưa chốt)

`QA/tree_run.py` (mới) chạy trên lõi `tree_sim` + `chest_unlock` + `inspect_sim` (luật soi mới nhất của nhánh). Mỗi ván bắt đầu với cây trống.

- **Thời điểm mua nút:** đầu mỗi ngày d ≥ 2, cộng điểm của ngày d − 1 rồi mua mọi nút đủ điều kiện và đủ điểm; rương mới mở sáng ngày d có trong shop từ ngày d. Bot mua nút rẻ trước, người chơi giỏi mua nút có tác động lên ván trước.
- **Nguồn điểm (chưa chốt, đo 3 loại):** *ngày* (3 điểm mỗi ngày qua), *Sổ* (1 điểm mỗi mục mới trong Sổ của ván, Rất hiếm và Huyền thoại 2), *nợ* (3 điểm mỗi kỳ trả đúng hạn).
- **Sổ Sưu Tập tính trong ván** (rỗng đầu ván). Điều kiện nút quy về thang một ván; nút Câu chuyện dùng điều kiện "qua ngày X" thay "đã chơi N ván" (mục 6, câu 3).
- **Rương mở khóa** chỉ có trong ván nếu mở nút cây (kịch bản *trong ván*). Rương chủ đề mở bằng thành tựu chưa chốt; giả định "Sổ của ván có ≥ 6 và ≥ 10 mục" (B2) hoặc "≥ 10 và ≥ 16" (B), hoặc không có (A).
- **Kịch bản vĩnh viễn:** mọi rương có từ ngày 1. Đây là cận trên của kịch bản trong ván, vì mở nút không tốn tiền, chỉ tốn điểm.
- M1 (chọn gia truyền) và M3 (Rương Đen từ ngày 1) không mô phỏng trong ván: M1 cộng dưới 1 điểm (giữ), M3 là thử thách (bot 14,3% xuống 7,9%, người chơi giỏi 43,9% xuống 38,5%); không nên mở tự do vào ván.
- Cách chơi: bot thận trọng và liều; người chơi giỏi 4 kiểu (thận trọng, liều, combo nhỏ, combo lớn, đều + giám định), λ = 0,75. Nhiễu: ±0,9 điểm ở n = 3.000 với 44%; ±1,6 ở n = 1.000.

## 2. Cây trong ván (đề xuất)

19 nút (bỏ M3 khỏi cây, mục 5), giá và điều kiện quy về một ván 7 ngày. Giá tính bằng điểm, **một nửa bản cũ**:

| Nhánh | Nút (cha) | Điểm | Điều kiện trong ván | Phần thưởng |
|---|---|---|---|---|
| Rương | R1 Kệ rương (gốc) | 3 | Sổ ≥ 3 | Ghi chú cửa hàng |
| | R2 Rác Quý (R1) | 4 | 2 loại rác | Rác Quý vào shop |
| | R3 Tổng Hợp (R1) | 6 | 3 nhóm nguồn gốc | Tổng Hợp vào shop |
| | R4 Cổ Phong (R3) | 7 | 1 tạo tác Cổ Phong | Cổ Phong vào shop |
| | R5 Thiên Cơ (R3) | 7 | 1 tạo tác Thiên Cơ | Thiên Cơ vào shop |
| Sưu tập | S1 (gốc) | 3 | Sổ ≥ 3 | Trang Sổ |
| | S2 (S1) | 4 | Sổ ≥ 6 | Trang Sổ |
| | M1 Chọn gia truyền (S2) | 5 | Sổ ≥ 8 | Chọn GT1 hoặc GT2 |
| | M2 Vốn khởi nghiệp nhỏ (M1) | 7 | Sổ ≥ 10 | **Cộng vốn 1.000.000 đ nếu giữ** (hoặc đổi thưởng không buff) |
| | S3 Trang Huyền thoại (S2) | 6 | 1 món Huyền thoại | Trang Sổ |
| | M4 Bí Khế quen mặt (S3) | 7 | 2 tạo tác | Sổ hiện điều kiện Bí Khế (thông tin) |
| Thần lực | T1 (gốc) | 3 | Sổ ≥ 3 | Phản ứng dí dỏm |
| | T2 (T1) | 4 | 3 nhóm nguồn gốc | Phản ứng ngoài đời |
| | T3 (T2) | 6 | Sổ ≥ 9 | Phản ứng ngoài đời |
| | T4 (T3) | 7 | 1 món Huyền thoại | Phản ứng món Huyền thoại |
| Câu chuyện | C1 (gốc) | 3 | Qua ngày 2 | Đoạn truyện |
| | C2 (C1) | 4 | Qua ngày 3 | Đoạn truyện |
| | C3 (C2) | 6 | Qua ngày 5 | Cảnh mới |
| | C4 (C3) | 7 | Qua ngày 6 | Cảnh mới |

Tổng 99 điểm. Với 3 điểm mỗi ngày, ván 7 ngày kiếm tối đa 18 điểm (khoảng 12 đến 14 thực tế vì ván thua sớm kết thúc trước ngày 7). Chỉ mở được một phần cây, đúng ý "chọn nhánh", không "mở hết".

## 3. Nút mở ở ngày nào (trung vị theo ván có mở; trong ngoặc: % ván mở nút đó)

Nguồn điểm "ngày 3", giá nút một nửa, rương chỉ qua cây (n = 1.000 mỗi cách chơi, bot 2.000):

| Nút | Bot thận trọng | Bot liều | Người chơi giỏi (liều) |
|---|---|---|---|
| R1 | ngày 2,2 (84%) | 2,2 (87%) | 2,2 (95%) |
| S1 | 3,0 (83%) | 3,0 (86%) | 3,8 (90%) |
| T1 | 3,6 (79%) | 3,6 (76%) | 5,0 (57%) |
| C1 | 3,9 (75%) | 3,8 (61%) | 3,0 (29%) |
| R3 Tổng Hợp | 6,9 (0%) | 7,0 (1%) | **3,5 (73%)** |
| R2 Rác Quý | 4,9 (37%) | 4,9 (43%) | 4,7 (44%) |
| R4 / R5 | — | — | 4,6 (23%) / 4,8 (21%) |
| M1 | 5,1 (42%) | 5,2 (44%) | 5,8 (48%) |
| M2 | — | — | 6,0 (3%) |
| Số nút mở trung bình | 5,5 | 5,4 | 5,7 |

Người chơi giỏi mở Tổng Hợp sớm vì ưu tiên nút có tác động; bot mua nút rẻ trước nên gần như không bao giờ mở được Tổng Hợp. Điểm kiếm trong ván: 11,4 đến 14,0. Giá nút đủ (bản cũ) chỉ mở 3,3 đến 4,0 nút, rương ở ngày 7.

**Nguồn điểm so sánh** (giá nút một nửa): *Sổ 1/2* chỉ 7,3 đến 10,7 điểm mỗi ván (cây mở 3,4 đến 4,6 nút), kẻ thua sớm hụt nhiều; *nợ 3* 8,1 đến 12,2 điểm, người trả nợ trễ bị phạt đúng hai lần (mất điểm lẫn tiền). *Ngày 3* cho điểm đều nhất, kẻ thua sớm vẫn có điểm của những ngày đã sống. Đề xuất **điểm theo ngày**, cộng thêm điểm Sổ nếu muốn thưởng sưu tập.

## 4. Tỉ lệ thắng (luật soi mới)

| Kịch bản | Bot thận trọng / liều | Người chơi giỏi: tối đa (liều + giám định) | Thận trọng + giám định |
|---|---|---|---|
| Nền, 4 rương cơ bản | 14,3 / 14,3 (n = 3.000) | 43,9 | 37,9 |
| **Vĩnh viễn:** 4 mở khóa, có từ ngày 1 | 12,4 / 12,8 | 44,3 | 39,9 |
| Vĩnh viễn: 4 mở khóa, shop tối đa 4 (cùng cấu hình dòng trên; chênh bot là nhiễu, n = 6.000) | 14,1 / 13,4 | 44,3 | 39,9 |
| Vĩnh viễn: 4 mở khóa + 2 chủ đề | 12,3 / 12,5 | 42,8 | 35,7 |
| Vĩnh viễn: 4 + 2 chủ đề, shop tối đa 4 | 12,0 / 12,2 | 42,7 | 37,5 |
| Vĩnh viễn: 4 + 2 chủ đề + M2 | 13,3 / 13,3 | 45,2 | 39,5 |
| **Trong ván A** (rương qua cây, không chủ đề) | 14,7 / 15,3 | **46,0** | 38,6 |
| Trong ván A + M2 cộng vốn | (như A: M2 chỉ 3% ván) | 46,0 | 38,7 |
| Trong ván B2 (chủ đề khi Sổ ≥ 6, 10) | 13,8 / 15,1 | 44,5 | 38,7 |
| Trong ván B (chủ đề khi Sổ ≥ 10, 16) | 14,2 / 15,2 (n = 2.000) | 46,1 (n = 1.000) | 38,4 |
| M2 cộng vốn từ ngày 1 (cận trên): nền | 15,3 / 15,2 | 45,5 | 39,5 |
| M3 Rương Đen từ ngày 1 | 7,9 / 7,8 | 38,5 | 33,1 |

Ghi chú:
- Người chơi giỏi 46,0% là **trường hợp sát trần nhất** (kịch bản A). Biên chỉ khoảng 1 điểm, trong nhiễu. Không vượt. Muốn rộng hơn: chủ đề có mặt (B2: 44,5%) hoặc đừng cho Tổng Hợp ở nút rẻ nhất.
- **Khối ván cuối của chuỗi không còn tồn tại**: không có chuỗi, mỗi ván làm lại từ đầu, nên "trôi lên theo chuỗi" không xảy ra.
- Một lần chạy đầu trên mã soi cũ (trước hợp nhất commit luật khó hơn) cho bot 18,8% và người chơi giỏi 50,4%. Kết quả đó **không dùng**; chạy lại toàn bộ trên luật soi mới nhất.

## 5. M1 đến M4 trong ván

| Mở khóa | Tác động đo được | Đề xuất |
|---|---|---|
| M1 Chọn gia truyền | GT1 hơn GT2 dưới 1 điểm; mở ở ngày 5 đến 6 (khoảng 45% ván) | **Giữ**, tác động nhỏ |
| M2 Vốn đầu ván | Trong ván: gần như không mở (3%), tác động 0. Từ ngày 1: +1 điểm bot, +1,6 đến 2,4 điểm người chơi giỏi (dưới trần ở luật soi mới) | **Đổi**: "vốn đầu ván" không còn nghĩa khi mở giữa ván. Hoặc bỏ phần thưởng vốn (phần thưởng không buff), hoặc thành khoản vốn khi mở (1.000.000 đ, đo ở trên: không tác động) |
| M3 Rương Đen từ ngày 1 | Mở giữa ván vô nghĩa (Rương Đen đã xuất hiện từ ngày 4). Là thử thách chỉ chọn lúc **vào ván** | **Bỏ khỏi cây** hoặc thành tùy chọn đầu ván ngoài cây |
| M4 Bí Khế quen mặt | Chỉ là thông tin | **Giữ**, điều kiện "2 tạo tác trong Sổ của ván" |

## 6. Câu hỏi mở cho chủ dự án

1. **Nguồn điểm trong ván:** mỗi ngày, mỗi mục Sổ mới, hay trả nợ đúng hạn? Đề xuất mỗi ngày (3 điểm), cộng thưởng Sổ nếu muốn khuyến khích sưu tập. Đây cũng là câu 1 của spec.
2. **Rương mở khóa và chủ đề vĩnh viễn hay chỉ trong ván?** Cả hai đều dưới trần 47% (vĩnh viễn tối đa 44,3%; trong ván 46,0%). Nếu vĩnh viễn, nút R2 đến R5 của cây thừa (rương đã có). Nếu chỉ trong ván, rương mở muộn, giá trị của cây nằm ở đây. Sổ Sưu Tập giữ giữa các ván hay không cũng chưa chốt (mô phỏng giả định Sổ làm lại mỗi ván).
3. **Nút Câu chuyện mở trong ván nghĩa là gì?** Mô phỏng dùng điều kiện "qua ngày X". Ván chỉ 7 ngày nên 4 nút có thể thành 2 nút. Việc của Claude 1.
4. **M2 và M4:** giữ phần cộng vốn (dưới luật soi mới không phá trần), đổi sang phần thưởng không buff, hay bỏ? M3 bỏ khỏi cây?
5. **Giá cây:** giá nửa bản cũ (tổng 99 điểm, 3 điểm mỗi ngày) để mỗi ván mở khoảng 5 đến 6 nút. Có muốn cây có thể mở gần hết trong một ván (thêm điểm, ví dụ 5 mỗi ngày) hay chỉ vài nút?
6. **Bảng Tổng Hợp** (hàng giả 10%, Thường 43%, Hiếm 25%, Rất hiếm 13%) được đề xuất cho luật soi cũ; bảng gốc phần 2 (hàng giả 20%) chưa đo lại dưới luật soi mới. Việc còn lại.
7. **Bot thường 12% đến 14%** khi nhiều rương có từ ngày 1, dưới mục tiêu 15%: nguồn gốc là luật soi (nền 14,3%). Nếu cần kéo lên, chỉnh ở phần 3.
8. **Thành tựu mở rương chủ đề** vẫn chưa chốt (mô phỏng giả định theo số mục Sổ của ván).

## 7. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_tree          # test khóa, khoảng 25 giây
nohup python3 tree_run.py 1000 > r.log &  # mục 3 và 4 (kịch bản trong ván), khoảng 15 phút
nohup python3 tree_final.py perm 1500 > p.log &   # mục 4, kịch bản vĩnh viễn, khoảng 10 phút
```

Mã: `QA/tree_run.py` (`RUN_TREE` cây, `SOURCES` nguồn điểm, `STATE` tham số, `run_static()` đo một cấu hình, `CONFIGS` bộ số của báo cáo), `QA/tree_final.py` (chế độ `perm`), `QA/tree_sim.py` (lõi dùng chung; phần chuỗi ván đã bỏ). Các số n = 3.000 ở mục 4 chạy bằng một tập lệnh tạm cùng `run_static()` và `tree_sim.static()` với hạt giống 900000.
