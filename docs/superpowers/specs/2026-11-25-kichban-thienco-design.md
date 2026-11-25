# Kịch bản mở rương Thiên Cơ: "Bộ máy Thiên Cơ" (cơ khí đồng hồ)

Trạng thái: **đã thu gọn theo kế hoạch 2026-11-25** (mục 5.17). Biên dịch thử bằng stub qua; **CHƯA chạy trong Unity, CHƯA nghe âm.** Mọi thông số là đề xuất.

## Ý tưởng
Rương là một bộ máy đồng hồ đang **bị lên dây**. Bấm mở là lên cót: tích–tắc dồn dập dần, rương giật theo từng nhịp và lún
xuống như lò xo nén. Đoàn bánh răng đồng trước rương **thiếu một bánh** — bánh lạc rơi từ trên xuống (Rigidbody thật, nảy, keng).
Người chơi nhét nó vào khớp; nếu không, **chim cu** bật ra khỏi cửa nhỏ, gáy, và tự đưa nó vào. Kết cục là lò xo xả:
bậc cao thì **cả rương bị bắn lên**, bậc Rác thì **lò xo bung hỏng**, văng mọi thứ tứ tung.

Khác các rương khác: hiệu ứng chính là **tích tụ lực cơ học rồi xả** (nén–bật), con lắc chạy bằng vật lý (đồng hồ cát đã bỏ),
tương tác "lắp bánh răng còn thiếu", nhân vật phụ là chim cu đồng hồ.

## Thành phần (đều trong file riêng)
- `Scripts/ChestFx/Chests/ThienCoHook.cs` — toàn bộ kịch bản (viết lại hoàn toàn bản cũ; giữ tên lớp để prefab/Setup cũ vẫn dùng).
- `Scripts/ChestFx/Chests/ThienCoRig.cs` — lưới bánh răng khối, lưới đồng hồ cát, `ThienCoImpact` (tiếng va chạm).
- `Editor/ThienCoFxSetup.cs` — gọi `ChestAudioRule.ApplyToProfile` MỖI lần chạy (lõi câm, `tension.enabled=false`); nạp 11 tiếng + `stingClips[5]` (.mp3 ElevenLabs) vào hook.
- `ThienCoFxPreset.cs` — không còn chỉnh âm lõi; chỉ giữ tên 5 file tiếng chốt.
- Âm: `Audio/Chests/thien_co/sfx_thienco_*.mp3` (danh mục `Tools/Audio/elevenlabs_chest_manifest.json`). Các `clockwork/*.wav` và `make_thienco_clockwork_sfx.py` để nguyên nhưng KHÔNG còn được gán.
- `Tools/CompileCheck/check_thien_co.sh`; `UnityStubAll.cs` thêm stub `AddTorque`, `AddExplosionForce`, `HingeJoint`, `Collision`, `RigidbodyInterpolation`.

**Không sửa file dùng chung trong Scripts/.** Hook bắt đầu ở `Start()` vì `ChestRevealSequence.Play` tạo prefab hook ngay lúc bấm mở.

## Nhịp (u = thời gian / thời gian dự kiến tới chốt; ước từ `BoxOpenSequence.Stages` tới giai đoạn 3, ~4,6 s không chìa)
| Lúc | Sự kiện (giống nhau mọi bậc — giữ bí mật) |
|---|---|
| 0 s (bấm mở) | Tiếng lên dây cót (1 sự kiện); dựng 3 bánh răng (giữa trống), con lắc (HingeJoint, được đẩy mỗi tiếng "tắc"), cửa chim cu |
| 0,3 s → chốt | Tích–tắc (lớp âm DUY NHẤT kéo dài): khoảng 0,65 s → 0,14 s, cao độ 0,95 → 1,35, to dần 0,24 → 0,52 (sau volume); rương giật theo nhịp, lún tới 6 % |
| u = 0,12 | Bánh răng lạc rơi xuống (vật lý, keng khi chạm) |
| 2,2 s sau khi rơi | Người chơi **bấm chuột vào bánh răng hoặc phím G** để lắp. Quá hạn (`autoInsertAfter` 2,2 s): chim cu ra gáy, hút bánh răng vào |
| khi lắp | 50 % (hạt giống) lệch răng: kẹt rung 0,6 s rồi vào lại. Khớp xong: đoàn răng quay theo tỉ số bán kính, nhanh dần |
| u = 0,5 / 0,85 | Chuông điểm: 1 tiếng / 2 tiếng (bỏ mốc 3 tiếng và lần lên dây thứ hai) |

## Chốt bậc (`OnTierLocked`) — âm theo bậc (A4)
- **Rác (xịt, hài):** tích–tắc dừng ngay, **lặng 0,2 s**, rồi **1 tiếng xịt** (`stingClips[0]`) cùng lúc *bô-iiing*: bánh răng thành vật thể tự do bị **lực nổ** văng tứ tung (nảy, keng),
  con lắc **đứt khớp** rơi; rương xẹp, nghiêng 7°. 0,6 s sau tiếng xịt không phát gì; đuôi: `sproing` +0,9 s, chim cu méo +1,3 s rồi rũ xuống. Không có tiếng hành động.
- **Thường / Hiếm:** TC (`stingClips[bậc]`, Thường 0,7 · Hiếm 0,85) ngay lúc chốt; lò xo bật rương nhảy (0,35 H / 0,55 H), nảy đáp; Hiếm có chim cu gáy (đuôi).
- **Rất hiếm:** TC 1,0 + HĐ `launch` (0,2 s sau chốt, ≤ 0,8); rương bay 0,8 H, hạt đuôi chim cu.
- **Huyền thoại:** như Rất hiếm + rương bay 1,15 H, **xoay một vòng 360°**; bánh răng tách ra **quay quanh rương như mô hình thiên văn trong 3 s rồi về đúng chỗ**;
  **12 bánh răng nhỏ** (0,3 cỡ bánh A) bắn từ miệng rương thành "mưa đồng" (thay đồng hồ cát cũ); chim cu gáy hai lần (đuôi).

Vòng màu 5 bậc và bóng vật phẩm do lõi làm, hook không đụng vào. Hook chỉ đọc bậc trong `OnTierLocked`; ngẫu nhiên = `System.Random(decorationSeed)`.

## Bản chốt sau khi thu gọn (kế hoạch 2026-11-25, mục 5.17)
| Hạng mục | Giá trị |
|---|---|
| Diễn viên | 1 (chim cu) + đồng hồ gắn rương (3 bánh răng, con lắc) |
| Rigidbody trước chốt | bánh răng lạc 1 + con lắc 2 (trụ kinematic + quả) = 3 (≤ 12) |
| Rigidbody sau chốt | Rác 3 bánh + con lắc; Huyền thoại thêm 12 bánh nhỏ (trần cứng 12) = 16 (≤ 40) |
| Hệ hạt / đèn riêng | 0 / 0 |
| Lớp âm kéo dài trước chốt | 1 (tích–tắc); không có vòng nền |
| Tiếng trong 0,3 s đầu sau chốt | Rác 1 (xịt, sau lặng 0,2 s) · Thường/Hiếm 1 (TC) · Rất hiếm/HT 2 (TC + launch) |
| Đuôi | ≤ 6 tiếng, từ +0,4 s (Rác từ +0,8 s), tới +3 s, cổng 0,15 s |
| `tension.enabled` | false |
| Chờ người chơi | ≤ 2,2 s (+ ~0,9 s chim cu hút vào) |
| Cú bật rương | ≤ 1,15 H, xoay 360° (HT) |

Đã bỏ: đồng hồ cát (48 hạt, vòng `sandLoop`, `UpdateHourglass`, tường kính), chuông 3 tiếng, lên dây lần 2, `winddown`, `bellBig`, `musicBoxRise`, tiếng chim cu "khen" khi người chơi tự lắp (âm numpy/tổng hợp không còn được gán — A9; file `.wav` giữ nguyên). TC thay bằng `stingClips` (mp3 `sfx_thienco_lock_*`).

## Thử trong Unity
1. `Tools/Cho Do La/Create Thien Co Chest FX Profile`. **Nếu prefab `Prefabs/ChestFx/ThienCo_Fx.prefab` đã có từ trước**, Setup chỉ điền ô âm trống — kiểm Inspector có đủ 15 clip; hồ sơ cũ giữ âm lượng tick cũ (0,6): hạ tay xuống 0,3 hoặc xoá asset để tạo lại.
2. Bàn thử (`Docs/HuongDanThuUnity.md`): chọn Chest_ThienCo, phím T đổi bậc, Space mở. Thử cả: bấm G / bấm chuột vào bánh răng rơi; để chim cu tự làm.
3. Kiểm: âm bắt đầu ngay khi bấm; Rác văng bánh răng, đổ đồng hồ cát; Huyền thoại bay xoay; R (reset) trả rương về đúng tư thế, không còn vật thừa.

## Rủi ro / chưa kiểm (trung thực)
- **Chưa chạy Unity, chưa nghe âm.** Chỉ có biên dịch thử bằng stub (`check_all.sh`, `check_thien_co.sh`) và đọc lại mã. Các file ElevenLabs chưa ai nghe; cần kiểm điều khoản giấy phép.
- Tỉ lệ, vị trí theo kích thước rương (hệ số `_k` từ bounds 0,5 m), hướng mặt trước (`BoxOpenSequence.FrontDirection`), độ cao bắn của 12 bánh răng nhỏ (tính theo `Mathf.Sqrt(_k)`, mục tiêu ≤ 3 S) cần chỉnh mắt.
- Lệch tư thế rương áp trong `LateUpdate` sau `BoxOpenSequence`; nếu `ChestRevealSequence` đo/đặt vị trí cùng khung có thể lệch nhẹ.
- Prefab `ThienCo_Fx.prefab` có thể đã tồn tại từ trước với giá trị cũ: Setup chỉ điền ô trống, nên khoảng cách tích–tắc (0,55/0,11 → 0,65/0,14) và `autoInsertAt` cũ đã bị thay bằng `autoInsertAfter`; kiểm Inspector hoặc xoá prefab để tạo lại. Ô `sandLoop/bellBig/winddown/musicBoxRise` cũ trên prefab sẽ bị bỏ qua.
- Va chạm tạo collider trên lớp Default (có thể chắn raycast khác lúc mở). Vật liệu Unlit (phẳng).
- Tiếng chốt Rác là `lock_rac` (xịt theo chủ đề), không còn `sproing` làm TC như bản kế hoạch gốc; `sproing` chuyển thành đuôi (+0,9 s) vì A4 cấm tiếng khác trong 0,6 s sau tiếng xịt.
- Nhánh lõi `claude/chest-fx-core` **chưa có trên remote** lúc làm bản đầu: chưa nối. Khi có API khung, cần chuyển phần `Start()` sang móc đó.
