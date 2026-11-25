# Kịch bản mở rương Chest_Golden — "Máy Xèng Kho Báu"

Ngày: 2026-11-25 · Nhánh: `claude/chest-seq-golden` · Code: `Assets/_Project/Scripts/ChestFx/Chests/GoldenSlotHook.cs`

## Ý tưởng
Bấm mở là cả rương **biến thành máy đánh bạc**: cần gạt mọc bên hông tự kéo, ba ô quay mọc trên nắp, cả rương
rung dần như máy đang chạy. Kết quả lộ ở **ô thứ ba** — giữ bí mật tới gần cuối. Thua thì máy "xì hơi" hài hước;
thắng thì JACKPOT với **đồng xu vật lý thật** phun ra, rơi, nảy, kêu leng keng. Đặc trưng riêng: *ba ô quay +
cần gạt + mèo thần tài + xu Rigidbody* — không rương nào khác dùng.

## Nhịp (giây tính từ lúc game gọi `ChestRevealSequence.Play`) — đã thu gọn theo kế hoạch 2026-11-25
| t | Hình | Âm |
|---|------|----|
| 0 | Dựng máy: bảng 3 ô quay trên nắp, cần gạt bên phải kéo xuống | `lever` "cạch" (0,5) + bật nền `spin_loop` NGAY (1 lớp, không có Nhịp) |
| 0 → khoá bậc | Ô quay nhanh dần; cả rương rung (Perlin) tăng dần; mỗi nhịp roulette rương giật nhẹ, 1 ô loé màu | nền: âm lượng 0,15→0,45, pitch 0,9→1,3 (+~45%) |
| 0 → khoá bậc | **Phím H** kéo cần (≤ 3 lần): căng thêm, rương giật nhẹ; lần 3 tự thả xu mồi (mèo ra sớm, xu ×1,5) | `lever` 0,5 (cách ≥ 0,2 s) |
| khoá +0 / +0,35 | Ô 1, ô 2 dừng, tô màu bậc | `reel_stop` ×2 (0,5) |
| +0,7…1,6 | Ô 3 vẫn quay (giữ bí mật; Thường+ giữ lâu hơn 30%) | nền vẫn ngân |
| **Rác** | Nền tắt ngay → im 0,2 s → ô 3 xám lệch; rương xẹp, cần rũ; 1 xu èo uột; mèo bị hất ngã | TC = `stingClips[0]` (0,8); 0,6 s sau đó không phát gì |
| **Thường** | Ô 3 trùng màu; rương nhảy nhẹ (0,05 m); 3 xu; mèo vẫy tay | TC = `stingClips[1]` (0,7) |
| **Hiếm → HT** | Ô 3 trùng màu; rương nhảy bật (HT nhảy cao hơn + xoay 360°); xu Rigidbody phun thành đợt | TC = `stingClips[2..4]` (0,85 / 1 / 1) |
| +0,4 → +3 s | Xu rơi, nảy | đuôi: leng keng ≤ 6 tiếng, cách ≥ 0,15 s, âm ≤ 0,4; Rác bắt đầu sau 0,6 s |

Màu 5 bậc vẫn nhảy rồi dừng đúng bậc, bóng vật phẩm vẫn to dần (do `ChestRevealSequence` lo, hook không đụng).

**Số mới (bản chốt):** xu Rác→HT = 1 / 3 / 12 / 22 / 36 (×1,5 nếu có xu mồi, trần 40) · diễn viên 1 (mèo) + máy gắn rương ·
kích thước xu theo cỡ rương (`0,09 × S`, lực × √(S/0,5)) · tương tác: H kéo cần (≤ 3) + xu mồi vật lý · lớp âm trước chốt: 1 ·
`tension.enabled = false`, hồ sơ lõi câm (`ChestAudioRule.ApplyToProfile`, gọi mỗi lần chạy menu).

## Bất biến
- Bậc do game quyết; hook chỉ nhận `OnTierLocked(tier)` để trang trí, không có đường nào đổi bậc.
- Ngẫu nhiên trang trí dùng `System.Random` riêng (trường `decorationSeed`, 0 = theo thời gian).
- Mọi vật tạo ra (rig, xu, vật liệu) bị huỷ ở `OnReset`/`OnDestroy`; vị trí/tỉ lệ/góc quay rương được trả lại (`_baseRot`).
- Không dùng phím G/K/L (cổng xu của bàn thử) và T/Y/Space/R/C; phím riêng của rương là H.

## Tương tác phụ
- **Khe xu mồi** (trigger trên đỉnh bảng ô quay): thả Rigidbody có component `GoldenToken` (hoặc tên chứa
  "Coin"/"Xu") vào khe khi máy đang quay → tiếng `token`, mèo thần tài ra sớm, tự vỗ cần gạt thêm lần nữa,
  độ căng +0.2, và nếu jackpot thì phun **×1.5 xu**. Gọi được từ code: `GoldenSlotHook.InsertToken()`.
- **Mèo thần tài** (dựng bằng primitive): thắng thì vẫy tay, thua thì bị hất ngã bằng lực vật lý.
- Thử nhanh: chuột phải component → **"Thả xu mồi (thử)"** (`DropTestToken`).

## Âm
`Assets/_Project/Audio/Chests/golden/sfx_golden_*.mp3` (ElevenLabs, danh mục `Tools/Audio/elevenlabs_chest_manifest.json`, mục `golden`):
`lever`, `spin_loop` (nền), `reel_stop`, `coin` (đuôi), `token`, và 5 tiếng chốt `lock_rac … lock_huyenThoai` gán vào
`stingClips[5]`. Không còn dùng `fizzle`/`jackpot`/`.wav` numpy (file cũ để nguyên). Hook không còn nhân `volume` hai lần;
tiếng lẻ đi qua `ChestAudioRule.TryGate`.

Kiểm bằng đọc mã (từ lúc bấm tới +3 s sau chốt):
1. Lớp kéo dài trước chốt: 1 (nền `spin_loop`).
2. 0,3 s đầu sau chốt: mọi bậc chỉ có `reel_stop` thứ nhất (ô 3 và TC phát sau ~1,6 s); lúc TC: Rác 1 tiếng (sau 0,2 s im), các bậc khác 1 tiếng (TC), không có HĐ.
3. Không chỗ nào nhân volume hai lần (`Play` truyền thẳng volume vào `PlayOneShot`).

## Cách thử trong Unity
1. Mở cảnh thử rương (xem `Docs/HuongDanThuUnity.md`), chọn gốc Chest_Golden (có `ChestRevealSequence`).
2. `Tools/Cho Do La/Add Golden Slot Hook To Selected Chest` (thêm hook + gán âm).
3. Play, mở rương với từng bậc (Rác → Huyền thoại). Kiểm: âm bắt đầu ngay khi bấm; bậc thấp xịt, bậc cao bùng nổ;
   xu rơi nảy trên sàn (nếu dưới rương không có collider, hook tự tạo nền tạm 3×3 m).
4. Trong lúc ô quay, bấm **H** (≤ 3 lần; lần 3 tự thả xu mồi) hoặc dùng "Thả xu mồi (thử)".

## Trung thực: chưa kiểm
- **Chưa chạy trong Unity, chưa nghe.** Chỉ biên dịch thử bằng stub: `Tools/CompileCheck/check_golden.sh` và `check_all.sh` đều qua.
  Các file `.mp3` ElevenLabs chưa ai nghe; cần nghe thử và kiểm điều khoản giấy phép trước khi phát hành.
- Chưa kiểm trong Unity: ô 3 dừng ~1,6 s sau chốt rồi mới tới TC (có thể thấy hơi trễ); thời lượng tiếng chốt Huyền thoại (3,5 s) có thể vượt mốc "đuôi hết trước +3 s".
- Kích thước/vị trí bảng ô quay, cần gạt, mèo tính từ bounds rương — có thể cần chỉnh tay. Xu theo cỡ rương (`_s`) chưa thử với rương thật.
- Hook dịch chuyển/scale/xoay gốc rương; nếu `BoxOpenSequence` hay hệ thống khác cũng đặt gốc rương mỗi khung thì cần kiểm tra xung đột.
- `CreateAllChestFxProfiles.cs` không được sửa ở phiên này; hồ sơ lõi câm chỉ được ghi khi chạy menu Add Golden Slot Hook (hồ sơ do `CreateOrUpdateGoldenProfile` tạo cần áp `ChestAudioRule.ApplyToProfile` ở nơi khác).

## Nối với khung lõi (`claude/chest-fx-core`)
Lúc viết, nhánh `origin/claude/chest-fx-core` **chưa tồn tại** → chưa merge. Cần nối sau:
- Nếu khung có sự kiện "bấm mở" riêng (vd. `OnPressed`) → gọi `Press()` từ đó thay cho việc dò cạnh `IsPlaying` trong `Update`.
- Nếu khung có API rung/nhảy/âm căng dùng chung → thay phần `LateUpdate` (shake/hop/squash) và `_loop`.
- Đổi `RootHookTemplate("Golden")` trong `CreateAllChestFxProfiles.cs` sang `GoldenSlotHook` (+ `GoldenSlotSetup.AssignClips`),
  và `ChestRevealDemo` (dòng thêm `GoldenGlintHook`) — để lại cho chủ/khung lõi quyết.
