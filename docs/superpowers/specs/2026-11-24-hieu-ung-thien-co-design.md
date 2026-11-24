# Hiệu ứng và âm thanh riêng: rương Thiên Cơ (Chest_ThienCo)

Dựa trên lõi ở [2026-11-24-hieu-ung-mo-rong-design.md](2026-11-24-hieu-ung-mo-rong-design.md). Lõi vẫn lo phần đã chốt (màu bậc nhảy rồi dừng đúng bậc trúng, bóng vật phẩm to dần nguyên hình); file này chỉ là bản sắc riêng.

## Ý tưởng: "cơ khí chính xác + lõi năng lượng cyan, phóng như tên lửa"

Khác Robot (cót thiếc, bíp bíp) và Không gian (khoang hạ cánh, radio): ở đây là **bánh cóc, khoá tầng, phụt hơi, tia cyan thẳng đứng**.

- **Vòng quay kiểu đồng hồ áp suất** (`PingPong`): màu bậc leo lên rồi tụt xuống như kim đồng hồ. Nhanh lâu hơn rương khác rồi **phanh gấp** ở cuối (đường cong slowdown riêng).
- **Tick bánh cóc** giọng **trầm dần** (pitch 1,1 → 0,8, ngược các rương khác vốn cao dần), như cơ cấu đang hết cót.
- **Hai bánh răng cyan phát sáng** nằm ngang trên miệng rương (hook): bánh lớn 16 răng **nhảy đúng một răng mỗi tick**, bánh nhỏ quay ngược gấp đôi; răng đổi màu theo màu bậc đang hiện.
- **Chốt bậc = các lớp khoá mở lần lượt**: số vòng khoá = hạng bậc (Thường 1, Hiếm 2, Rất hiếm 3, Huyền thoại 4), cách nhau 0,13 s, mỗi vòng bật lên cao dần như các tầng tên lửa rồi loang ra. Xong thì **cột phụt cyan** (hạt Streak, nón hẹp 4°, bắn thẳng lên).
- **Huyền thoại**: thêm chớp lõi năng lượng (60 hạt sao trắng-cyan toả tròn) + âm hoà điện tử dài.
- **Rác (hài)**: bánh răng **kẹt**: rung lắc, khựng, chuyển xám, rơi vài tia lửa xỉn; âm hộp số kẹt + lò xo "boing".
- Hạt chung: Streak, hướng Up, nhanh (1,3), nhỏ, sống ngắn, 70% cyan. Tia dài 1,35×, mảnh, xoay nhanh 60°/s.

## Nhịp giây (ước lượng, tính từ lúc vòng quay bắt đầu)

| t (s) | Hình | Âm |
|---|---|---|
| 0 | Bánh răng hiện, hạt Streak cyan phụt lên đều | nền đồng hồ cơ lên dây (3,5 s), tick bánh cóc mỗi 0,05 s |
| 0–2,1 | Bánh răng nhảy răng liên tục, màu bậc quét lên xuống | tick trầm dần |
| 2,1–3,0 | Phanh gấp, tick thưa tới 0,38 s | |
| ≈3,0 | Chốt bậc. Vòng khoá 1..N bật mỗi 0,13 s | tiếng chốt theo bậc (1,2–4,5 s) |
| 3,0 + 0,13·N | Cột phụt cyan; Huyền thoại thêm chớp lõi | âm đệm (phụt hơi / servo / ping), số lượng = hạng bậc |
| sau chốt | Bóng vật phẩm phóng to | tiếng tên lửa đánh lửa |
| +0,4..1,0 | Bánh răng mờ dần rồi ẩn | |

Giai đoạn nắp: Hé nắp → tiếng mở niêm khí nén; Mở toang → tiếng cửa khoang thuỷ lực.

## Bảng âm

Xem [Assets/_Project/Audio/Chests/thien_co/README.md](../../../Assets/_Project/Audio/Chests/thien_co/README.md) (13 file, ElevenLabs, **chưa ai nghe thử**).

## File

- `Assets/_Project/Scripts/ChestFx/Chests/ThienCoFxPreset.cs`: giá trị hồ sơ + tên file âm.
- `Assets/_Project/Scripts/ChestFx/Chests/ThienCoHook.cs`: bánh răng, vòng khoá, cột phụt, kẹt răng.
- `Assets/_Project/Editor/ThienCoFxSetup.cs`: menu **Tools/Cho Do La/Create Thien Co Chest FX Profile** → `ScriptableObjects/ChestFx/ChestFx_ThienCo.asset` + `Prefabs/ChestFx/ThienCo_Fx.prefab` (gắn vào `customFxPrefab`). Thiếu âm riêng thì lấy âm chung.
- `Tools/CompileCheck/check_thien_co.sh`: biên dịch thử bằng stub.

## Cách thử trong Unity

1. `git lfs pull` (GLB là file LFS). Mở cảnh thử, chạy **Tools/Cho Do La/Add Chest Reveal Test**.
2. Chạy **Tools/Cho Do La/Create Thien Co Chest FX Profile**.
3. Chọn rương Chest_ThienCo trong cảnh (phím C đổi rương), kéo `ChestFx_ThienCo` vào ô `profile` của `ChestRevealSequence` (hoặc thêm vào danh sách `profiles` của `ChestRevealDemo`).
4. Play: T đổi bậc, Space mở. Kiểm từng bậc, nhất là Rác (kẹt) và Huyền thoại (4 vòng + chớp lõi).
5. Chỉnh trong Inspector của prefab `ThienCo_Fx` (`gearRadius`, `ringInterval`...) nếu bánh răng to/nhỏ so với miệng tên lửa.
