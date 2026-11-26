# Checklist công việc — CHỢ ĐỒ LẠ

File này chỉ giữ các việc **chưa nghiệm thu**. Mỗi mục có một agent phụ trách và đầu ra kiểm tra được. Agent phụ trách gửi bằng chứng qua PR, bản chạy, video hoặc kết quả kiểm thử phù hợp. Claude 1 rà luật, Claude 2 kiểm tra kinh tế và ca biên, Codex rà tích hợp, chủ dự án nghiệm thu. Agent nộp việc ghi trạng thái ngay dưới mục của mình để các agent khác biết. Khi đã nghiệm thu, **xóa hẳn mục việc cùng các dòng mô tả thuộc mục đó khỏi file**. Lịch sử Git hoặc PR giữ bằng chứng hoàn thành.

Claude 1 sở hữu thiết kế và nội dung. Claude 2 sở hữu cân bằng và QA. Antigravity sở hữu cảnh, prefab, tương tác 3D và UI. Codex sở hữu gameplay, dữ liệu, lưu ván và kiểm thử tự động. Codex và Antigravity thống nhất giao diện dữ liệu trước khi nối gameplay với cảnh.

## ƯU TIÊN SỐ 1 — Bản phát hành đầu tiên (v1)

**Trạng thái: ĐÃ DUYỆT bởi chủ dự án (2026-11-26).**

Mục tiêu duy nhất: **một bản PC chơi được từ ngày 1 đến kết quả ngày 7, đủ để bán trên Steam.** Bối cảnh là chợ trời trong một con hẻm thành phố Việt Nam (xem [Docs/DaChot.md](Docs/DaChot.md)). Phạm vi chơi lấy đúng mục 7 của [GDD](Docs/GameDesign/README.md): phòng trọ, khu chợ nhỏ, bàn mở và soi, Dì Mận, Kim Khuy, Anh Bảy, 8 đến 12 hiện vật, 2 gia truyền, 3 loại hộp thường và 1 loại đặc biệt, 6 tạo tác thuộc 2 hệ, 3 combo, 2 điều kiện Bí Khế.

**Luật khóa phạm vi:** mọi ý tưởng mới ghi vào mục "Bản sau" ở cuối phần này, **không đưa vào v1**. Agent không bắt đầu việc nào ngoài danh sách dưới đây cho tới khi v1 phát hành, trừ khi chủ dự án cho phép.

Việc theo thứ tự. Việc sau phụ thuộc việc trước:

1. [ ] **Codex** — Đẩy nhánh `codex/sprint-1` lên GitHub và mở pull request. **Đây là việc chặn mọi thứ khác.** Nghiệm thu khi người khác tải về và chạy được kiểm thử.
2. [ ] **Antigravity** — Hoàn thành map hẻm Việt Nam (đang làm) ở mức nhỏ gọn theo kích thước trong spec market-town: phòng trọ, quầy Dì Mận, bàn mở và soi, chỗ Anh Bảy thu nợ. Nghiệm thu khi đi bộ hết khu chợ trong Play Mode.
3. [ ] **Codex + Antigravity** — Nối lõi gameplay Sprint 1 vào cảnh: mua hộp, mở, soi, bán, kết thúc ngày, thu nợ, tự lưu. Nghiệm thu khi người chơi tự đi hết một ngày trong Play Mode (mục Sprint 1 bên dưới).
4. [ ] **Codex + Antigravity** — Gia truyền, tạo tác, combo, Bí Khế và đủ 7 ngày đến màn kết quả (mục Sprint 2 bên dưới). Nghiệm thu khi chơi được tới kết quả ngày 7 và tải lại đúng trạng thái.
5. [ ] **Antigravity** — Hình ảnh hộp và cảm giác mở hộp: dùng **4 rương mới** (xem bên dưới) cho 3 loại hộp thường và 1 loại đặc biệt. Thêm âm thanh và ánh sáng cơ bản. **Cập nhật 2026-11-26:** chủ dự án thấy 4 rương cũ ghép thử (Việt Nam, Robot, Magic, Mimic) **không hợp** bối cảnh hẻm Việt Nam. Thay bằng **4 rương mới** dựng riêng, đang giao cho một phiên local có Blender:

   | Hộp | Nguồn gốc đồ | Rương mới (đề xuất hình dáng) |
   |---|---|---|
   | H1 Hộp Gỗ Nhỏ (100 xu) | Quá khứ | Thùng gỗ cũ đóng đinh, dây buộc |
   | H2 Hộp Kim Loại Mờ (350 xu) | Hiện đại, Tương lai | Hộp tôn sơn mờ, có tem chữ Việt |
   | H3 Hộp Vải Dày (800 xu) | Thần thoại, Cổ tích | Bao tải vải bố buộc dây rút, có bùa |
   | RD Rương Đen (1.500 xu) | Mọi nhóm, canh bạc | Vali đen cũ, khóa số |

   Đây là ngoại lệ có chủ đích cho luật "không làm thêm rương": chỉ đúng 4 rương này. Chỉ đổi hình ảnh. Giá, tỷ lệ và luật của từng hộp giữ nguyên theo [NoiDungMau.md](Docs/Content/NoiDungMau.md). Antigravity chờ 4 file GLB mới trước khi làm cảnh mở (cảnh mở cũ của 17 rương không dùng lại cho v1).
6. [ ] **Claude 1** — Viết lại lời thoại mở màn, Dì Mận, Kim Khuy và Anh Bảy cho khớp bối cảnh hẻm Việt Nam, giọng Gen Z. Chơi thử một ván thật khi bước 4 xong và gửi danh sách sửa ngắn.
7. [ ] **Claude 2** — Chạy lại mô phỏng cân bằng **chỉ khi** số liệu thay đổi so với bản đã đo. Chốt bộ số cho v1.
8. [ ] **Codex** — Sửa lỗi từ lần chơi thử, đóng gói bản PC (Windows). Nghiệm thu khi chạy được trên máy khác từ đầu đến kết quả ngày 7.
9. [ ] **Chủ dự án** — Trang Steam (phí Steam Direct), ảnh chụp, clip ngắn cảnh mở hộp và cảnh Anh Bảy đòi nợ để đăng TikTok. Bắt đầu đăng clip ngay khi bước 5 có hình.

### Bản sau (không làm trong v1)

- Đấu giá, sòng bạc, cầm cố, giám định, phục chế và các dịch vụ trong Sprint 4.
- Meta progression giữa các ván.
- Toàn bộ Sprint 5: 38 món 5 bậc, rác có công dụng ẩn, soi lỗi lấy bằng chứng, trả giá kiểu Ace Attorney, cây phát triển, thần lực, thành tựu, rương chủ đề mở khóa.
- Các rương chủ đề còn lại ngoài 4 rương dùng cho v1.
- Hướng game hành động cypherpunk ([tạm gác](docs/superpowers/specs/2026-11-26-cypherpunk-hanh-dong-design.md)).

## Sprint 0 — Nền móng

Đã nghiệm thu toàn bộ.

## Sprint 1 — Một ngày chơi First-Person

- [ ] **Codex** — Triển khai tiền, hộp, kho, định giá mẫu, bán hàng, kết thúc ngày và kỳ thu nợ. Nghiệm thu khi giao dịch hợp lệ cập nhật một lần và lượt mua đặt lại đúng ngày.
  - *Bàn giao từ các mục đã nghiệm thu:* đặc tả hành vi nằm ở `QA/test_economy.py` và `QA/economy_model.py` (kết quả tái hiện ở [Docs/Balance/Sprint1.md](Docs/Balance/Sprint1.md)). Cần chặn lỗi D-01: bấm đúp "kết thúc ngày" không được thu nợ hai lần. Hợp đồng dữ liệu còn thiếu các lệnh và trường của luật mới, xem [Docs/Content/YeuCauDuLieu.md](Docs/Content/YeuCauDuLieu.md). Số liệu nạp từ `Docs/Content/data/content.json`.
  - **Codex báo (local, chưa đẩy lên GitHub):** lõi gameplay Sprint 1 trên nhánh `codex/sprint-1` (kệ hộp và manh mối, mua, mở, soi, bán, bàn 3 hộp và kho 8 ô, thu nợ cuối ngày, tự lưu chống lặp giao dịch), kiểm thử C# và 25 test dữ liệu đạt. Còn chờ nối cảnh và xác minh Play Mode. **Chưa nghiệm thu** vì mã chưa lên GitHub để người khác chạy lại. Cần đẩy nhánh và mở pull request (gộp sau pull request #1 vì dùng `content.json`).
  - **Claude 1 chốt câu hỏi "thanh toán phạt cũ trước nợ gốc":** không tách phạt và gốc. Nợ tồn là **một con số duy nhất**, tiền trả trừ vào tổng, và phạt 20% tính trên **toàn bộ nợ còn lại** (nợ tồn mới bằng nợ còn lại nhân 1,2, làm tròn lên). Cách này khớp mô hình `QA/economy_model.py` và ca kiểm thử mới số 8 trong GDD mục 2 (phải trả 410, chỉ trả 5 thì nợ tồn 486, không phải 485). Nếu Codex đang tách hai phần thì sẽ lệch 1 xu khi trả ít hơn phần phạt cũ. Dữ liệu: `rules.penalty_base` trong `content.json`.
- [ ] **Antigravity** — Dựng quầy, bàn mở, cầm xoay hiện vật và UI giao dịch. Nghiệm thu khi người chơi tự đi hết vòng từ chọn hộp đến nhận tiền bán trong Play Mode.
  - *Bàn giao mô hình 3D:* 24 mô hình `.glb` (hộp mù, hiện vật, tạo tác, gia truyền, Bí Khế) đã có ở PR #5, hướng dẫn nhập và bảng ID ở [Docs/Art/BanGiao3D.md](Docs/Art/BanGiao3D.md). Cần Git LFS và gói đọc glTF trước khi dùng.

## Sprint 2 — Tạo tác và ván 7 ngày

- [ ] **Codex** — Nối gia truyền, tier, trang bị, combo, Bí Khế, tiến trình 7 ngày và lưu ván. Nghiệm thu khi có thể chơi tới kết quả và tải lại đúng trạng thái mà không lặp giao dịch.
  - *Bàn giao từ các mục đã nghiệm thu:* đặc tả hành vi cho nợ tồn, combo, Bí Khế, hoãn nợ GT1 và lưu ván nằm ở các test trong `QA/` (xem [Docs/Balance/Sprint2.md](Docs/Balance/Sprint2.md)). Lưu ván chỉ tự động, theo GDD mục 6.
- [ ] **Antigravity** — Dựng Kho Tạo Tác, UI sổ và trang bị, cảnh thu nợ cùng màn kết quả. Nghiệm thu khi người thử đọc được nợ, combo, tiến độ Bí Khế và lý do kết thúc ván.

## Sprint 3 — Cân bằng và nghiệm thu vertical slice

- [ ] **Claude 1** — Chơi thử và rà nhịp, độ rõ thông tin, quyết định giữ tiền hay đầu tư. Nghiệm thu khi có phản hồi từ ván hoàn chỉnh và danh sách điều chỉnh đã duyệt.
  - *Tiến độ:* Đã rà soát trên giấy và sửa 6 vấn đề, xem [Docs/Content/RaSoatGiay.md](Docs/Content/RaSoatGiay.md). **Chưa thể chơi thử thật** vì chưa có bản chạy. Việc này chờ Codex và Antigravity hoàn thành bản chơi được.
  - *Đã duyệt và đồng bộ nội dung (2026-11-22):* gói điều chỉnh ở [Docs/Content/DieuChinhSauCanBang.md](Docs/Content/DieuChinhSauCanBang.md) đã được chủ dự án duyệt, gồm giá nền hệ số 0,85 nhân 1,10, phạt rủi ro theo hộp, phí giám định 15% và chứng nhận +25%, gói tạo tác và combo, GT1 hoãn nợ, kho 8 ô, chỉ lưu tự động. Các file [NoiDungMau.md](Docs/Content/NoiDungMau.md), [GiaTruyenTaoTacBiKhe.md](Docs/Content/GiaTruyenTaoTacBiKhe.md), [DichVuVaMeta.md](Docs/Content/DichVuVaMeta.md) và GDD đã cập nhật theo. **Phần còn lại của mục này: chơi thử một ván thật khi Codex và Antigravity có bản chạy.**
  - *Mục J (luật manh mối và kệ hộp) đã được chủ dự án duyệt (2026-11-22)* và Claude 2 đã đo lại theo đúng luật ([Docs/Balance/KyNangManhMoi.md](Docs/Balance/KyNangManhMoi.md)): người chơi tính hoàn hảo thắng 38% đến 46% (cận trên), chấp nhận được. Đọc manh mối chỉ cộng +2 đến +4 điểm, phần lớn kỹ năng đến từ chọn loại hộp mỗi lượt (+5 đến +17 điểm).
  - **D-28 đã chốt (Claude 1, theo đo của Claude 2):** mua thêm manh mối có thể sai không đáng ở mọi giá. Thay bằng **lời mách của Dì Mận**: trả 15 xu để biết chắc nhóm nguồn gốc thật của hiện vật trong hộp, một lần mỗi hộp, giá niêm yết không đổi (thắng +2,3 đến +2,7 điểm, bot chỉ mua 3 đến 4 lần mỗi ván). **Codex cần đổi `BuyClue`** từ "lộ thêm manh mối kế tiếp" sang "trả về nhóm nguồn gốc thật", tối đa một lần mỗi hộp. **O-04 đã chốt:** để người chơi tự tính, giao diện không hiển thị giá trị kỳ vọng của loại hộp (Antigravity lưu ý). **Việc còn mở:** sổ tay có ghi hiện vật đã gặp theo từng loại hộp hay không, để người chơi có dữ liệu mà tính.
  - **Claude 2 báo (đo sau khi nghiệm thu):** biến cố E3 và việc chọn hộp ([Docs/Balance/BienCoE3.md](Docs/Balance/BienCoE3.md)): biến cố trung tính, nhưng H2 chi phối (66% đến 71% lượt mua) và Rương Đen không ai mua (D-29, D-30). Đã đo cấu hình lời tăng theo giá hộp ([Docs/Balance/LoiTheoGiaHop.md](Docs/Balance/LoiTheoGiaHop.md)): **C3** (lời mỗi hộp 100 / 160 / 220 / 260 xu, giá nền theo bảng trong tài liệu) làm việc chọn hộp phụ thuộc vào vốn và giữ độ khó (bot thường 19% đến 29%). **Chủ dự án đã chọn phương án C (giá hộp dao động ±20% theo ngày)**, không chọn C3 (B) vì C3 làm khoảng cách kỹ năng gần như mất. Luật ở [NoiDungMau.md](Docs/Content/NoiDungMau.md) mục 5.3 và 5.4, dữ liệu ở `shelf.day_price` và `shelf.price_follows_clue.multiplier` trong `content.json`, so sánh ở mục K của [DieuChinhSauCanBang.md](Docs/Content/DieuChinhSauCanBang.md). Bảng giá niêm yết tuyệt đối cũ được thay bằng bảng hệ số nhân giá hôm nay. **Claude 2:** đo lại bằng bộ chiến thuật đầy đủ và các luật dịch vụ, đo biên độ ±15% và ±20%, đo kết hợp hạ giá Rương Đen (ví dụ 1.250); `QA/test_tier_profit.py` (C3) giữ tắt. **Codex:** thêm giá hôm nay (dao động theo seed, ngày, loại hộp, công bố đầu ngày) và đổi giá niêm yết thành giá hôm nay nhân hệ số, thay cho bảng giá tuyệt đối đang dùng ở pull request #2.
- [ ] **Codex** — Sửa lỗi cốt lõi, chạy hồi quy và đóng gói bản PC. Nghiệm thu khi bản chạy chơi được từ đầu đến kết quả ngày 7 với các ca biên đã kiểm.
- [ ] **Antigravity** — Hoàn thiện tương tác, âm thanh, ánh sáng, phản hồi mở hộp và khả năng đọc UI. Nghiệm thu qua một ván chơi trên bản PC.

## Sprint 4 — Mở rộng sau nghiệm thu vertical slice

- [ ] **Codex** — Tích hợp luật đã duyệt, lưu giao dịch và tiến trình meta. Nghiệm thu khi các luồng cập nhật nhất quán và khôi phục đúng sau tải lại.
  - *Bàn giao từ mục đã nghiệm thu:* đặc tả hành vi nằm ở `QA/services_model.py` và `QA/test_services.py` (ca chống giao dịch lặp, kèm test fuzz), luật ở [Docs/Content/DichVuVaMeta.md](Docs/Content/DichVuVaMeta.md), báo cáo ở [Docs/Balance/Sprint4.md](Docs/Balance/Sprint4.md). Yêu cầu bắt buộc: kết quả ngẫu nhiên xác định theo (seed, ngày, lượt) hoặc mã hiện vật, mọi thao tác tiền có mã giao dịch lưu cùng bản lưu, mỗi hiện vật chỉ ở đúng một nơi.
- [ ] **Antigravity** — Dựng NPC, quầy dịch vụ, giao diện và phản hồi biến cố. Nghiệm thu khi người chơi hoàn thành từng luồng trong cảnh chợ.
  - *Lưu ý giao diện:* tiền đấu giá về vào đầu ngày kế tiếp và không dùng trả nợ ngày gửi, UI cần ghi rõ để người chơi không thấy bị phạt oan ([DichVuVaMeta.md](Docs/Content/DichVuVaMeta.md) mục 2). Hiển thị tỷ lệ hàng giả của từng hộp khi xem hộp.

## Sprint 5 — Hướng mở rộng đã bàn với chủ dự án (chờ duyệt bản thiết kế)

Sáu bản thiết kế ở `docs/superpowers/specs/` (rương và sưu tập, mở màn, một ngày, thần lực, soi lỗi và trả giá, tier đồ trong rương) mô tả hướng mới. **Chưa việc nào dưới đây được bắt đầu cho tới khi chủ dự án duyệt bản thiết kế tương ứng.** Hướng nghệ thuật mới: **low-poly, màu trung tính**. Mô hình 3D đã có đủ ở `Assets/_Project/Art/Items/LowPoly/`, hướng dẫn nhập ở [Docs/Art/BanGiao3D.md](Docs/Art/BanGiao3D.md).

- [ ] **Chủ dự án** — Duyệt 6 bản thiết kế. Chốt mã vật phẩm chính thức (mã tạm `R`, `H`, `X`, `L` chưa khớp `content.json`). Duyệt rủi ro bản quyền của quả cầu ngọc (X04), kiếm sáng ba màu (X03) và cục sạc chuột vàng (H04) trước khi phát hành.
- [ ] **Claude 1** — Đưa 38 món theo 5 bậc (Rác, Thường, Hiếm, Rất hiếm, Huyền thoại) vào nội dung: nhóm nguồn gốc, hồ sơ chuẩn, danh sách lỗi thật và lỗi nhiễu, công dụng ẩn của rác, thần lực. Viết lời thoại mở màn, tình huống đời thường, thành tựu. Nghiệm thu khi `Docs/Content` đủ cho Codex nạp.
- [ ] **Claude 2** — Cân bằng hệ mới: giá và tỉ lệ ra bậc của 4 rương cơ bản và các rương mở khóa, tỉ lệ rác, **4 ô tạo tác** (BK1 mở ô thứ 5) và hệ quả lên cộng hưởng, mức phạt và lỗ khi soi sai thật giả, giá ép khi soi thiếu, điểm phát triển của cây. Nghiệm thu khi mô phỏng cho thấy ngày 1 không thua ngay và đường thắng vẫn tồn tại.
- [ ] **Codex** — Dữ liệu và gameplay: loại vật phẩm rác kèm công dụng, 5 bậc, rương mới, cây phát triển hai lớp, soi lỗi (lượt soi, bằng chứng, phân loại), trả giá, thành tựu, hộp ngày 1 sắp sẵn, `equip_slots` từ 3 lên 4. Nghiệm thu khi kiểm thử tự động bao phủ luật mới và lưu/tải giữ nguyên bằng chứng.
- [ ] **Antigravity** — Nhập và chạy thử trong Unity: thêm gói glTFast, `git lfs pull`, thay các `*_Placeholder` bằng mô hình, **chạy thử `HeldItemViewmodel`, `BoxOpenSequence`, `ScrollUnroll`** (mới chỉ kiểm tra cú pháp), giảm poly và nén texture. Dựng giao diện soi lỗi, màn trả giá kiểu Ace Attorney, mini-game buổi sáng, cảnh hồi tưởng, chữ chạy kiểu light novel, giao diện cây phát triển, lớp phủ tình trạng và dấu hiệu hàng giả. Nghiệm thu khi người chơi tự đi hết vòng từ mua rương, mở, soi lỗi, đến trả giá trong Play Mode.
