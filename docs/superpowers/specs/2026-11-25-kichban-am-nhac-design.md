ĐÃ THAY bằng bản chìa khoá, xem kế hoạch 2026-11-26 (`2026-11-26-don-sach-ruong-chia-khoa-ke-hoach.md`, mục 6.7).

# Rương Âm nhạc: vật mở khoá

- Vật mở khoá: **đĩa than** dựng đứng (đĩa đen dẹt + 3 rãnh xám/đen + nhãn hồng giữa), 5 khối cơ bản, mép đĩa hướng +Z.
- Hành động mở: `Insert` (đẩy vào khe rồi xoay 90°, rồi thu nhỏ biến mất). Nền `sfx_amnhac_bed_loop.mp3`, tiếng cắm `sfx_amnhac_key_insert.mp3`.
- Nhịp bóp méo mới: "nhún theo nhịp", chu kỳ Squash/Jump +1, các hệ số khác mặc định. Rung 3 nấc và chuỗi 1,35 s theo chuẩn chung.
- Mã: `Scripts/ChestFx/Chests/AmNhacKeyHook.cs`. Toàn bộ hook cũ (kịch bản 4 giai đoạn, Bass, Vinyl, âm numpy) đã xoá; xem lịch sử Git nếu cần.

**Chưa kiểm:** chưa chạy Unity, chưa nhìn hình đĩa trong cảnh, chưa nghe âm; mới chỉ biên dịch bằng mcs (`check_all.sh`, `check_chia_khoa.sh`).
