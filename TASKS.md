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
- [ ] **Claude 2** — So sánh chiến thuật thận trọng, săn combo và liều lĩnh trên seed tái lập. Nghiệm thu khi có tỷ lệ thắng, ngày hụt tiền, tần suất combo và đề xuất có số liệu.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/Sprint3.md](Docs/Balance/Sprint3.md). Chạy `cd QA && python3 sprint3_report.py`. 10.000 seed, dùng nội dung thật của Claude 1 (nhánh `claude/relaxed-cerf-wgnu7l`).
  - **Kết quả chính:** nội dung gốc thắng 0% ở cả ba chiến thuật (H2, H3, Rương Đen lỗ kỳ vọng). Với giá nền đề xuất: thận trọng 41,9%, săn combo 19,3%, liều 48,9%. Tạo tác hiện net âm (chi 996 xu, thu thêm 170 xu).
  - **Cập nhật theo mục tiêu "ván khó, nhiều đánh đổi" của chủ dự án:** đề xuất giá nền ở Sprint 3 quá dễ, đã thay bằng [Docs/Balance/DoKho.md](Docs/Balance/DoKho.md) (ba chiến thuật thắng 21,8% / 21,8% / 24,3%, chênh 2,5 điểm; mục tiêu mã hóa trong `QA/test_difficulty.py`).
  - **Đã đo đề xuất của Claude 1** (TC3, CB3, GT1 mới, chứng nhận giám định, đấu giá k 1,40): xem [Docs/Balance/DoLaiDeXuat.md](Docs/Balance/DoLaiDeXuat.md) và `QA/test_proposal.py`. Cần chỉnh: phí giám định 15%, CP1 +5% và CB1 +10%, hạ giá CP3 và TC3 còn 500 kèm CB3 +15% giá bán, đấu giá k 1,16. Với gói này bot thường 15% đến 17%, bot đầu tư 26%, người giám định chọn lọc 24% đến 32%.
  - **Cần chủ dự án duyệt:** bảng giá nền ván khó và hệ số bán món chưa giám định. **Cần Claude 1:** cho CP1 và CB1 áp dụng mọi hiện vật (nhân 1,25), cập nhật nội dung theo giá mới, làm sống lại các lựa chọn đang chết (giám định, cầm cố, giảm phạt nợ, lối ra sau khi thiếu tiền) theo mục 5 của DoKho.md.
  - **Cập nhật sau gói đã chốt (commit d21818e của Claude 1):** GT1 ba điểm khớp giả định đã đo, không cần đo lại. Đã đo phạt rủi ro O-01 bằng đủ chín chiến thuật: G1 đạt mục đích (liều có giám định chỉ hơn thận trọng có giám định 1,9 điểm, trước đó 8,0) nhưng đẩy hướng đầu tư lên 31% đến 38% và trần lên 38,5%. Khuyến nghị duyệt G1 và nâng trần test lên 41. Số liệu: mục 10 của [Docs/Balance/DoLaiDeXuat.md](Docs/Balance/DoLaiDeXuat.md). **Chờ chủ dự án duyệt mục G**, rồi bật `RISK_PENALTY_APPROVED` trong `QA/test_risk_penalty.py`.
  - **Claude 1 báo:** chủ dự án đã duyệt mục G (phạt rủi ro) và cả gói. Việc còn lại của Claude 2: bật `RISK_PENALTY_APPROVED`, nâng trần lên 41, rồi nghiệm thu mục này.
  - **Đã đo kỹ năng đọc manh mối** ([Docs/Balance/KyNangManhMoi.md](Docs/Balance/KyNangManhMoi.md), `QA/clue_sim.py`): bot đọc đúng và chỉ mua hộp lời thắng 57% đến 62% so với 23% đến 38% khi không đọc, quá mạnh so với mục tiêu ván khó. Đòn bẩy hiệu quả nhất là giá hộp đi theo manh mối (λ khoảng 0,75 đến 0,9); bắt buộc mua mỗi ngày và mua thêm manh mối 30 xu không có tác dụng. **Cần Claude 1:** chốt cách manh mối gắn với hiện vật (D-26) và cách thể hiện giá theo manh mối, rồi Claude 2 đo lại.
  - **Claude 1 trả lời D-26, D-27, D-28 và T-04 (chờ chủ dự án duyệt mục J của [DieuChinhSauCanBang.md](Docs/Content/DieuChinhSauCanBang.md)):** manh mối gắn với hiện vật bên trong (H1 và H2 có 2 đúng 1 sai, H3 và Rương Đen 1 đúng 2 sai), kệ hộp M = 1 (mỗi lượt mua bày 1 hộp mỗi loại, chọn 1 hoặc dừng), giá niêm yết đi theo manh mối miễn phí đầu tiên với λ = 0,75, mua thêm manh mối 15 xu. Đo bằng `QA/clue_rules.py` trên `clue_sim.py`: bot đọc tối ưu 33,9 / 38,3 / 43,5% (so với 57–62% trước đó). Dữ liệu nguồn: `Docs/Content/data/content.json`. **Cần Claude 2** đo lại với đúng luật kệ hộp và đưa vào test. Ngoài ra `QA/economy_model.py` còn `INVENTORY_CAPACITY = 10`, gói đã duyệt là kho 8 ô và bàn mở 3 hộp.
- [ ] **Codex** — Sửa lỗi cốt lõi, chạy hồi quy và đóng gói bản PC. Nghiệm thu khi bản chạy chơi được từ đầu đến kết quả ngày 7 với các ca biên đã kiểm.
- [ ] **Antigravity** — Hoàn thiện tương tác, âm thanh, ánh sáng, phản hồi mở hộp và khả năng đọc UI. Nghiệm thu qua một ván chơi trên bản PC.

## Sprint 4 — Mở rộng sau nghiệm thu vertical slice

- [ ] **Codex** — Tích hợp luật đã duyệt, lưu giao dịch và tiến trình meta. Nghiệm thu khi các luồng cập nhật nhất quán và khôi phục đúng sau tải lại.
  - *Bàn giao từ mục đã nghiệm thu:* đặc tả hành vi nằm ở `QA/services_model.py` và `QA/test_services.py` (ca chống giao dịch lặp, kèm test fuzz), luật ở [Docs/Content/DichVuVaMeta.md](Docs/Content/DichVuVaMeta.md), báo cáo ở [Docs/Balance/Sprint4.md](Docs/Balance/Sprint4.md). Yêu cầu bắt buộc: kết quả ngẫu nhiên xác định theo (seed, ngày, lượt) hoặc mã hiện vật, mọi thao tác tiền có mã giao dịch lưu cùng bản lưu, mỗi hiện vật chỉ ở đúng một nơi.
- [ ] **Antigravity** — Dựng NPC, quầy dịch vụ, giao diện và phản hồi biến cố. Nghiệm thu khi người chơi hoàn thành từng luồng trong cảnh chợ.
  - *Lưu ý giao diện:* tiền đấu giá về vào đầu ngày kế tiếp và không dùng trả nợ ngày gửi, UI cần ghi rõ để người chơi không thấy bị phạt oan ([DichVuVaMeta.md](Docs/Content/DichVuVaMeta.md) mục 2). Hiển thị tỷ lệ hàng giả của từng hộp khi xem hộp.
