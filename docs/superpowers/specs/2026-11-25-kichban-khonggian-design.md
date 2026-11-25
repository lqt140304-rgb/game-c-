# Kịch bản mở rương Không Gian — "Phi vụ tiếp tế" (đã thu gọn theo kế hoạch 2026-11-25, mục 5.11)

Rương: `Chest_KhongGian` (Assets/_Project/Art/Chests/Chest_KhongGian.glb).
Mã: `Scripts/ChestFx/Chests/KhongGianMissionHook.cs` (đạo diễn, hook DUY NHẤT), `Editor/KhongGianFxSetup.cs`
(gắn hook + gán âm vào prefab, ghi hồ sơ lõi câm), `KhongGianFxPreset.cs` (tên file âm). `KhongGianHook.cs` và
`KhongGianSynth.cs` còn nằm trong repo nhưng không được gắn / không được gọi nữa. Kiểm: `Tools/CompileCheck/check_khonggian_mission.sh`.

## Ý tưởng (giữ nguyên)
Mất trọng lực quanh rương (mảnh vụn trôi), tàu mẹ tiếp nhiên liệu (người chơi bấm B thả pin), tên lửa trúng nắp đúng
lúc nắp bật. Kết cục chỉ lộ khi bậc đã chốt: Rác máy xịt, rương rớt bịch; bậc cao rương nhảy bằng động cơ.

## Đã cắt so với bản đầu
- Hook cũ `KhongGianHook` (sao, vòng sonar, sao băng, radio rè): gỡ khỏi prefab (Setup tự gỡ nếu prefab cũ còn).
- Thùng quà + dù, đồng hồ nhiên liệu, pháo hoa Huyền thoại: bỏ hẳn.
- Tên lửa bay vòng bán kính 3,2 S → 1,4 S. Mảnh trôi 6–9 → 4. Pin tự 3 → 2, người chơi 6 → 3 (kẹp cứng trong mã).
- Tàu vào từ 6 S → 3 S, lơ lửng ở 2,1 S → 1,8 S; Rác lủi đi tới 3 S (không còn 7 S); bậc cao chào 1 vòng bán kính 1,2 S rồi lên tối đa 3 S.

## Nhịp (giây từ lúc bấm; T = lúc nắp bật mạnh, tự đọc từ stage đầu có lidAngle ≥ 30°, mặc định ≈ 5,3 s)
| Thời điểm | Diễn biến | Âm |
|---|---|---|
| 0.0 | Mất trọng lực: rương nổi ~4% kích thước, nghiêng lắc; 4 bu-lông/mảnh vụn bốc lên trôi | Nền: máy nổ (vòng lặp) vào nhỏ + 1 bíp radio |
| 0.3 → ~1.6 | Tàu mẹ bay vào từ 3 S, lơ lửng ở 1,8 S | 1 bíp (sự kiện) |
| ~1.6 → T−1.8 | 2 pin tự rơi + ≤ 3 pin bấm B; mỗi pin: tia xanh, rương lún | "pin vào" (sự kiện, cách ≥ 0,2 s) |
| T−1.8 | Tên lửa bay vòng 1,4 S rồi lao xuống nắp | tiếng phóng (sự kiện) |
| T | Trúng nắp, nắp bật (giống nhau mọi bậc), mảnh vụn văng | HĐ: tên lửa nổ (duy nhất); +0,25 s: xì khí nắp |
| Chốt | Rác: máy tắt phụt, im 0,2 s, tiếng xịt, rương rớt bịch, tàu ho khói lủi đi (biến mất ở 3 S, ~1,6 s). Bậc cao: rương nhảy, lửa đáy, tia ở miệng | TC = `stingClips[bậc]`; Rác thêm đuôi: máy ho (+0,9 s) và bíp trầm (~+1,2 s). Bậc cao: đuôi 1 bíp (+0,45 s) |
| Revealed + 2,5 | Mảnh vụn thu nhỏ rồi biến mất | — |

## Âm (A9–A12: chỉ file ElevenLabs trong `Audio/Chests/khong_gian/`)
| Ô trên hook | File | Vai |
|---|---|---|
| `engineClip` | `sfx_khonggian_engine_loop.mp3` | Nền (vòng lặp, đã có nhịp pít-tông nên không có lớp Nhịp) |
| `blipClip` | `sfx_khonggian_radio_blip.mp3` | sự kiện + đuôi |
| `fuelClip` | `sfx_khonggian_fuel_dock.mp3` | sự kiện (pin vào) |
| `rocketClip` | `sfx_khonggian_rocket_launch.mp3` | sự kiện |
| `impactClip` | `sfx_khonggian_rocket_hit.mp3` | HĐ |
| `hatchClip` | `sfx_khonggian_hatch_hiss.mp3` | sự kiện (xì khí nắp) |
| `sputterClip` | `sfx_khonggian_engine_die.mp3` | đuôi Rác |
| `stingClips[0..4]` | `sfx_khonggian_lock_rac … lock_huyenThoai.mp3` | TC |

Hồ sơ lõi: `ChestAudioRule.ApplyToProfile` mỗi lần chạy menu Setup → `tension.enabled = false`, mọi ô âm lõi trống.
Nền 0,15 → 0,45, cao độ 0,8 → 1,25 (+~55%); Rác và Hiếm+ dừng nền ngay, Thường fade 0,3 s.
`sfxVolume` (0,8) chỉ nhân một lần trong `Sfx()`; mỗi loại tiếng bị kẹp bởi trần của `ChestAudioRule`.

## Bất biến
- Bậc do game quyết; hook chỉ đọc bậc ở `OnTierLocked`. Ngẫu nhiên dùng `System.Random(seed)` chỉ cho trang trí.
- Không đụng vòng quay màu 5 bậc và bóng vật phẩm (vẫn do ChestRevealSequence).
- Rương không gắn Rigidbody: lò xo–giảm chấn cộng dồn lên vị trí/góc, gỡ ra đầu mỗi khung (`DefaultExecutionOrder(-500)`).
- Hạt chỉ dùng `ParticleDirection.Radial` (không velocityOverLifetime).
- `OnReset`/`OnDestroy` dọn mọi vật đã tạo, dừng nền, trả vị trí/góc.

## Bản chốt (khớp kế hoạch mục 5.11)
Hook 1 · diễn viên 2 (tàu mẹ, tên lửa) · mảnh trôi 4 / pin 2 tự + ≤ 3 người chơi · tên lửa 1,4 S, tàu vào từ 3 S · `tension.enabled = false`.

## Trung thực: chưa làm / chưa kiểm
- **Chưa chạy trong Unity, chưa nghe âm** (cả 6 file mới và 5 tiếng chốt đều do ElevenLabs tạo, chưa ai nghe). Chỉ biên dịch thử bằng stub (`check_khonggian_mission.sh`, `check_all.sh` đều OK).
- Prefab `KhongGianFx` chưa mở trong Unity: ô âm chỉ được gán khi chủ dự án chạy menu `Tools/Cho Do La/Create Khong Gian Chest FX Profile` (chạy lại được).
- Chưa kiểm bằng mắt: tàu chào 1 vòng 1,2 S rồi lên ≤ 3 S, tàu lủi đi ở 3 S, thời điểm các tiếng (đọc từ mã, chưa đo).
- Rigidbody dùng `linearVelocity/linearDamping` (Unity 6). Phím B chưa kiểm xung đột; có `DropFuelCell()` để UI gọi.
- File mp3 vòng nền `engine_loop` chưa kiểm khoảng lặng hai đầu bằng tai.
