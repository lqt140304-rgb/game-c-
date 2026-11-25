# Thiết kế: hiệu ứng và âm thanh rương Rác Quý (`Chest_RacQuy`)

Trạng thái: **đề xuất, đã biên dịch thử bằng stub, chưa chạy trong Unity, chưa nghe âm**. Giữ nguyên điểm chung đã chốt (nhảy màu bậc rồi dừng đúng bậc trúng; bóng vật phẩm to dần) — lõi lo phần đó, file này chỉ thêm bản sắc riêng. Lõi: [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md).

## Ý tưởng

Thùng rác tròn móp, nắp vòm, tem "QUÝ". Mở ra là **lười, lộn xộn, bốc mùi**: hơi thối xanh vàng rỉ ra, ruồi bay vòng quanh, ọc ọc. Bậc thấp càng hài (kèn buồn, thở dài, xì hơi). Bậc cao **lật kèo**: ruồi bay biến, một cột lấp lánh trắng vàng phụt thẳng lên, hợp xướng thiêng liêng — "giữa đống rác có vàng".

Khác các rương khác ở:
- **Vòng quay Shuffle chậm và lì**: `minInterval` 0,07 s, giữ nhanh lâu (đường cong tới 70% mới chậm 12%), rồi "bỏ cuộc" đột ngột như lon lăn dừng; `nearMissChance` 0,8 (thùng "ợ" qua bậc kế rồi tụt về); tối thiểu 3,0 s.
- **Hạt**: SoftDot to (0,055), chậm (0,28), sống lâu (2,1 s), xoáy (Swirl), màu phụ xanh vàng bệnh `#9EC72E`, trộn 0,75. Bùng khi khoá có trọng lực 0,6 (vụn rác rơi lại).
- **Tia** ngắn (×0,8), xoay ngược (−40°/s), chớp đèn yếu hơn (4,5).
- **Hook `RacQuyHook`** (dựng bằng code): đám hơi thối riêng (nhấp nháy theo màu đang quay), 7 con ruồi đen (vật liệu Unlit đục, vì hạt cộng sáng không vẽ được màu đen) quay quanh thùng, cột thánh quang cho Rất hiếm/Huyền thoại.

## Nhịp giây (rương không chìa, thời gian giai đoạn mặc định)

| Giây | Việc |
|---|---|
| ~0,9–2,5 | Gỡ niêm: **xé băng dính** (`tape_rip`) |
| 3,7 | Hé nắp: **nắp bật "boong" + xì hơi** (`lid_pop`), bắt đầu quay màu (tick lon thiếc, cao độ 0,8→1,25), **ọc ọc** + **ruồi vo ve**, hơi thối rỉ ra, ruồi xuất hiện |
| ~4,3 | (bước 7 của vòng quay) **lon lăn lạch cạch** |
| 3,7–6,7 | Màu nhảy lung tung, nhanh lâu rồi đột ngột chậm; 80% có "suýt trượt" |
| 6,7 | Khoá màu: tiếng chốt theo bậc (bảng dưới), tia ngắn xoay ngược, bùng hạt rơi lại |
| | Rác: phụt hơi thối to (45 hạt), thêm ruồi, **xì hơi** |
| | Hiếm+: ruồi biến mất, hơi thối ngừng |
| | Rất hiếm: cột lấp lánh 1,2 s · Huyền thoại: cột lấp lánh dày 2,6 s |
| 6,7–7,9 | Bóng phóng to: **vút nhớt nhợt + "bụp"** (`whoosh`); sau chốt một tiếng lon lăn (accent) |
| ~7,9 | Mở toang: **nắp thùng rơi loảng xoảng** (`lid_clang`) |

## Bảng âm

| Bậc | Tiếng chốt |
|---|---|
| Rác | kèn trombone buồn tụt giọng + thở dài (+ hook: xì hơi) |
| Thường | lon rỗng rơi "keng" nảy |
| Hiếm | hai chai thuỷ tinh chạm, đi lên, lấp lánh |
| Rất hiếm | nắp thùng xoay tròn trên sàn rồi harp lướt lên |
| Huyền thoại | nắp loảng xoảng rồi **hợp xướng thiêng liêng** + chuông |

Chi tiết mô tả: [Audio/Chests/rac_quy/README.md](../../../Assets/_Project/Audio/Chests/rac_quy/README.md). Âm nền quay dùng bộ chung (`Synth/sfx_chest_open_shimmer.wav`).

## File

- `Assets/_Project/Scripts/ChestFx/Chests/RacQuyFxPreset.cs` — `RacQuyFxPreset.Apply(p)`.
- `Assets/_Project/Scripts/ChestFx/Chests/RacQuyHook.cs` — hook riêng.
- `Assets/_Project/Editor/RacQuyFxSetup.cs` — menu **Tools/Cho Do La/Create Rac Quy Chest FX Profile**: tạo `ScriptableObjects/ChestFx/ChestFx_RacQuy.asset` + `ChestFx_RacQuy_Hook.prefab` (gắn vào `customFxPrefab`), gắn clip theo tên; thiếu file riêng thì lấy bộ chung.
- `Tools/CompileCheck/check_rac_quy.sh` — biên dịch thử (bản sao stub + `PrefabUtility.SaveAsPrefabAsset`).
- `Assets/_Project/Art/Chests/Chest_RacQuy.glb` — chép từ nhánh `claude/chest-rac-quy` (hiện là con trỏ LFS, xem dưới).

## Cách thử trong Unity

1. `git lfs pull` (GLB đang là con trỏ LFS 131 byte).
2. Chạy menu **Tools/Cho Do La/Create Rac Quy Chest FX Profile**; Console không được có cảnh báo thiếu âm.
3. **Tools/Cho Do La/Add Chest Reveal Test**, nhấn C tới `Chest_RacQuy`, kéo `ChestFx_RacQuy.asset` vào `profile` của `ChestRevealSequence` (menu thử chung chỉ tự gán hồ sơ Golden).
4. Play: T chọn bậc, Space mở. Thử cả 5 bậc; xem ruồi có thấy được không (đen trên nền tối), hơi thối có quá đặc, cột thánh quang có đủ "lật kèo".
5. Nghe 14 file âm; tick 0,13 s có đục khi chồng không.

## Chưa kiểm chứng

Chưa chạy Unity, chưa nghe âm, hạt chưa thấy tận mắt; quỹ đạo ruồi (orbitalY 3,5–6 với không gian World) và độ to các âm chồng nhau (ọc ọc + ruồi + nền + tick) có thể cần chỉnh.
