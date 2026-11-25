# Thiết kế: hiệu ứng và âm thanh rương Không gian (Chest_KhongGian)

Trạng thái: **đề xuất, đã dựng code, chưa chạy trong Unity, chưa nghe âm**. Giữ nguyên điểm chung đã chốt (màu bậc nhảy rồi dừng đúng bậc; bóng vật phẩm to dần) — lõi lo phần đó, file này chỉ là bản sắc riêng. Xem [lõi](2026-11-24-hieu-ung-mo-rong-design.md).

## Ý tưởng: "trạm radar dò tín hiệu"

Khoang hạ cánh (chuông trên 3 chân, tấm chắn nhiệt cam, cửa sổ tròn, ăng-ten). Mở rương = đếm ngược radio + dò sóng:

- **Vòng quay kiểu Shuffle**: màu nhảy lung tung như núm dò đài tìm tín hiệu; khả năng "suýt trượt" 70% ("mất sóng… bắt lại"). Chậm rất muộn (đường cong dốc ở cuối), tối thiểu 3,0 s cho khớp nền đếm ngược 3 s.
- **Tick = ping sonar**, cao độ 0,8 → 1,6. Mỗi tick, hook phát **vòng sonar phẳng** lan ra từ chân rương theo màu đang hiện.
- **Hạt**: bụi sao `Streak` bay `Radial`, không trọng lực, màu phụ lam lạnh; hook thêm **trường sao** lấp lánh trôi chậm quanh khoang trong lúc quay.
- **Tia**: dài (x1,35), quay ngược chậm (−8°/s) như trôi trong không gian.
- **Khi khoá**:
  - Rác: "Houston, ta có sự cố" — tiếng chốt rè hỏng, tia lửa xám rơi lả tả, radio rè thêm 2 lần (cao độ tụt), vòng sonar nhỏ yếu ớt. Không có sao băng.
  - Thường → Huyền thoại: **mưa sao băng** chéo qua khoang, số = 6 × hạng; tiếng sao băng phát hạng lần, cao dần.
  - Hiếm trở lên: **tên lửa hãm** phun khói cam dưới tấm chắn nhiệt.
  - Huyền thoại: thêm **3 vòng vang khổng lồ** lan chậm ("tiếng vang vô tận") + dàn nhạc vũ trụ 4 s.

## Nhịp giây (rương không chìa, mốc mặc định của lõi)

| Giây | Sự kiện |
|---|---|
| 3,7 | Hé nắp: bắt đầu quay; nền đếm ngược radio (3 s), radio rè, trường sao bật, ping + vòng sonar mỗi tick |
| 3,7 – 6,7 | Shuffle chậm dần (0,05 → 0,5 s), suýt trượt 70% |
| ~6,7 | Bật nắp: khoá màu; tiếng chốt theo bậc; khí xì cửa khoang; tia, chớp; sao băng / tên lửa hãm / Houston; vút warp khi bóng bật ra |
| +0,15 … +0,8 | Tiếng sao băng nối nhau (số = hạng) |
| +0,45 / 0,9 / 1,35 | (Huyền thoại) 3 vòng vang lớn |
| ~7,9 | Mở toang: tiếng chân khoang chạm đất |

## Bảng âm

Chi tiết và mô tả gửi ElevenLabs: [Audio/Chests/khong_gian/README.md](../../../Assets/_Project/Audio/Chests/khong_gian/README.md). 12 file, tất cả < 70 KB.

| Chỗ dùng | File |
|---|---|
| tickClips | sfx_khonggian_tick |
| lockClipsByTier | sfx_khonggian_lock_rac / _thuong / _hiem / _ratHiem / _huyenThoai |
| whooshClip | sfx_khonggian_whoosh_warp |
| openAmbientClip | sfx_khonggian_countdown |
| accentClips | sfx_khonggian_meteor (accentsBase 0, +1/bậc) |
| stageClips | 3: sfx_khonggian_hatch_hiss · 4: sfx_khonggian_landing_thud |
| KhongGianHook | staticClip = sfx_khonggian_static_burst, meteorClip = sfx_khonggian_meteor |

## File

- `Assets/_Project/Scripts/ChestFx/Chests/KhongGianFxPreset.cs` — `KhongGianFxPreset.Apply(p)` và tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/KhongGianHook.cs` — sonar, trường sao, sao băng, tên lửa hãm, Houston, vòng vang.
- `Assets/_Project/Editor/KhongGianFxSetup.cs` — menu `Tools/Cho Do La/Create Khong Gian Chest FX Profile`: tạo `ScriptableObjects/ChestFx/ChestFx_KhongGian.asset`, gắn âm, tạo prefab `Prefabs/ChestFx/KhongGianFx.prefab` (chứa hook) làm `customFxPrefab`.
- `Tools/CompileCheck/check_khong_gian.sh` — biên dịch thử bằng stub (bản sao stub có thêm `SaveAsPrefabAsset`, `StopEmitting`).

## Cách thử trong Unity

1. Mở dự án, chờ import `Chest_KhongGian.glb` (nếu là con trỏ 130 byte: `git lfs pull`).
2. Chạy `Tools/Cho Do La/Create Khong Gian Chest FX Profile`; Console không được báo thiếu âm.
3. `Tools/Cho Do La/Add Chest Reveal Test`, bấm C tới Chest_KhongGian, kéo `ChestFx_KhongGian` vào `profile` của `ChestRevealSequence` (bàn thử lõi chỉ tự gắn hồ sơ Golden).
4. Play: T chọn bậc, Space mở, R đóng. Thử đủ 5 bậc; xem vòng sonar có nằm đúng chân rương, sao băng có bay chéo từ trên xuống (nếu hướng sai, chỉnh `Quaternion.Euler(125, 90, 0)` trong hook), khói tên lửa có phụt xuống dưới.
5. Nghe: tick 0,13 s có đục khi dồn 0,05 s không; nền đếm ngược có lấn tiếng chốt không.
