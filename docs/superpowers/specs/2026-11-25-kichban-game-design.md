ĐÃ THAY bằng bản chìa khoá, xem kế hoạch 2026-11-26 (mục 6.12). Bản voxel vỡ-lắp-lại cũ đã xoá cùng mã của nó.

# Rương Game — chìa pixel 8-bit
- Mã: `Scripts/ChestFx/Chests/GameKeyHook.cs` (một hook, một diễn viên là chiếc chìa; không vật lý, không hạt, không đèn, không phím).
- Vật mở khoá: chìa ghép từ cube vuông nhỏ (đầu 3x3 rỗng giữa, thân 1 hàng, mũi + 2 răng xanh), vàng + xanh, mũi hướng +Z; hành động `Insert`.
- Nhịp: chuỗi chuẩn của mọi rương (chìa 1,35 s, rung 3 nấc, bóp méo theo bậc); tính cách "giật như khung hình" = chu kỳ +1. Nền `sfx_game_chip_loop`, tiền tố `sfx_game_`.
- Chưa kiểm: chưa chạy Unity, chưa nghe âm, chưa nhìn hình chìa thật; mới biên dịch thử bằng mcs (check_all, check_chia_khoa).
