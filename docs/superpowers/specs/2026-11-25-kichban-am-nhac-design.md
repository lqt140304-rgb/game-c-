# Kịch bản Chest_AmNhac — "Loa Bass Kẹo Kéo"

Trạng thái: **đã thu gọn theo kế hoạch 2026-11-25** (mục 5.7). Code + âm ElevenLabs (.mp3) xong, **chưa chạy trong Unity, chưa ai nghe** (môi trường viết không có Unity; chỉ biên dịch thử bằng stub `Tools/CompileCheck/check_am_nhac_bass.sh`).

## Ý tưởng
Rương biến thành một cái **loa sub**. Bấm mở là nhạc lên ngay; rương nén–bật như màng loa theo từng tiếng kick, sóng âm lan trên sàn làm mọi vật xung quanh **nảy thật** (Rigidbody). Một **loa kẹo kéo** (kiểu loa kéo đường phố VN) tự lăn vào, cắm điện, rít feedback rồi đập theo. Người chơi thả **đĩa than** lên mâm và **bấm đúng phách** để quẩy mạnh hơn. Kết là một cú **DROP** (bậc cao) hoặc **rút phích – xịt** (bậc thấp).

Khác các rương khác: hiệu ứng là nhịp điệu do người chơi góp phần, vật lý theo phách, và một nhân vật phụ có bánh xe.

## Dòng thời gian (giây tính từ lúc `ChestRevealSequence.Play`)
| t | Sự kiện |
|---|---|
| 0,05 | Kick đầu tiên. Tempo 96 BPM, tăng dần tới ~150 (`tempoRiseSeconds` = 4). Mâm Record bắt đầu quay. |
| từ phách 4 | Thêm hi-hat phách lẻ. |
| 1,2 | Loa kẹo kéo lăn vào (tiếng bánh xe), cập bên rương, rít feedback, màng loa đập theo kick. |
| từ phách 8 | Thêm clap phách 2/4. |
| ≤ 5 | Đĩa than lơ lửng trên rương; người chơi bấm **V** hoặc click để thả. Không thả thì tự rơi ở 5 s. Rơi trúng mâm → tiếng kim + scratch, mở lớp bass "wub" phách lẻ, độ quẩy +0,3. |
| khi vòng màu chậm lại | (khoảng cách 2 lần đổi màu > 0,27 s) phát riser 1,2 s — **giống nhau mọi bậc**. |
| khóa bậc | Rẽ nhánh theo bậc (dưới). |

Tới lúc khóa, **mọi thứ giống nhau ở mọi bậc** (giữ bí mật); độ mạnh chỉ phụ thuộc thời gian và độ quẩy của người chơi.

## Bảng nhịp sau khi thu gọn (số mới)
| Hạng mục | Trước | Sau |
|---|---|---|
| Đồ nhỏ quanh rương | 7 | **5** |
| Sàn va chạm | 12 S | **4 S** |
| Loa kẹo kéo xuất phát | 3,35 S | **2,0 S** (≤ 3 S, nằm trong sàn) |
| Vật của cảnh bị đẩy (`shakeSceneBodies`) | bật | **tắt mặc định** (0 vật) |
| Lớp nhịp cùng lúc | kick + hat + clap + wub (4) | **2**: kick + (wub nếu đĩa trên mâm, nếu không thì hat); clap chỉ ở DROP |
| `feedback` / `wheels` | 0,35 / 0,5 | 1 lần 0,35 / 0,3 |
| Đám đông | Rất hiếm + Huyền thoại | **chỉ Huyền thoại**, đuôi +0,3 s sau DROP, 0,4 |
| Nhịp sau DROP (Hiếm / Rất hiếm / HT) | 2,8 / 4 / 5,5 s | 1,6 / 2,1 / 2,6 s (+0,35 s im) để kết ≤ 3 s (T6) |
| Nguồn âm trễ cùng lúc | không giới hạn | ≤ 4 |
| `tension.enabled` + ô âm lõi | còn tick/lock/accent lõi | **tắt, trống** (`ChestAudioRule.ApplyToProfile`, gọi mỗi lần chạy menu) |

## Âm (A1–A12)
- Chỉ dùng mp3 ElevenLabs trong `Audio/Chests/am_nhac/` (kick, hat, clap, wub, scratch, riser, drop, record_stop, plug_pop, wheels, feedback, needle_drop, crowd). Không còn nạp `seq/*.wav` (file để nguyên).
- Tiếng chốt (TC) = `stingClips[bậc]` = `sfx_amnhac_lock_<bậc>.mp3`, hook tự phát.
- Chưa có mp3 cho `skip` (bấm B trượt phách), `deflate`, `horn`: ô để trống, hook bỏ qua.
- Trần âm lượng (sau nhân `masterVolume` mặc định 0,8, nhân đúng một lần): nhịp 0,55 · sự kiện 0,5 · HĐ 0,8 · TC 1,0 · đuôi 0,4. Sự kiện lẻ trước chốt cách ≥ 0,2 s (`ChestAudioRule.TryGate`).
- Trước chốt: 2 lớp (kick, hat/wub) + tiếng lẻ (bánh xe, feedback, riser 0,45, kim, scratch của người chơi).

## Kết theo bậc
- **Rác**: nhạc tắt ngay (`Stop`, không fade), **im 0,2 s**, 1 tiếng TC (`lock_rac`), 0,6 s sau đó không phát gì; đuôi: `record_stop` +0,9 s (0,4), `plug_pop` +1,3 s (0,4). Phích văng, loa đổ, rương xẹp, đèn tắt (vật lý như cũ).
- **Thường**: TC (`lock_thuong`, 0,7) + scratch (HĐ 0,5) lúc chốt; nhịp đi tiếp nhỏ dần 2,2 s nhưng chỉ bắt đầu lại từ +0,4 s. Bỏ "pụp" deflate.
- **Hiếm / Rất hiếm / Huyền thoại**: TC ngay lúc chốt (0,85 / 1,0 / 1,0), rương hút vào 0,35 s, rồi DROP (HĐ 0,7 / 0,8 / 0,8): đẩy văng vật của rương, loa kẹo kéo bị hất bay, 3 vòng sóng. Chỉ Huyền thoại có đám đông (+0,3 s) và còi hơi (+0,6 s, chưa có file).

Vòng màu 5 bậc và bóng vật phẩm to dần **không bị đụng tới**; âm khóa/tick/accent của lõi nay tắt.

## Tương tác
- **B**: bấm theo phách. Lệch ≤ 0,11 s → scratch, vật nảy thêm, tempo +0,5 BPM (tối đa +8), combo tăng độ quẩy. Lệch → tiếng kim nhảy, độ quẩy giảm nhẹ.
- **V / click đĩa**: thả đĩa than.
- Gắn `AmNhacVinyl` lên **bất kỳ Rigidbody** nào (vd. đĩa người chơi cầm): rơi vào mâm khi rương đang mở là tính.
- Rigidbody có sẵn của cảnh **không còn bị đẩy** mặc định (`shakeSceneBodies = false`); bật tay trong Inspector nếu muốn thử.

## Bất biến
Hook chỉ trang trí: không gọi/đổi gì liên quan bậc; ngẫu nhiên (vị trí vật, bên loa vào) theo `seed` cố định. Hook **có đổi `localScale` gốc rương** trong lúc chạy (nén–bật) và trả lại khi reset; `BoxOpenSequence` không ghi scale gốc nên không giẫm nhau. Lớp Record do hook quay (setup tắt `spinRecord`).

## File
- `Scripts/ChestFx/Chests/AmNhacBassHook.cs`, `AmNhacVinyl.cs`
- `Editor/AmNhacBassSetup.cs` — menu **Tools/Cho Do La/Wire Selected Chest As Am Nhac (Loa Bass)**
- `Audio/Chests/am_nhac/*.mp3` — ElevenLabs (xem `Tools/Audio/elevenlabs_chest_manifest.json`). `seq/*.wav` (numpy) để nguyên nhưng không còn nạp.
- `Tools/CompileCheck/check_am_nhac_bass.sh` (+ `UnityStubAmNhacBass.cs`)
AmNhacHook cũ (equaliser) giữ nguyên file, menu mới gỡ nó khỏi rương để không phát đám đông hai lần.

## Thử trong Unity
1. Theo `Docs/HuongDanThuUnity.md` đặt Chest_AmNhac vào cảnh, chạy *Add Chest Reveal Test*.
2. Chọn rương → *Wire Selected Chest As Am Nhac (Loa Bass)*.
3. Play, mở rương (phím P/T của demo). Thử: bấm B theo nhạc, bấm V; ép từng bậc để nghe 3 kiểu kết.
4. Kiểm: nhạc có lên **ngay khi bấm** không; vật/loa có nảy, không xuyên sàn; scale rương về đúng sau reset; không lỗi Console.

## Chưa kiểm (trung thực)
- **Chưa chạy Unity, chưa nghe** bất kỳ âm nào; mức âm lượng và độ dài đuôi là tính theo mã, chưa nghe thật.
- File mp3 ElevenLabs chưa ai nghe; cần kiểm giấy phép.
- Riser bắt đầu khi vòng màu chậm lại (khoảng cách > 0,27 s); chưa đo có đúng ≤ 1,2 s trước chốt.
- `check_am_nhac_bass.sh` đã sửa stub riêng (stub chung đã có đủ nên bản cũ bị trùng).

## Rủi ro cũ
- Chưa nghe, chưa xem: cường độ lực, kích thước vật, âm lượng đều là đoán. Tune ở Inspector (`hopHeight`, `masterVolume`, `topBpm`).
- Nhịp tính theo `Update` (lệch ~1 khung hình), chưa dùng `AudioSettings.dspTime`.
- Loa kẹo kéo trượt bằng đặt vận tốc, bánh chỉ là hình (không WheelCollider).
- Khung chung (`ChestFxHook`) chưa có callback "vừa bấm mở"; hook dò `reveal.IsPlaying`. Khi nhánh `claude/chest-fx-core` có API đó thì đổi sang (xem mục cuối).
