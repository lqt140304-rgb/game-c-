# Kịch bản mở rương Chest_Game — "THẾ GIỚI VOXEL 8-BIT"

Nhánh `claude/chest-seq-game`. Mã: `Assets/_Project/Scripts/ChestFx/Chests/GameVoxelHook.cs`, menu `Editor/GameVoxelFxSetup.cs`,
âm ElevenLabs `Audio/Chests/game/sfx_game_*.mp3` (danh mục `Tools/Audio/elevenlabs_chest_manifest.json`; `.wav` numpy cũ trong `voxel/` không còn được gán), biên dịch thử `Tools/CompileCheck/check_game_voxel.sh`.

## Ý tưởng đặc trưng
Rương là một **khối dấu hỏi trong game 8-bit**: bật lơ lửng, bị nhân vật pixel đội đầu húc, rồi **vỡ vụn thành tối đa 80 voxel có vật lý thật**
(Rigidbody văng ra, nảy trên sàn) và **tự lắp lại** khối-từng-khối trong lúc quay màu. Chữ arcade là 1 dòng TextMesh nổi trên rương (đã bỏ lớp CRT phủ màn hình). Không rương nào khác dùng cơ chế vỡ-voxel-lắp-lại.

## Nhịp (giây, với BoxOpenSequence mặc định) — đã thu gọn theo kế hoạch 2026-11-25 (mục 5.12)
| t | Hình | Âm |
|---|---|---|
| 0,0 (bấm mở) | rương nhảy lên lơ lửng, dấu "?" voxel quay trên đầu (không còn CRT phủ màn hình, không đếm 3-2-1) | `press_start` (chèn xu) ngay lập tức, vol 0,4 |
| 0,3 | nhạc vòng bắt đầu; nhân vật pixel chạy vào dưới rương | `chip_loop` lặp, vol 0,15 → 0,45, pitch tới ~1,55× |
| ~0,9–2 | húc đầu (tối đa 3 lần): rương nảy, xu vật lý bật ra | `jump`, `bump`, `coin` qua cổng 0,2 s / 0,12 s; nhạc +5% cao mỗi cú |
| roulette bắt đầu | rương vỡ thành tối đa 80 voxel (renderer thật ẩn) | `shatter` (0,5) |
| +0,7 | voxel bị hút về, đổi màu theo màu đang quay | `reassemble` (0,4), `voxel_tick` mỗi bước (0,35, qua `TryGate`) |
| chốt bậc | nhạc DỪNG ngay; lắp nốt voxel 0,22 s (khoảng lặng), rương thật hiện lại | sau 0,22 s: đúng 1 tiếng chốt `stingClips[bậc]` |

Chữ arcade chỉ còn 1 dòng TextMesh nổi trên rương, hiện 2 s ở các bậc có chữ: "GAME OVER" (Rác), "NICE!" (Hiếm), "1-UP!!" (Rất hiếm), "HIGH SCORE!!!" (Huyền thoại).
Kết theo bậc (TC = `sfx_game_lock_*.mp3`, gán vào `stingClips[5]`):
- **Rác**: nhạc dừng ngay, lặng 0,2 s, 1 tiếng xịt; "GAME OVER"; rương xẹp; nhân vật ngã sấp bằng vật lý; một voxel xám lăn ra. Không phát gì khác trong 0,6 s sau đó.
- **Thường**: nảy nhẹ, ít xu. **Hiếm**: "NICE!". **Rất hiếm**: "1-UP!!", rương bật cao xoay 360°, mưa xu.
- **Huyền thoại**: "HIGH SCORE!!!", rương xoay 540°, pháo voxel cầu vồng (3 đợt x 8 = 24 khối), mưa xu, nhân vật nhảy xoay ăn mừng.
Số xu = min(24, (bậc²+1)×(1+power)). Đuôi (xu, nhảy, 1-up) từ +0,4 s, tối đa 6 tiếng, cách ≥ 0,15 s, vol ≤ 0,4.

### Bản chốt
| Diễn viên | 1 (nhân vật pixel) + dấu "?" gắn trên rương |
|---|---|
| Voxel Rigidbody | ≤ 80 (chỉ trong pha quay màu, bị hút về trước chốt) |
| Xu HT | ≤ 24 |
| Húc đầu | ≤ 3 |
| Phủ màn hình | không |
| `tension.enabled` | false (`ChestAudioRule.ApplyToProfile` gọi mỗi lần chạy menu) |

## Tương tác phụ
- **[Z] thả XU**: xu vật lý rơi vào rương → +1 mạng (`one_up`). Nếu ra **Rác** mà còn mạng: "CONTINUE?" — nhân vật đứng dậy húc lại… và vẫn GAME OVER (tối đa 1 lần). Bậc cao: mạng dư phát thêm jingle thưởng.
- **[X] thả NẤM**: +1 power (tối đa 3): rương phồng to 7%/nấc, pháo và mưa xu cuối nhiều hơn.
- Sự kiện tự chạy: nhân vật pixel chạy vào, húc khối, ngã/ăn mừng.

## Bất biến
Hook chỉ đọc bậc qua `OnTierLocked`; không gọi gì đổi kết quả. Ngẫu nhiên (`System.Random` theo TickCount) chỉ cho hướng văng, sắc độ voxel.
Không sửa file dùng chung: hook phát hiện lúc bấm mở bằng `reveal.IsPlaying` trong `Update` (khung hiện chưa có callback "OnPlay").
Dịch chuyển cả rương được cộng ở `LateUpdate` và gỡ ở `Update`, nên không đánh nhau với rung của BoxOpenSequence.

## Thử trong Unity
1. Âm mp3 ElevenLabs đã có sẵn trong `Audio/Chests/game/`.
2. Mở cảnh thử rương (Docs/HuongDanThuUnity.md), chọn gốc Chest_Game → `Tools/Cho Do La/Game Voxel/Add Hook To Selected`
   (gắn hook + âm, tạo/gán `ChestFx_GameVoxel.asset`, tắt GamePixelHook cũ nếu có).
3. Play, Space để mở; T đổi bậc ép; thử Z/X trong lúc diễn; R đóng lại (kiểm dọn sạch voxel/nhân vật, rương hiện lại).

## Trung thực: chưa làm / chưa kiểm
- **Sau thu gọn 2026-11-25:** chưa chạy Unity, chưa ai nghe âm mp3 ElevenLabs (cần nghe thử + kiểm giấy phép). Chưa đo cỡ/vị trí chữ TextMesh, độ hợp của `chip_loop` (8 s, có vòng lặp liền mạch không) hay số voxel thực.
- `check_all.sh` qua; `check_game_voxel.sh` lỗi từ trước (stub `UnityStubGameVoxel.cs` trùng với `UnityStubAll.cs`: `DrawTextureWithTexCoords`, `AddTorque`), không sửa vì ngoài phạm vi.
- **Chưa chạy trong Unity, chưa ai nghe âm.** Chỉ biên dịch thử bằng stub (`check_game_voxel.sh`, 3 bộ define: OK).
- Chưa đo được: số voxel thực, va chạm voxel với collider sẵn có của rương, vị trí nhân vật/dấu hỏi theo hướng rương, cỡ chữ CRT.
- Màu voxel lấy từ `baseColorFactor/_BaseColor/_Color`; nếu vật liệu dùng texture trắng thì voxel sẽ nhạt — cần chỉnh.
- Khi chạy không có BoxOpenSequence, roulette bắt đầu ngay nên phần mở đầu bị cắt ngắn (vẫn chạy được).
- **Nối khung lõi**: nhánh `claude/chest-fx-core` chưa tồn tại lúc làm (fetch lỗi). Khi có API khung (vd. callback lúc Play / hàng đợi giai đoạn),
  cần: thay việc dò `IsPlaying` bằng callback, và cho khung biết thời lượng mở đầu để roulette chờ đủ ~3,7 s.
- `check_all.sh` dùng chung chưa có `UnityStubGameVoxel.cs` / ForceMode.VelocityChange… nên sẽ báo lỗi stub với file này; dùng `check_game_voxel.sh`
  hoặc bổ sung các API đó vào UnityStubAll.cs khi gộp.
