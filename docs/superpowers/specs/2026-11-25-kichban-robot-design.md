# Chest_Robot: ĐÃ THAY bằng bản chìa khoá, xem kế hoạch 2026-11-26 (mục 6.10)

Kịch bản cũ (sạc pin, drone, đứng dậy, màn chữ) đã bị xoá khỏi mã. Lịch sử Git còn giữ bản cũ.

- Vật mở khoá: **thẻ chip** (thẻ dẹt xám kim loại + chip vàng + 4 sọc mạch cyan + 3 chân vàng ở mũi +Z), `RobotKeyHook`.
- Nhịp mới: thẻ bay vòng cung 1 s tới khe, hành động `Insert` 0,35 s, biến mất ở 1,35 s, rồi rung 3 nấc và bóp méo theo bậc (chuỗi chuẩn chung).
- Tính cách: cứng, Squash −20% (cả bậc Rác), chu kỳ −1 (tối thiểu 1). Nền `sfx_robot_hum_loop.mp3`.
- Chưa kiểm: chưa chạy Unity, chưa nghe âm; mới biên dịch qua stub (check_all, check_chia_khoa). Hình thẻ có thể thô.
