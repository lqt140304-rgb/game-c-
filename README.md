# CHỢ ĐỒ LẠ

**CHỢ ĐỒ LẠ** là game săn cổ vật roguelite 3D góc nhìn thứ nhất, chơi đơn trên PC. Bạn là người săn hàng ở một khu chợ kỳ ảo, nơi đồ cổ, công nghệ tương lai, vật phẩm ngoài hành tinh và hiện vật thần thoại nằm cạnh nhau. Trong bảy ngày, bạn phải kiếm đủ tiền để trả từng kỳ nợ giang hồ đúng hạn.

- **Săn hộp mù.** Mua tối đa 3 hộp mỗi ngày, tự mang ra bàn, mở niêm phong và cầm hiện vật lên kiểm tra. Chất lượng, tính xác thực và độ hiếm giúp bạn quyết định bán, giữ, giám định hay phục chế.
- **Xây lối chơi từng ván.** Cổ vật gia truyền đầu ván cho nội tại bị động. Tạo tác có tier và hệ cộng hưởng để ghép combo. Bí Khế mở theo điều kiện ngay trong từng ván.
- **Quản lý tiền mặt.** Giữ tiền cho kỳ nợ cuối ngày vì Anh Bảy chỉ thu khi đến hạn, trong suốt 7 ngày. NPC và biến cố thay đổi cơ hội giao dịch. Đấu giá, sòng bạc và cầm cố mở rộng lựa chọn kinh tế.
- **Theo dõi tiến trình.** Sổ tay ghi nợ, giao dịch và khám phá. Lưu game giữ tiến độ ván, còn meta progression theo dõi mở khóa giữa các ván.

Dự án chọn **Unity 6 (6000.6.9f1) + URP** và **Blender**, với mỹ thuật stylized 3D. Bản chơi thử tập trung vào một khu chợ nhỏ, vòng mua–mở–soi–bán hộp, combo tạo tác và đủ 7 ngày trả nợ. Các mốc tiền, giá, xác suất và số lượng nội dung mẫu là thông số thử nghiệm.

- [Game Design](Docs/GameDesign/README.md) — Luật chơi, phạm vi và các câu hỏi thiết kế còn mở.
- [Cân bằng và kiểm thử](Docs/Balance/README.md) — Giả định tiền, nợ, giá hộp, xác suất rơi và ca kiểm thử biên.
- [Checklist sprint](TASKS.md) — Claude 1 phụ trách thiết kế, Claude 2 phụ trách kinh tế và QA, Antigravity phụ trách 3D và tích hợp, Codex phụ trách hệ thống và kiểm thử.

## Quy tắc cho agent

Mọi agent (Claude, Codex, Antigravity...) làm việc trên repo này đều áp dụng:

1. **Mỗi phiên làm một việc.** Xong việc thì chuyển sang phiên mới cho việc sau, để không mất ngữ cảnh và tiết kiệm hạn mức. Mọi thứ cần nhớ phải nằm trong repo, không nằm trong lịch sử trò chuyện.
2. **Nội dung đã chốt phải ghi ra file `.md` trong repo** và thêm vào mục lục [Docs/DaChot.md](Docs/DaChot.md). Đầu mỗi phiên, đọc file này trước.
3. **Đầu mỗi phiên mới, bật Ponytail mức `full` và dùng quy trình Superpowers** (brainstorming trước khi xây, spec ở `docs/superpowers/specs/`). Cài Ponytail cho Claude Code: `/plugin marketplace add DietrichGebert/ponytail` rồi `/plugin install ponytail@ponytail`.
4. Không dùng credit dịch vụ trả phí (Meshy...) và không tự quyết thay chủ dự án khi chưa được phép.
