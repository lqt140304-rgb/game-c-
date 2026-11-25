# Hiệu ứng + âm thanh rương Thể thao (Chest_TheThao)

Chủ đề: **ngày thi đấu**. Rương là quả bóng low-poly tách đôi trên chân đế cam. Không phá phần chung (màu bậc nhảy rồi dừng đúng bậc, bóng vật phẩm to dần) — lõi lo.

## Bản sắc
- **Vòng quay PingPong** như thanh lực sút: leo lên rồi tụt xuống; `nearMissChance` 0,65 = khoảnh khắc "trúng cột dọc".
- **Tick = tiếng đá bóng** (0,13 s), pitch 0,85 → 1,3: cú sút nặng lúc đầu, chạm nhẹ khi chậm lại.
- **Hạt Streak + Fountain, trọng lực 0,7**: dải confetti bắn lên rồi rơi; màu phụ cam chân đế (mix 0,5).
- **Tia ngắn (×0,85), xoay nhanh 70°/s** như bóng xoáy.
- **TheThaoHook** (code): còi khai cuộc khi vòng quay bắt đầu; từ Hiếm trở lên hai **pháo giấy hai bên** (trái/phải nghiêng 25°) như khán đài; Huyền thoại thêm **còi hơi** + **3 pháo hoa** cách 0,35 s.
- Accent: vỗ tay/bóng nảy, Rác 0 cái, mỗi bậc +1.

## Nhịp (giây, từ lúc bắt đầu vòng quay)
| t | Sự kiện |
|---|---|
| 0 | còi khai cuộc + nền khán đài trống dồn (4 s), confetti nhỏ phun đều |
| 0–3 | tick đá bóng, PingPong chậm dần |
| ~3 | chốt bậc: âm chốt theo bậc, tia, đèn; Hiếm+ pháo giấy hai bên |
| 3,35 / 3,7 / 4,05 | Huyền thoại: pháo hoa 1-2-3 (cùng còi hơi lúc 3) |
| sau chốt | bóng vật phẩm phóng to + whoosh bóng bay; vỗ tay/nảy rải rác |
Giai đoạn hộp: Bật nắp (3) = tiếng đường may bung; Mở toang (4) = bóng nảy.

## Âm
Bảng chi tiết: `Assets/_Project/Audio/Chests/the_thao/README.md`. Rác hài (đá hụt, bóng xì hơi, khán giả "ôi"); Thường còi + vỗ tay nhẹ; Hiếm hai còi + hò reo; Rất hiếm lưới rung + gầm + còi hơi; Huyền thoại cả sân vận động nổ tung + pháo hoa.

## File
- `Assets/_Project/Scripts/ChestFx/Chests/TheThaoFxPreset.cs`, `TheThaoHook.cs`
- `Assets/_Project/Editor/TheThaoFxSetup.cs` (menu Tools/Cho Do La/Create The Thao Chest FX Profile)
- `Tools/CompileCheck/check_the_thao.sh`
- `Assets/_Project/Art/Chests/Chest_TheThao.glb` (chép từ nhánh claude/chest-the-thao, LFS)

## Cách thử trong Unity
1. Chạy Tools/Cho Do La/Add Chest Reveal Test (cảnh thử chung).
2. Chọn object rương trong Hierarchy → Tools/Cho Do La/Create The Thao Chest FX Profile: tạo `ScriptableObjects/ChestFx/ChestFx_TheThao.asset` và gắn `TheThaoHook` (kèm clip còi/còi hơi) vào object đang chọn.
3. Kéo hồ sơ vào ô `profile` của ChestRevealSequence (hoặc thêm vào danh sách `profiles` của ChestRevealDemo), thay mô hình bằng Chest_TheThao.glb.
4. Play: T đổi bậc, Space mở, R đóng. Thử đủ 5 bậc, nghe kỹ Rác và Huyền thoại.
5. Chỉnh: âm lượng (ambient 0,3 có thể to), `cannonOffset` theo cỡ thật của bóng.

## Chưa kiểm
Chưa chạy trong Unity, chưa nghe âm, chưa thấy hạt. Chỉ biên dịch thử bằng stub.
