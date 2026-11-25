ĐÃ THAY bằng bản chìa khoá, xem kế hoạch 2026-11-26 (don-sach-ruong-chia-khoa-ke-hoach), mục 6.3.

# Rương Chest_Golden — bản chìa khoá
Vật mở khoá: **đồng xu vàng to** (viền nâu vàng + mặt vàng dẹt + chữ "$" bằng 3 khối), xoay 2 vòng khi bay, rồi **rơi** (`KeyAction.Drop`) vào khe.
Nhịp chung: 1,35 s tới `chia_khoa_xong`, rương rung 3 nấc rồi bóp méo/nảy theo bậc; riêng Golden nảy mạnh (jumpMul 1,2). Nền `sfx_golden_spin_loop`, tiếng cắm `sfx_golden_key_insert`.
Mã: `Scripts/ChestFx/Chests/GoldenKeyHook.cs`. Máy xèng cũ (GoldenSlotHook, GoldenGlintHook, GoldenSlotSetup, âm .wav) đã xoá.

**Chưa kiểm:** chưa chạy Unity, chưa nghe âm; mới biên dịch thử bằng stub (check_all, check_chia_khoa).
