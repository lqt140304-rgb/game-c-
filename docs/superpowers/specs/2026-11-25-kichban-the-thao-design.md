ĐÃ THAY bằng bản chìa khoá, xem kế hoạch 2026-11-26 (don-sach-ruong-chia-khoa-ke-hoach, mục 6.13).

# Rương Thể thao: vật mở khoá
- Vật: quả bóng đá (cầu trắng + 5 mảng đen dẹt), dựng bằng khối cơ bản trong `TheThaoKeyHook.cs`.
- Nhịp: bay vòng cung 1,0 s, "Strike" (đá vào khe) 0,35 s, thu nhỏ biến mất; rồi rung 3 nấc, bóp méo theo bậc (Jump +20%).
- Nền `sfx_thethao_bed_loop.mp3`, tiếng cắm `key_insert`, tiếng chốt `lock_*` (mp3 đã có trong repo).
- Đã xoá toàn bộ kịch bản cũ (trận đấu, bóng Rigidbody, âm numpy `match/`).
- Chưa kiểm: chưa chạy Unity, chưa nghe âm, chưa xem hình quả bóng trong game (mảng đen có thể lệch).
