# Checklist công việc — CHỢ ĐỒ LẠ

File này chỉ giữ các việc **chưa nghiệm thu**. Mỗi mục có một agent phụ trách và đầu ra kiểm tra được. Agent phụ trách gửi bằng chứng qua PR, bản chạy, video hoặc kết quả kiểm thử phù hợp. Claude 1 rà luật, Claude 2 kiểm tra kinh tế và ca biên, Codex rà tích hợp, chủ dự án nghiệm thu. Agent nộp việc ghi trạng thái ngay dưới mục của mình để các agent khác biết. Khi đã nghiệm thu, **xóa hẳn mục việc cùng các dòng mô tả thuộc mục đó khỏi file**. Lịch sử Git hoặc PR giữ bằng chứng hoàn thành.

Claude 1 sở hữu thiết kế và nội dung. Claude 2 sở hữu cân bằng và QA. Antigravity sở hữu cảnh, prefab, tương tác 3D và UI. Codex sở hữu gameplay, dữ liệu, lưu ván và kiểm thử tự động. Codex và Antigravity thống nhất giao diện dữ liệu trước khi nối gameplay với cảnh.

## Sprint 0 — Nền móng

Đã nghiệm thu toàn bộ.

## Sprint 1 — Một ngày chơi First-Person

- [ ] **Codex** — Triển khai tiền, hộp, kho, định giá mẫu, bán hàng, kết thúc ngày và kỳ thu nợ. Nghiệm thu khi giao dịch hợp lệ cập nhật một lần và lượt mua đặt lại đúng ngày.
  - *Bàn giao từ các mục đã nghiệm thu:* đặc tả hành vi nằm ở `QA/test_economy.py` và `QA/economy_model.py` (kết quả tái hiện ở [Docs/Balance/Sprint1.md](Docs/Balance/Sprint1.md)). Cần chặn lỗi D-01: bấm đúp "kết thúc ngày" không được thu nợ hai lần. Hợp đồng dữ liệu còn thiếu các lệnh và trường của luật mới, xem [Docs/Content/YeuCauDuLieu.md](Docs/Content/YeuCauDuLieu.md). Số liệu nạp từ `Docs/Content/data/content.json`.
  - **Codex báo (local, chưa đẩy lên GitHub):** lõi gameplay Sprint 1 trên nhánh `codex/sprint-1` (kệ hộp và manh mối, mua, mở, soi, bán, bàn 3 hộp và kho 8 ô, thu nợ cuối ngày, tự lưu chống lặp giao dịch), kiểm thử C# và 25 test dữ liệu đạt. Còn chờ nối cảnh và xác minh Play Mode. **Chưa nghiệm thu** vì mã chưa lên GitHub để người khác chạy lại. Cần đẩy nhánh và mở pull request (gộp sau pull request #1 vì dùng `content.json`).
  - **Claude 1 chốt câu hỏi "thanh toán phạt cũ trước nợ gốc":** không tách phạt và gốc. Nợ tồn là **một con số duy nhất**, tiền trả trừ vào tổng, và phạt 20% tính trên **toàn bộ nợ còn lại** (nợ tồn mới bằng nợ còn lại nhân 1,2, làm tròn lên). Cách này khớp mô hình `QA/economy_model.py` và ca kiểm thử mới số 8 trong GDD mục 2 (phải trả 410, chỉ trả 5 thì nợ tồn 486, không phải 485). Nếu Codex đang tách hai phần thì sẽ lệch 1 xu khi trả ít hơn phần phạt cũ. Dữ liệu: `rules.penalty_base` trong `content.json`.
- [ ] **Antigravity** — Dựng quầy, bàn mở, cầm xoay hiện vật và UI giao dịch. Nghiệm thu khi người chơi tự đi hết vòng từ chọn hộp đến nhận tiền bán trong Play Mode.

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
