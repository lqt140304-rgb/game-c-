# Kịch bản mở rương: Chest_RacQuy — "Mèo hoang lật thùng rác"

Trạng thái: **đã viết mã, đã thu gọn theo kế hoạch 2026-11-25** (mục 5.2: cắt rác trước chốt, sửa lỗi nhỏ, chuẩn hoá âm). Biên dịch thử bằng stub (không có Unity). **CHƯA chạy trong Unity, CHƯA ai nghe âm.**
Thay cho bản cũ [2026-11-24-ruong-rac-quy-design](2026-11-24-ruong-rac-quy-design.md) (chủ yếu hạt mùi + ruồi).
Bất biến giữ theo [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md): bậc do game quyết (`Play(rolledTier, …)`), màu 5 bậc vẫn nhảy rồi dừng đúng bậc, bóng vật phẩm vẫn to dần.

## Ý tưởng

Thùng rác ở chợ, một con mèo mướp cam đói đi ngang. Cả màn là chuyện **con mèo lật thùng**: rình, ngoáy mông, nhảy lên miệng thùng, cào cho thùng lắc, rồi — tuỳ bậc — thùng chỉ ợ một cái (mèo chán bỏ đi) hoặc đổ ầm, rác **vật lý** văng khắp sàn, mèo bị hất tung, và **món quý lóe sáng giữa đống rác**.

Đặc trưng riêng (không rương nào khác có): **một nhân vật sống (mèo) là người mở rương**, rác là Rigidbody thật nằm lại trên sàn, và người chơi **dụ mèo bằng xương cá**.

## Nhịp (rương không chìa, thời gian giai đoạn mặc định; mốc khoá ≈ 6,5–7 s) — số mới sau thu gọn

| Giây | Hình | Âm |
|---|---|---|
| **0,0 (bấm)** | Ruồi xuất hiện quanh miệng thùng. Mèo từ ngoài khung (trái camera) đi vào. | **Nền** (`bed_loop`, 1 vòng lặp duy nhất) vào ngay: âm lượng 0,15 → 0,45, pitch 1,0 → 1,5 tới lúc khoá; ruồi vo ve 1 lần (0,3); "meo" lúc +0,25 s |
| 0,2 – 1,3 | Mèo đi tới cạnh thùng | nền dâng đều |
| 1,3 – 3,7 | Mèo rình, ngoáy mông, cứ ~0,9 s đẩy thùng nghiêng nhẹ | nền dâng tiếp |
| **3,7** (vòng quay màu bắt đầu) | Mèo **phóng lên miệng thùng**, thùng giật; đứng cào. Mỗi bước màu: thùng giật hướng ngẫu nhiên, ~45% bật ra một món rác (Rigidbody), **tối đa 10 món trước khoá**. | ừng ực; lon kêu lẻ tẻ (mọi tiếng lẻ cách nhau ≥ 0,2 s, ≤ 0,5) |
| **≈6,5 khoá màu** | **Đến đây mọi bậc trông và nghe như nhau** (giữ bí mật). | |
| — Rác | Thùng chỉ ợ, rớt **một chiếc giày**, phụt mùi, thêm ruồi. Mèo ngửi, **cụp đuôi bỏ đi**. | Nền **dừng ngay** → lặng 0,2 s → **1 tiếng xịt** (`deflate`). Không gì khác trong 0,6 s |
| — Thường | Đổ 40° rồi dựng lại, 8 món rác. Mèo nhảy xuống, kêu, đi mất. | TC `lock_thuong` (0,7) + loảng xoảng (HĐ 0,5); đuôi: meo |
| — Hiếm | Đổ 58°, 14 món; ruồi tan; mèo xù lông rồi lùi; sau 0,9 s sao lóe 3 lần. | TC `lock_hiem` (0,85) + loảng xoảng (HĐ 0,7); +0,5 s khè (đuôi) |
| — Rất hiếm / Huyền thoại | Đổ 70° / 85°, 22 / 32 món bắn tung; mèo bị hất văng; cột sáng; lóe giữa đống rác. | TC `lock_ratHiem` / `lock_huyenThoai` (1,0) + BÙM (HĐ 0,8); +0,3 s loảng xoảng (0,5); +0,7 s mèo thét (0,4) |
| khoá + 0,35 s | Thùng lò xo dựng lại → bóng vật phẩm mọc ở miệng thùng. Rác nằm lại trên sàn tới Reset. | |

Độ mạnh rác/nghiêng chỉ theo tier đã truyền vào; ngẫu nhiên (hướng, loại rác, vị trí) theo `System.Random(decorationSeed)` riêng của hook, chỉ trang trí.

## Tương tác phụ: ném xương cá

Trong lúc mở (trước khoá), **phím F hoặc chuột phải**: một xương cá (Rigidbody) bay theo cung từ camera, rơi cạnh thùng. Nếu mèo đang rình (trước 3,7 s), nó kêu "meo" thèm, chạy tới gặm (xương biến mất), rồi về chỗ với **hệ số hăng máu 1,6**: các cú giật thùng và lực văng rác mạnh hơn. Không đổi bậc, không đổi nhịp lõi. Ném sau khi mèo đã lên thùng thì xương chỉ nằm lại như rác.

## Âm (sau thu gọn: chỉ .mp3 ElevenLabs theo chủ đề)

Danh mục: `Tools/Audio/elevenlabs_chest_manifest.json`, mục `rac_quy`; file ở `Assets/_Project/Audio/Chests/rac_quy/`. Hook nạp qua `RacQuyFxSetup.FillClips` (luôn ghi đè, bỏ các `.wav` numpy trong `meo/`, file `.wav` và `make_racquy_meo_sfx.py` giữ nguyên, không xoá).
- Hồ sơ lõi **câm** (`ChestAudioRule.ApplyToProfile` mỗi lần chạy menu): `tension.enabled = false`, không tick/nền/accent/stage/whoosh/lock.
- `stingClips[5]` trên hook = `sfx_racquy_lock_*.mp3`; hook phát theo bậc (Thường+). Bậc Rác phát `deflate` làm TC (đã bỏ tiếng `fart`).
- Tiếng lẻ trước chốt qua `ChestAudioRule.TryGate` (≥ 0,2 s), âm lượng ≤ 0,5; `fxVolume` nhân đúng một lần (mặc định 0,8).
**Chưa nghe**: chưa ai nghe thử, chưa kiểm điều khoản giấy phép ElevenLabs.

## Mã

| File | Việc |
|---|---|
| `Scripts/ChestFx/Chests/RacQuyHook.cs` | Viết lại toàn bộ: mèo (khối cơ bản), rác Rigidbody, sàn va chạm tạm, nghiêng thùng (lò xo), âm dâng, xương cá, lóe, dọn khi Reset |
| `Scripts/ChestFx/Chests/RacQuyFxPreset.cs` | Không đổi (vòng quay Shuffle, suýt trượt…) |
| `Editor/RacQuyFxSetup.cs` | Ghi hồ sơ lõi câm; nạp clip `.mp3` + `stingClips` vào prefab hook (cả prefab đã có) |
| `Tools/CompileCheck/check_rac_quy.sh` | Biên dịch stub (UnityStub + UnityStubAll), 3 bộ define |

**Bắt đầu ngay khi bấm** mà không sửa lõi: prefab hook được `Instantiate` bên trong `ChestRevealSequence.Play()`, hook tự tìm `ChestRevealSequence` ở cha và khi thấy `IsPlaying` thì vào cuộc (frame đầu). **Cần nối sau** khi nhánh `claude/chest-fx-core` có API khung (lúc viết, nhánh này **chưa tồn tại** trên origin): chuyển `BeginOnPress` sang callback kiểu `OnPlayStart`, và nếu khung cho đặt mốc khoá/độ dài, dùng nó để dâng âm theo phần trăm thay vì theo giây.

Sửa lỗi (kế hoạch 5.2): `_hasBaseRot` đặt lại khi dọn (mở lần 2 sau khi xoay rương không giật hướng); sàn tạm `4·S × 4·S` thay cho 6 m cố định; rác trước chốt ≤ 10 (T3: ≤ 10 rác + 1 xương = 11 Rigidbody ≤ 12).

Hạn chế đã biết: thùng chỉ **xoay** quanh gốc (vì `BoxOpenSequence` ghi đè vị trí gốc để rung), nên "đổ" là lắc nghiêng quanh tâm gốc chứ không lăn; mèo/rác là khối cơ bản, vật liệu URP Lit (rơi về Standard, rồi Unlit).

## Thử trong Unity

1. `Tools/Cho Do La/Create Rac Quy Chest FX Profile` (ghi lại hồ sơ câm và nạp clip `.mp3` vào `ChestFx_RacQuy_Hook.prefab`).
2. Mở cảnh thử rương (`Tools/Cho Do La/Add Chest Reveal Test`), gán profile RacQuy cho Chest_RacQuy, chạy: T/Y/Space như bàn thử cũ để mở với từng bậc, R để reset.
3. Kiểm (cả âm: chỉ 1 lớp nền trước chốt, Rác chỉ có một tiếng xịt sau 0,2 s lặng; mở 2 lần liên tiếp sau khi xoay rương không giật hướng): âm trống có ngay khi bấm; F/chuột phải ném xương trước giây ~3,7; mèo gặm; thùng giật; Rác thì xịt, Huyền thoại thì bùm + mèo bay; rác nằm lại, Reset dọn sạch và thùng đứng thẳng lại; màu vẫn dừng đúng bậc, bóng vẫn to đủ.
4. Nếu sàn cảnh thấp hơn đáy thùng, rác rơi lên sàn tạm vô hình (đặt ở đáy thùng) — chỉnh `EnsureFloor` nếu cần.
