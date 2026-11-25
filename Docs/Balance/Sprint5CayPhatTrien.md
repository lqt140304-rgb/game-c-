# Sprint 5 phần 4: cây kỹ năng trong ván (Claude 2)

Viết lại ngày 2026-11-25 sau hai thay đổi của chủ dự án: (1) **cây kỹ năng chỉ dùng trong ván, không có tiến trình vĩnh viễn** ([ruong-suu-tap](../../docs/superpowers/specs/2026-11-23-ruong-suu-tap-design.md) mục 4, [DaChot](../DaChot.md)); (2) luật soi lỗi mới và khó hơn (phạt sót 10%, trần trả giá 1,05, nhà vá lỗi 12% / 30%, dấu lạ 15%; [Sprint5SoiLoi.md](Sprint5SoiLoi.md)). **Mọi con số dưới đây là ĐỀ XUẤT hoặc GIẢ ĐỊNH của Claude 2, chưa chốt.** Điều cần chủ dự án quyết ở mục 6.

> **Phần đã bỏ.** Bản trước đo *chuỗi 45 ván* (điểm cuối ván tích lũy, "nút đầu ván 2, cả cây ván 37", tỉ lệ thắng theo khối 5 ván, M2 đẩy người chơi giỏi lên 50,6%). Toàn bộ không còn ý nghĩa vì không có tiến trình giữa các ván, và đo trên luật soi cũ. Mã chuỗi vẫn nằm ở `QA/tree_sim.py` (`chain`, đã ghi chú là bỏ), không còn test. Điểm thắng 8 / thua 2 và giá cây 200 điểm của bản trước bị thay bằng mục 2.

> **Tiền tệ:** số tiền ghi bằng đồng Việt Nam. Mô phỏng tính theo đơn vị 10.000 đ (`CASH` 100 = 1.000.000 đ), in qua `QA/currency.py`. "Điểm" là điểm cây, không phải tiền.

## Kết luận ngắn

Luật soi dùng để đo là bản mới nhất của nhánh (phạt sót 10%, trần 108%, nhà vá 12% / 30%, dấu lạ 15%; [Sprint5SoiLoi.md](Sprint5SoiLoi.md)). Nền 4 rương cơ bản: bot thường **15,0% / 16,9%**, người chơi tính toán hoàn hảo **44,1%** (liều + giám định, n = 3.000). **Biên dưới trần 47% chỉ khoảng 3 điểm**, và đó là toàn bộ ngân sách cho cây.

1. **Một ván 7 ngày chỉ mở được một phần cây.** Giá nút "một nửa" (cây dễ): khoảng 5 đến 6 nút trên 19, nút đầu ngày 2, Rác Quý và Tổng Hợp ngày 4 đến 5. Giá "đủ" (cây khó, theo yêu cầu "cây khó mở hơn"): 3 đến 4 nút, nút đầu ngày 2, Rác Quý và Tổng Hợp ngày 7 (20% đến 33% ván), M2 không bao giờ.
2. **Cây dễ làm người chơi hoàn hảo vượt trần.** Rương mở sớm giữa ván (không chủ đề, n = 3.000): **48,0%** với giá nửa, 47,7% với giá 75%. Bot vẫn 15,1% / 17,0%. **Cây khó (giá đủ) giữ ở 44,6%**, bot 15,0% / 16,9%, tức gần như bằng nền. Vậy "cây khó mở hơn" đúng là hướng an toàn cho trần.
3. **Thưởng hậu hĩnh có trần.** Thử thưởng vốn khi mở nút đầu tiên (R1, ngày 2, 84% đến 95% ván), cây giá đủ: **+1.000.000 đ: 47,0%** (sát trần), **+2.000.000 đ: 49,1%**, **+3.000.000 đ: 51,8%**. Mỗi 1.000.000 đ thưởng sớm cộng khoảng 2,4 điểm cho người hoàn hảo. Bot: 16,6% / 17,6%, 17,3% / 18,8%, 18,5% / 19,7%, tức **bot luôn nằm trong 15% đến 20%**; chỉ người hoàn hảo bị chặn. **Mức tối đa đề xuất: tổng thưởng có tác động lên tiền hoặc rương không quá khoảng 1.000.000 đ quy đổi** (mỗi nút thưởng lớn phải thay bằng thưởng không buff: cảnh, bộ truyện, phản ứng thần lực, trang Sổ). Nếu muốn thưởng lớn hơn, phải hạ chỗ khác (ví dụ trần trả giá) và đo lại.
4. **Kịch bản rương vĩnh viễn (mọi rương mở khóa có từ ngày 1) cũng sát trần:** người chơi giỏi 46,8% (4 mở khóa), 45,0% đến 46,0% (4 mở khóa + 2 chủ đề, shop tối đa 4), 47,8% khi thêm M2. Bot 13,7% đến 15,3%. Thêm chủ đề làm bot tụt về 13,7% (dưới 15%) nhưng hạ người chơi giỏi.
5. **Thêm rương mở khóa có từ ngày 1 đẩy người chơi hoàn hảo lên sát 47% và bot xuống 14%** dù cây không liên quan. Nếu chủ dự án chọn rương vĩnh viễn, nên chặn ở đó (xem [Sprint5RuongMoKhoa.md](Sprint5RuongMoKhoa.md)).
6. **M2 (vốn +1.000.000 đ) cộng khoảng 1,5 đến 2,5 điểm cho người hoàn hảo** (nền 45,8% lên 47,6% ở n = 1.500; 4 mở khóa + 2 chủ đề 45,3% lên 47,7%): đủ để chạm trần. Nếu M2 giữ thưởng vốn thì nó chiếm gần hết ngân sách ở ý 3. Giữ hay bỏ: mục 6, câu 4.
7. **Luật shop "tối đa 4 rương mở khóa hoặc chủ đề mỗi ngày"** (chủ dự án chốt) đo dưới luật soi mới: không đổi gì khi chỉ có 4 mở khóa; với thêm 2 chủ đề, người chơi giỏi 46,0% so với 45,0% khi bày hết (trong nhiễu), bot 13,7% cả hai. Phương án "tối đa 2" không cần.
8. **Bot dưới 15% khi nhiều rương từ ngày 1 (13,7% đến 14,4%)**, nhưng với cây khó trong ván bot ở nền (15,0% / 16,9%). Vấn đề của bot là chuyện rương vĩnh viễn, không phải cây.

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

19 nút (bỏ M3 khỏi cây, mục 5), giá và điều kiện quy về một ván 7 ngày. Giá tính bằng điểm: bảng này là **"giá đủ" (cây khó, đề xuất)**; **"giá nửa" (cây dễ) chia đôi mọi giá** (R1 2, R3 3, M2 4) và làm người hoàn hảo vượt trần:

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

Tổng 99 điểm. Với 3 điểm mỗi ngày, ván 7 ngày kiếm tối đa 18 điểm (khoảng 12 đến 14 thực tế vì ván thua sớm kết thúc trước ngày 7). Chỉ mở được 3 đến 4 nút (giá đủ), đúng ý "chọn nhánh, không mở hết".

## 3. Nút mở ở ngày nào (trung vị theo ván có mở; trong ngoặc: % ván mở nút đó)

Nguồn điểm "ngày 3", **giá nửa (cây dễ)**, rương chỉ qua cây (n = 1.000 mỗi cách chơi, bot 2.000). Với **giá đủ (bảng mục 2)**, nút đầu vẫn ngày 2 (84% đến 95%), Rác Quý và Tổng Hợp ngày 7 (22% đến 33%), cây mở 3,3 đến 4,1 nút:

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

Người chơi giỏi mở Tổng Hợp sớm vì ưu tiên nút có tác động; bot mua nút rẻ trước nên gần như không bao giờ mở được Tổng Hợp. Điểm kiếm trong ván: 11,4 đến 14,0. 

**Nguồn điểm so sánh** (giá nút một nửa): *Sổ 1/2* chỉ 7,3 đến 10,7 điểm mỗi ván (cây mở 3,4 đến 4,6 nút), kẻ thua sớm hụt nhiều; *nợ 3* 8,1 đến 12,2 điểm, người trả nợ trễ bị phạt đúng hai lần (mất điểm lẫn tiền). *Ngày 3* cho điểm đều nhất, kẻ thua sớm vẫn có điểm của những ngày đã sống. Đề xuất **điểm theo ngày**, cộng thêm điểm Sổ nếu muốn thưởng sưu tập.

## 4. Tỉ lệ thắng (luật soi mới nhất, trần 108%)

Giỏi = liều + giám định, cách chơi cao nhất. Bot = thận trọng / liều. n = 3.000 (bot 4.000 đến 6.000) với hạt giống 900000 trừ khi ghi khác; sai số khoảng ±0,9 điểm ở 45%, ±0,6 ở 15%.

| Kịch bản | Bot | Giỏi | Thận trọng + giám định |
|---|---|---|---|
| Nền, 4 rương cơ bản | 15,0 / 16,9 | **44,1** | 37,7 |
| **Trong ván, cây khó (giá đủ), không chủ đề** | 15,0 / 16,9 | **44,6** | (n = 1.000) 37,6 |
| Trong ván, giá 75% | 15,1 / 17,0 | 47,7 | |
| **Trong ván, cây dễ (giá nửa), không chủ đề** | 15,1 / 17,0 | **48,0** | 39,8 |
| Trong ván, cây dễ + chủ đề mở khi Sổ ≥ 6, 10 | 15,3 / 16,8 | 46,9 | 40,4 |
| Cây khó + thưởng 1.000.000 đ khi mở R1 | 16,6 / 17,6 | **47,0** | |
| Cây khó + thưởng 2.000.000 đ | 17,3 / 18,8 | 49,1 | |
| Cây khó + thưởng 3.000.000 đ | 18,5 / 19,7 | 51,8 | |
| Vĩnh viễn: 4 mở khóa | 15,2 / 15,3 (shop tối đa 4) | 46,8 | 41,7 |
| Vĩnh viễn: 4 mở khóa + 2 chủ đề, shop tối đa 4 | 13,7 / 13,7 | 46,0 | 38,6 |
| Vĩnh viễn: 4 + 2 chủ đề, bày hết | n/a | 45,0 | 37,7 |
| Vĩnh viễn: 4 + 2 chủ đề + M2 cộng vốn | n/a | 47,8 | 42,1 |
| M2 cộng vốn từ ngày 1, nền (n = 1.500, hạt 0) | 17,0 / 17,1 | 47,6 | 42,0 |
| M3 Rương Đen từ ngày 1 (n = 1.500) | 8,6 / 9,2 | 41,5 | 34,9 |
| M1 chọn GT1 (n = 1.500) | 16,4 / 15,5 | 46,6 | 40,9 |

Ghi chú:
- **Vượt trần có thật ở hai chỗ:** cây dễ (48,0%) và cây khó cộng thưởng từ 2.000.000 đ trở lên (49,1% và 51,8%). Không giấu: nếu chủ dự án muốn cây dễ mở hoặc thưởng hậu hĩnh, trần 47% bị phá, và phải đổi một thứ khác (xem mục "Kết luận" ý 3).
- Các số n = 1.500 hạt giống 0 có nhiễu lớn hơn: ví dụ "+ 4 mở khóa" cho 50,7% ở hạt giống 0 nhưng 46,8% ở n = 3.000 hạt giống 900000. Dùng số n = 3.000.
- **Khối ván cuối của chuỗi không còn tồn tại**: không có chuỗi, mỗi ván làm lại từ đầu.
- Hai lần chạy trước trên các bản luật soi trung gian (trần 105% rồi 103%, nhà vá 12% / 30% rồi 15% / 35%) cho số khác 1 đến 4 điểm; **bỏ**, dùng bảng trên. Luật soi còn có thể đổi, chạy lại bằng lệnh ở mục 7.

## 5. M1 đến M4 trong ván

| Mở khóa | Tác động đo được | Đề xuất |
|---|---|---|
| M1 Chọn gia truyền | GT1 hơn GT2 dưới 1 điểm; mở ở ngày 5 đến 6 (khoảng 45% ván) | **Giữ**, tác động nhỏ |
| M2 Vốn đầu ván | Trong ván: gần như không mở (3%), tác động 0. Từ ngày 1: +1 điểm bot, +1,5 đến 2,5 điểm người chơi giỏi (chạm trần 47,6%) | **Đổi**: "vốn đầu ván" không còn nghĩa khi mở giữa ván. Hoặc bỏ phần thưởng vốn (phần thưởng không buff), hoặc thành khoản vốn khi mở (1.000.000 đ, đo ở trên: không tác động) |
| M3 Rương Đen từ ngày 1 | Mở giữa ván vô nghĩa (Rương Đen đã xuất hiện từ ngày 4). Là thử thách chỉ chọn lúc **vào ván** | **Bỏ khỏi cây** hoặc thành tùy chọn đầu ván ngoài cây |
| M4 Bí Khế quen mặt | Chỉ là thông tin | **Giữ**, điều kiện "2 tạo tác trong Sổ của ván" |

## 6. Câu hỏi mở cho chủ dự án

1. **Nguồn điểm trong ván:** mỗi ngày, mỗi mục Sổ mới, hay trả nợ đúng hạn? Đề xuất mỗi ngày (3 điểm), cộng thưởng Sổ nếu muốn khuyến khích sưu tập. Đây cũng là câu 1 của spec.
2. **Rương mở khóa và chủ đề vĩnh viễn hay chỉ trong ván?** Cả hai đều dưới trần 47% (vĩnh viễn tối đa 44,3%; trong ván 46,0%). Nếu vĩnh viễn, nút R2 đến R5 của cây thừa (rương đã có). Nếu chỉ trong ván, rương mở muộn, giá trị của cây nằm ở đây. Sổ Sưu Tập giữ giữa các ván hay không cũng chưa chốt (mô phỏng giả định Sổ làm lại mỗi ván).
3. **Nút Câu chuyện mở trong ván nghĩa là gì?** Mô phỏng dùng điều kiện "qua ngày X". Ván chỉ 7 ngày nên 4 nút có thể thành 2 nút. Việc của Claude 1.
4. **M2 và M4:** giữ phần cộng vốn (dưới luật soi mới không phá trần), đổi sang phần thưởng không buff, hay bỏ? M3 bỏ khỏi cây?
5. **Giá cây:** "giá đủ" (bảng mục 2 chia 1; tổng 99 điểm, 3 điểm mỗi ngày) mở 3 đến 4 nút mỗi ván và giữ trần; "giá nửa" mở 5 đến 6 nút nhưng vượt trần. Chủ dự án có chọn cây khó (giá đủ) không, và có chấp nhận thưởng tối đa khoảng 1.000.000 đ quy đổi?
6. **Bảng Tổng Hợp** (chưa đo lại trên luật soi mới) (hàng giả 10%, Thường 43%, Hiếm 25%, Rất hiếm 13%) được đề xuất cho luật soi cũ; bảng gốc phần 2 (hàng giả 20%) chưa đo lại dưới luật soi mới. Việc còn lại.
7. **Bot thường 13,7% đến 14,4%** khi nhiều rương có từ ngày 1, dưới mục tiêu 15% (chỉ khi rương vĩnh viễn; cây khó trong ván giữ 15,0% / 16,9%).
8. **Thành tựu mở rương chủ đề** vẫn chưa chốt (mô phỏng giả định theo số mục Sổ của ván).

## 7. Cách tái hiện

```bash
cd QA
python3 -m unittest -v test_tree          # test khóa, khoảng 25 giây
nohup python3 tree_run.py 1000 > r.log &  # mục 3 và 4 (kịch bản trong ván), khoảng 15 phút
nohup python3 tree_final.py perm 1500 > p.log &   # mục 4, kịch bản vĩnh viễn, khoảng 10 phút
```

Mã: `QA/tree_run.py` (`RUN_TREE` cây, `SOURCES` nguồn điểm, `STATE` tham số, `run_static()` đo một cấu hình, `CONFIGS` bộ số của báo cáo), `QA/tree_final.py` (chế độ `perm`), `QA/tree_sim.py` (lõi dùng chung; phần chuỗi ván đã bỏ). Các số n = 3.000 ở mục 4 chạy bằng một tập lệnh tạm cùng `run_static()` và `tree_sim.static()` với hạt giống 900000.
