# Kịch bản Chest_AmNhac — "Loa Bass Kẹo Kéo"

Trạng thái: code + âm tổng hợp xong, **chưa chạy trong Unity, chưa ai nghe** (môi trường viết không có Unity; chỉ biên dịch thử bằng stub `Tools/CompileCheck/check_am_nhac_bass.sh`, âm chỉ kiểm bằng số: độ dài, đỉnh < 0 dBFS).

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

## Kết theo bậc
- **Rác**: record-stop (đĩa rên chậm dần), phích cắm bật khỏi loa kẹo kéo (Rigidbody văng), loa đổ nghiêng (AddForceAtPosition), rương xẹp và *giữ nguyên xẹp* kèm tiếng "xịt" xì hơi. Tắt nhạc, đèn tắt.
- **Thường**: drop hụt — một tiếng "pụp" + scratch, rương bật nhẹ, nhịp đi tiếp 2,2 s rồi tắt.
- **Hiếm / Rất hiếm / Huyền thoại**: im 0,35 s (rương hút vào), rồi DROP: boom sub + đẩy văng mọi vật (lực theo bậc), 3 vòng sóng, nhịp tối đa 2,8 / 4 / 5,5 s. Rất hiếm+ có đám đông (`sfx_amnhac_crowd.mp3`), Huyền thoại thêm còi hơi.

Âm khóa bậc của khung chung vẫn phát (nhỏ hơn), vòng màu 5 bậc và bóng vật phẩm to dần **không bị đụng tới**.

## Tương tác
- **B**: bấm theo phách. Lệch ≤ 0,11 s → scratch, vật nảy thêm, tempo +0,5 BPM (tối đa +8), combo tăng độ quẩy. Lệch → tiếng kim nhảy, độ quẩy giảm nhẹ.
- **V / click đĩa**: thả đĩa than.
- Gắn `AmNhacVinyl` lên **bất kỳ Rigidbody** nào (vd. đĩa người chơi cầm): rơi vào mâm khi rương đang mở là tính.
- Rigidbody có sẵn trong bán kính 2,5× rương (không kinematic, ≤ 5 kg, không CharacterController) cũng nảy theo.

## Bất biến
Hook chỉ trang trí: không gọi/đổi gì liên quan bậc; ngẫu nhiên (vị trí vật, bên loa vào) theo `seed` cố định. Hook **có đổi `localScale` gốc rương** trong lúc chạy (nén–bật) và trả lại khi reset; `BoxOpenSequence` không ghi scale gốc nên không giẫm nhau. Lớp Record do hook quay (setup tắt `spinRecord`).

## File
- `Scripts/ChestFx/Chests/AmNhacBassHook.cs`, `AmNhacVinyl.cs`
- `Editor/AmNhacBassSetup.cs` — menu **Tools/Cho Do La/Wire Selected Chest As Am Nhac (Loa Bass)**
- `Audio/Chests/am_nhac/seq/*.wav` — tạo bằng `python3 Tools/Audio/make_amnhac_sfx.py`
- `Tools/CompileCheck/check_am_nhac_bass.sh` (+ `UnityStubAmNhacBass.cs`)
AmNhacHook cũ (equaliser) giữ nguyên file, menu mới gỡ nó khỏi rương để không phát đám đông hai lần.

## Thử trong Unity
1. Theo `Docs/HuongDanThuUnity.md` đặt Chest_AmNhac vào cảnh, chạy *Add Chest Reveal Test*.
2. Chọn rương → *Wire Selected Chest As Am Nhac (Loa Bass)*.
3. Play, mở rương (phím P/T của demo). Thử: bấm B theo nhạc, bấm V; ép từng bậc để nghe 3 kiểu kết.
4. Kiểm: nhạc có lên **ngay khi bấm** không; vật/loa có nảy, không xuyên sàn; scale rương về đúng sau reset; không lỗi Console.

## Chưa làm / rủi ro
- Chưa nghe, chưa xem: cường độ lực, kích thước vật, âm lượng đều là đoán. Tune ở Inspector (`hopHeight`, `masterVolume`, `topBpm`).
- Nhịp tính theo `Update` (lệch ~1 khung hình), chưa dùng `AudioSettings.dspTime`.
- Loa kẹo kéo trượt bằng đặt vận tốc, bánh chỉ là hình (không WheelCollider).
- Khung chung (`ChestFxHook`) chưa có callback "vừa bấm mở"; hook dò `reveal.IsPlaying`. Khi nhánh `claude/chest-fx-core` có API đó thì đổi sang (xem mục cuối).
