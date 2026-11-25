# Bàn giao cho Antigravity — CHỢ ĐỒ LẠ

Dùng nhánh `codex/sprint-2-artifacts` để đọc giao diện dữ liệu và chạy thử các tính năng chung. Nhánh này đã chứa lõi Sprint 1 và cảnh tích hợp từ PR #9. Giữ công việc map đang làm trên nhánh của bạn và dùng worktree riêng để thử, nhất là khi Unity đang có cảnh chưa lưu. Đọc [DataContract.md](../../../Docs/Architecture/DataContract.md) trước. Dữ liệu gameplay ở [content.json](../../../Docs/Content/data/content.json).

## Có thể dựng và kiểm tra ngay

| Phần | Điểm nối và thiết kế |
| --- | --- |
| Vòng chợ Sprint 1 | Mở `Assets/_Project/Scenes/Market_Town.unity`. Cảnh gọi `Sprint1MarketService` và dùng một `MarketRun` cho giá hôm nay, giá niêm yết, mua, mở, soi, bán và tự lưu. Kiểm tra hộp đã mua còn trên bàn khi tải lại, hàng giả chỉ lộ sau bán và thao tác kết thúc ngày không bị lặp. |
| Kim Khuy và Trang bị | Tương tác với Kim Khuy để mua, hoặc nhấn **I** để mở riêng. Bốn ô dọc bên trái, ô 5 mở bằng BK1. Nhân vật bên phải cầm món vừa gắn lên ngắm. Hệ và Combo là icon sáng/mờ, rê vào combo sẽ thấy icon món còn thiếu. Giữ chúng trong màn Trang bị. |
| Tài sản nhân vật và Tạo Tác | Mô hình do phiên Claude khác phụ trách. Dùng mã `CP1–CP3` và `TC1–TC3`. Các đường dẫn prefab và sprite nằm trong DataContract. Nhân vật preview dùng điểm cầm `TactPreviewHand` hoặc xương tay phải Humanoid và trigger `InspectTact`. Một động tác cầm lên ngắm dùng chung. |
| Điện thoại và Thành tựu | Nhấn **P** để mở, dùng chuột chọn trang. Trang chính đọc tiền, ngày và giờ khi đồng hồ bật. **Thành tựu** có 6 mốc đang chạy. Thẻ góc trên bên phải tự tắt sau khoảng 3 giây, xếp lần lượt khi đạt nhiều mốc và không chặn thao tác. Có thể dựng phần giơ điện thoại hoặc làm mới giao diện quanh các dữ liệu hiện có. |
| Ngày và ngủ | Gắn `SleepBedInteractable` và collider lên giường phòng trọ khi vị trí giường đã sẵn sàng. Đây là điểm kích hoạt đồng hồ. Ngày bắt đầu 08:00, chợ đóng 18:00 sau 9 phút chơi, đồng hồ dừng 19:00. Ngủ chốt nợ và tự lưu rồi sang sáng. Có thể dùng `ClockPhase` và `ClockMinute` để nối ánh sáng. |
| Gia truyền | Sổ nợ đọc tên gia truyền, nợ tồn và kỳ hoãn. GT1 cho chọn hoãn trước khi ngủ. GT2 thêm manh mối ở H1. UI đọc kết quả từ gameplay. |

## Tài liệu hoặc nhánh riêng

- [Thiết kế 13 thành tựu mở rộng](../specs/2026-11-25-thanh-tuu-mo-rong-design.md), gồm **Hải quay xe** và **Một giấc mộng đẹp**. Hiện mới là thiết kế. Nếu dựng UI, dùng sáu mốc hiện có để nghiệm thu trước.
- [Nhánh mini-game](https://github.com/lqt140304-rgb/game-c-/tree/codex/morning-minigames-design). Đánh răng và ăn sáng chạy riêng qua **Tools → Cho Do La → Open Morning Prototype**. Chờ map hoàn thiện rồi mới gắn vào phòng trọ và quán. Thiết kế cụ thể nằm trong `docs/superpowers/specs/2026-11-24-mini-game-danh-rang-an-sang-design.md` trên nhánh đó.
- [Nhánh Cây Phát Triển](https://github.com/lqt140304-rgb/game-c-/tree/codex/skill-tree) gồm bản thử. Theo yêu cầu chủ dự án, phần mở rộng và liên kết Thành tựu ↔ Cây Phát Triển để sau.
- Vòng chọn đồ cầm tay hiện là thiết kế. Giữ phím để hiện vòng, rê về vật rồi thả để chọn. Vòng dành cho món lấy ra tay. Điện thoại và Trang bị Tạo Tác là hai giao diện riêng.

## Kiểm tra trong Unity

1. Mở `Market_Town`, mua → mở → soi → bán, rồi tải lại để kiểm tra tiền, kho và hộp.
2. Mở điện thoại, làm một mốc lần đầu và kiểm tra thẻ thành tựu không chặn điều khiển. Tải lại để kiểm tra thẻ cũ không phát lại.
3. Mua tại Kim Khuy, dùng I để gắn/tháo, kiểm tra hover combo hiện đúng icon và BK1 mở ô 5 trước ngày 5.
4. Sau khi gắn giường, thử lúc chợ đóng, ngủ, tải lại và xem ngày mới. Kiểm tra cả thu nợ thường và GT1 hoãn kỳ.

Codex đã chạy kiểm thử C# và Python cho logic. Các cảnh, bố cục và hoạt ảnh vẫn cần xác minh bằng Play Mode.
