# Thiết kế: hiệu ứng và âm thanh rương Tuổi thơ Việt Nam (Chest_TuoiTho)

Trạng thái: **đề xuất, đã viết code, chưa chạy trong Unity, chưa ai nghe âm**. Giữ nguyên điểm chung đã chốt (nhảy màu bậc rồi dừng đúng bậc; bóng vật phẩm to dần) — lõi lo phần đó, file này chỉ là bản sắc riêng.

Liên quan: [lõi hiệu ứng](2026-11-24-hieu-ung-mo-rong-design.md).

## Ý tưởng

Rương là **đèn ông sao năm cánh nằm ngửa**, cán tre. Chủ đề: **đêm Trung thu ấm áp, đồ chơi tuổi thơ** (bi ve, chuông xe đạp, còi ô tô đồ chơi, nhạc hộp, trống ếch, hộp kẹo thiếc).

- **Vòng quay PingPong, chậm và mềm** (0,07 → 0,5 s): màu lên rồi xuống như đèn lồng đung đưa, khác hẳn kiểu máy xèng Cycle của Golden.
- **Hạt đom đóm**: SoftDot nhỏ, thưa (14/s), xoáy (Swirl) quanh đèn, sống lâu 2,2 s, hơi bay lên như hơi nóng nến. Màu phụ cam ánh nến (mix 0,6).
- **Tia**: ngắn hơn (×0,9), tan chậm 1,6 s, **xoay ngược chiều** chậm (-12°/s) như đèn ông sao quay. Chớp sáng dịu (4,5) — ấm, không chói.
- **Hook `TuoiThoHook`** (code):
  - Một **đèn nến** (Point light cam) ở miệng rương, nhấp nháy theo nhiễu Perlin; **mỗi tick nến giật sáng** một nhịp.
  - Khi khoá bậc ≥ Thường: **đàn đom đóm xếp thành ngôi sao năm cánh** nằm ngang trên miệng đèn rồi trôi lên/loang ra; số đom đóm 25 → 75 theo bậc; nến sáng lên theo bậc.
  - **Rác: nến tắt phụt** (đi kèm tiếng đồ chơi hết pin) — hài.

## Nhịp giây (rương không chìa, thời gian giai đoạn mặc định)

| Giây | Sự kiện |
|---|---|
| 0,0 – 2,5 | Rung, gỡ niêm (lõi) |
| 3,7 | Hé nắp: tiếng tre kẽo kẹt + giấy sột soạt; nền phố Trung thu (trống, lục lạc, dế); nến bật; đom đóm xoáy; quay màu bắt đầu, tick bi ve |
| 3,7 – 6,7 | PingPong chậm dần (tối thiểu 3,0 s), tick cao dần 1,0 → 1,3; nến giật theo tick |
| 6,7 | Khoá: tiếng chốt theo bậc, ngôi sao đom đóm, tia xoay ngược, tiếng lồng đèn bung (bóng phóng to) |
| 6,7 – 8,x | Âm đệm: lục lạc, bi ve lăn, trống ếch — 0 cho Rác, +1 mỗi bậc (Huyền thoại 4) |
| Mở toang | Tiếng nắp hộp kẹo thiếc bật |

## Bảng âm

| Vai trò | File | Ý |
|---|---|---|
| Tick | `sfx_tuoitho_tick` | bi ve gõ nhau (0,13 s) |
| Rác | `sfx_tuoitho_lock_rac` | đồ chơi hết pin, nhạc tụt giọng |
| Thường | `sfx_tuoitho_lock_thuong` | một tiếng chuông xe đạp |
| Hiếm | `sfx_tuoitho_lock_hiem` | còi ô tô đồ chơi bíp bíp |
| Rất hiếm | `sfx_tuoitho_lock_ratHiem` | nhạc hộp 5 nốt đi lên |
| Huyền thoại | `sfx_tuoitho_lock_huyenThoai` | trống ếch + lục lạc + nhạc hộp + tiếng cười trẻ con xa xa |
| Bóng to dần | `sfx_tuoitho_grow` | lồng đèn giấy bung, vút ấm |
| Nền | `sfx_tuoitho_ambient` | phố đêm Trung thu |
| Đệm | `bells`, `marbles`, `frogdrum` | lục lạc, bi ve lăn, trống ếch |
| Giai đoạn 2 / 4 | `bamboo`, `candytin` | cán tre kẽo kẹt / hộp kẹo thiếc bật |

Mô tả gốc ở `Assets/_Project/Audio/Chests/tuoi_tho/README.md`.

## File

- `Assets/_Project/Scripts/ChestFx/Chests/TuoiThoFxPreset.cs` — `TuoiThoFxPreset.Apply(p)` + tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/TuoiThoHook.cs` — nến + ngôi sao đom đóm.
- `Assets/_Project/Editor/TuoiThoFxSetup.cs` — menu `Tools/Cho Do La/Create Tuoi Tho Chest FX Profile` → `ScriptableObjects/ChestFx/ChestFx_TuoiTho.asset`; thiếu âm riêng thì lấy âm chung.
- `Tools/CompileCheck/check_tuoi_tho.sh` — biên dịch thử bằng bản sao stub có vá thêm `EmitParams`, `MoveTowards`, `FloorToInt`.

## Cách thử trong Unity

1. `git lfs pull` (đảm bảo `Chest_TuoiTho.glb` không phải pointer 130 byte).
2. Chạy `Tools/Cho Do La/Create Tuoi Tho Chest FX Profile`; xem Console không có cảnh báo thiếu âm.
3. `Tools/Cho Do La/Add Chest Reveal Test`, bấm C tới Chest_TuoiTho; trên gốc rương kéo `ChestFx_TuoiTho` vào `profile` của `ChestRevealSequence` và Add Component `TuoiThoHook`.
4. Play: T đổi bậc, Space mở. Thử đủ 5 bậc; kiểm Rác nến tắt, Huyền thoại ngôi sao rõ hình năm cánh (chỉnh `starRadius` nếu lệch miệng đèn).
5. Nghe: tick có bị đục không; âm Huyền thoại 3,6 s có đè âm đệm không.
