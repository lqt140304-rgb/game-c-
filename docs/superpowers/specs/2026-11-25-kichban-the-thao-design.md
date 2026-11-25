# Kịch bản mở rương Thể thao: "Loạt luân lưu phút bù giờ"

Trạng thái: **đã viết mã, biên dịch thử bằng stub; CHƯA chạy trong Unity, CHƯA nghe âm.** Thay cho hiệu ứng cũ (còi + pháo giấy) ở [spec 2026-11-24](2026-11-24-hieu-ung-the-thao-design.md).

## Ý tưởng
Rương là **khung thành**. Khi bấm mở, một trận luân lưu diễn ra quanh rương bằng **vật lý thật**: bóng Rigidbody với vật liệu nảy (bounciness 0,78), lực **Magnus** (bóng xoáy thì cong), va chạm với cột dọc/xà/rương/mặt sân. Cú sút cuối quyết định cảnh kết, nhưng **bậc trúng do game quyết** (`ChestRevealSequence.Play(rolledTier, …)`); hook chỉ **đọc** `RolledTier` để chọn kết cục. Ngẫu nhiên (độ cong, hướng xà) chỉ trang trí theo `decorationSeed`. Màu 5 bậc nhảy/dừng và bóng vật phẩm lớn dần vẫn do lõi lo, không đổi.

Đặc trưng riêng: **cả rương là thủ môn bị đập** — rương nảy, nghiêng, nén/giãn theo lực va chạm thật (lò xo-giảm chấn cộng lên trên `BoxOpenSequence`, gỡ ra mỗi khung, trả lại khi reset).

## Nhịp (rương không chìa, giai đoạn mặc định; chốt ≈ 6,7 s)
| Giây | Hình | Âm |
|---|---|---|
| 0,0 (bấm mở) | khung thành mọc lên quanh rương, trọng tài trượt vào, bóng ở chấm phạt đền (chéo 40° để không che máy quay) | **còi khai cuộc**, đám đông rì rầm, trống bắt đầu 0,7 s/nhịp |
| 0,9 | cú sút 1 **trúng cột dọc**, bóng nảy lung tung, rương lắc | keng + "ồ" |
| 3,7 (bắt đầu quay màu) | bảng **thay người**, bóng 2 rơi từ trên; 0,75 s sau sút xoáy **dội xà** | |
| chốt −2,0 | bảng **bù giờ** giơ lên | riser dâng, đám đông to/cao lên, trống dồn tới 0,13 s |
| chốt −0,7 | **lặng như tờ** (trống tắt, đám đông nín) → cú sút quyết định (bí mật tới lúc bóng tới khung) | tiếng sút |
| chốt | kết cục theo bậc (dưới) | |
| sau mở toang 3–4,5 s | sân thu nhỏ biến mất | |

Rương có chìa / lệch giờ: giờ chốt được ước lại ở `OnRouletteStart`; nếu chốt tới trước cú sút, cú sút diễn nhanh 0,25 s.

## Kết cục theo bậc
- **Rác**: bóng bay vọt qua xà ra ngoài. Âm **cắt phụt** (đám đông tắt), xì hơi + "aaaw", kèn buồn wah-wah, còi kết thúc 3 tiếng; rương **xẹp** (nén 0,8) và nghiêng, trọng tài buông tay.
- **Thường**: bóng lăn sệt chậm vào miệng rương; vỗ tay lẹt đẹt, rương nảy nhẹ.
- **Hiếm**: sút căng vào lưới; tiếng lưới + đám đông gầm, rương nảy rõ.
- **Rất hiếm**: siêu phẩm cong ngoặt vào + kèn hơi; **VAR**: màn hình xanh, tua lại, bóng "ma" chiếu chậm 0,3× đúng quỹ đạo vừa ghi, tít tít, gầm lần hai.
- **Huyền thoại**: như Rất hiếm + rương **bật lên lộn 360°**, lực nổ thật (`AddExplosionForce`) thổi mọi quả bóng, **mưa 8 quả bóng**.

## Tương tác phụ (chỉ trang trí)
- **B**: "thay người" — thả thêm bóng (tối đa 10) ngay trên rương; đập vào rương thì rương nảy theo lực.
- **K**: sút quả bóng tự do gần chấm phạt đền vào khung, có xoáy → cong thật.
Hai hệ input (cũ/mới) đều hỗ trợ.

## File
- `Scripts/ChestFx/Chests/TheThaoHook.cs` (điều phối, viết lại; giữ tên `kickoffClip`/`airHornClip` để `CreateAllChestFxProfiles` vẫn dùng được), `TheThaoBall.cs` (bóng: Bezier theo kịch bản rồi thả cho vật lý, Magnus, nuốt vào miệng), `TheThaoFxPreset.cs` (+`FillHookClips`).
- `Editor/TheThaoFxSetup.cs`: thêm hook vào rương đang chọn và điền âm. Hook cũng tự điền trong `OnValidate`.
- Âm: `Audio/Chests/the_thao/match/*.wav` (17 file, `Tools/Audio/make_thethao_match_sfx.py`).
- Kiểm: `Tools/CompileCheck/check_the_thao.sh` (vá bản sao stub, không sửa stub chung); `UnityStubAll.cs` được **thêm** PhysicsMaterial/Collision/… cho `check_all.sh`.
- Không sửa `ChestRevealSequence`, `ChestFxFactory`, `BoxOpenSequence`.

## Thử trong Unity
1. Mở cảnh thử rương, chọn object `Chest_TheThao` → `Tools/Cho Do La/Create The Thao Chest FX Profile` (thêm TheThaoHook + điền âm).
2. Play, phím T/Y chọn bậc, Space mở; trong lúc mở bấm **B**, **K**. Thử đủ 5 bậc, R để reset (rương phải về đúng vị trí/góc/tỉ lệ).
3. Cần kiểm: bóng có ở trong khung hình không (khoảng `spotDistanceFactor`), cỡ bóng (`ballRadiusFactor`), âm lượng tương đối, tiếng trống có dày quá không, va chạm với sàn thật của cảnh, Unity 6 có báo lỗi gì về Rigidbody kinematic + ContinuousSpeculative không.

## Nối với khung lõi (`claude/chest-fx-core`, đã merge 2026-11-25)
- Hook bắt đầu ở `OnPress` (ngay khi bấm mở); `Update` dò `IsPlaying` vẫn giữ làm dự phòng.
- `TheThaoFxPreset.Apply` đặt `profile.tension.enabled = false`: rương này **tự** dựng độ căng (trống dồn, đám đông, riser) và tự rẽ theo bậc ở cú sút cuối, để không chồng hai lớp âm. Hồ sơ `ChestFx_TheThao.asset` đã tạo từ trước cần tắt tay ô này (menu chỉ áp preset khi tạo mới).
- Chưa dùng `ChestScript`/`OnCue`/`ChestMotion`/`ChestPhysicsFx` của lõi: chuyển động rương là lò xo riêng trong hook. Có thể chuyển sang `ChestMotion` sau nếu lõi muốn thống nhất (lưu ý: đừng bật cả hai cùng lúc, sẽ cộng chồng).
- Giờ chốt vẫn ước từ `box.Stages` (giả định `rouletteStartStage=2`, `lockStage=3`), chỉnh lại ở `OnRouletteStart`; nếu lõi có API "giây tới chốt" thì thay vào.
