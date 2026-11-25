# Kịch bản mở rương Thiên Cơ: "Bộ máy Thiên Cơ" (cơ khí đồng hồ)

Trạng thái: **mã đã viết, biên dịch thử bằng stub qua; CHƯA chạy trong Unity, CHƯA nghe âm.** Mọi thông số là đề xuất.

## Ý tưởng
Rương là một bộ máy đồng hồ đang **bị lên dây**. Bấm mở là lên cót: tích–tắc dồn dập dần, rương giật theo từng nhịp và lún
xuống như lò xo nén. Đoàn bánh răng đồng trước rương **thiếu một bánh** — bánh lạc rơi từ trên xuống (Rigidbody thật, nảy, keng).
Người chơi nhét nó vào khớp; nếu không, **chim cu** bật ra khỏi cửa nhỏ, gáy, và tự đưa nó vào. Kết cục là lò xo xả:
bậc cao thì **cả rương bị bắn lên**, bậc Rác thì **lò xo bung hỏng**, văng mọi thứ tứ tung.

Khác các rương khác: hiệu ứng chính là **tích tụ lực cơ học rồi xả** (nén–bật), con lắc và đồng hồ cát chạy bằng vật lý,
tương tác "lắp bánh răng còn thiếu", nhân vật phụ là chim cu đồng hồ.

## Thành phần (đều trong file riêng)
- `Scripts/ChestFx/Chests/ThienCoHook.cs` — toàn bộ kịch bản (viết lại hoàn toàn bản cũ; giữ tên lớp để prefab/Setup cũ vẫn dùng).
- `Scripts/ChestFx/Chests/ThienCoRig.cs` — lưới bánh răng khối, lưới đồng hồ cát, `ThienCoImpact` (tiếng va chạm).
- `Editor/ThienCoFxSetup.cs` — thêm điền 15 âm vào hook trên prefab.
- `ThienCoFxPreset.cs` — hạ âm lượng tick vòng quay (0,3) và nền (0,15) để tích–tắc riêng dẫn nhịp.
- `Audio/Chests/thien_co/clockwork/*.wav` + `Tools/Audio/make_thienco_clockwork_sfx.py`.
- `Tools/CompileCheck/check_thien_co.sh`; `UnityStubAll.cs` thêm stub `AddTorque`, `AddExplosionForce`, `HingeJoint`, `Collision`, `RigidbodyInterpolation`.

**Không sửa file dùng chung trong Scripts/.** Hook bắt đầu ở `Start()` vì `ChestRevealSequence.Play` tạo prefab hook ngay lúc bấm mở.

## Nhịp (u = thời gian / thời gian dự kiến tới chốt; ước từ `BoxOpenSequence.Stages` tới giai đoạn 3, ~4,6 s không chìa)
| Lúc | Sự kiện (giống nhau mọi bậc — giữ bí mật) |
|---|---|
| 0 s (bấm mở) | Tiếng lên dây cót; dựng 3 bánh răng (giữa trống), con lắc (HingeJoint, được đẩy mỗi tiếng "tắc"), đồng hồ cát, cửa chim cu |
| 0,45 s → chốt | Tích–tắc: khoảng 0,55 s → 0,11 s, cao độ 0,95 → 1,35, to dần; rương giật theo nhịp, lún tới 6 %; hạt cát (Rigidbody) rơi nhanh dần |
| u = 0,12 | Bánh răng lạc rơi xuống (vật lý, keng khi chạm) |
| tới u = 0,5 | Người chơi **bấm chuột vào bánh răng hoặc phím G** để lắp. Quá hạn: chim cu ra gáy, hút bánh răng vào |
| khi lắp | 50 % (hạt giống) lệch răng: kẹt rung 0,6 s rồi vào lại. Khớp xong: đoàn răng quay theo tỉ số bán kính, nhanh dần |
| u = 0,35 / 0,5 / 0,65 / 0,9 | Chuông điểm 1 / lên dây lần 2 / chuông 2 / chuông 3 tiếng |

## Chốt bậc (`OnTierLocked`) — âm theo bậc
- **Rác (xịt, hài):** tích–tắc **tắt phựt**, kẹt răng; 0,35 s: *bô-iiing* — bánh răng thành vật thể tự do bị **lực nổ** văng tứ tung (nảy, keng),
  con lắc **đứt khớp** rơi, đồng hồ cát **đổ nghiêng**, tường biến mất, hạt tràn sàn; rương xẹp, nghiêng 7°. 0,6 s: âm "hết dây cót" chậm dần, tụt cao độ, phựt. 1,3 s: chim cu ló ra gáy méo rồi rũ xuống.
- **Thường / Hiếm:** chuông = hạng (1/2 tiếng, cao dần), lò xo bật: rương nhảy (≈4 cm / 10 cm), nảy đáp; Hiếm có chim cu gáy.
- **Rất hiếm:** hộp nhạc rải hợp âm dâng 1 s → 3 tiếng chuông → tiếng phóng, rương bay ≈20 cm, hạt cát bị lực nổ hất tung.
- **Huyền thoại:** như trên + rương bay ≈52 cm, **xoay một vòng 360°** trên không; bánh răng tách ra **quay quanh rương như mô hình thiên văn**;
  vỏ cát mở, hạt nổ thành mưa đồng; chuông lớn ngân; chim cu gáy hai lần.

Vòng màu 5 bậc và bóng vật phẩm do lõi làm, hook không đụng vào. Hook chỉ đọc bậc trong `OnTierLocked`; ngẫu nhiên = `System.Random(decorationSeed)`.

## Thử trong Unity
1. `Tools/Cho Do La/Create Thien Co Chest FX Profile`. **Nếu prefab `Prefabs/ChestFx/ThienCo_Fx.prefab` đã có từ trước**, Setup chỉ điền ô âm trống — kiểm Inspector có đủ 15 clip; hồ sơ cũ giữ âm lượng tick cũ (0,6): hạ tay xuống 0,3 hoặc xoá asset để tạo lại.
2. Bàn thử (`Docs/HuongDanThuUnity.md`): chọn Chest_ThienCo, phím T đổi bậc, Space mở. Thử cả: bấm G / bấm chuột vào bánh răng rơi; để chim cu tự làm.
3. Kiểm: âm bắt đầu ngay khi bấm; Rác văng bánh răng, đổ đồng hồ cát; Huyền thoại bay xoay; R (reset) trả rương về đúng tư thế, không còn vật thừa.

## Rủi ro / chưa kiểm
- Chưa chạy Unity: tỉ lệ, vị trí theo kích thước rương (hệ số `_k` từ bounds 0,5 m), hướng mặt trước (`BoxOpenSequence.FrontDirection`) cần chỉnh mắt.
- Lệch tư thế rương áp trong `LateUpdate` sau `BoxOpenSequence`; nếu ChestRevealSequence đo/đặt vị trí cùng khung có thể lệch nhẹ.
- Hạt cát 1,6 cm (vật lý Unity không ổn với vật quá nhỏ); tối đa 48 hạt. Va chạm tạo collider trên lớp Default (có thể chắn raycast khác lúc mở).
- Vật liệu Unlit (phẳng). Âm tổng hợp chưa nghe.
- Nhánh lõi `claude/chest-fx-core` **chưa có trên remote** lúc làm: chưa nối. Khi có API khung (ví dụ sự kiện "bấm mở"/pha), cần chuyển phần `Start()` sang móc đó và để lõi điều khiển nhịp giây thay cho ước lượng `EstimateSecondsToLock`.
