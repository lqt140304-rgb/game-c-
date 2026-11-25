# Kịch bản mở rương Chest_Magic: TRẬN PHÁP RUNE

**Đã thu gọn theo kế hoạch 2026-11-25 (mục 5.4).** Ngày 2026-11-25 · nhánh `claude/chest-seq-magic` · thay cho `MagicHook` cũ (vòng hạt quanh miệng rương).

## Ý tưởng

Mở rương phép không phải "mở nắp": người chơi **dựng một trận pháp**. Ba viên đá rune (vật thể vật lý thật) rơi xuống
ba điểm trên sàn quanh rương, một **đốm linh hồn (wisp)** bay chạm từng viên để thắp sáng, tia sáng nối ba viên thành
tam giác, vòng trận trên sàn sáng lên. Rương **nhấc khỏi sàn, bay lơ lửng và xoay theo vòng**, vòng **co dần** về
rương, mọi thứ xoay nhanh và rung mạnh dần. Đến lúc khóa bậc, phép hoặc **xịt** (Rác) hoặc **dập xuống nổ sóng xung kích**
(bậc cao). Đặc trưng riêng: *rương bay + đá vật lý + wisp*; không rương nào khác dùng.

## Bất biến (không đổi)

- Bậc do game quyết qua `ChestRevealSequence.Play(rolledTier, …)`. `MagicRuneRitual` chỉ là `ChestFxHook`, chỉ đọc bậc
  ở `OnTierLocked`, không bao giờ ghi.
- Vòng 5 màu vẫn nhảy rồi dừng đúng bậc; bóng vật phẩm vẫn to dần tới hình đầy đủ (lõi lo, không bị đụng).
- Ngẫu nhiên (góc đặt đá, xoay, độ nảy) theo `decorationSeed` (0 = theo đồng hồ), chỉ trang trí.
- Rương luôn được trả về đúng vị trí/hướng cũ ~0,6 s sau khóa và khi `OnReset`.

## Nhịp (giây kể từ lúc bấm mở)

| t | Hình | Âm |
|---|---|---|
| 0,0 | `Play()` → `IsPlaying` bật, hook phát hiện ngay trong `Update`. Rương rung nhẹ. | **`build` bắt đầu ngay**: trống trầm + gõ đá thưa |
| 0,15 / 0,5 / 0,85 | 3 đá Rigidbody rơi từ 1,3 m, nảy, lò xo (AddForce) kéo trượt vào điểm; ngồi vào chỗ → tiếng cạch | `stone` mỗi viên |
| 0,5 – ~2,2 | Wisp sà xuống, chạm từng viên → viên sáng tím, nối tam giác | `wisp` (cao dần mỗi viên) |
| ~2,2 → khóa | Rương bay lên (≈0,28 m), xoay từ 30 → 540°/s, nghiêng lắc; vòng co về rương, dày & sáng dần; đá rung; wisp bay vòng nhanh dần; đèn tím mạnh dần | `build` dâng: ù lên cao độ, nhịp gõ dồn từ 0,7 s xuống 0,07 s, gió xoáy to dần — **giống nhau mọi bậc** |
| khóa (lõi quyết, mặc định ở stage 3 của BoxOpenSequence) | Kết theo bậc, bảng dưới | `build` dừng phắt, phát `end_<bậc>` |

Độ căng dùng `expectedBuildSeconds` (6 s) để chia; nếu khóa muộn hơn 10 s thì `build` đã hết, chỉ còn hình (xem "chưa làm").

## Kết theo bậc

| Bậc | Rương | Vật lý / phụ | Âm `end_*` |
|---|---|---|---|
| Rác | Ngừng xoay, **rơi tự do** xuống sàn, nảy 1 lần, nghiêng lắc rồi về hướng cũ | Vòng chớp tắt phụt; 3 đá đổ nhào (Rigidbody bật lại); **wisp "hắt xì" rớt xuống sàn như hòn bi**; khói xám | tụt cao độ như bóng xì hơi + "bụp" + tiếng ré cao su (hài) |
| Thường | Hạ êm, thôi xoay | Vòng thu vào rương, đá chớp màu bậc | 1 tiếng chuông |
| Hiếm | Như Thường | + đá nảy lên (impulse) | 2 nốt + lấp lánh |
| Rất hiếm | **Vọt lên rồi DẬP xuống** | Sóng xung kích: đá văng ra (impulse hướng tâm ra ngoài), bụi sáng, đèn chớp | arpeggio + nổ trầm |
| Huyền thoại | Vọt cao hơn, dập mạnh hơn | Như trên + **cột sáng** màu bậc, wisp xoắn ốc bay lên theo cột | nổ trầm + hợp âm trưởng phồng + lấp lánh dài 3,6 s |

## Tương tác phụ

- **Người chơi tự đặt trận:** viên đá có `MagicRuneStone` (+ Rigidbody) nằm trong 0,3 m quanh một điểm trận lúc bấm mở
  được dùng luôn thay cho đá rơi. Đá của người chơi không bị xóa khi reset.
- **Ném thêm đá vào vòng** khi đang tụ phép: viên đó bị hút lên quỹ đạo quanh rương (lực thật); khi kết, bị hất văng
  (bậc cao) hoặc rơi bịch (Rác).
- **Nhân vật phụ:** wisp — thắp đá, bay vòng, kết Rác thì rớt, kết Huyền thoại thì bay lên theo cột sáng.

## File

- `Assets/_Project/Scripts/ChestFx/Chests/MagicRuneRitual.cs` — toàn bộ kịch bản (hook).
- `Assets/_Project/Scripts/ChestFx/Chests/MagicRuneStone.cs` — thẻ đá rune (đăng ký tĩnh, không tìm cả cảnh).
- `Assets/_Project/Editor/MagicRuneFxSetup.cs` — menu `Tools/Cho Do La/Magic Rune/…`.
- `Assets/_Project/Audio/Chests/magic/*.mp3` — âm ElevenLabs theo `Tools/Audio/elevenlabs_chest_manifest.json` (mục `magic`). Các `.wav` numpy trong `magic_rune/` và `make_magic_rune_sfx.py` để nguyên nhưng không còn được gán.
- `Tools/CompileCheck/check_magic_rune.sh` — biên dịch thử bằng stub.
- Không sửa file dùng chung nào.

## Cách thử trong Unity

1. Mở cảnh có Chest_Magic (ChestRevealTestSetup hoặc cảnh chợ). Chọn gốc rương (có `ChestRevealSequence`).
2. `Tools/Cho Do La/Magic Rune/Setup On Selected Chest` → thêm `MagicRuneRitual`, gán âm, tắt `MagicHook` cũ.
3. Cần **sàn có Collider** dưới rương để đá nảy thật (không có thì sau 1,6 s đá vẫn tự vào chỗ).
4. Play, bấm mở: nghe `build` vang NGAY lúc bấm. Thử từng bậc (ChestRevealDemo/phím test có sẵn), kiểm:
   Rác xịt + rương rơi bịch; Huyền thoại dập + cột sáng; màu 5 bậc dừng đúng; bóng vật phẩm lớn đủ.
5. Thử tương tác: `Tools/Cho Do La/Magic Rune/Create Player Rune Stone`, kéo đá tới gần một điểm trận (≈0,55 m quanh rương) rồi mở; hoặc ném đá vào vòng khi đang tụ phép.

## Thu gọn 2026-11-25 (mục 5.4) — số mới

| Hạng mục | Trước | Sau |
|---|---|---|
| Cột sáng HT | cao 6 m cố định, sống ~2 s+ | **2,5 S**, tâm 1,25 S trên rương, sống 1,5 s sau dập |
| Hạt lúc dập (RH / HT) | 45 / 80 | **35 / 60** |
| Đèn Light | 2 (rune + wisp) | **1** (wisp không còn đèn, giữ trail + vật liệu additive) |
| Diễn viên | wisp + 3 đá trong 0,9 S | không đổi |
| Rigidbody | 3 đá (+ ≤ 3 đá người chơi) | không đổi |
| `tension.enabled` | — | false (Setup gọi `ChestAudioRule.ApplyToProfile` mỗi lần chạy) |
| Wisp xoắn HT | bay lên không trần | trần 2 S |

Sửa lỗi: (1) rương chỉ **cộng độ lệch** lên gốc (`WriteChest`, ghi ở `LateUpdate`; gốc = giá trị BoxOpenSequence vừa
ghi nếu khác giá trị hook ghi khung trước), Reset trả về đúng vị trí/góc cũ; (2) nền `buildClip` luôn `loop = true`.

Âm (A1–A12): nền `rune_bed_loop` 0,15 → 0,45, cao độ +45% tới chốt, không lớp Nhịp (nền đã có trống). Sự kiện
`stone` 0,4 / `wisp` 0,35 qua cổng 0,2 s. Chốt: nền `Stop()` ngay; TC = `stingClips[bậc]` (Rác: lặng 0,2 s rồi xịt 0,7;
Thường 0,7; Hiếm 0,85; RH/HT 1,0); HĐ = `slamClip` 0,6 lúc dập, chỉ RH/HT. Không đuôi. Không nhân `volume` hai lần.

## Trung thực: chưa chạy / chưa nghe

- **Chưa chạy trong Unity** (môi trường không có Unity). Chỉ biên dịch thử bằng stub (`check_magic_rune.sh`, `check_all.sh` đều OK) — không bắt lệch API thật hay lỗi chạy.
- **Chưa ai nghe** các file âm; chỉ biết độ dài đúng. Có thể cần chỉnh âm lượng/độ chói.
- Rủi ro cần xem: (a) bóng vật phẩm/glow của lõi tính `MouthPosition` khi rương đang bay → có thể lệch vài cm trong lúc lơ lửng; (b) nếu gốc rương có Rigidbody/CharacterController riêng, việc đặt `transform` trực tiếp có thể đánh nhau với vật lý của nó; (c) nắp (BoxOpenSequence) mở trong lúc rương xoay — cần nhìn xem có đẹp không; (d) nền giờ lặp nên không còn im đoạn cuối, nhưng chỗ nối vòng lặp chưa ai nghe; (e) `WriteChest` dò "BoxOpenSequence đã ghi" bằng so sánh vị trí/góc, chưa thử với rung thật của BoxOpenSequence.
- Particle chỉ dùng `ChestFxFactory.CreateParticles` + shape Circle, không đụng orbitalX/Y/Z.

## Nối với khung chung (`claude/chest-fx-core`)

Lúc viết (2026-11-25) nhánh `claude/chest-fx-core` **chưa có trên origin**, nên chưa merge. Kịch bản chỉ dùng API
`ChestFxHook` hiện có + `ChestRevealSequence.IsPlaying` để bắt lúc bấm. Khi khung có, cần nối:
- sự kiện "bấm mở" chính thức (thay việc dò `IsPlaying` trong `Update`);
- nếu khung cho kéo dài/hoãn khóa bậc theo kịch bản, đặt khóa sau khi vòng co xong (~5–6 s) để `build` khớp;
- nếu khung có kênh âm/ducking chung, đưa `build`/`end_*` qua đó;
- nếu khung có cờ "rương đang bị kịch bản di chuyển", báo để lõi bám `MouthPosition` theo.
