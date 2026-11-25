# Kịch bản mở rương Không Gian — "Phi vụ tiếp tế"

Rương: `Chest_KhongGian` (Assets/_Project/Art/Chests/Chest_KhongGian.glb).
Mã: `Scripts/ChestFx/Chests/KhongGianMissionHook.cs` (đạo diễn), `KhongGianSynth.cs` (âm tổng hợp trong code),
`Editor/KhongGianFxSetup.cs` (gắn hook vào prefab), kiểm: `Tools/CompileCheck/check_khonggian_mission.sh`.

## Ý tưởng
Tàu mẹ bay tới tiếp tế cho chiếc rương-tàu đổ bộ. Vùng quanh rương mất trọng lực, người chơi nạp pin cho tàu
nổ máy, tàu mẹ bắn tên lửa bay vòng rồi trúng nắp làm bật nắp, sau đó thả dù mang món quà xuống. Kết cục chỉ lộ khi
bậc đã chốt. Đặc trưng riêng (không rương nào khác có): **trọng lực thấp + tên lửa dẫn đường + thùng quà thả dù**.

## Nhịp (giây tính từ lúc bấm mở; T = lúc nắp bật mạnh, tự đọc từ stage đầu có lidAngle ≥ 30°, mặc định ≈ 5.3 s)
| Thời điểm | Diễn biến |
|---|---|
| 0.0 | Máy nổ rì rầm (pitch thấp, nhỏ) + bíp radio. Mất trọng lực: rương nổi ~4% kích thước, nghiêng lắc; 6–9 bu-lông/mảnh vụn (Rigidbody) bốc lên trôi lơ lửng (trọng lực 18%). Đồng hồ nhiên liệu hiện cạnh rương. |
| 0.3 → ~1.6 | Tàu mẹ bay vào theo đường cong Bezier, nghiêng theo hướng bay, rồi lơ lửng phía trên. |
| ~1.6 → T−1.8 | Tiếp nhiên liệu: 3 pin tự rơi theo quỹ đạo đạn đạo trọng lực thấp vào miệng rương; **người chơi bấm B** thả thêm (tối đa 6). Mỗi pin: "cộp", tia sáng xanh, rương lún rồi bật, đồng hồ dâng, máy gầm cao hơn. |
| T−1.8 | Phóng tên lửa (Rigidbody, dẫn đường bằng lực): bay vòng quanh rương, kéo khói + lửa, rồi lao xuống. |
| T | Trúng nắp đúng lúc BoxOpenSequence bật nắp: nổ (giống nhau mọi bậc — giữ bí mật), rương bị hất + nghiêng, mảnh vụn văng ra bằng xung lực. |
| Khi bậc chốt (sau T) | Kết cục theo bậc (dưới). |
| Revealed + 2.5 | Mảnh vụn thu nhỏ và biến mất; thùng quà ở lại tới khi reset. |

## Âm theo bậc
- Bắt đầu ngay khi bấm: vòng lặp máy nổ tổng hợp (răng cưa 46/92 Hz + nhiễu nâu, nhịp pít-tông), độ căng = thời gian tới T + số pin → pitch 0.45→1.55, âm lượng tăng.
- **Rác**: máy "xìiiì" trượt 600→70 Hz + 5 tiếng pụp hụt hơi, máy tắt; trọng lực trở lại, rương rớt bịch; tàu mẹ ho khói lủi đi (bíp trầm); dù xịt, thùng rơi tự do đập đất làm rương giật mình. Hài, rớt đột ngột.
- **Thường → Rất hiếm**: máy gầm vọt (pitch theo bậc), hợp âm rải (bậc+1 nốt), rương nhảy bằng động cơ (lửa dưới đáy), tàu mẹ lượn vòng chào bay lên, thùng quà màu bậc thả dù bung "phựt", đáp êm cạnh rương, dù xẹp.
- **Huyền thoại**: như trên + 5 nốt ngân dài + 3 pháo hoa nổ trên trời màu bậc.
- Âm khóa/tick của khung chung (profile) vẫn chạy song song.

## Bất biến
- Bậc do game quyết (`ChestRevealSequence.Play(rolledTier, ...)`); hook chỉ đọc bậc ở `OnTierLocked`. Ngẫu nhiên dùng `System.Random(seed)` chỉ cho trang trí.
- Không đụng vòng quay màu 5 bậc và bóng vật phẩm (vẫn do ChestRevealSequence).
- Rương không gắn Rigidbody (tránh đánh nhau với BoxOpenSequence): dùng lò xo–giảm chấn cộng dồn lên vị trí/góc, gỡ ra đầu mỗi khung (`DefaultExecutionOrder(-500)`). Vật lý thật nằm ở pin, mảnh vụn, tên lửa, thùng quà + một sàn tạm BoxCollider.
- Particle chỉ dùng `ParticleDirection.Radial` (không bật velocityOverLifetime → không có lỗi orbital khác chế độ).

## Cách thử trong Unity
1. `Tools/Cho Do La/Create Khong Gian Chest FX Profile` (chạy lại được: prefab cũ sẽ được thêm `KhongGianMissionHook`).
2. Mở cảnh thử rương như `Docs/HuongDanThuUnity.md`, chọn Chest_KhongGian, bấm P (mở) — thử từng bậc bằng T.
3. Trong lúc tàu mẹ lơ lửng, bấm **B** để thả pin. Kiểm: âm bắt đầu ngay khi bấm; tên lửa trúng đúng lúc nắp bật; Rác rớt xịt; Huyền thoại có pháo hoa.

## Trung thực: chưa làm / chưa kiểm
- **Chưa chạy trong Unity, chưa nghe âm.** Chỉ biên dịch thử bằng stub (`check_khonggian_mission.sh`, `check_all.sh` đều OK). Stub đã được bổ sung `AudioClip.Create/SetData`, `ForceMode.Acceleration/VelocityChange`, `Rigidbody.position`, `DefaultExecutionOrder`.
- Dùng `Rigidbody.linearVelocity/linearDamping` (Unity 6). Unity cũ hơn cần đổi sang `velocity/drag`.
- Tỉ lệ (kích thước tàu, độ cao) ước theo bounds rương; có thể cần chỉnh. Vật thể dùng primitive + vật liệu unlit/additive của ChestFxFactory (chưa có mô hình tàu riêng).
- Phím B: chưa kiểm xung đột với điều khiển khác; có `DropFuelCell()` để UI gọi.
- Nhánh lõi `claude/chest-fx-core` chưa có trên origin lúc làm (fetch báo không tìm thấy) → chưa nối. Khi lõi có API khung: cần một callback "OnPlay/bấm mở" chính thức (hiện hook dựa vào `Start()` của prefab customFx được Instantiate trong `Play`) và một mốc "nắp bật" (hiện tự tính từ `Box.Stages`).
