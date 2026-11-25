# Kịch bản mở rương Thể thao: "Loạt luân lưu phút bù giờ"

Trạng thái: **đã viết mã, biên dịch thử bằng stub; CHƯA chạy trong Unity, CHƯA nghe âm.** Thay cho hiệu ứng cũ (còi + pháo giấy) ở [spec 2026-11-24](2026-11-24-hieu-ung-the-thao-design.md).

## Ý tưởng
Rương là **khung thành**. Khi bấm mở, một trận luân lưu diễn ra quanh rương bằng **vật lý thật**: bóng Rigidbody với vật liệu nảy (bounciness 0,78), lực **Magnus** (bóng xoáy thì cong), va chạm với cột dọc/xà/rương/mặt sân. Cú sút cuối quyết định cảnh kết, nhưng **bậc trúng do game quyết** (`ChestRevealSequence.Play(rolledTier, …)`); hook chỉ **đọc** `RolledTier` để chọn kết cục. Ngẫu nhiên (độ cong, hướng xà) chỉ trang trí theo `decorationSeed`. Màu 5 bậc nhảy/dừng và bóng vật phẩm lớn dần vẫn do lõi lo, không đổi.

Đặc trưng riêng: **cả rương là thủ môn bị đập** — rương nảy, nghiêng, nén/giãn theo lực va chạm thật (lò xo-giảm chấn cộng lên trên `BoxOpenSequence`, gỡ ra mỗi khung, trả lại khi reset).

> **Đã thu gọn theo kế hoạch 2026-11-25 (mục 5.13).** Phần dưới là bản mới; bản cũ (trọng tài, 3 bảng, VAR, đám đông lặp, sàn 14 S) đã bỏ.

## Nhịp (rương không chìa, giai đoạn mặc định; chốt ≈ 6,7 s)
| Giây | Hình | Âm |
|---|---|---|
| 0,0 (bấm mở) | khung thành mọc lên quanh rương, bóng ở chấm phạt đền (1,6 S, chéo 40°) | còi khai cuộc (sự kiện), **Nhịp** = trống 0,7 s → 0,15 s/nhịp |
| 0,9 | cú sút 1 **trúng cột dọc**, bóng nảy, rương lắc | keng, rồi "ồ" trễ 0,25 s |
| 3,7 (bắt đầu quay màu) | bóng 2 rơi từ trên; 0,75 s sau sút xoáy **dội xà** | tiếng sút |
| chốt −1,2 | | **riser** (lớp thứ 2, 0,4) |
| chốt −0,7 | **lặng như tờ** (trống ngừng, riser tắt) → cú sút quyết định (bí mật tới lúc bóng tới khung) | tiếng sút |
| chốt | kết cục theo bậc (dưới) | TC = `stingClips[bậc]` |
| sau mở toang 3–4,5 s | sân thu nhỏ biến mất | |

Rương có chìa / lệch giờ: giờ chốt được ước lại ở `OnRouletteStart`; nếu chốt tới trước cú sút, cú sút diễn nhanh 0,25 s.

## Kết cục theo bậc
- **Rác**: bóng bay vọt qua xà. Im (từ cú sút, ≥ 0,2 s) → **1 tiếng xịt** (TC `lock_rac`), rương **xẹp**; 0,6 s không gì khác; đuôi "aaaw" +0,5 s, còi kết +1,2 s (âm 0,35).
- **Thường**: bóng lăn sệt vào miệng rương; TC `lock_thuong` (0,6), vỗ tay đuôi +0,5 s.
- **Hiếm**: sút căng vào lưới; HĐ tiếng lưới (0,7) + TC `lock_hiem`; đuôi tiếng gầm +0,4 s.
- **Rất hiếm**: siêu phẩm cong ngoặt vào; HĐ lưới + TC `lock_ratHiem`; đuôi gầm +0,4 s, kèn hơi +0,6 s. (VAR đã bỏ.)
- **Huyền thoại**: như Rất hiếm (TC `lock_huyenThoai`) + rương **bật lên lộn 360°**, lực nổ thật thổi mọi quả bóng, **mưa 4 quả bóng**; tiếng nảy lúc rơi là đuôi.

## Bản chốt (số sau khi thu gọn)
| Hạng mục | Giá trị |
|---|---|
| Diễn viên | 1 (khung thành bao rương) + bóng. Bỏ trọng tài |
| Bóng | 2 kịch bản + ≤ 4 người chơi (trần cứng 4 trong mã) + HT mưa 4 |
| Bảng/VAR/trọng tài | bỏ hẳn (`Board`, `Pop`, `Var`, `Referee*` đã xoá khỏi hook) |
| Chấm phạt đền | 1,6 S (trần cứng, kể cả prefab cũ lưu 2,6) |
| Sàn | 4 S (cũ 14 S) |
| Lớp âm trước chốt | 2 (trống + riser); không có nền |
| `tension.enabled` | false (Setup ghi lại mỗi lần chạy bằng `ChestAudioRule.ApplyToProfile`) |
| Âm | chỉ `.mp3` ElevenLabs; `stingClips[5]` = `sfx_thethao_lock_*.mp3` |

## Tương tác phụ (chỉ trang trí)
- **B**: "thay người" — thả thêm bóng (tối đa 4) ngay trên rương; đập vào rương thì rương nảy theo lực.
- **K**: sút quả bóng tự do gần chấm phạt đền vào khung, có xoáy → cong thật.
Hai hệ input (cũ/mới) đều hỗ trợ.

## File
- `Scripts/ChestFx/Chests/TheThaoHook.cs` (điều phối, viết lại; giữ tên `kickoffClip`/`airHornClip` để `CreateAllChestFxProfiles` vẫn dùng được), `TheThaoBall.cs` (bóng: Bezier theo kịch bản rồi thả cho vật lý, Magnus, nuốt vào miệng), `TheThaoFxPreset.cs` (+`FillHookClips`).
- `Editor/TheThaoFxSetup.cs`: thêm hook vào rương đang chọn và điền âm. Hook cũng tự điền trong `OnValidate`.
- Âm: `Audio/Chests/the_thao/*.mp3` (ElevenLabs, theo `Tools/Audio/elevenlabs_chest_manifest.json`). Các `match/*.wav` (numpy) và `make_thethao_match_sfx.py` để nguyên nhưng không còn được nạp.
- Kiểm: `Tools/CompileCheck/check_the_thao.sh` (vá bản sao stub, không sửa stub chung); `UnityStubAll.cs` được **thêm** PhysicsMaterial/Collision/… cho `check_all.sh`.
- Không sửa `ChestRevealSequence`, `ChestFxFactory`, `BoxOpenSequence`.

## Thử trong Unity
1. Mở cảnh thử rương, chọn object `Chest_TheThao` → `Tools/Cho Do La/Create The Thao Chest FX Profile` (thêm TheThaoHook + điền âm).
2. Play, phím T/Y chọn bậc, Space mở; trong lúc mở bấm **B**, **K**. Thử đủ 5 bậc, R để reset (rương phải về đúng vị trí/góc/tỉ lệ).
3. Cần kiểm: bóng có ở trong khung hình không (khoảng `spotDistanceFactor`), cỡ bóng (`ballRadiusFactor`), âm lượng tương đối, tiếng trống có dày quá không, va chạm với sàn thật của cảnh, Unity 6 có báo lỗi gì về Rigidbody kinematic + ContinuousSpeculative không.

## Nối với khung lõi (`claude/chest-fx-core`, đã merge 2026-11-25)
- Hook bắt đầu ở `OnPress` (ngay khi bấm mở); `Update` dò `IsPlaying` vẫn giữ làm dự phòng.
- Menu Setup gọi `ChestAudioRule.ApplyToProfile` MỖI lần chạy: `tension.enabled = false` và mọi ô âm lõi trống; hook tự phát trống, riser và tiếng chốt.
- Chưa dùng `ChestScript`/`OnCue`/`ChestMotion`/`ChestPhysicsFx` của lõi: chuyển động rương là lò xo riêng trong hook. Có thể chuyển sang `ChestMotion` sau nếu lõi muốn thống nhất (lưu ý: đừng bật cả hai cùng lúc, sẽ cộng chồng).
- Giờ chốt vẫn ước từ `box.Stages` (giả định `rouletteStartStage=2`, `lockStage=3`), chỉnh lại ở `OnRouletteStart`; nếu lõi có API "giây tới chốt" thì thay vào.

## Chưa kiểm (trung thực)
- **Chưa chạy Unity, chưa nghe** (mã chỉ qua `check_all.sh` và `check_the_thao.sh`, tức bắt lỗi cú pháp/kiểu).
- Âm ElevenLabs chưa ai nghe; thiếu hẳn tiếng kèn buồn (trombone) trong danh mục nên đuôi Rác dùng "aaaw" + còi kết (dùng lại file còi khai cuộc, trầm hơn).
- Âm lượng cuối = số truyền × `extraVolume` 0,8 (một lần duy nhất): TC Hiếm trở lên ≈ 0,8, Thường 0,6, trống 0,25–0,55, riser 0,4, đuôi ≤ 0,4. Chưa nghe độ cân bằng.
- Giờ dọn sân (`Outro`: 3 s / 4,5 s sau khi mở toang + 0,6 s thu nhỏ) chưa chỉnh; có thể vượt "≤ 4 s sau chốt" của T6 tuỳ độ dài đoạn kết lõi. Cần đo trong Unity.
- Prefab/cảnh đã lưu `maxExtraBalls = 10`, `spotDistanceFactor = 2.6` được chặn bằng trần cứng trong mã (4 và 1,6).
