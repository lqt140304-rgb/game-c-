# Thiết kế: hiệu ứng và âm thanh rương Nhật Bản cổ (Chest_NhatBan)

Trạng thái: **đề xuất, chưa chạy trong Unity, chưa nghe âm.** Dựa trên lõi ở [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md); điểm chung (a) màu bậc nhảy rồi dừng đúng bậc trúng, (b) bóng vật phẩm to dần, **giữ nguyên**, do lõi lo.

## Ý tưởng: "Daruma lắc lư, tô mắt thứ hai"

Rương là búp bê Daruma tròn như quả trứng, sơn đỏ gạch, chỏm đầu là nắp. Bản sắc:

- **Vòng quay kiểu PingPong**: màu bậc leo lên rồi tụt xuống như búp bê lật đật đung đưa. "Suýt trượt" 65% = búp bê suýt ngã sang bậc kế rồi tự đứng lại.
- **Tick là tiếng mõ gỗ** (temple block), cao độ 0,85 → 1,35, kèm **nhúm 3 cánh hoa** bật ra mỗi tick (hook).
- **Hạt**: cánh hoa anh đào hồng (SoftDot, màu phụ hồng 75%) **xoáy** quanh thân (Swirl), bay chậm (tốc độ 0,45, sống 1,9 s), lúc chốt rơi từ từ (trọng lực 0,12).
- **Chốt = "tô mắt thứ hai"**: chuông chùa trong trẻo (Hiếm), chuông bát + chuông gió + koto (Rất hiếm), trống hội taiko + tiếng hô (Huyền thoại). Tia ngắn (×0,85), quay chậm 8°/s như đĩa mặt trời, chớp đèn vừa phải (5,5).
- **Mưa hoa anh đào** rơi từ trên đầu búp bê lúc chốt, số cánh tăng theo bậc (20 → 80).
- **Rác (hài)**: Daruma **ngã nghiêng 35° rồi bật dậy** lắc lư tắt dần ("ngã bảy lần, đứng dậy tám lần") kèm tiếng lăn và "boing".
- **Rất hiếm / Huyền thoại**: **nhịp trống taiko** — 3 / 5 nhịp cách 0,2 s, mỗi nhịp đèn đỏ cam loé và một vòng cánh hoa bùng; nhịp cuối mạnh nhất.
- Âm đệm sau chốt: chuông gió (2 kiểu) và tiếng gảy koto, 1 cái cho Rác tới 5 cái cho Huyền thoại.

## Nhịp giây (rương không chìa, giai đoạn mặc định của BoxOpenSequence)

| Giây | Sự kiện |
|---|---|
| 0,0 – 2,5 | Rung, gỡ niêm (lõi) |
| 3,7 | Hé nắp: tiếng nắp gỗ nhấc (`lid_open`), trống dồn + chuông gió nền (`ambient`, 3,5 s), bắt đầu quay màu PingPong, tick mõ 0,06 s, mỗi tick một nhúm cánh hoa |
| 3,7 – 6,7 | Tick chậm dần tới 0,5 s; có thể suýt trượt rồi lùi về. Vòng quay tối thiểu 3,0 s (lõi kéo dài giữ "Hé nắp") |
| ~6,7 | Bật nắp: khoá màu, chuông/trống chốt theo bậc, tia, mưa hoa từ trên. Rác: ngã + bật dậy trong 1,1 s. Rất hiếm/Huyền thoại: 3/5 nhịp trống đèn đỏ (0,2 s/nhịp → tới ~7,3 / ~7,7) |
| 6,7 – 7,9 | Bóng vật phẩm phóng to (lõi) với tiếng quạt giấy mở + cánh hoa (`grow`); chuông gió, koto rải rác |
| ~8 | Mở toang: tiếng chỏm gỗ đặt xuống (`lid_thud`) |

## Bảng âm

Xem [Audio/Chests/nhat_ban/README.md](../../../Assets/_Project/Audio/Chests/nhat_ban/README.md) (13 file ElevenLabs, mô tả đầy đủ). Chỗ nào thiếu file riêng thì `NhatBanFxSetup` lấy bộ chung ở `Assets/_Project/Audio/Chests/` (rồi tới `Synth/`).

## File

| File | Vai trò |
|---|---|
| `Scripts/ChestFx/Chests/NhatBanFxPreset.cs` | `NhatBanFxPreset.Apply(p)` + tên file âm |
| `Scripts/ChestFx/Chests/NhatBanHook.cs` | hook: nhúm cánh hoa mỗi tick, mưa hoa lúc chốt, Daruma ngã-bật (Rác), nhịp trống đèn (Rất hiếm+) |
| `Editor/NhatBanFxSetup.cs` | menu `Tools/Cho Do La/Create Nhat Ban Chest FX Profile`: tạo `ScriptableObjects/ChestFx/ChestFx_NhatBan.asset` + prefab `ChestFx_NhatBanHook.prefab` (gắn vào `customFxPrefab`), gắn clip |
| `Audio/Chests/nhat_ban/` | 13 mp3 + README |
| `Tools/CompileCheck/check_nhat_ban.sh` | biên dịch thử bằng stub (thêm tạm `SaveAsPrefabAsset`, `StopCoroutine` vào bản sao stub) |
| `Art/Chests/Chest_NhatBan.glb` | chép từ nhánh `claude/chest-nhat-ban` (con trỏ Git LFS) |

## Rủi ro đã biết

- Cú ngã Rác xoay `reveal.transform` (gốc rig) quanh trục Z cục bộ; nếu `BoxOpenSequence` cũng xoay gốc ở giai đoạn 3–4 thì hai bên giành nhau. Tắt bằng `tumbleOnTrash` trên prefab hook. Hướng ngã có thể cần đổi trục tuỳ hướng mô hình.
- Mưa hoa dùng hình Circle xoay 90°, hạt có tốc độ ban đầu nhỏ; chưa thấy tận mắt hướng rơi.

## Cách thử trong Unity

1. `git lfs pull` để có `Chest_NhatBan.glb` thật (không phải con trỏ 131 byte).
2. `Tools/Cho Do La/Create Nhat Ban Chest FX Profile` → kiểm `ChestFx_NhatBan.asset` có đủ clip, `customFxPrefab` = `ChestFx_NhatBanHook`.
3. `Tools/Cho Do La/Add Chest Reveal Test`, rồi trên rig Chest_NhatBan kéo `ChestFx_NhatBan` vào `profile` của `ChestRevealSequence` (bàn thử mặc định gắn hồ sơ Golden; phím C đổi rương nhưng không đổi hồ sơ theo chestId nếu chưa có trong danh sách `profiles` của ChestRevealDemo — thêm tay).
4. Play: T chọn bậc, Space mở. Thử đủ 5 bậc; xem Rác có ngã-bật đúng chiều không, Huyền thoại có 5 nhịp đèn.
5. Nghe 13 âm, chỉnh âm lượng trong hồ sơ.
