# Kịch bản mở rương Chest_CoPhong — NGỰ KIẾM TIÊN KHÍ

Thay hiệu ứng mực tàu cũ (CoPhongHook) bằng một kịch bản riêng: `Scripts/ChestFx/Chests/CoPhongNguKiemHook.cs`.
Cơ chế ĐẶC TRƯNG: **vòng phi kiếm quỹ đạo phá bùa phong ấn**. Không rương nào khác dùng cơ chế này.

## Bất biến (không đổi)
- Bậc trúng do game quyết qua `ChestRevealSequence.Play(rolledTier, ...)`. Hook chỉ đọc bậc ở `OnTierLocked`, không ghi gì.
- Vòng màu 5 bậc và bóng vật phẩm vẫn do lõi chạy như cũ. Ngẫu nhiên chỉ dùng để trang trí (`decorationSeed`, 0 = mỗi lần một khác).
- Trước lúc chốt bậc, mọi thứ chạy GIỐNG NHAU ở mọi bậc (số kiếm, số bùa, nhịp đâm, độ căng), nên không lộ kết quả.
- Vị trí và góc xoay của rương chỉ được **cộng thêm** một độ lệch trong LateUpdate. Lệch này được gỡ ra nếu không ai ghi đè. Vì vậy rung của BoxOpenSequence vẫn giữ nguyên. Khi Reset, rương trở về đúng chỗ.

## Nhịp (giây tính từ lúc bấm mở)
| t | Hình | Âm |
|---|---|---|
| 0 | Mây mù trải sẵn quanh chân rương. 7 phi kiếm vút lên từ dưới mây. Rương bắt đầu rời đất. | `draw_sword` (xoẹt rút kiếm), `hum_loop` bắt đầu nhỏ |
| 0–1.2 | Rương bay lơ lửng (lò xo). Kiếm vào quỹ đạo. Dải rồng khí dài dần. | hum: pitch 0.8 → tăng |
| 1.6 → chốt | Lần lượt từng kiếm tách vòng đâm một lá bùa (6 lá). Lá bùa bật ra (Rigidbody + lực) rồi cháy thành tro. Rương giật nhẹ mỗi cú đâm. Kiếm quay nhanh dần (60 → 330°/s). Rương rung mạnh dần. | `strike` mỗi cú đâm; hum lên tới pitch 1.5, to dần |
| 2–4 (theo hạt giống) | **Sự kiện phụ**: hạc giấy bay ngang, nhả 1 viên linh thạch | `crane` |
| bất kỳ (trước chốt) | **Tương tác**: chuột trái / phím **G** / `DropSpiritStone()` → linh thạch rơi theo trọng lực thật (tối đa 6). Nếu chạm miệng rương thì bị hút vào; nếu nảy trên mây quá 2.4 giây cũng bị hút. Mỗi viên: linh khí +1, rương nảy, kiếm nhanh hơn, rồng dài hơn. | `stone` (cao độ tăng dần) |
| chốt bậc | rẽ nhánh theo bậc ↓ | `end_<bậc>` |

## Kết theo bậc
- **Rác**: hum "xì hơi" tụt cao độ rồi tắt. Kiếm mất linh khí rơi tự do (Rigidbody, quay lung tung) và cắm xiên xuống đất, có rung lò xo. Rương rớt bịch, nảy 2 lần, tung bụi. Bùa còn sót bay phấp phới mà không cháy ("phong ấn còn nguyên"). Rồng khí rũ xuống rồi tan. Âm: xì + 3 tiếng kiếm lạch cạch.
- **Thường**: kiếm rơi gần như thẳng đứng và cắm xuống. Rương hạ êm. Rồng nhỏ chui vào miệng. Âm: dâng ngắn rồi một tiếng chuông.
- **Hiếm**: các bùa còn lại cháy cùng lúc. Kiếm dựng thành vương miện chĩa xuống rồi lần lượt lao vào miệng rương. Rồng lượn rộng thêm rồi lao vào. Rương bật nhảy. Âm: dâng, nổ trầm, hợp âm chuông.
- **Rất hiếm**: như Hiếm, nhưng nhảy cao hơn và xoay 360° trên không.
- **Huyền thoại**: nhảy cao nhất, xoay 720°. Sóng xung kích dạng vòng lan ra và đẩy văng bằng lực mọi bùa và linh thạch đang rời. Âm: dâng dài, rồng gầm, nổ, chiêng lớn.

## Âm (`Audio/Chests/co_phong/ngu_kiem/`, tổng hợp bằng `Tools/Audio/make_cophong_ngukiem_sfx.py`)
`hum_loop` (2 giây, lặp liền), `draw_sword`, `strike`, `stone`, `crane`, `sword_land`, `end_rac|thuong|hiem|ratHiem|huyenThoai`. Âm cũ của hồ sơ (tick, lock, ambient, xé niêm…) vẫn giữ nguyên.

## Thử trong Unity
1. Menu **Tools → Cho Do La → Setup Co Phong Ngu Kiem**. Lệnh này tạo/cập nhật `ChestFx_CoPhong.asset` và prefab `ChestFx_CoPhongNguKiem.prefab` (đã gán âm), rồi đặt prefab đó làm `customFxPrefab`.
2. Mở cảnh thử như `Docs/HuongDanThuUnity.md`, chọn Chest_CoPhong, ép bậc bằng **T**, mở bằng **Space**. Trong lúc mở, bấm **G** hoặc chuột trái để thả linh thạch.
3. Kiểm: âm phát ngay khi bấm; mỗi bậc kết khác nhau; vòng màu dừng đúng bậc; rương về đúng vị trí sau khi **R**.

## Trung thực: chưa chạy / chưa nghe
- Môi trường này không có Unity. Code mới chỉ qua biên dịch thử bằng stub (`Tools/CompileCheck/check_co_phong_ngu_kiem.sh`, `check_all.sh`): đã bắt lỗi cú pháp và kiểu, chưa bắt được sai API Unity thật hay lỗi lúc chạy.
- Chưa xem hình, chưa nghe âm. Kích thước kiếm/bùa, độ cao bay và độ lớn lực đều là ước lượng theo kích thước rương, cần chỉnh khi thử.
- Rủi ro cần xem: (a) bùa được đặt bằng hộp bao, nên với hũ tròn bùa có thể lơ lửng cách thân một chút; (b) Quad mặc định có thể chỉ hiện một mặt; (c) phím chuột trái có thể trùng thao tác khác của game; (d) bấm mở ở tư thế cầm rương thì độ lệch bay sẽ cộng vào vị trí đang cầm.
- Không dùng orbital của particle. Chỉ dùng hệ hạt Up/Radial qua `ChestFxFactory`, nên tránh được lỗi orbitalX/Y/Z khác chế độ.

## Nối với khung lõi (claude/chest-fx-core)
Lúc làm, nhánh `origin/claude/chest-fx-core` **chưa tồn tại** (fetch báo không thấy ref), nên chưa nối được.
Hiện tại hook tự phát hiện lúc bấm mở bằng cách theo dõi `reveal.IsPlaying`, vì lõi chưa có callback kiểu "OnPlayStart".
Khi khung lõi có API, cần nối như sau:
- thay đoạn theo dõi trong `Update()` bằng callback bắt đầu của khung;
- nếu khung có điều khiển tư thế rương hoặc lớp âm chung, chuyển `ApplyChestOffset` và `_hum` sang API đó.
