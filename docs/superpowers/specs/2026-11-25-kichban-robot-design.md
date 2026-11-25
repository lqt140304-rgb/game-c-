# Kịch bản mở rương: Chest_Robot — "RƯƠNG BIẾN HÌNH"

Trạng thái: **đã viết mã, biên dịch qua stub (check_robot_bienhinh.sh, 3 bộ define). CHƯA chạy trong Unity, CHƯA ai nghe âm.**

## Ý tưởng
Rương là một robot ngủ đông, hết pin. Bấm mở = gửi yêu cầu khởi động: phải **sạc pin** qua khe trên lưng. Bu-lông, bánh răng rơi xuống (Rigidbody thật), một **drone phụ tá** bay tới thả cục pin vào khe, người chơi cũng thả/ném đồ vào được. Đầy pin → rương **mọc hai chân đứng dậy**, dậm chân tại chỗ ngày càng nhanh, mắt nháy theo màu vòng quay, **màn chữ chạy tự kiểm hệ thống**. Khoá bậc → xịt (Rác) hoặc bật nhảy bùng nổ (Hiếm+), hai tay vươn ra đỡ món quà.

Không rương nào khác có: sạc pin bằng vật lý + biến hình đứng dậy + drone + màn chữ.

## Nhịp (rương không chìa, giai đoạn mặc định)
| Giây | Việc | Âm |
|---|---|---|
| 0,0 | Bấm mở (bắt cạnh lên `IsPlaying`, không đợi nắp). Khe sạc, thanh pin 5 ô, mắt mờ | `press` "cạch + chirp"; máy rì rì `hum_loop` bắt đầu pitch 0,55 |
| 0,45 | Bu-lông rơi vào khe (khe hút như nam châm) | `slot` keng + xẹt; rương nảy; pitch máy +1 nấc |
| 0,8–2,0 | Drone bay tới, thả pin, bay ra đứng xem | `drone_loop` |
| 1,7–3 | Bánh răng/bu-lông rơi tiếp; chữ "NẠP +x%" | |
| ~3,7 | Vòng màu bắt đầu: đủ pin (thiếu thì sạc nhanh bằng tia điện), **chân duỗi, rương đứng dậy**, dậm chân nhanh dần, rung mạnh dần | `servo`; bíp 0,45 s → 0,07 s, pitch tăng; máy rú lên tới 1,7 |
| ~6,5 | Khoá bậc (xem dưới) | |
| sau | Bóng món đủ cỡ: tay đỡ dưới bóng. `OnRevealed`: chữ kết | |

Tới lúc khoá **mọi bậc giống hệt nhau** (giữ bí mật).

## Theo bậc
- **Rác — XỊT:** máy tụt pitch rồi tắt, `fizzle` (xẹt xẹt, bụp, "bwoo-oop" buồn), mắt chập chờn rồi tắt, khói, chân sụm, rương đổ nghiêng, 4 bu-lông bung văng (Rigidbody), một tay thò ra rồi rũ; drone tới chọc chọc rồi chính nó hết pin rơi bịch (Rigidbody).
- **Thường:** hắt hơi khói nhỏ, "TẠM ỔN", nhảy khẽ 8 cm, `blast_1`.
- **Hiếm / Rất hiếm / Huyền thoại:** `blast_2..4` (rú lên rồi nổ hợp âm, dài và dày dần), rương **nhảy bằng Rigidbody** (v = √(2gh), h = 25/45/70 cm), mưa bu-lông/bánh răng ăn mừng, drone bay vòng. Huyền thoại: lơ lửng 1 s ở đỉnh, phun lửa ở chân, mắt chớp trắng.

## Bất biến
Hook chỉ đọc `tier` trong `OnTierLocked`. Pin, số vật thả vào… **không** ảnh hưởng bậc. Ngẫu nhiên `System.Random(decorationSeed)` chỉ cho vị trí rơi/độ văng. Vòng màu và bóng món của lõi giữ nguyên. Cả rương di chuyển bằng độ lệch cộng ở LateUpdate, gỡ ở Update khung sau → không đè rung của `BoxOpenSequence`. Đồ của người chơi rơi vào khe **không bị phá**: bị "hút điện" rồi nhả văng ra; chỉ vật do hook sinh bị nuốt.

## Tương tác người chơi
Thả/ném vật có Rigidbody vào khe trên lưng (trigger). Gắn `RobotChargePart` (Pin 35%, Bánh răng 25%, Bu-lông 15%) để định giá; đồ lạ 20%.

## File
- `Scripts/ChestFx/Chests/RobotTransformHook.cs` (kịch bản), `RobotChargeSocket.cs`, `RobotChargePart.cs`
- `Editor/RobotTransformSetup.cs` — menu `Tools/Cho Do La/Robot Bien Hinh (gan len ruong dang chon)`: gắn hook, gỡ `RobotHook` cũ, điền âm
- `Audio/Chests/robot/transform/*.wav` — tạo bằng `Tools/Audio/make_robot_sfx.py` (numpy, không bản quyền)
- `Tools/CompileCheck/check_robot_bienhinh.sh`; thêm `Font.material` vào `UnityStubAll.cs` (stub, chỉ cho biên dịch thử)

## Thử trong Unity
1. Theo `Docs/HuongDanThuUnity.md` đặt Chest_Robot có `ChestRevealSequence`.
2. Chọn gốc rương → menu Robot Bien Hinh. Phím P/T như hướng dẫn.
3. Kiểm: âm vang ngay khi bấm; vật rơi trúng khe; chân/tay bám rương; Rác xịt, Huyền thoại bay. Thả một Cube có Rigidbody vào khe khi đang sạc.

## Chưa kiểm / rủi ro
- Chưa chạy Unity: vị trí chân/tay/khe theo bounds đo tự động, có thể lệch; font `LegacyRuntime.ttf` (Unity 6) dự phòng `Arial.ttf`; tiếng Việt trên TextMesh chưa xem.
- Vật thể rơi cần collider của rương/sàn để không xuyên; khe dùng lực hút nên vẫn trúng.
- Hồ sơ `ChestFx_Robot` vẫn phát stageClips cũ (windup, steam…) của lõi — có thể chồng âm; nếu ồn, xoá stageClips trong hồ sơ.
- Chưa nối với khung `claude/chest-fx-core` (xem mục dưới).

## Nối với khung chung (claude/chest-fx-core)
Ngày 2026-11-25 nhánh `claude/chest-fx-core` **chưa tồn tại trên origin**, nên chưa merge. Khi khung có:
- Thay việc bắt cạnh `IsPlaying` trong `Update` bằng sự kiện "bấm mở" của khung (nếu có).
- Nếu khung có API nhịp/thời gian chung (độ dài pha, độ căng), cho `_bootT0`/2,8 s đọc từ đó thay vì hằng số.
- Nếu khung đăng ký kịch bản theo `chestId`, đăng ký `RobotTransformHook` cho "Robot" thay cho menu gắn tay.
