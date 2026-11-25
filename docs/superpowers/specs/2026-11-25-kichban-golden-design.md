# Kịch bản mở rương Chest_Golden — "Máy Xèng Kho Báu"

Ngày: 2026-11-25 · Nhánh: `claude/chest-seq-golden` · Code: `Assets/_Project/Scripts/ChestFx/Chests/GoldenSlotHook.cs`

## Ý tưởng
Bấm mở là cả rương **biến thành máy đánh bạc**: cần gạt mọc bên hông tự kéo, ba ô quay mọc trên nắp, cả rương
rung dần như máy đang chạy. Kết quả lộ ở **ô thứ ba** — giữ bí mật tới gần cuối. Thua thì máy "xì hơi" hài hước;
thắng thì JACKPOT với **đồng xu vật lý thật** phun ra, rơi, nảy, kêu leng keng. Đặc trưng riêng: *ba ô quay +
cần gạt + mèo thần tài + xu Rigidbody* — không rương nào khác dùng.

## Nhịp (giây tính từ lúc game gọi `ChestRevealSequence.Play`)
| t | Hình | Âm |
|---|------|----|
| 0 | Dựng máy: bảng 3 ô quay trên nắp, cần gạt bên phải kéo xuống | `lever` "cạch" + bật vòng lặp `spin_loop` NGAY |
| 0 → khoá bậc | Ô quay nhanh dần; cả rương rung (Perlin) tăng tới `maxShake`; mỗi nhịp roulette rương giật nảy nhẹ, 1 ô loé màu đang hiện | pitch 0.7→1.6, âm lượng 0.35→0.85 (tiệm cận, `tensionRise` = 2.5 s) |
| khoá +0 / +0.35 | Ô 1, ô 2 dừng, tô màu bậc | `reel_stop` ×2 |
| +0.7…0.9 | Ô 3 vẫn quay (giữ bí mật) | pitch còn dâng thêm |
| **Thấp** (Rác, Thường) | Ô 3 dừng màu xám lệch; rương xẹp (squash), cần gạt rũ; 1 / 3 xu èo uột rơi ra; mèo thần tài nhô lên rồi bị hất ngã (Rigidbody) | vòng lặp **tụt pitch về 0.25 rồi tắt**, kèn hụt `fizzle` "uơ-uơ-uơơ" + xì hơi |
| **Cao** (Hiếm+) | Ô 3 trùng màu; rương nhảy bật (Huyền thoại nhảy cao hơn + xoay 360°); 14 / 26 / 45 xu Rigidbody phun thành đợt; mèo thần tài vẫy tay | vòng lặp cắt gọn → `jackpot` (hợp âm đi lên + chuông dồn) + leng keng mỗi va chạm xu |

Màu 5 bậc vẫn nhảy rồi dừng đúng bậc, bóng vật phẩm vẫn to dần (do `ChestRevealSequence` lo, hook không đụng).

## Bất biến
- Bậc do game quyết; hook chỉ nhận `OnTierLocked(tier)` để trang trí, không có đường nào đổi bậc.
- Ngẫu nhiên trang trí dùng `System.Random` riêng (trường `decorationSeed`, 0 = theo thời gian).
- Mọi vật tạo ra (rig, xu, vật liệu) bị huỷ ở `OnReset`/`OnDestroy`; vị trí/tỉ lệ rương được trả lại.

## Tương tác phụ
- **Khe xu mồi** (trigger trên đỉnh bảng ô quay): thả Rigidbody có component `GoldenToken` (hoặc tên chứa
  "Coin"/"Xu") vào khe khi máy đang quay → tiếng `token`, mèo thần tài ra sớm, tự vỗ cần gạt thêm lần nữa,
  độ căng +0.2, và nếu jackpot thì phun **×1.5 xu**. Gọi được từ code: `GoldenSlotHook.InsertToken()`.
- **Mèo thần tài** (dựng bằng primitive): thắng thì vẫy tay, thua thì bị hất ngã bằng lực vật lý.
- Thử nhanh: chuột phải component → **"Thả xu mồi (thử)"** (`DropTestToken`).

## Âm
`Assets/_Project/Audio/Chests/golden/sfx_golden_*.wav` — tổng hợp bằng `python3 Tools/Audio/make_golden_slot_sfx.py`
(numpy, hạt giống cố định, không bản quyền): lever, spin_loop (1 s, lặp liền), reel_stop, fizzle, jackpot, coin, token.
Là âm TẠM; có thể thay bằng bản thu/ElevenLabs cùng tên file.

## Cách thử trong Unity
1. Mở cảnh thử rương (xem `Docs/HuongDanThuUnity.md`), chọn gốc Chest_Golden (có `ChestRevealSequence`).
2. `Tools/Cho Do La/Add Golden Slot Hook To Selected Chest` (thêm hook + gán âm).
3. Play, mở rương với từng bậc (Rác → Huyền thoại). Kiểm: âm bắt đầu ngay khi bấm; bậc thấp xịt, bậc cao bùng nổ;
   xu rơi nảy trên sàn (nếu dưới rương không có collider, hook tự tạo nền tạm 3×3 m).
4. Trong lúc ô quay, dùng "Thả xu mồi (thử)".

## Trung thực: chưa kiểm
- **Chưa chạy trong Unity, chưa nghe trong game.** Chỉ biên dịch thử bằng stub: `Tools/CompileCheck/check_golden.sh`
  và `check_all.sh` đều OK. File WAV đã tạo nhưng chưa nghe thử trên loa.
- Kích thước/vị trí bảng ô quay, cần gạt, mèo được tính từ bounds rương — có thể cần chỉnh tay.
- Hook dịch chuyển/scale gốc rương (cộng-trừ phần lệch mỗi khung); nếu `BoxOpenSequence` hay hệ thống khác cũng
  đặt vị trí gốc rương mỗi khung thì cần kiểm tra xung đột.
- Không có particle mới (không đụng orbitalX/Y/Z). `GoldenGlintHook` cũ vẫn có thể chạy song song.
- Chưa nối vào `CreateAllChestFxProfiles` / `ChestRevealDemo` (file dùng chung) — xem phần dưới.

## Nối với khung lõi (`claude/chest-fx-core`)
Lúc viết, nhánh `origin/claude/chest-fx-core` **chưa tồn tại** → chưa merge. Cần nối sau:
- Nếu khung có sự kiện "bấm mở" riêng (vd. `OnPressed`) → gọi `Press()` từ đó thay cho việc dò cạnh `IsPlaying` trong `Update`.
- Nếu khung có API rung/nhảy/âm căng dùng chung → thay phần `LateUpdate` (shake/hop/squash) và `_loop`.
- Đổi `RootHookTemplate("Golden")` trong `CreateAllChestFxProfiles.cs` sang `GoldenSlotHook` (+ `GoldenSlotSetup.AssignClips`),
  và `ChestRevealDemo` (dòng thêm `GoldenGlintHook`) — để lại cho chủ/khung lõi quyết.
