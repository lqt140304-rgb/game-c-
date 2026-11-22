# Checklist công việc — CHỢ ĐỒ LẠ

File này chỉ giữ các việc **chưa nghiệm thu**. Mỗi mục có một agent phụ trách và đầu ra kiểm tra được. Agent phụ trách gửi bằng chứng qua PR, bản chạy, video hoặc kết quả kiểm thử phù hợp. Claude 1 rà luật, Claude 2 kiểm tra kinh tế và ca biên, Codex rà tích hợp, chủ dự án nghiệm thu. Agent nộp việc ghi trạng thái ngay dưới mục của mình để các agent khác biết. Khi đã nghiệm thu, **xóa hẳn mục việc cùng các dòng mô tả thuộc mục đó khỏi file**. Lịch sử Git hoặc PR giữ bằng chứng hoàn thành.

Claude 1 sở hữu thiết kế và nội dung. Claude 2 sở hữu cân bằng và QA. Antigravity sở hữu cảnh, prefab, tương tác 3D và UI. Codex sở hữu gameplay, dữ liệu, lưu ván và kiểm thử tự động. Codex và Antigravity thống nhất giao diện dữ liệu trước khi nối gameplay với cảnh.

## Sprint 0 — Nền móng

- [ ] **Claude 1** — Trình chủ dự án duyệt các luật còn mở cần cho prototype, đặc biệt xử lý thiếu tiền và thắng/thua. Nghiệm thu khi có quy tắc và ví dụ kiểm thử trong [GDD](Docs/GameDesign/README.md).
- [ ] **Claude 2** — Lập bảng giả định vốn, nợ, giá, giá bán và xác suất rơi. Nghiệm thu khi có bảng cân bằng ban đầu và ca kiểm thử vượt giới hạn mua, trả nợ sớm, thiếu tiền, bán trùng.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/README.md](Docs/Balance/README.md) có bảng giả định (mục 1–2), mô phỏng khả thi (mục 3) và 16 ca biên BT-01–BT-16 (mục 4).
  - **Cần Claude 1 chốt** (mục 5 của tài liệu): hậu quả thiếu tiền, điều kiện thắng/thua, sức chứa kho, phí giám định/phục chế, bán tức thì hay không. Codex dùng bảng ở mục 2 làm dữ liệu mẫu.
- [ ] **Codex** — Thống nhất hợp đồng dữ liệu hộp, hiện vật, kho, ngày, nợ và thông báo thay đổi với Antigravity. Nghiệm thu khi mỗi bên xác định được dữ liệu đọc và hành động ghi.
- [ ] **Antigravity** — Khởi tạo Unity 6.3 LTS URP và scene thử. Nghiệm thu khi mở đúng phiên bản và chạy Play Mode với cảnh tối thiểu.

## Sprint 1 — Một ngày chơi First-Person

- [ ] **Claude 1** — Soạn danh mục hộp, hiện vật mẫu, manh mối niêm phong và hướng dẫn soi. Nghiệm thu khi dữ liệu đủ để dựng vòng mua–mở–soi–bán.
- [ ] **Claude 2** — Kiểm thử dòng tiền mẫu và các ca mua hộp thứ tư, hết tiền, kho đầy, bán trùng. Nghiệm thu khi có kết quả tái hiện và lỗi được ghi nhận.
  - **Trạng thái: đã nộp, chờ chủ dự án nghiệm thu.** Bằng chứng: [Docs/Balance/Sprint1.md](Docs/Balance/Sprint1.md). Chạy `cd QA && python3 -m unittest -v test_economy` (17 test đạt) và `python3 scenario_cashflow.py`.
  - **Lỗi ghi nhận:** D-01 `kết thúc ngày` bấm đúp thu nợ hai ngày (Codex, Antigravity chặn ở UI). D-02 hộp chưa mở có chiếm kho không, D-03 hậu quả thiếu tiền (Claude 1 chốt). Chi tiết trong tài liệu.
  - **Codex:** `QA/test_economy.py` là đặc tả hành vi, có thể chuyển sang kiểm thử C#.
- [ ] **Codex** — Triển khai tiền, hộp, kho, định giá mẫu, bán hàng, kết thúc ngày và kỳ thu nợ. Nghiệm thu khi giao dịch hợp lệ cập nhật một lần và lượt mua đặt lại đúng ngày.
- [ ] **Antigravity** — Dựng quầy, bàn mở, cầm xoay hiện vật và UI giao dịch. Nghiệm thu khi người chơi tự đi hết vòng từ chọn hộp đến nhận tiền bán trong Play Mode.

## Sprint 2 — Tạo tác và ván 7 ngày

- [ ] **Claude 1** — Soạn nội tại gia truyền, hai hệ tạo tác, combo và hai Bí Khế mẫu. Nghiệm thu khi mỗi hiệu ứng có điều kiện kích hoạt, hiệu lực và ví dụ.
- [ ] **Claude 2** — Kiểm thử lịch nợ, combo, tiến độ Bí Khế và tải lại ván. Nghiệm thu khi có ca trả đúng hạn, chặn trả trước, thiếu tiền, đạt và chưa đạt điều kiện.
- [ ] **Codex** — Nối gia truyền, tier, trang bị, combo, Bí Khế, tiến trình 7 ngày và lưu ván. Nghiệm thu khi có thể chơi tới kết quả và tải lại đúng trạng thái mà không lặp giao dịch.
- [ ] **Antigravity** — Dựng Kho Tạo Tác, UI sổ và trang bị, cảnh thu nợ cùng màn kết quả. Nghiệm thu khi người thử đọc được nợ, combo, tiến độ Bí Khế và lý do kết thúc ván.

## Sprint 3 — Cân bằng và nghiệm thu vertical slice

- [ ] **Claude 1** — Chơi thử và rà nhịp, độ rõ thông tin, quyết định giữ tiền hay đầu tư. Nghiệm thu khi có phản hồi từ ván hoàn chỉnh và danh sách điều chỉnh đã duyệt.
- [ ] **Claude 2** — So sánh chiến thuật thận trọng, săn combo và liều lĩnh trên seed tái lập. Nghiệm thu khi có tỷ lệ thắng, ngày hụt tiền, tần suất combo và đề xuất có số liệu.
- [ ] **Codex** — Sửa lỗi cốt lõi, chạy hồi quy và đóng gói bản PC. Nghiệm thu khi bản chạy chơi được từ đầu đến kết quả ngày 7 với các ca biên đã kiểm.
- [ ] **Antigravity** — Hoàn thiện tương tác, âm thanh, ánh sáng, phản hồi mở hộp và khả năng đọc UI. Nghiệm thu qua một ván chơi trên bản PC.

## Sprint 4 — Mở rộng sau nghiệm thu vertical slice

- [ ] **Claude 1** — Chốt luật biến cố, đấu giá, sòng bạc, cầm cố và mở khóa meta. Nghiệm thu khi mỗi hệ có vai trò, luồng và điều kiện hoàn tất.
- [ ] **Claude 2** — Đo ảnh hưởng dịch vụ đến thanh khoản, nợ và tiến trình giữa ván. Nghiệm thu khi có ca chống giao dịch lặp và báo cáo cân bằng.
- [ ] **Codex** — Tích hợp luật đã duyệt, lưu giao dịch và tiến trình meta. Nghiệm thu khi các luồng cập nhật nhất quán và khôi phục đúng sau tải lại.
- [ ] **Antigravity** — Dựng NPC, quầy dịch vụ, giao diện và phản hồi biến cố. Nghiệm thu khi người chơi hoàn thành từng luồng trong cảnh chợ.
