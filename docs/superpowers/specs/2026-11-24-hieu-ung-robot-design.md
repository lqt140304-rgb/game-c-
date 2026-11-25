# Thiết kế: hiệu ứng và âm thanh rương Robot (`Chest_Robot`)

Trạng thái: **đề xuất, chưa chạy trong Unity, âm chưa ai nghe.** Lõi dùng chung và các điểm đã chốt (màu bậc nhảy rồi dừng đúng bậc; bóng món to dần tới hình hoàn chỉnh) xem [hieu-ung-mo-rong](2026-11-24-hieu-ung-mo-rong-design.md); file này chỉ là phần riêng của rương Robot.

## Ý tưởng

Robot dây cót bằng thiếc, lùn mập; nắp là cái đầu vòm có mắt tròn và ăng-ten lò xo. Mở rương = **lên dây cót cho robot khởi động**: cót kêu tạch tạch, bánh răng quay, mắt nháy theo màu, lúc chốt thì tóe tia lửa điện. Bậc Rác là **robot chập mạch hụt hơi** (mắt chập chờn rồi tắt lịm, bánh răng đứng hình và lún xuống, khói xám). Huyền thoại là **nhạc khởi động** và mắt chớp ba lần.

Khác các rương khác:
- **Kiểu nhảy màu PingPong** (như vạch pin sạc lên rồi xuống), "suýt trượt" cao 0,65 (robot "lỗi" vượt một nấc rồi tự sửa).
- **Hạt Streak, Radial, rơi nhẹ** (trọng lực 0,6): tia lửa điện, màu phụ xanh điện `#59F2FF`.
- **Tia ngắn (0,8), quay nhanh 120°/s** như răng bánh răng; đèn chớp mạnh 8.
- **Hook `RobotHook`**: đèn "mắt" nháy sáng/tối mỗi tick theo màu đang hiện; vòng bánh răng (đa giác 10 cạnh) quay 720°/s trên miệng rương, chậm dần theo vòng quay.
- Tick xen kẽ **bíp máy tính** và **tạch cót** (lõi chọn ngẫu nhiên một trong hai mỗi lần).

## Nhịp (rương không chìa, thời gian giai đoạn mặc định)

| Giây | Hình | Âm |
|---|---|---|
| 0,0 | Rương rung | `windup` (lên dây cót) |
| ~0,9 | Gỡ niêm | `steam` (xì hơi khớp) |
| ~3,7 | Hé nắp: bắt đầu quay PingPong; mắt bật, nháy mỗi tick; bánh răng hiện, quay nhanh | `servo` (khớp kêu), nền `gears`, tick bíp/cót cao độ 0,85 → 1,7 |
| 3,7 – ~6,7 | Nhảy màu chậm dần (0,06 → 0,5 s), bánh răng chậm dần theo | |
| ~6,7 | **Khoá màu**. Rác: mắt chập chờn ~0,8 s rồi lịm, bánh răng đứng và lún, 8 tia lửa + khói xám. Thường+: tia lửa 15×(hạng+1), bánh răng quay lại 240 + 260×hạng °/s và to lên tới 1,8×; Huyền thoại: mắt chớp 3 lần trắng/vàng cam | chốt theo bậc, `whoosh` sạc điện, 1–5 âm đệm (`spark`, `babble`, `steam`, `servo`) |
| ~7,9 | Mở toang | `servo` |
| ~8,5 | Bóng hoàn chỉnh, `onRevealed`: mắt về sáng thường (Rác giữ lịm) | |

`minRouletteSeconds` = 3,0 s (dài hơn mặc định 0,2 s để kịp nghe cót), nên các mốc sau "Hé nắp" lùi khoảng 0,2 s so với bảng lõi.

## Âm

Danh sách và mô tả: [Audio/Chests/robot/README.md](../../../Assets/_Project/Audio/Chests/robot/README.md) (14 file ElevenLabs). Thiếu file nào thì menu lấy âm chung ở `Audio/Chests/` (tick, chốt, whoosh, creak, thud, nền).

## File

| File | Vai trò |
|---|---|
| `Scripts/ChestFx/Chests/RobotFxPreset.cs` | `RobotFxPreset.Apply(p)` và tên file âm |
| `Scripts/ChestFx/Chests/RobotHook.cs` | mắt nháy, bánh răng, chập mạch, tia lửa, khói |
| `Editor/RobotFxSetup.cs` | menu **Tools/Cho Do La/Create Robot Chest FX Profile** → `ScriptableObjects/ChestFx/ChestFx_Robot.asset`; nếu đang chọn rương có `ChestRevealSequence` thì gắn luôn `RobotHook` |
| `Art/Chests/Chest_Robot.glb` | chép từ nhánh `claude/chest-robot` (hiện chỉ là con trỏ Git LFS, xem dưới) |
| `Tools/CompileCheck/check_robot.sh` | biên dịch thử với stub |

## Cách thử trong Unity

1. `git lfs pull` (GLB trong repo là con trỏ LFS 131 byte; phiên dựng không tải được đối tượng LFS).
2. `Tools/Cho Do La/Add Chest Reveal Test`, bấm **C** tới `Chest_Robot`, chọn gốc rương (đối tượng có `ChestRevealSequence`).
3. `Tools/Cho Do La/Create Robot Chest FX Profile` (tạo hồ sơ, gắn `RobotHook`), kéo `ChestFx_Robot` vào ô `profile`.
4. Play: **T** đổi bậc, **Space** mở, **R** đóng. Thử đủ 5 bậc, nhất là Rác (chập mạch) và Huyền thoại (chớp mắt).
5. Chỉnh: vị trí mắt (`RobotHook` đặt đèn cách miệng 0,06 m lên và 0,12 m ra trước, có thể lệch với mắt thật của mô hình), `cogRadius`, âm lượng.

## Chưa kiểm chứng

- Chưa chạy trong Unity; chỉ biên dịch bằng `mcs` với stub.
- Hạt, bánh răng, đèn mắt chưa thấy tận mắt; vòng bánh răng dùng vật liệu cộng sáng với texture SoftDot nên có thể nhoè.
- Âm chưa nghe; tick 0,13 s có thể vẫn hơi dài ở nhịp 0,06 s.
