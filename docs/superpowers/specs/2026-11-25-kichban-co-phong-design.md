# Kịch bản mở rương Chest_CoPhong — NGỰ KIẾM TIÊN KHÍ

Thay hiệu ứng mực tàu cũ (CoPhongHook) bằng một kịch bản riêng: `Scripts/ChestFx/Chests/CoPhongNguKiemHook.cs`.
Cơ chế ĐẶC TRƯNG: **vòng phi kiếm quỹ đạo phá bùa phong ấn**. Không rương nào khác dùng cơ chế này.

## Bất biến (không đổi)
- Bậc trúng do game quyết qua `ChestRevealSequence.Play(rolledTier, ...)`. Hook chỉ đọc bậc ở `OnTierLocked`, không ghi gì.
- Vòng màu 5 bậc và bóng vật phẩm vẫn do lõi chạy như cũ. Ngẫu nhiên chỉ dùng để trang trí (`decorationSeed`, 0 = mỗi lần một khác).
- Trước lúc chốt bậc, mọi thứ chạy GIỐNG NHAU ở mọi bậc (số kiếm, số bùa, nhịp đâm, độ căng), nên không lộ kết quả.
- Vị trí và góc xoay của rương chỉ được **cộng thêm** một độ lệch trong LateUpdate. Lệch này được gỡ ra nếu không ai ghi đè. Vì vậy rung của BoxOpenSequence vẫn giữ nguyên. Khi Reset, rương trở về đúng chỗ.

## Nhịp (giây tính từ lúc bấm mở) — đã thu gọn theo kế hoạch 2026-11-25
| t | Hình | Âm |
|---|---|---|
| 0 | Mây mù trải sẵn quanh chân rương (15 hạt). **5** phi kiếm vút lên từ dưới mây. Rương bắt đầu rời đất. | `sword_draw` (1 sự kiện), `sword_hum_loop` vào nhỏ |
| 0–1.2 | Rương bay lơ lửng (lò xo). Kiếm vào quỹ đạo (bán kính 0,95 S). | Nền: âm lượng 0,15 → 0,45, cao độ 0,8 → 1,2 |
| 1.6 → chốt | Lần lượt từng kiếm tách vòng đâm một trong **4** lá bùa. Lá bùa bật ra (Rigidbody + lực) rồi cháy thành tro. Rương giật nhẹ mỗi cú đâm. Kiếm quay nhanh dần (60 → 330°/s). | Nhịp = `strike` mỗi cú đâm, cách 1,4 → 0,55 s, âm lượng 0,25 → 0,55 |
| bất kỳ (trước chốt) | **Tương tác**: chuột trái / phím **G** / `DropSpiritStone()` → linh thạch rơi theo trọng lực thật (**tối đa 4 viên mỗi lần mở**). Chạm miệng rương hoặc nảy quá 2,4 s thì bị hút: linh khí +1, rương nảy, kiếm nhanh hơn. | `spirit_stone` (≤ 0,5, qua cổng 0,2 s) |
| chốt bậc | rẽ nhánh theo bậc ↓ | TC = `stingClips[bậc]` (`sfx_cophong_lock_*`) |

Đã bỏ: hạc giấy (và tiếng `crane`), dải rồng khí.

## Kết theo bậc
- **Rác**: nền **Stop() ngay** → lặng 0,2 s → **1 tiếng xịt** (`lock_rac`), rồi im thêm 0,6 s. Kiếm mất linh khí rơi tự do (Rigidbody, quay lung tung) và cắm xiên xuống đất, có rung lò xo. Rương rớt bịch, nảy 2 lần, tung bụi. Bùa chưa bị đâm bay phấp phới bằng chuyển động theo mã (không thêm Rigidbody) và không cháy ("phong ấn còn nguyên"). Đuôi: tiếng kiếm cắm đất qua cổng 0,15 s, ≤ 6 tiếng, ≤ 0,4.
- **Thường**: nền fade 0,3 s; TC `lock_thuong` (≤ 0,7). Kiếm rơi gần như thẳng đứng và cắm xuống. Rương hạ êm.
- **Hiếm**: nền dừng; TC `lock_hiem` (0,85). Các bùa còn lại cháy cùng lúc. Kiếm dựng thành vương miện chĩa xuống (cao 0,6 S trên miệng) rồi lần lượt lao vào miệng rương. Rương bật nhảy.
- **Rất hiếm**: nền dừng; TC `lock_ratHiem` (1,0). Như Hiếm, nhưng nhảy cao hơn và xoay 360° trên không.
- **Huyền thoại**: nền dừng; TC `lock_huyenThoai` (1,0, đã có tiếng chuông/chiêng). Nhảy cao nhất, xoay 360° nhanh gấp đôi. Sóng xung kích dạng vòng (bán kính tối đa 2,8 S) đẩy văng bằng lực mọi bùa và linh thạch đang rời.

## Bản chốt (sau thu gọn)
| Hạng mục | Giá trị |
|---|---|
| Diễn viên ngoài kiếm/bùa bám rương | 0 |
| Kiếm / bùa / linh thạch | 5 / 4 / ≤ 4 (tổng mỗi lần mở) |
| Lớp âm kéo dài trước chốt | 2 (hum + strike) |
| Tiếng trong 0,3 s đầu sau chốt | Rác: 0 (xịt ở +0,2 s, rồi im 0,6 s); khác: 1 (TC) |
| `tension.enabled` | false (`ChestAudioRule.ApplyToProfile` mỗi lần chạy menu) |

## Âm (`Audio/Chests/co_phong/`, ElevenLabs — danh mục `Tools/Audio/elevenlabs_chest_manifest.json`)
`sword_hum_loop` (nền), `strike` (nhịp), `sword_draw`, `spirit_stone` (sự kiện), `sword_land` (đuôi), `lock_rac|thuong|hiem|ratHiem|huyenThoai` (TC, mảng `stingClips[5]`). Không còn nạp `.wav` numpy trong `ngu_kiem/` (file và script cũ để nguyên). `brush_stroke` (bùa cháy) trong danh mục không có ô `field` nên chưa gán (bùa cháy im, tránh lớp thứ 3).

## Thử trong Unity
1. Menu **Tools → Cho Do La → Setup Co Phong Ngu Kiem** (nạp các `.mp3` ElevenLabs). Lệnh này tạo/cập nhật `ChestFx_CoPhong.asset` và prefab `ChestFx_CoPhongNguKiem.prefab` (đã gán âm), rồi đặt prefab đó làm `customFxPrefab`.
2. Mở cảnh thử như `Docs/HuongDanThuUnity.md`, chọn Chest_CoPhong, ép bậc bằng **T**, mở bằng **Space**. Trong lúc mở, bấm **G** hoặc chuột trái để thả linh thạch.
3. Kiểm: âm phát ngay khi bấm; mỗi bậc kết khác nhau; vòng màu dừng đúng bậc; rương về đúng vị trí sau khi **R**.

## Trung thực: chưa chạy / chưa nghe
- **Đợt thu gọn 2026-11-25 cũng chưa chạy Unity và chưa nghe âm nào**; chỉ qua `check_co_phong_ngu_kiem.sh` và `check_all.sh` (stub). Cần xem: rương bay cao 0,35 S trước chốt (giữ vì là ý "lơ lửng trên mây", vượt thước T8 ≤ 0,05 S); đỉnh nhảy Huyền thoại ≈ 0,65 S tính theo cỡ rương, chưa so với chiều cao H.
- Môi trường này không có Unity. Code mới chỉ qua biên dịch thử bằng stub (`Tools/CompileCheck/check_co_phong_ngu_kiem.sh`, `check_all.sh`): đã bắt lỗi cú pháp và kiểu, chưa bắt được sai API Unity thật hay lỗi lúc chạy.
- Chưa xem hình, chưa nghe âm. Kích thước kiếm/bùa, độ cao bay và độ lớn lực đều là ước lượng theo kích thước rương, cần chỉnh khi thử.
- Rủi ro cần xem: (a) bùa được đặt bằng hộp bao, nên với hũ tròn bùa có thể lơ lửng cách thân một chút; (b) Quad mặc định có thể chỉ hiện một mặt; (c) phím chuột trái có thể trùng thao tác khác của game; (d) bấm mở ở tư thế cầm rương thì độ lệch bay sẽ cộng vào vị trí đang cầm.
- Rác/Thường: bùa chưa bị đâm bay bằng chuyển động theo mã nên số Rigidbody sau chốt Rác ≤ 5 (đúng 5 kiếm).
- Không dùng orbital của particle. Chỉ dùng hệ hạt Up/Radial qua `ChestFxFactory`, nên tránh được lỗi orbitalX/Y/Z khác chế độ.

## Nối với khung lõi (claude/chest-fx-core)
Lúc làm, nhánh `origin/claude/chest-fx-core` **chưa tồn tại** (fetch báo không thấy ref), nên chưa nối được.
Hiện tại hook tự phát hiện lúc bấm mở bằng cách theo dõi `reveal.IsPlaying`, vì lõi chưa có callback kiểu "OnPlayStart".
Khi khung lõi có API, cần nối như sau:
- thay đoạn theo dõi trong `Update()` bằng callback bắt đầu của khung;
- nếu khung có điều khiển tư thế rương hoặc lớp âm chung, chuyển `ApplyChestOffset` và `_hum` sang API đó.
