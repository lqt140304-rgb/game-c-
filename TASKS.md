# Checklist công việc — CHỢ ĐỒ LẠ

File này chỉ giữ các việc **chưa nghiệm thu**. Mỗi mục có một agent phụ trách và đầu ra kiểm tra được. Agent phụ trách gửi bằng chứng qua PR, bản chạy, video hoặc kết quả kiểm thử phù hợp. Claude 1 rà luật, Claude 2 kiểm tra kinh tế và ca biên, Codex rà tích hợp, chủ dự án nghiệm thu. Agent nộp việc ghi trạng thái ngay dưới mục của mình để các agent khác biết. Khi đã nghiệm thu, **xóa hẳn mục việc cùng các dòng mô tả thuộc mục đó khỏi file**. Lịch sử Git hoặc PR giữ bằng chứng hoàn thành.

Claude 1 sở hữu thiết kế và nội dung. Claude 2 sở hữu cân bằng và QA. Antigravity sở hữu cảnh, prefab, tương tác 3D và UI. Codex sở hữu gameplay, dữ liệu, lưu ván và kiểm thử tự động. Codex và Antigravity thống nhất giao diện dữ liệu trước khi nối gameplay với cảnh.

## Sprint 0 — Nền móng

- [ ] **Claude 2** — Lập bảng giả định vốn, nợ, giá, giá bán và xác suất rơi. Nghiệm thu khi có bảng cân bằng ban đầu và ca kiểm thử vượt giới hạn mua, trả nợ sớm, thiếu tiền, bán trùng.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/README.md](Docs/Balance/README.md) có bảng giả định (mục 1–2), mô phỏng khả thi (mục 3) và 16 ca biên BT-01–BT-16 (mục 4).
  - **Cần Claude 1 chốt** (mục 5 của tài liệu): hậu quả thiếu tiền, điều kiện thắng/thua, sức chứa kho, phí giám định/phục chế, bán tức thì hay không. Codex dùng bảng ở mục 2 làm dữ liệu mẫu.
- [ ] **Codex** — Thống nhất hợp đồng dữ liệu hộp, hiện vật, kho, ngày, nợ và thông báo thay đổi với Antigravity. Nghiệm thu khi mỗi bên xác định được dữ liệu đọc và hành động ghi.
- [ ] **Antigravity** — Khởi tạo Unity 6.3 LTS URP và scene thử. Nghiệm thu khi mở đúng phiên bản và chạy Play Mode với cảnh tối thiểu.

## Sprint 1 — Một ngày chơi First-Person

- [ ] **Claude 2** — Kiểm thử dòng tiền mẫu và các ca mua hộp thứ tư, hết tiền, kho đầy, bán trùng. Nghiệm thu khi có kết quả tái hiện và lỗi được ghi nhận.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/Sprint1.md](Docs/Balance/Sprint1.md). Chạy `cd QA && python3 -m unittest -v test_economy` (17 test đạt) và `python3 scenario_cashflow.py`.
  - **Lỗi ghi nhận:** D-01 `kết thúc ngày` bấm đúp thu nợ hai ngày (Codex, Antigravity chặn ở UI). D-02 hộp chưa mở có chiếm kho không, D-03 hậu quả thiếu tiền (Claude 1 chốt). Chi tiết trong tài liệu.
  - **Codex:** `QA/test_economy.py` là đặc tả hành vi, có thể chuyển sang kiểm thử C#.
- [ ] **Codex** — Triển khai tiền, hộp, kho, định giá mẫu, bán hàng, kết thúc ngày và kỳ thu nợ. Nghiệm thu khi giao dịch hợp lệ cập nhật một lần và lượt mua đặt lại đúng ngày.
- [ ] **Antigravity** — Dựng quầy, bàn mở, cầm xoay hiện vật và UI giao dịch. Nghiệm thu khi người chơi tự đi hết vòng từ chọn hộp đến nhận tiền bán trong Play Mode.

## Sprint 2 — Tạo tác và ván 7 ngày

- [ ] **Claude 2** — Kiểm thử lịch nợ, combo, tiến độ Bí Khế và tải lại ván. Nghiệm thu khi có ca trả đúng hạn, chặn trả trước, thiếu tiền, đạt và chưa đạt điều kiện.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/Sprint2.md](Docs/Balance/Sprint2.md). Chạy `cd QA && python3 -m unittest -v test_economy test_sprint2` (52 test đạt). Luật nợ lấy từ đề xuất của Claude 1 (commit `643ae03`). Combo và Bí Khế dùng giá trị tạm vì nội dung Sprint 2 của Claude 1 chưa có.
  - **Cần chủ dự án chọn:** luật nợ tồn làm tỷ lệ thắng giảm mạnh và không ván nào thiếu tiền rồi gỡ lại được (D-05, O-02). Claude 2 đề xuất tăng giá bán 1,3 lần, chưa áp dụng.
  - **Cần Claude 1:** chốt D-02, D-05 đến D-08 và chính sách lưu ván, soạn combo và Bí Khế thật, rà bảng giá nền (hộp H1 cho 4,58 lần giá hộp, Rương Đen 0,89 lần).
  - **Codex:** các test trong `QA/` là đặc tả hành vi cho nợ tồn, combo, Bí Khế và lưu ván.
- [ ] **Codex** — Nối gia truyền, tier, trang bị, combo, Bí Khế, tiến trình 7 ngày và lưu ván. Nghiệm thu khi có thể chơi tới kết quả và tải lại đúng trạng thái mà không lặp giao dịch.
- [ ] **Antigravity** — Dựng Kho Tạo Tác, UI sổ và trang bị, cảnh thu nợ cùng màn kết quả. Nghiệm thu khi người thử đọc được nợ, combo, tiến độ Bí Khế và lý do kết thúc ván.

## Sprint 3 — Cân bằng và nghiệm thu vertical slice

- [ ] **Claude 1** — Chơi thử và rà nhịp, độ rõ thông tin, quyết định giữ tiền hay đầu tư. Nghiệm thu khi có phản hồi từ ván hoàn chỉnh và danh sách điều chỉnh đã duyệt.
  - *Tiến độ:* Đã rà soát trên giấy và sửa 6 vấn đề, xem [Docs/Content/RaSoatGiay.md](Docs/Content/RaSoatGiay.md). **Chưa thể chơi thử thật** vì chưa có bản chạy. Việc này chờ Codex và Antigravity hoàn thành bản chơi được.
  - *Đã trả lời báo cáo Sprint 2–3 của Claude 2:* [Docs/Content/DieuChinhSauCanBang.md](Docs/Content/DieuChinhSauCanBang.md) đề xuất sáu điều chỉnh A–F theo mục tiêu của chủ dự án (không để thắng quá dễ, nhiều đánh đổi). Điểm chính: giá nền của Claude 2 nhân 0,85 (bot thắng khoảng 15%, săn combo khoảng 28%), giữ luật nợ khắt khe vì chừa tiền không giúp gỡ lại đáng kể, thêm chứng nhận giám định +25%, kho 8 ô, chỉ lưu tự động. **Chờ chủ dự án duyệt từng mục A–F**, sau đó Claude 2 chạy lại. Script đo: `QA/difficulty_sweep.py`, `QA/d05_recovery.py`.
- [ ] **Claude 2** — So sánh chiến thuật thận trọng, săn combo và liều lĩnh trên seed tái lập. Nghiệm thu khi có tỷ lệ thắng, ngày hụt tiền, tần suất combo và đề xuất có số liệu.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/Sprint3.md](Docs/Balance/Sprint3.md). Chạy `cd QA && python3 sprint3_report.py`. 10.000 seed, dùng nội dung thật của Claude 1 (nhánh `claude/relaxed-cerf-wgnu7l`).
  - **Kết quả chính:** nội dung gốc thắng 0% ở cả ba chiến thuật (H2, H3, Rương Đen lỗ kỳ vọng). Với giá nền đề xuất: thận trọng 41,9%, săn combo 19,3%, liều 48,9%. Tạo tác hiện net âm (chi 996 xu, thu thêm 170 xu).
  - **Cập nhật theo mục tiêu "ván khó, nhiều đánh đổi" của chủ dự án:** đề xuất giá nền ở Sprint 3 quá dễ, đã thay bằng [Docs/Balance/DoKho.md](Docs/Balance/DoKho.md) (ba chiến thuật thắng 21,8% / 21,8% / 24,3%, chênh 2,5 điểm; mục tiêu mã hóa trong `QA/test_difficulty.py`).
  - **Đã đo đề xuất của Claude 1** (TC3, CB3, GT1 mới, chứng nhận giám định, đấu giá k 1,40): xem [Docs/Balance/DoLaiDeXuat.md](Docs/Balance/DoLaiDeXuat.md) và `QA/test_proposal.py`. Cần chỉnh: phí giám định 15%, CP1 +5% và CB1 +10%, hạ giá CP3 và TC3 còn 500 kèm CB3 +15% giá bán, đấu giá k 1,16. Với gói này bot thường 15% đến 17%, bot đầu tư 26%, người giám định chọn lọc 24% đến 32%.
  - **Cần chủ dự án duyệt:** bảng giá nền ván khó và hệ số bán món chưa giám định. **Cần Claude 1:** cho CP1 và CB1 áp dụng mọi hiện vật (nhân 1,25), cập nhật nội dung theo giá mới, làm sống lại các lựa chọn đang chết (giám định, cầm cố, giảm phạt nợ, lối ra sau khi thiếu tiền) theo mục 5 của DoKho.md.
- [ ] **Codex** — Sửa lỗi cốt lõi, chạy hồi quy và đóng gói bản PC. Nghiệm thu khi bản chạy chơi được từ đầu đến kết quả ngày 7 với các ca biên đã kiểm.
- [ ] **Antigravity** — Hoàn thiện tương tác, âm thanh, ánh sáng, phản hồi mở hộp và khả năng đọc UI. Nghiệm thu qua một ván chơi trên bản PC.

## Sprint 4 — Mở rộng sau nghiệm thu vertical slice

- [ ] **Claude 1** — Chốt luật biến cố, đấu giá, sòng bạc, cầm cố và mở khóa meta. Nghiệm thu khi mỗi hệ có vai trò, luồng và điều kiện hoàn tất.
  - *Tiến độ:* Đã có bản nháp trong [Docs/Content/DichVuVaMeta.md](Docs/Content/DichVuVaMeta.md): 6 biến cố, đấu giá, sòng bạc, cầm cố, 4 mở khóa meta, mỗi hệ có vai trò, luồng, điều kiện hoàn tất và ca kiểm thử.
  - *Đã chỉnh theo khoảng đo của Claude 2:* đấu giá k kỳ vọng 1,16 (đã sửa lại theo `DoKho.md` của Claude 2) và chỉ món từ 1.300 xu, sòng bạc cược 100/250/500 và 2 lượt mỗi ngày, cầm cố thêm tạo tác, biến cố giữ dưới ±20%. Các dịch vụ là nguồn đánh đổi, không phải đường cứu. **Cần Claude 2 chạy lại** khi chủ dự án duyệt giá nền mới, rồi chủ dự án nghiệm thu.
- [ ] **Claude 2** — Đo ảnh hưởng dịch vụ đến thanh khoản, nợ và tiến trình giữa ván. Nghiệm thu khi có ca chống giao dịch lặp và báo cáo cân bằng.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/Sprint4.md](Docs/Balance/Sprint4.md). Chạy `cd QA && python3 -m unittest discover -s .` (97 test đạt) và `python3 sprint4_sim.py`. Dựa trên luật dịch vụ của Claude 1 (commit `d6b597e`, nhánh `claude/relaxed-cerf-wgnu7l`), 45 test bám sát các ca của Claude 1 kèm fuzz chống giao dịch lặp.
  - **Kết quả chính:** đấu giá k = 1,16 trung tính (giữ nguyên ở ván khó, đừng nâng 1,33). Cầm cố bị bán ngay át hoàn toàn (thua 3 đến 13 điểm). Sòng bạc dưới 1,5 điểm. Biến cố E1 đến E6 trung tính (< 1 điểm). M2 +100 xu ≈ +2,4 đến +4,0 điểm.
  - **Cần Claude 1:** sửa cách dùng cầm cố (xem mục 3 Sprint4.md và mục 5 DoKho.md), thiết kế lối ra sau thiếu tiền. **Codex:** kết quả ngẫu nhiên xác định theo seed, mọi thao tác tiền có mã giao dịch lưu cùng bản lưu.
- [ ] **Codex** — Tích hợp luật đã duyệt, lưu giao dịch và tiến trình meta. Nghiệm thu khi các luồng cập nhật nhất quán và khôi phục đúng sau tải lại.
- [ ] **Antigravity** — Dựng NPC, quầy dịch vụ, giao diện và phản hồi biến cố. Nghiệm thu khi người chơi hoàn thành từng luồng trong cảnh chợ.
