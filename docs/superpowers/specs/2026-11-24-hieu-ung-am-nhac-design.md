# Thiết kế: hiệu ứng và âm thanh rương Âm nhạc (Chest_AmNhac)

Trạng thái: **đề xuất, chưa chạy trong Unity, chưa nghe âm**. Giữ nguyên điểm chung đã chốt (màu bậc nhảy rồi dừng đúng bậc trúng, bóng vật phẩm to dần) trong [lõi](2026-11-24-hieu-ung-mo-rong-design.md); phần dưới là bản sắc riêng.

## Ý tưởng: "Jukebox lên sóng"

Rương là jukebox mini neon hồng/xanh ngọc có đĩa than. Mở rương = bỏ xu vào jukebox, kim hạ xuống đĩa, trống dồn, rồi "drop" bài nhạc theo bậc.

- **Vòng quay kiểu đồng hồ VU** (`PingPong`): màu leo lên rồi tụt xuống như vạch âm lượng, chốt ở bậc trúng. Tỉ lệ "suýt trượt" cao (0,65) cho cảm giác "thêm một vạch nữa...".
- **Tick = hi-hat** khô, cao độ chỉ tăng nhẹ (1,0 → 1,25) để vẫn nghe như trống.
- **Hạt = nốt nhạc lấp lánh** (`Sparkle`) xoáy lên (`Swirl`), trộn 60% xanh ngọc neon với màu bậc, trọng lực âm nhẹ nên nốt cứ bay lên.
- **Tia quay nhanh** (60°/s) như đèn sân khấu quét.
- **Đĩa than quay** (`BoxOpenSequence.spinRecord`) theo độ sáng quầng: bắt đầu quay khi hé nắp.
- **Hook `AmNhacHook`**: 8 cột equalizer neon quanh miệng rương. Lúc quay, mỗi tick các cột nhảy tới độ cao theo bậc đang hiện (xen hồng/xanh). Khi chốt: cột lấy màu bậc, nảy theo nhịp 120 BPM, đèn điểm nháy hồng/xanh theo phách. **Rác**: cột xẹp dần và đèn tắt (nhạc tắt, hài). Từ **Rất hiếm**: tiếng khán giả reo.

## Nhịp giây (rương không chìa, thời gian giai đoạn mặc định)

| Giây | Hình | Âm |
|---|---|---|
| 0,0 | rương rung | `coin_slot` (bỏ xu vào jukebox) |
| 3,7 | hé nắp: bắt đầu quay màu, cột EQ mọc lên, nốt nhạc xoáy, đĩa bắt đầu quay | `needle_drop`, nền `drumroll` 3,0 s, hi-hat mỗi tick |
| 3,7 – 6,7 | màu VU leo/tụt chậm dần (0,06 → 0,5 s/nhịp, cuối ≈ 120 BPM) | hi-hat thưa dần |
| 6,7 | khoá màu: chớp, tia quét, bùng nốt; cột EQ chốt màu bậc rồi nảy theo nhịp | `lock_<bậc>` + `crowd` (Rất hiếm trở lên) + `note_pluck` 0/2/4/6/8 lần |
| 6,7 – 7,9 | bóng phóng to (lõi) | `whoosh` (tua băng + riser) |
| ~7,9 | mở toang | `lid_thud` (bộ chung) |

`minRouletteSeconds` = 3,0 (khớp trống dồn), nên giữ "Hé nắp" bị kéo thêm khoảng 0,2 s so với lõi.

## Bảng âm

Xem [Audio/Chests/am_nhac/README.md](../../../Assets/_Project/Audio/Chests/am_nhac/README.md). Chốt theo bậc: Rác = kim đĩa trượt + kazoo xì hơi; Thường = hợp âm piano điện; Hiếm = hai hợp âm ghi-ta funk; Rất hiếm = arpeggio synthwave + snare; Huyền thoại = solo ghi-ta rock, power chord, khán giả.

## File

- `Assets/_Project/Scripts/ChestFx/Chests/AmNhacFxPreset.cs`, `AmNhacHook.cs`
- `Assets/_Project/Editor/AmNhacFxSetup.cs`: menu `Tools/Cho Do La/Create Am Nhac Chest FX Profile` và `Tools/Cho Do La/Wire Selected Chest As Am Nhac`
- `Assets/_Project/Audio/Chests/am_nhac/*.mp3` + README
- `Assets/_Project/Art/Chests/Chest_AmNhac.glb` (chép từ nhánh `claude/chest-am-nhac`)
- `Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs`: **chép nguyên phần `spinRecord`** từ nhánh `claude/chest-am-nhac` (mặc định tắt)
- `Tools/CompileCheck/check_am_nhac.sh`

## Cách thử trong Unity

1. `git lfs pull` (GLB phải ~257 KB, không phải con trỏ 131 byte).
2. Mở cảnh thử, chạy `Tools/Cho Do La/Add Chest Reveal Test`. Bấm C tới khi hiện Chest_AmNhac (hoặc thay chest).
3. Chọn gốc rương (có `ChestRevealSequence`), chạy `Tools/Cho Do La/Wire Selected Chest As Am Nhac`: gắn hồ sơ `ChestFx_AmNhac`, thêm `AmNhacHook` (đã gán tiếng khán giả) và **bật `spinRecord`**.
4. Play: T chọn bậc, Space mở, R đóng. Thử cả 5 bậc; nhất là Rác (cột xẹp, đèn tắt) và Huyền thoại.
5. Chỉnh nếu cần: `AmNhacHook.radius/maxBarHeight` theo kích thước rương thật, `recordSpinAxis` nếu đĩa quay sai trục.

## Chưa kiểm chứng

Chưa chạy Unity (chỉ biên dịch bằng stub), chưa nhìn thấy hạt/cột EQ/tia, chưa nghe âm nào, chưa kiểm giấy phép ElevenLabs.
