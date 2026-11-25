# Kịch bản mở rương: Chest_RacQuy — "Mèo hoang lật thùng rác"

Trạng thái: **đã viết mã, biên dịch thử bằng stub (không có Unity). CHƯA chạy trong Unity, CHƯA ai nghe âm.**
Thay cho bản cũ [2026-11-24-ruong-rac-quy-design](2026-11-24-ruong-rac-quy-design.md) (chủ yếu hạt mùi + ruồi).
Bất biến giữ theo [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md): bậc do game quyết (`Play(rolledTier, …)`), màu 5 bậc vẫn nhảy rồi dừng đúng bậc, bóng vật phẩm vẫn to dần.

## Ý tưởng

Thùng rác ở chợ, một con mèo mướp cam đói đi ngang. Cả màn là chuyện **con mèo lật thùng**: rình, ngoáy mông, nhảy lên miệng thùng, cào cho thùng lắc, rồi — tuỳ bậc — thùng chỉ ợ một cái (mèo chán bỏ đi) hoặc đổ ầm, rác **vật lý** văng khắp sàn, mèo bị hất tung, và **món quý lóe sáng giữa đống rác**.

Đặc trưng riêng (không rương nào khác có): **một nhân vật sống (mèo) là người mở rương**, rác là Rigidbody thật nằm lại trên sàn, và người chơi **dụ mèo bằng xương cá**.

## Nhịp (rương không chìa, thời gian giai đoạn mặc định; mốc khoá ≈ 6,5–7 s)

| Giây | Hình | Âm |
|---|---|---|
| **0,0 (bấm)** | Ruồi xuất hiện quanh miệng thùng. Mèo từ ngoài khung (trái camera) đi vào. | **Vòng trống "thùng rỗng + rác lạch cạch" bắt đầu ngay**, pitch 0,85, âm lượng nhỏ; ruồi vo ve; "meo" |
| 0,2 – 1,3 | Mèo đi tới cạnh thùng | pitch/âm lượng dâng đều (+0,11 pitch/giây) |
| 1,3 – 3,7 | Mèo rình, ngoáy mông nhanh dần, cứ ~0,9 s đẩy thùng nghiêng nhẹ (lò xo) | dâng tiếp, tiếng tape/nắp của bộ cũ |
| **3,7** (vòng quay màu bắt đầu) | Mèo **phóng lên miệng thùng**, thùng giật mạnh; đứng cào. Mỗi bước màu: thùng giật nghiêng hướng ngẫu nhiên (mạnh dần), ~45% bật ra một món rác (Rigidbody). Thùng lắc lư liên tục. | ừng ực; lon kêu lẻ tẻ; vòng trống dâng nhanh hơn (×1,3), trần pitch 1,7 |
| **≈6,5 khoá màu** | **Đến đây mọi bậc trông và nghe như nhau** (giữ bí mật). | |
| — Rác | Thùng chỉ ợ (giật ngược), rớt **một chiếc giày**, phụt mùi, thêm ruồi. Mèo nhảy xuống, ngửi, **cụp đuôi bỏ đi**. | **Xịt**: vòng trống tụt pitch về 0,25 trong 0,4 s rồi tắt + tiếng xì hơi rít tụt giọng + "phụt" + tiếng xì hơi cũ |
| — Thường | Thùng đổ nghiêng 40° rồi lò xo dựng lại, 8 món rác văng. Mèo nhảy xuống, kêu, đi mất. | tắt trống, đổ rác loảng xoảng |
| — Hiếm | Đổ 58°, 14 món; ruồi tan; mèo **xù lông** (phình to) rồi lùi. Sau 0,9 s một **ngôi sao lóe 3 lần giữa đống rác**. | loảng xoảng + **khè** |
| — Rất hiếm / Huyền thoại | Đổ 70° / 85° (gần lật), 22 / 32 món **bắn tung**; **mèo bị hất văng** (thêm Rigidbody, xoay vòng); cột sáng (bộ cũ); lóe giữa đống rác (to hơn). | **BÙM** trầm + rền + lấp lánh, mèo **kêu thất thanh**, tiếng chốt hợp xướng của bộ cũ |
| khoá + 0,35 s | Thùng lò xo dựng lại → bóng vật phẩm mọc ở miệng thùng như lõi quy định. Rác nằm lại trên sàn tới khi Reset. | |

Độ mạnh rác/nghiêng chỉ theo tier đã truyền vào; ngẫu nhiên (hướng, loại rác, vị trí) theo `System.Random(decorationSeed)` riêng của hook, chỉ trang trí.

## Tương tác phụ: ném xương cá

Trong lúc mở (trước khoá), **phím F hoặc chuột phải**: một xương cá (Rigidbody) bay theo cung từ camera, rơi cạnh thùng. Nếu mèo đang rình (trước 3,7 s), nó kêu "meo" thèm, chạy tới gặm (xương biến mất), rồi về chỗ với **hệ số hăng máu 1,6**: các cú giật thùng và lực văng rác mạnh hơn. Không đổi bậc, không đổi nhịp lõi. Ném sau khi mèo đã lên thùng thì xương chỉ nằm lại như rác.

## Âm (mới, tổng hợp bằng code)

`Tools/Audio/make_racquy_meo_sfx.py` → `Assets/_Project/Audio/Chests/rac_quy/meo/*.wav` (numpy, sóng sin/răng cưa/nhiễu, hạt cố định, không bản quyền): `build_loop` (2 s, lặp), `meow`, `meow_wants`, `yowl`, `hiss`, `clatter`, `clank`, `deflate`, `boom`, `throw`. Các mp3 ElevenLabs cũ trong `rac_quy/` vẫn dùng (ừng ực, ruồi, xì hơi, tiếng chốt theo bậc, tick).
**Chưa nghe**: chỉ kiểm độ dài bằng code. Tiếng mèo tổng hợp có thể nghe giả; nên thay bằng mẫu thật cùng tên.

## Mã

| File | Việc |
|---|---|
| `Scripts/ChestFx/Chests/RacQuyHook.cs` | Viết lại toàn bộ: mèo (khối cơ bản), rác Rigidbody, sàn va chạm tạm, nghiêng thùng (lò xo), âm dâng, xương cá, lóe, dọn khi Reset |
| `Scripts/ChestFx/Chests/RacQuyFxPreset.cs` | Không đổi (vòng quay Shuffle, suýt trượt…) |
| `Editor/RacQuyFxSetup.cs` | Điền thêm 10 clip mèo vào prefab hook (cả prefab đã có) |
| `Tools/CompileCheck/check_rac_quy.sh` | Biên dịch stub (UnityStub + UnityStubAll), 3 bộ define |

**Bắt đầu ngay khi bấm** mà không sửa lõi: prefab hook được `Instantiate` bên trong `ChestRevealSequence.Play()`, hook tự tìm `ChestRevealSequence` ở cha và khi thấy `IsPlaying` thì vào cuộc (frame đầu). **Cần nối sau** khi nhánh `claude/chest-fx-core` có API khung (lúc viết, nhánh này **chưa tồn tại** trên origin): chuyển `BeginOnPress` sang callback kiểu `OnPlayStart`, và nếu khung cho đặt mốc khoá/độ dài, dùng nó để dâng âm theo phần trăm thay vì theo giây.

Hạn chế đã biết: thùng chỉ **xoay** quanh gốc (vì `BoxOpenSequence` ghi đè vị trí gốc để rung), nên "đổ" là lắc nghiêng quanh tâm gốc chứ không lăn; mèo/rác là khối cơ bản, vật liệu URP Lit (rơi về Standard, rồi Unlit).

## Thử trong Unity

1. `Tools/Cho Do La/Create Rac Quy Chest FX Profile` (điền clip mới vào `ChestFx_RacQuy_Hook.prefab`). Lần đầu Unity tự import các `.wav` mới.
2. Mở cảnh thử rương (`Tools/Cho Do La/Add Chest Reveal Test`), gán profile RacQuy cho Chest_RacQuy, chạy: T/Y/Space như bàn thử cũ để mở với từng bậc, R để reset.
3. Kiểm: âm trống có ngay khi bấm; F/chuột phải ném xương trước giây ~3,7; mèo gặm; thùng giật; Rác thì xịt, Huyền thoại thì bùm + mèo bay; rác nằm lại, Reset dọn sạch và thùng đứng thẳng lại; màu vẫn dừng đúng bậc, bóng vẫn to đủ.
4. Nếu sàn cảnh thấp hơn đáy thùng, rác rơi lên sàn tạm vô hình (đặt ở đáy thùng) — chỉnh `EnsureFloor` nếu cần.
