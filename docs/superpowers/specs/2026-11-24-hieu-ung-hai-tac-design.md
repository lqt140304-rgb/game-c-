# Thiết kế: hiệu ứng và âm thanh rương Hải tặc (Chest_HaiTac)

Trạng thái: **đề xuất, chưa chạy trong Unity, âm chưa ai nghe.** Dựa trên lõi [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md); điểm chung đã chốt (màu bậc nhảy rồi dừng đúng bậc trúng, bóng vật phẩm to dần tới hình hoàn chỉnh) do lõi lo, không bị đổi.

## Ý tưởng: "Bão biển, mở não"

Đầu lâu thuyền trưởng đội mũ ba góc ngồi trên đống xu; mở rương là nắp sọ + mũ lật ra sau. Mọi thứ đi theo **biển động**:

- **Vòng quay kiểu sóng** (`PingPong`): màu leo lên rồi tụt xuống như con thuyền nhấp nhô trên sóng. Đường chậm dần có một **"sóng dồn"**: đang chậm lại thì bị đẩy nhanh lên một chút (đoạn 38–55%), rồi mới chậm hẳn. Suýt trượt 70% (hải tặc thích trêu).
- **Tick la bàn đồng** và **cao độ đi XUỐNG** (1,1 → 0,8), ngược mọi rương khác: con thuyền chìm dần vào kết quả.
- **Hạt bọt biển xoáy** (`SoftDot` + `Swirl`, màu phụ xanh bọt biển `#59EBD9`, trộn 50%): xoáy nước quanh đầu lâu. Bùng hạt có trọng lực 0,4 như bọt sóng vỡ rơi xuống.
- **Tia quay ngược** (−40°/s) theo chiều xoáy nước, dài ×1,3.
- **Hook `HaiTacHook`** (code): mỗi nhịp đổi màu bắn một vốc bọt nước màu bậc đang hiện ở miệng sọ (sóng vỗ mạn thuyền); khi khoá: **mạch xu vàng phun** (12 + 14 × hạng xu, rơi trọng lực 1), **đại bác xa** (Hiếm 1 phát, Rất hiếm 2, Huyền thoại 3, mỗi phát thấp và nhỏ dần như loạt pháo mạn tàu), **Huyền thoại thêm "sóng thần"**: vòng 160 giọt bọt dạng vệt lao ra quanh chân rương. Ra **Rác**: mở xong có con **hải âu kêu chế giễu**.

## Nhịp giây (Chest_HaiTac có `Key`, nên cộng 2,15 s; số gần đúng)

| Giây | Việc |
|---|---|
| 0 – 2,15 | Chìa khóa xoay, rút ra |
| ~3,05 | Rung: **đầu lâu cười "ha ha ha"** |
| ~4,35 | Gỡ niêm: **dấu sáp đỏ vỡ tách** |
| ~5,85 | Hé nắp: **bắt đầu quay**, nắp sọ kẽo kẹt, nền sóng vỗ + hải âu xa, bọt nước bắn mỗi tick |
| 5,85 – ~8,85 | Màu lên xuống như sóng, có đoạn "sóng dồn" nhanh lại, rồi chậm (0,06 → 0,5 s); quay tối thiểu 3,0 s |
| **~8,85** | Bật nắp: **khoá màu**, tiếng chốt theo bậc, tia quay ngược, xu phun, bùng bọt |
| ~9,2 / 9,65 / 10,1 | Đại bác 1 / 2 / 3 (theo bậc); Huyền thoại có sóng thần ngay lúc khoá |
| 8,85 – ~10,05 | Bóng phóng to (tiếng xoáy nước vút lên) |
| ~10,05 | Mở toang: tiếng nắp chạm (bộ chung) |
| ~10,35 / ~10,65 | Bóng hoàn chỉnh / mở xong (Rác: hải âu kêu) |

## Bảng âm (thư mục `Assets/_Project/Audio/Chests/hai_tac/`, chi tiết mô tả ở README.md cùng thư mục)

| Bậc | Tiếng chốt |
|---|---|
| Rác | ủng ướt rút khỏi nước "ọc" + accordion lạc giọng tụt 3 nốt (hài) |
| Thường | một tiếng chuông tàu đồng |
| Hiếm | hai tiếng chuông tàu đi lên + sóng vỗ nhỏ |
| Rất hiếm | đàn fiddle điệu shanty vút lên + mưa xu |
| Huyền thoại | sóng thần gầm + hợp xướng thuỷ thủ hô "heave ho" (3,5 s) |

Âm đệm: tick la bàn, cười đầu lâu, dấu sáp vỡ, sọ kẽo kẹt, nền sóng, xoáy nước vút, đại bác, xu, hải âu. Accent sau khoá: xu và hải âu (Rác 0, mỗi bậc thêm 1).

## File

- `Assets/_Project/Scripts/ChestFx/Chests/HaiTacFxPreset.cs` – `HaiTacFxPreset.Apply(p)` và tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/HaiTacHook.cs` – hook riêng.
- `Assets/_Project/Editor/HaiTacFxSetup.cs` – menu `Tools/Cho Do La/Create Hai Tac Chest FX Profile` (tạo `ScriptableObjects/ChestFx/ChestFx_HaiTac.asset`) và `Tools/Cho Do La/Add Hai Tac FX To Selected Chest` (gắn hồ sơ + HaiTacHook kèm clip).
- `Tools/CompileCheck/check_hai_tac.sh` – biên dịch thử với stub.
- `Assets/_Project/Art/Chests/Chest_HaiTac.glb` – chép từ nhánh `claude/chest-hai-tac`.

## Cách thử trong Unity

1. Mở cảnh thử, chạy `Tools/Cho Do La/Add Chest Reveal Test`; bấm C cho tới Chest_HaiTac (hoặc kéo GLB vào cảnh và làm lại các bước của menu đó).
2. Chọn rương Hải tặc, chạy `Tools/Cho Do La/Add Hai Tac FX To Selected Chest`.
3. Play: T chọn bậc, Space mở. Thử đủ 5 bậc; nghe tick (có đục không), xem xu và sóng thần ở Huyền thoại, hải âu ở Rác.
4. Chỉnh tại `ChestFx_HaiTac.asset` và component `HaiTacHook`.

## Chưa kiểm chứng

Chưa chạy trong Unity; hạt chưa thấy tận mắt (đặc biệt vòng sóng thần dùng shape Circle + hướng Radial – hướng Radial của factory có thể ghi đè shape); âm chưa nghe; mốc giây là ước tính từ thời lượng giai đoạn mặc định.
