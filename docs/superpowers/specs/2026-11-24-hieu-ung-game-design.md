# Hiệu ứng và âm thanh rương Game (Chest_Game)

Ngày 2026-11-24. Trạng thái: **đã dựng, chưa chạy trong Unity, chưa nghe âm.** Dựa trên lõi ở [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md); không sửa file lõi.

## Ý tưởng

Rương là tay cầm game khổng lồ dựng nghiêng trên chân đế tròn. Mở rương như **chơi một màn 8-bit**: bấm nút, máy khởi động, đồng hồ sức mạnh chạy lên xuống, rồi "LEVEL UP" theo bậc. Rác là "game over" hài; Huyền thoại là nhạc phá đảo.

- **Vòng quay màu**: `PingPong` (lên xuống như thanh lực) + đường cong giảm tốc **bậc thang** (5 nấc, tiếp tuyến hằng): tốc độ tick tụt từng số như hộp số, không trôi mượt. `nearMissChance` 0,7 ("suýt nữa thì…").
- **Hạt của lõi**: `Sparkle`, `Radial`, sống ngắn 0,9 s, pha màu xanh lá màn hình CRT `#59FF8C`, trọng lực nhẹ 0,35.
- **Tia**: `raySpin = 0` (pixel art không xoay), ngắn 0,8 s, chớp 6,5.
- **Hook `GamePixelHook`** (code, không asset): hạt **hình vuông cứng, không xoay** (texture trắng trơn).
  - Mỗi nhịp nhảy màu: bắn **(hạng + 1) ô vuông** màu đang hiện thẳng lên như bộ đếm điểm (đọc được bậc trong lúc quay).
  - Khoá Rác: 10 ô xám nhảy ra rồi rơi bịch (trọng lực 1,2) — "game over".
  - Khoá Thường trở lên: nổ 22 × hạng ô vuông tứ phía; Huyền thoại thêm **đợt 2** (66 ô sáng hơn) sau 0,25 s.

Phần chung chủ dự án đã chốt (màu nhảy rồi dừng đúng bậc, bóng to dần nguyên hình) do lõi lo, không đổi.

## Nhịp (rương không chìa, thời gian giai đoạn mặc định)

| Giây | Hình | Âm |
|---|---|---|
| 0,0 | Rương rung | `button_click` (bấm nút) |
| 3,7 | Hé nắp: bắt đầu quay, ô vuông nhảy lên theo nhịp | `boot` (máy bật) + nền `ambient_select` 3 s + tick 0,1 s, cao độ 1,0 → 1,5 |
| 3,7–6,5 | PingPong, tụt tốc 5 nấc (0,06 → 0,40 s) | tick |
| 6,5 | Khoá: chớp, tia không xoay, nổ ô vuông | chốt theo bậc + vút `whoosh_grow`; sau đó accent coin/jump: 0 (Rác), 2, 4, 6, 8 (Huyền thoại) |
| 6,75 | (Huyền thoại) đợt ô vuông thứ 2 | — |
| 7,7 | Mở toang | tiếng nắp chung + `press_start` |
| 8,0–8,3 | Bóng hoàn chỉnh, xong | — |

## Bảng âm

Xem [Audio/Chests/game/README.md](../../../Assets/_Project/Audio/Chests/game/README.md) (13 file, mô tả tiếng Anh đã gửi).

| Bậc | Tiếng chốt |
|---|---|
| Rác | game over tụt giọng, kết bằng tiếng xì hơi (hài), 2,5 s |
| Thường | "nhặt đồ" 2 nốt, 1 s |
| Hiếm | level up 3 nốt đi lên, 1,5 s |
| Rất hiếm | power-up fanfare, 2,5 s |
| Huyền thoại | nhạc phá đảo + trống, 4,5 s |

## File

- `Assets/_Project/Scripts/ChestFx/Chests/GameFxPreset.cs` — `GameFxPreset.Apply(p)`, tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/GamePixelHook.cs` — hook ô vuông.
- `Assets/_Project/Editor/GameFxSetup.cs` — menu `Tools/Cho Do La/Create Game Chest FX Profile` (tạo `ScriptableObjects/ChestFx/ChestFx_Game.asset`, gắn clip; thiếu clip riêng thì lấy bộ chung) và `Tools/Cho Do La/Add Game Pixel Hook To Selected`.
- `Assets/_Project/Audio/Chests/game/` — 13 mp3 + README.
- `Assets/_Project/Art/Chests/Chest_Game.glb` — chép từ nhánh `claude/chest-game`.
- `Tools/CompileCheck/check_game.sh` — biên dịch thử bằng stub.

## Cách thử trong Unity

1. `Tools/Cho Do La/Add Chest Reveal Test` (bàn thử chung), `Tools/Cho Do La/Create Game Chest FX Profile`.
2. Trên bàn thử bấm C tới Chest_Game; kéo `ChestFx_Game` vào `profile` của `ChestRevealSequence` (hoặc vào danh sách `profiles` của `ChestRevealDemo`).
3. Chọn gốc rương (có `ChestRevealSequence`) → `Tools/Cho Do La/Add Game Pixel Hook To Selected`.
4. Play: T chọn bậc, Space mở, R đóng. Thử đủ 5 bậc; nghe tick có đục không, Rác có buồn cười không, Huyền thoại có đè tiếng accent quá không.
5. Chỉnh nếu cần: `pixelSize`, `burstPerRank` trên hook; `accentsPerTier`, `ambientVolume` trên hồ sơ.

## Chưa kiểm chứng

- Chưa chạy Unity; chỉ biên dịch bằng stub (bắt lỗi cú pháp/kiểu).
- Chưa nghe âm; chưa thấy hạt. `Texture2D.whiteTexture` với shader Particles/Unlit có thể cho ô vuông hơi mờ cạnh khi xa (lọc bilinear) — chấp nhận được, nếu muốn sắc nét thì tạo texture 4×4 `FilterMode.Point`.
- Đường cong bậc thang dùng tiếp tuyến vô hạn; cần xem lõi có đọc `Evaluate` đúng như mong muốn (hằng từng đoạn) không.
- Hook phải gắn tay (hoặc qua `customFxPrefab`): khi demo đổi sang rương khác bằng phím C, hook không tự đi theo.
