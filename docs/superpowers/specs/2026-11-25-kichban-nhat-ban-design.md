# Kịch bản mở rương Chest_NhatBan — "Nhát kiếm Iai"

Trạng thái: **đã viết mã, biên dịch thử bằng stub (qua), CHƯA chạy trong Unity, CHƯA ai nghe âm.** Thay bản Daruma cũ (2026-11-24).

## Ý tưởng

Một samurai vô hình. Ngay khi bấm mở, một thanh katana trong vỏ và chiếc nón lá hiện cạnh rương; tiếng sáo shakuhachi và **tiếng tim taiko** bắt đầu. Thời gian chậm lại, **một nhát chém vụt qua dây niêm** — nhưng dây và giấy niêm **vẫn nằm yên**. Kiếm thong thả nhập vỏ, **"tách"**, lúc đó dây mới đứt đôi rơi xuống bằng vật lý (kiểu phim kiếm hiệp). Rồi tim dồn dập tới lúc khoá bậc, và kết cục khác hẳn theo bậc.

Điểm riêng không rương nào có: **độ trễ giữa nhát chém và vết cắt** ("chém xong mới đứt"), cùng cơ chế **người chơi ném quả cho kiếm chém**.

## Nhịp (BoxOpenSequence mặc định, có thể lệch vì lõi kéo dài giữ)

| Giây | Hình | Âm |
|---|---|---|
| 0,0 (bấm) | Kiếm + nón to dần (0,5 s); lá anh đào lơ lửng (không trọng lực) quanh rương | Sáo nền to dần; tim "thịch-thịch" 60 nhịp/phút |
| mỗi nhịp tim | Cả rương phồng nhẹ (scale +1,5…4%), lưỡi kiếm run | Tim, cao dần |
| ~0,9 (giai đoạn 0) | Im lặng, **Time.timeScale = 0,25**; rút kiếm; một nhát vụt qua dây niêm (vệt sáng, lá bị chém tung) — **giống nhau mọi bậc** | Sáo tắt → "shiiing" rút kiếm → vút |
| +0,35 s thật | Kiếm về chỗ, trượt vào vỏ | **"tách"** |
| ngay sau | Thời gian trở lại; dây Binding + giấy Seal (cắt lưới thật thành 2 nửa) rơi bằng Rigidbody, rương nảy nhẹ | Dây đứt + rơi lộp bộp |
| giai đoạn 2 | Kiếm nâng lên thế jodan | Tim dồn dần tới 170 nhịp/phút, sáo cao dần (pitch 1 → 1,3) |
| khoá bậc | xem bảng dưới (âm kết = `lockClipsByTier`) | Tim dừng, sáo tắt phụt |

Vòng màu 5 bậc, bóng vật phẩm to dần: **do lõi giữ nguyên**, hook không đụng.

## Kết theo bậc (khớp nhịp file `sfx_nhatban_iai_fin_*.wav`)

| Bậc | Hình | Âm |
|---|---|---|
| Rác | Một **chiếc dép** rơi từ trên xuống ngay lưỡi kiếm; kiếm chém hụt, bật "boing", **văng đi** (Rigidbody, xoay); dép đáp lên rương; 0,85 s sau **nón rơi** lăn; lá rụng hết; rương **xẹp và nghiêng** rồi lắc lại | "bẹp" + "boing" + sáo tụt giọng (xịt, hài) |
| Thường | 1 nhát qua miệng rương, "tách", rương nảy nhẹ, nón cúi chào | 1 vút, tách, koto |
| Hiếm | 2 nhát **đồng thời** (chữ X), nảy 5 cm | 2 vút, taiko 2 nhịp, koto |
| Rất hiếm | 3 nhát đồng thời, rương nhảy + xoay 90° | 3 vút, taiko, gong |
| Huyền thoại | 6 nhát hình sao, **slow-mo**, "tách" rồi rương bật cao + xoay 360°, mưa lá, chớp sáng | 6 vút dồn, taiko 5 nhịp, gong lớn + lấp lánh |

Bí mật: trước khoá, mọi bậc trông và nghe như nhau (cùng nhát chém mở niêm, cùng tim). Chỉ khác ở khoá.

## Tương tác phụ

- **Bấm K** (`NhatBanHook.TossPersimmon()`): ném một quả hồng từ camera theo đường parabol về kiếm. Vật có `NhatBanOffering` bay vào tầm kiếm → kiếm chém đôi (cắt lưới thật, 2 nửa văng vật lý) và cộng **khí**: thêm lá, sáo cao hơn, mỗi khí thêm 1 nhát ở đoạn kết (tối đa +3). Khí **không đổi bậc**.
- Vật của người chơi không có `NhatBanOffering` không bao giờ bị phá.
- Nhân vật phụ: "samurai vô hình" = nón lá + kiếm; cúi chào khi trúng, rơi nón khi Rác.

## Bất biến

- Bậc do `ChestRevealSequence.Play(rolledTier, ...)` quyết; hook chỉ đọc bậc trong `OnTierLocked`. Ngẫu nhiên (góc nhát, xoay mảnh) theo `decorationSeed`.
- Tư thế rương: hook chỉ **cộng thêm** (nảy, xoay, phồng, xẹp) trong `LateUpdate`, tôn trọng vị trí BoxOpenSequence đặt, trả lại khi reset/huỷ. `Time.timeScale` luôn được khôi phục (tắt bằng `globalSlowMo`).
- Renderer Seal/Binding gốc bị ẩn khi cắt, bật lại khi `OnReset`.

## File

- `Scripts/ChestFx/Chests/NhatBanHook.cs` (điều phối), `NhatBanProps.cs` (katana/nón/dép/quả dựng bằng primitive, cắt lưới, `NhatBanOffering`), `NhatBanFxPreset.cs`.
- `Editor/NhatBanFxSetup.cs`: menu **Tools/Cho Do La/Create Nhat Ban Chest FX Profile** — giờ **ghi đè** preset và âm (để thay bản Daruma), dựng lại prefab hook có gắn âm.
- `Audio/Chests/nhat_ban/sfx_nhatban_iai_*.wav` (12 file), tạo bằng `python3 Tools/Audio/make_nhatban_sfx.py` (numpy, không mẫu, không bản quyền).
- `Tools/CompileCheck/check_nhat_ban.sh`. Đã thêm tối thiểu vào `UnityStubAll.cs` (dùng chung): `Rigidbody.position`, `Physics.IgnoreCollision`, `Mesh.isReadable`.

## Thử trong Unity

1. Chạy menu Create Nhat Ban Chest FX Profile (hoặc Create All). Xem Console không có cảnh báo thiếu âm.
2. Mở cảnh thử rương (Docs/HuongDanThuUnity.md), chọn Chest_NhatBan, mở với từng bậc (phím P/T của menu cầm rương).
3. Kiểm: âm bắt đầu ngay lúc bấm; dây chỉ rơi sau tiếng "tách"; Rác có dép + kiếm văng + rương xẹp; HT có 6 nhát + rương bay xoay; màu và bóng vẫn dừng đúng bậc.
4. Trong lúc tim đập, bấm K vài lần: quả bị chém đôi, đoạn kết thêm nhát.
5. Reset rồi mở lại: rương về đúng chỗ, dây niêm hiện lại, timeScale = 1.

## Rủi ro đã biết (chưa kiểm được)

- Lưới glTFast có thể không `isReadable` → dự phòng "ép dẹt" từng nửa (xấu hơn).
- Hướng `FrontDirection` sai thì kiếm chém phía sau rương.
- Kích thước kiếm/nón theo bề ngang rương (1,6×) — có thể cần chỉnh.
- Nếu game điều khiển `Time.timeScale` (pause), tắt `globalSlowMo`.
- Va chạm tạm (thân hộp + sàn khi bên dưới trống) có thể đè lên collider có sẵn của cảnh.
- Âm do mã tổng hợp, chưa nghe; chỉnh trong `make_nhatban_sfx.py` rồi chạy lại.

## Nối với lõi (claude/chest-fx-core)

Xem mục cuối khi đã fetch.
