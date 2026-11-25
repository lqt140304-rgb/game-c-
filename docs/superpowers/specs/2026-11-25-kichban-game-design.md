# Kịch bản mở rương Chest_Game — "THẾ GIỚI VOXEL 8-BIT"

Nhánh `claude/chest-seq-game`. Mã: `Assets/_Project/Scripts/ChestFx/Chests/GameVoxelHook.cs`, menu `Editor/GameVoxelFxSetup.cs`,
âm `Audio/Chests/game/voxel/*.wav` (tạo bằng `Tools/Audio/make_game_voxel_sfx.py`), biên dịch thử `Tools/CompileCheck/check_game_voxel.sh`.

## Ý tưởng đặc trưng
Rương là một **khối dấu hỏi trong game 8-bit**: bật lơ lửng, bị nhân vật pixel đội đầu húc, rồi **vỡ vụn thành hàng trăm voxel có vật lý thật**
(Rigidbody văng ra, nảy trên sàn) và **tự lắp lại** khối-từng-khối trong lúc quay màu. Màn hình phủ lớp **CRT** (vạch quét, nhấp nháy, chớp trắng)
với chữ arcade. Không rương nào khác dùng cơ chế vỡ-voxel-lắp-lại.

## Nhịp (giây, với BoxOpenSequence mặc định)
| t | Hình | Âm |
|---|---|---|
| 0,0 (bấm mở) | CRT bật, "INSERT COIN"; rương nhảy lên lơ lửng, dấu "?" voxel quay trên đầu | `insert_coin` ngay lập tức |
| 0,35–1,3 | đếm 3-2-1, "GAME START!" | `count_beep` ×3 cao dần, `count_go` |
| 1,3 | nhạc vòng bắt đầu; nhân vật pixel chạy vào dưới rương | `loop_bar` lặp |
| ~1,9–3,7 | húc đầu (tối đa 6 lần): rương nảy, xu vật lý bật ra | `jump`, `bump`, `coin`; nhạc +5% cao/nhanh mỗi cú |
| roulette bắt đầu (~3,7) | **CRASH!** rương vỡ thành voxel (renderer thật ẩn) | `shatter` |
| +0,7 | voxel bị hút về theo đường vồng, đổi màu theo màu đang quay; "LOADING ####...." | `reassemble`, `voxel_tick` mỗi bước, nhạc dâng tới ~1,55× |
| chốt bậc | lắp nốt 8% cuối (giữ bí mật tới lúc chốt), rương thật hiện lại, đáp đất | — |

Kết theo bậc (màu 5 bậc vẫn nhảy rồi dừng đúng bậc; bóng vật phẩm vẫn to dần — phần đó vẫn do ChestRevealSequence lo):
- **Rác**: nhạc "tụt băng" (pitch trượt 0,3 rồi tắt) → `end_0_rac` (đi xuống + "xịt"); "GAME OVER"; rương xẹp; nhân vật ngã sấp bằng vật lý; một voxel xám lăn ra.
- **Thường** `end_1`: "+1 COIN", nảy nhẹ, ít xu. **Hiếm** `end_2`: "NICE!". **Rất hiếm** `end_3`: "1-UP!!", rương bật cao xoay 360°, mưa xu.
- **Huyền thoại** `end_4`: trống cuộn → nổ → fanfare; "HIGH SCORE!!!", rương xoay 720°, pháo voxel cầu vồng có vật lý, mưa xu dày, nhân vật nhảy xoay ăn mừng.
Số xu = (bậc²+1)×(1+power), tối đa 60.

## Tương tác phụ
- **[Z] thả XU**: xu vật lý rơi vào rương → +1 mạng (`one_up`). Nếu ra **Rác** mà còn mạng: "CONTINUE?" — nhân vật đứng dậy húc lại… và vẫn GAME OVER (tối đa 2 lần). Bậc cao: mạng dư phát thêm jingle thưởng.
- **[X] thả NẤM**: +1 power (tối đa 3): rương phồng to 7%/nấc, pháo và mưa xu cuối nhiều hơn.
- Sự kiện tự chạy: nhân vật pixel chạy vào, húc khối, ngã/ăn mừng.

## Bất biến
Hook chỉ đọc bậc qua `OnTierLocked`; không gọi gì đổi kết quả. Ngẫu nhiên (`System.Random` theo TickCount) chỉ cho hướng văng, sắc độ voxel.
Không sửa file dùng chung: hook phát hiện lúc bấm mở bằng `reveal.IsPlaying` trong `Update` (khung hiện chưa có callback "OnPlay").
Dịch chuyển cả rương được cộng ở `LateUpdate` và gỡ ở `Update`, nên không đánh nhau với rung của BoxOpenSequence.

## Thử trong Unity
1. Chạy `python3 Tools/Audio/make_game_voxel_sfx.py` (đã chạy sẵn, wav đã commit).
2. Mở cảnh thử rương (Docs/HuongDanThuUnity.md), chọn gốc Chest_Game → `Tools/Cho Do La/Game Voxel/Add Hook To Selected`
   (gắn hook + âm, tạo/gán `ChestFx_GameVoxel.asset`, tắt GamePixelHook cũ nếu có).
3. Play, Space để mở; T đổi bậc ép; thử Z/X trong lúc diễn; R đóng lại (kiểm dọn sạch voxel/nhân vật, rương hiện lại).

## Trung thực: chưa làm / chưa kiểm
- **Chưa chạy trong Unity, chưa ai nghe âm.** Chỉ biên dịch thử bằng stub (`check_game_voxel.sh`, 3 bộ define: OK).
- Chưa đo được: số voxel thực, va chạm voxel với collider sẵn có của rương, vị trí nhân vật/dấu hỏi theo hướng rương, cỡ chữ CRT.
- Màu voxel lấy từ `baseColorFactor/_BaseColor/_Color`; nếu vật liệu dùng texture trắng thì voxel sẽ nhạt — cần chỉnh.
- Khi chạy không có BoxOpenSequence, roulette bắt đầu ngay nên phần mở đầu bị cắt ngắn (vẫn chạy được).
- **Nối khung lõi**: nhánh `claude/chest-fx-core` chưa tồn tại lúc làm (fetch lỗi). Khi có API khung (vd. callback lúc Play / hàng đợi giai đoạn),
  cần: thay việc dò `IsPlaying` bằng callback, và cho khung biết thời lượng mở đầu để roulette chờ đủ ~3,7 s.
- `check_all.sh` dùng chung chưa có `UnityStubGameVoxel.cs` / ForceMode.VelocityChange… nên sẽ báo lỗi stub với file này; dùng `check_game_voxel.sh`
  hoặc bổ sung các API đó vào UnityStubAll.cs khi gộp.
