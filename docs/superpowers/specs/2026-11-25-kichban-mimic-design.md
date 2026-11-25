# Kịch bản mở rương Chest_Mimic — "RƯƠNG ĐÓI" (2026-11-25)

Code: `Assets/_Project/Scripts/ChestFx/Chests/MimicFeedHook.cs` (+ `MimicFeedFood.cs`, `MimicFeedMouth.cs`),
Editor: `Assets/_Project/Editor/MimicFeedSetup.cs`, âm: `Assets/_Project/Audio/Chests/mimic_feed/`,
kiểm biên dịch: `Tools/CompileCheck/check_mimic_feed.sh`.

## Ý tưởng
Rương là một con vật đói. Nó **không chịu mở** cho đến khi được cho ăn. Người chơi phải ném đồ lạ vào miệng nó;
nó tinh nghịch nhảy né, nhai, nuốt, ợ; nếu bị bỏ đói nó tự bò đi vồ đồ. Trong lúc "tiêu hoá" (vòng màu) một con
ruồi vo ve và nó nhảy đớp. Cuối cùng: bậc thấp → xì hơi xẹp lép, nhổ rác ra; bậc cao → nổ tung, nhảy vọt xoay
tròn, đáp đất hất văng mọi thứ xung quanh. Không rương nào khác có màn "cho ăn để mở khoá".

## Vật lý thật
- Thân rương = Rigidbody proxy (hộp va chạm khít bounds rương, khoá xoay). Mô hình đi theo proxy trong LateUpdate
  (cộng thêm phần rung của BoxOpenSequence) nên **không đổi cây rig**. Nghiêng theo vận tốc, co giãn kiểu lò xo, xoay
  khi nhảy là trang trí.
- Sàn vô hình tạo dưới đáy rương; va chạm với collider sẵn có của rương bị bỏ qua (IgnoreCollision).
- Đồ ăn (dép, lon, xương, táo — khối đơn giản) là Rigidbody rơi từ trên xuống, ném theo đường đạn đạo.
- Miệng = trigger cầu; **bất kỳ Rigidbody không-kinematic nào** người chơi thả vào cũng bị ăn, và được **nhổ trả
  nguyên vẹn** lúc chốt (hoặc khi reset). Vật đang cầm trên tay (kinematic) không bị giật.
- Bậc cao: OverlapSphere + xung lực hất văng vật quanh rương.

## Nhịp (stage mặc định của BoxOpenSequence)
| Giây | Việc |
|---|---|
| 0,0 | Bấm mở → **âm bắt đầu ngay**: khịt mũi, bụng réo (lặp), tim đập chậm. 0,12 s rương giật nảy. |
| 0,55–1,4 | 4 món đồ lạ rơi lả tả quanh trước mặt rương. |
| ~2,1 | Tới "Gỡ niêm": BoxOpenSequence dừng (waitForPlayer bật tạm thời). Rương há/ngáp nắp đòi ăn. |
| người chơi | Bấm chuột vào món (hoặc phím **F** = món gần nhất) → ném vào miệng. Lần ném đầu ~50 % rương nhảy né. Mỗi miếng: ngoạm, nhai 3 cái, ực. Cần **2 miếng**. |
| +4,5 s im | Rương tự bò nhảy tới món gần nhất và vồ. Tối đa 14 s thì tự đi tiếp (an toàn). |
| sau miếng 2 | Ợ nhẹ → BoxOpenSequence đi tiếp: Hé nắp → vòng màu (core) chạy. |
| vòng màu | Mỗi bước màu: nhai/co giãn, đôi khi nhảy nhẹ. Ruồi vo ve; ~35–50 % thời gian nó nhảy đớp ruồi. Tim nhanh dần, bụng réo cao dần. Còn ~2,6 s trước chốt: tiếng **dâng** (giống mọi bậc → giữ bí mật). |
| chốt | Xem bảng dưới. Bóng vật phẩm (core) to dần từ miệng như cũ. +2,6 s rương trượt về chỗ cũ. |

## Âm theo bậc lúc chốt
| Bậc | Âm | Rương |
|---|---|---|
| Rác | mọi âm **tắt phụt** → 0,14 s lặng → xì hơi xịt; 0,75 s nhổ đồ rác đã ăn ra; nấc cụt | xẹp lép rồi phồng lại |
| Thường | tắt phụt → ợ nhỏ | nhảy nhẹ, nhổ trả vật của người chơi |
| Hiếm | dâng → nổ nhỏ (cao) + ợ to | nhảy, xoay 360° |
| Rất hiếm | nổ vừa + ợ to | nhảy cao, xoay 360°, hất văng |
| Huyền thoại | nổ lớn | nhảy vọt (4,2 m/s), xoay 720°, **đáp đất nổ lần 2 + hất văng** |

Tiếng chốt/tick của profile ChestFx_Mimic (core) vẫn phát chồng lên.

## Bất biến
- Bậc do `ChestRevealSequence.Play(rolledTier, …)` quyết; hook chỉ đọc bậc trong `OnTierLocked`. Trước đó không
  có gì phụ thuộc bậc (kể cả số miếng phải ăn, tiếng dâng).
- Ngẫu nhiên (`decorationSeed`) chỉ trang trí.
- Vòng màu nhảy rồi dừng đúng bậc, bóng vật phẩm to dần: vẫn do core; hook chỉ **chèn thời gian chờ** trước vòng màu
  (bật `waitForPlayer` của stage 1 lúc chạy, trả lại giá trị cũ khi xong/reset).

## Thử trong Unity
1. Mở cảnh thử rương (xem Docs/HuongDanThuUnity.md), chọn rig Chest_Mimic.
2. Menu **Tools → Cho Do La → Mimic: Them kich ban Ruong Doi vao ruong dang chon** (gỡ MimicHook cũ, tắt bite intro,
   gán 16 âm, tạo/cập nhật profile).
3. Play, mở rương (phím như cũ), bấm chuột vào đồ rơi hoặc F. Thử mỗi bậc bằng phím chọn bậc của ChestRevealDemo.
4. Kiểm: âm có ngay lúc bấm; rương không xuyên sàn; bóng vật phẩm vẫn ở trên miệng; reset trả rương về đúng chỗ.

## Trung thực: chưa chạy / chưa nghe
- **Chưa chạy trong Unity**, chỉ biên dịch với stub (`check_mimic_feed.sh`, 4 bộ define). Chưa nghe âm nào.
- Rủi ro cần xem khi chạy: chiều trục nắp (đọc `lidHingeAxis` qua reflection); rương có collider lớn bao miệng có thể
  chặn đồ ném (đã IgnoreCollision với đồ ăn tự tạo, nhưng vật ngoài thì chưa); camera khoá chuột → bấm lấy tâm màn hình.
- Kiểm biên dịch: `check_mimic_feed.sh` = `check_all.sh` (toàn bộ Scripts + Editor, 3 bộ define) + bộ define Unity 6.
  Sửa file dùng chung duy nhất: thêm `RigidbodyInterpolation`/`interpolation` vào `UnityStubAll.cs` (stub, 2 dòng).

## Nối với khung (`claude/chest-fx-core`, đã merge vào nhánh này)
- Bắt đầu bằng `ChestFxHook.OnPress` (gọi trong `Play()`); việc dò `IsPlaying` trong Update chỉ còn là dự phòng.
- **Âm căng chung**: khi `reveal.Tension` có (mặc định bật), hook KHÔNG tự phát nền bụng réo / nhịp tim / tiếng dâng /
  xịt / nổ lúc chốt nữa — `MimicFeedSetup` gán clip Mimic vào `profile.tension` (loop = bụng réo, pulse = tim,
  deflate = xì hơi, swell = nổ; `pitchEnd` 1,5) để khung lo phần "dâng rồi rẽ theo bậc". Hook vẫn phát khịt mũi lúc
  bấm, ngoạm/nhai/ợ/nhổ/ruồi, cú nổ khi đáp đất, và rương co giật theo `OnTensionPulse`. Tắt `profile.tension.enabled`
  thì hook tự phát bộ âm riêng như bảng trên.
- Chưa dùng: `ChestScript`/`Gate`/`ChestItemSlot`/`ChestMotion` của khung. Màn cho ăn vẫn tự chặn bằng `waitForPlayer`
  của stage 1; nếu muốn đồng bộ có thể đổi sang bước `Gate` + `ChestItemSlot` sau. Không bật `ChestMotion` cho rương
  này: nó cũng ghi vị trí rương trong LateUpdate, sẽ giành với proxy vật lý của Rương Đói.
