# Kịch bản mở rương Chest_NhatBan — "Nhát kiếm Iai"

Trạng thái: **đã thu gọn theo kế hoạch 2026-11-25 (mục 5.15)**, biên dịch thử bằng stub (qua), CHƯA chạy trong Unity, CHƯA ai nghe âm. Thay bản Daruma cũ (2026-11-24).

## Ý tưởng

Một samurai vô hình. Ngay khi bấm mở, một thanh katana trong vỏ và chiếc nón lá hiện cạnh rương; tiếng sáo shakuhachi và **tiếng tim taiko** bắt đầu. Kiếm vung chậm (chỉ hoạt hình của kiếm, **không đụng `Time.timeScale`**), **một nhát chém vụt qua dây niêm** — nhưng dây và giấy niêm **vẫn nằm yên**. Kiếm thong thả nhập vỏ, **"tách"**, lúc đó dây mới đứt đôi rơi xuống bằng vật lý (kiểu phim kiếm hiệp). Rồi tim dồn dập tới lúc khoá bậc, và kết cục khác hẳn theo bậc.

Điểm riêng không rương nào có: **độ trễ giữa nhát chém và vết cắt** ("chém xong mới đứt"), cùng cơ chế **người chơi ném quả cho kiếm chém**.

## Nhịp (BoxOpenSequence mặc định, có thể lệch vì lõi kéo dài giữ)

| Giây | Hình | Âm |
|---|---|---|
| 0,0 (bấm) | Kiếm + nón to dần (0,5 s); lá anh đào lơ lửng (không trọng lực) quanh rương | Nền = sáo (1 vòng lặp, âm lượng 0,15 → 0,45); Nhịp = taiko, 0,75 s giữa hai tiếng |
| mỗi nhịp tim | Cả rương phồng nhẹ (scale +1,5…4%), lưỡi kiếm run | Tim, cao dần |
| ~0,9 (giai đoạn 0) | Im lặng; rút kiếm; một nhát vụt qua dây niêm (vệt sáng, lá bị chém tung) — **giống nhau mọi bậc** | Nền + nhịp tạm tắt → rút kiếm (0,5) → vút (0,5) |
| +0,35 s | Kiếm trôi chậm 0,35 s rồi về chỗ, trượt vào vỏ | **"tách"** (0,5) |
| ngay sau | Dây Binding + giấy Seal (cắt lưới thật thành 2 nửa) rơi bằng Rigidbody, rương nảy nhẹ | Dây đứt (0,5), cách "tách" ≥ 0,2 s |
| giai đoạn 2 | Kiếm nâng lên thế jodan | Taiko dồn tới 0,17 s giữa hai tiếng (âm lượng 0,25 → 0,55), sáo cao dần (pitch 1 → 1,45) |
| khoá bậc | xem bảng dưới (TC = `stingClips[bậc]`, hook phát) | Nhịp + nền dừng ngay (Thường: nền fade 0,3 s) |

Vòng màu 5 bậc, bóng vật phẩm to dần: **do lõi giữ nguyên**, hook không đụng.

## Kết theo bậc (TC = `sfx_nhatban_lock_<bậc>.mp3` qua `stingClips`; không còn dùng `fin_*.wav`)

| Bậc | Hình | Âm |
|---|---|---|
| Rác | Một **chiếc dép** rơi từ trên xuống ngay lưỡi kiếm; kiếm chém hụt, bật "boing", **văng đi** (Rigidbody, xoay); dép đáp lên rương; 0,85 s sau **nón rơi** lăn; lá rụng hết; rương **xẹp và nghiêng** rồi lắc lại | Tắt phụt → lặng 0,2 s → 1 tiếng xịt (TC); 0,6 s sau đó không gì khác; ~1 s sau: dép bẹp (đuôi 0,4) |
| Thường | 1 nhát qua miệng rương, "tách", rương nảy nhẹ, nón cúi chào | TC 0,7; "tách" + koto (đuôi ≤ 0,4) |
| Hiếm | 2 nhát **đồng thời** (chữ X), nảy 5 cm | TC 0,85; "tách" + koto (đuôi) |
| Rất hiếm | 3 nhát đồng thời, rương nhảy + xoay 90° | TC 1,0; "tách" + koto (đuôi) |
| Huyền thoại | 5 nhát hình sao, kiếm trôi chậm, "tách" rồi rương bật cao + xoay 360°, mưa lá (≤ 160 hạt), chớp sáng | TC 1,0 (đoạn kết đã chứa nổ, không thêm HĐ); "tách" + koto (đuôi) |

Bí mật: trước khoá, mọi bậc trông và nghe như nhau (cùng nhát chém mở niêm, cùng tim). Chỉ khác ở khoá.

## Tương tác phụ

- **Bấm K** (`NhatBanHook.TossPersimmon()`): ném một quả hồng từ camera theo đường parabol về kiếm. Vật có `NhatBanOffering` bay vào tầm kiếm → kiếm chém đôi (cắt lưới thật, 2 nửa văng vật lý) và cộng **khí**: thêm lá, sáo cao hơn, mỗi khí thêm 1 nhát ở đoạn kết (tối đa **+2**); tối đa **6** quả, không ném sau khi chốt. Khí **không đổi bậc**.
- Vật của người chơi không có `NhatBanOffering` không bao giờ bị phá.
- Nhân vật phụ: "samurai vô hình" = nón lá + kiếm; cúi chào khi trúng, rơi nón khi Rác.

## Bất biến

- Bậc do `ChestRevealSequence.Play(rolledTier, ...)` quyết; hook chỉ đọc bậc trong `OnTierLocked`. Ngẫu nhiên (góc nhát, xoay mảnh) theo `decorationSeed`.
- Tư thế rương: hook chỉ **cộng thêm** (nảy, xoay, phồng, xẹp) trong `LateUpdate`, tôn trọng vị trí BoxOpenSequence đặt, trả lại khi reset/huỷ. Hook **không bao giờ** đụng `Time.timeScale` (đã xoá `globalSlowMo`).
- Renderer Seal/Binding gốc bị ẩn khi cắt, bật lại khi `OnReset`.

## File

- `Scripts/ChestFx/Chests/NhatBanHook.cs` (điều phối), `NhatBanProps.cs` (katana/nón/dép/quả dựng bằng primitive, cắt lưới, `NhatBanOffering`), `NhatBanFxPreset.cs`.
- `Editor/NhatBanFxSetup.cs`: menu **Tools/Cho Do La/Create Nhat Ban Chest FX Profile** — giờ **ghi đè** preset và âm (để thay bản Daruma), dựng lại prefab hook có gắn âm.
- Âm: các `.mp3` ElevenLabs trong `Audio/Chests/nhat_ban/` (`sfx_nhatban_lock_*`, `shakuhachi_loop`, `taiko`, `katana_draw`, `slash`, `sheath`, `rope_cut`, `sandal`, `koto_pluck`). Các `iai_*.wav` numpy cũ còn trong thư mục nhưng không gán nữa.
- `Tools/CompileCheck/check_nhat_ban.sh`. Đã thêm tối thiểu vào `UnityStubAll.cs` (dùng chung): `Rigidbody.position`, `Physics.IgnoreCollision`, `Mesh.isReadable`.

## Thử trong Unity

1. Chạy menu Create Nhat Ban Chest FX Profile (hoặc Create All). Xem Console không có cảnh báo thiếu âm.
2. Mở cảnh thử rương (Docs/HuongDanThuUnity.md), chọn Chest_NhatBan, mở với từng bậc (phím P/T của menu cầm rương).
3. Kiểm: âm bắt đầu ngay lúc bấm; dây chỉ rơi sau tiếng "tách"; Rác có dép + kiếm văng + rương xẹp; HT có 5 nhát + rương bay xoay; màu và bóng vẫn dừng đúng bậc.
4. Trong lúc tim đập, bấm K vài lần: quả bị chém đôi, đoạn kết thêm nhát.
5. Reset rồi mở lại: rương về đúng chỗ, dây niêm hiện lại (timeScale chưa bao giờ bị đổi).

## Rủi ro đã biết (chưa kiểm được)

- Lưới glTFast có thể không `isReadable` → dự phòng "ép dẹt" từng nửa (xấu hơn).
- Hướng `FrontDirection` sai thì kiếm chém phía sau rương.
- Kích thước kiếm/nón theo bề ngang rương (1,6×) — có thể cần chỉnh.
- Va chạm tạm (thân hộp + sàn khi bên dưới trống) có thể đè lên collider có sẵn của cảnh.
- Âm ElevenLabs chưa ai nghe; chưa kiểm điều khoản giấy phép. Tiếng chém quả hồng dùng lại `slash.mp3` (danh mục không có tiếng riêng).
- Chưa chạy Unity: nhịp thật (nền ≤ 0,2 s sau bấm, lặng 0,2 s của Rác, khoảng cách taiko) mới kiểm bằng đọc mã.

## Nối với lõi (đã sửa theo kế hoạch 2026-11-25)

- `profile.tension.enabled = false` và mọi ô âm lõi trống (Setup gọi `ChestAudioRule.ApplyToProfile(p)` MỖI lần chạy menu). Hook luôn tự phát nền (sáo), nhịp (taiko, nguồn riêng để cắt ngay lúc chốt) và tiếng chốt.
- Hook khởi động ở `OnPress` (Start chỉ là dự phòng). Hook có 2 `AudioSource` riêng (nền, nhịp); tiếng lẻ phát qua `ChestRevealSequence.PlayOneShot`. `fxVolume` (0,8) nhân đúng một lần.
- Không dùng `ChestScript`/`ChestMotion`: hook tự ghi tư thế rương. Nếu sau này thêm `ChestScript`, **phải** chuyển nảy/xoay/xẹp sang `reveal.Motion.Run(...)`.
- Ném quả hồng tự làm (phím K + `NhatBanOffering`), chưa dùng `ChestItemSlot`/bước `Gate`. Bàn thử của lõi cũng dùng phím **K** (Golden) — chỉ trùng khi thử hai rương cùng lúc.

## Âm trong kịch bản (tự kiểm bằng đọc mã, từ lúc bấm tới +3 s sau chốt)

1. Lớp kéo dài trước chốt: **2** (nền sáo + nhịp taiko).
2. 0,3 s đầu sau chốt: Rác **1** (xịt, sau 0,2 s lặng); Thường..HT **1** (TC). Không HĐ.
3. Đuôi: Rác dép (~1 s), bậc khác "tách" + koto — ≤ 2 tiếng, ≤ 0,4.
4. Không nhân `volume` hai lần: chỉ hàm `Sfx` nhân `fxVolume`.
